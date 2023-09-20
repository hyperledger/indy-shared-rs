use std::cell::{RefCell, RefMut};
use std::fmt::Debug;
use std::fs::File;
use std::io::{self, BufReader, BufWriter, Read, Seek, Write};
use std::path::{Path, PathBuf};

use indy_utils::base58;
use rand::random;
use sha2::{Digest, Sha256};

use crate::anoncreds_clsignatures::{
    Error as ClError, ErrorKind as ClErrorKind, RevocationTailsAccessor, RevocationTailsGenerator,
    Tail,
};
use crate::error::Error;

const TAILS_BLOB_TAG_SZ: u8 = 2;
const TAIL_SIZE: usize = Tail::BYTES_REPR_SIZE;

pub struct TailsFileReader {
    file: RefCell<Option<BufReader<File>>>,
    path: PathBuf,
}

impl TailsFileReader {
    pub fn new<P: Into<PathBuf>>(path: P) -> Self {
        Self {
            file: RefCell::new(None),
            path: path.into(),
        }
    }

    fn opened(&self) -> Result<RefMut<'_, BufReader<File>>, io::Error> {
        let mut inner = self.file.borrow_mut();
        if inner.is_none() {
            inner.replace(BufReader::new(File::open(&self.path)?));
        }
        Ok(RefMut::map(inner, |file| file.as_mut().unwrap()))
    }

    fn read(&self, pos: i64, buf: &mut [u8]) -> Result<(), io::Error> {
        let mut file = self.opened()?;
        let offset = pos - file.stream_position()? as i64;
        file.seek_relative(offset)?;
        file.read_exact(buf)?;
        Ok(())
    }
}

impl Debug for TailsFileReader {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("TailsFileReader")
            .field("path", &self.path)
            .finish_non_exhaustive()
    }
}

impl RevocationTailsAccessor for TailsFileReader {
    fn access_tail(
        &self,
        tail_id: u32,
        accessor: &mut dyn FnMut(&Tail),
    ) -> std::result::Result<(), ClError> {
        trace!("access_tail >>> tail_id: {:?}", tail_id);

        let mut tail_bytes = [0u8; TAIL_SIZE];
        self.read(
            TAIL_SIZE as i64 * tail_id as i64 + TAILS_BLOB_TAG_SZ as i64,
            &mut tail_bytes,
        )
        .map_err(|e| {
            error!("IO error reading tails file: {e}");
            ClError::new(ClErrorKind::InvalidState, "Could not read from tails file")
        })?;
        let tail = Tail::from_bytes(tail_bytes.as_slice())?;
        accessor(&tail);

        trace!("access_tail <<< res: ()");
        Ok(())
    }
}

pub trait TailsWriter: std::fmt::Debug {
    fn write(
        &mut self,
        generator: &mut RevocationTailsGenerator,
    ) -> Result<(String, String), Error>;
}

#[derive(Debug)]
pub struct TailsFileWriter {
    root_path: PathBuf,
}

impl TailsFileWriter {
    pub fn new(root_path: Option<String>) -> Self {
        Self {
            root_path: root_path
                .map(PathBuf::from)
                .unwrap_or_else(std::env::temp_dir),
        }
    }
}

impl TailsWriter for TailsFileWriter {
    fn write(
        &mut self,
        generator: &mut RevocationTailsGenerator,
    ) -> Result<(String, String), Error> {
        struct TempFile<'a>(&'a Path);
        impl TempFile<'_> {
            pub fn rename(self, target: &Path) -> Result<(), Error> {
                let path = std::mem::ManuallyDrop::new(self).0;
                std::fs::rename(path, target)
                    .map_err(|e| err_msg!("Error moving tails temp file {path:?}: {e}"))
            }
        }
        impl Drop for TempFile<'_> {
            fn drop(&mut self) {
                if let Err(e) = std::fs::remove_file(self.0) {
                    error!("Error removing tails temp file {:?}: {e}", self.0);
                }
            }
        }

        let temp_name = format!("{:020}.tmp", random::<u64>());
        let temp_path = self.root_path.join(temp_name);
        let file = File::options()
            .read(true)
            .write(true)
            .create_new(true)
            .open(temp_path.clone())
            .map_err(|e| err_msg!(IOError, "Error creating tails temp file {temp_path:?}: {e}"))?;
        let temp_handle = TempFile(&temp_path);
        let mut buf = BufWriter::new(file);
        let mut hasher = Sha256::default();
        let version = &[0u8, 2u8];
        buf.write_all(version)?;
        hasher.update(version);
        while let Some(tail) = generator.try_next()? {
            let tail_bytes = tail.to_bytes()?;
            buf.write_all(&tail_bytes)?;
            hasher.update(&tail_bytes);
        }
        let mut file = buf
            .into_inner()
            .map_err(|e| err_msg!("Error flushing output file: {e}"))?;
        let tails_size = file.stream_position()?;
        let hash = base58::encode(hasher.finalize());
        let target_path = self.root_path.join(&hash);
        drop(file);
        temp_handle.rename(&target_path)?;
        let target_path = target_path.to_string_lossy().into_owned();
        debug!(
            "TailsFileWriter: wrote tails file [size {}]: {}",
            tails_size, target_path
        );
        Ok((target_path, hash))
    }
}
