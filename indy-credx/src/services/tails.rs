use rand::random;
use std::cell::RefCell;
use std::fs::File;
use std::io::{BufReader, BufWriter, Read, Seek, SeekFrom, Write};
use std::path::{Path, PathBuf};

use indy_utils::base58;
use sha2::{Digest, Sha256};

use crate::error::Error;
use crate::ursa::{
    cl::{RevocationTailsAccessor, RevocationTailsGenerator, Tail},
    errors::{UrsaCryptoError, UrsaCryptoErrorKind},
};

const TAILS_BLOB_TAG_SZ: u8 = 2;
const TAIL_SIZE: usize = Tail::BYTES_REPR_SIZE;

#[derive(Debug)]
pub struct TailsReader {
    inner: Box<RefCell<dyn TailsReaderImpl>>,
}

impl TailsReader {
    pub(crate) fn new<TR: TailsReaderImpl + 'static>(inner: TR) -> Self {
        Self {
            inner: Box::new(RefCell::new(inner)),
        }
    }
}

pub trait TailsReaderImpl: std::fmt::Debug + Send {
    fn hash(&mut self) -> Result<Vec<u8>, Error>;
    fn read(&mut self, size: usize, offset: usize) -> Result<Vec<u8>, Error>;
}

impl RevocationTailsAccessor for TailsReader {
    fn access_tail(
        &self,
        tail_id: u32,
        accessor: &mut dyn FnMut(&Tail),
    ) -> std::result::Result<(), UrsaCryptoError> {
        trace!("access_tail >>> tail_id: {:?}", tail_id);

        let tail_bytes = self
            .inner
            .borrow_mut()
            .read(
                TAIL_SIZE,
                TAIL_SIZE * tail_id as usize + TAILS_BLOB_TAG_SZ as usize,
            )
            .map_err(|_| {
                UrsaCryptoError::from_msg(
                    UrsaCryptoErrorKind::InvalidState,
                    "Can't read tail bytes from file",
                )
            })?; // FIXME: IO error should be returned

        let tail = Tail::from_bytes(tail_bytes.as_slice())?;
        accessor(&tail);

        trace!("access_tail <<< res: ()");
        Ok(())
    }
}

#[derive(Debug)]
pub struct TailsFileReader {
    path: String,
    file: Option<BufReader<File>>,
    hash: Option<Vec<u8>>,
}

impl TailsFileReader {
    pub fn new(path: &str) -> TailsReader {
        TailsReader::new(Self {
            path: path.to_owned(),
            file: None,
            hash: None,
        })
    }

    pub fn open(&mut self) -> Result<(), Error> {
        if self.file.is_some() {
            Ok(())
        } else {
            let file = File::open(self.path.clone())?;
            self.file.replace(BufReader::new(file));
            Ok(())
        }
    }

    pub fn close(&mut self) {
        self.file.take();
    }
}

impl TailsReaderImpl for TailsFileReader {
    fn hash(&mut self) -> Result<Vec<u8>, Error> {
        if self.hash.is_some() {
            return Ok(self.hash.as_ref().unwrap().clone());
        }

        self.open()?;
        let file = self.file.as_mut().unwrap();
        file.seek(SeekFrom::Start(0))?;
        let mut hasher = Sha256::default();
        let mut buf = [0u8; 1024];

        loop {
            let sz = file.read(&mut buf)?;
            if sz == 0 {
                self.hash = Some(hasher.finalize().to_vec());
                return Ok(self.hash.as_ref().unwrap().clone());
            }
            hasher.update(&buf[0..sz]);
        }
    }

    fn read(&mut self, size: usize, offset: usize) -> Result<Vec<u8>, Error> {
        let mut buf = vec![0u8; size];

        self.open()?;
        let file = self.file.as_mut().unwrap();
        file.seek(SeekFrom::Start(offset as u64))?;
        file.read_exact(buf.as_mut_slice())?;

        Ok(buf)
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
                .unwrap_or_else(|| std::env::temp_dir()),
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
                    .map_err(|e| err_msg!("Error moving tails temp file: {}", e))
            }
        }
        impl Drop for TempFile<'_> {
            fn drop(&mut self) {
                if let Err(e) = std::fs::remove_file(self.0) {
                    error!("Error removing tails temp file: {}", e);
                }
            }
        }

        let temp_name = format!("{:020}.tmp", random::<u64>());
        let temp_path = self.root_path.with_file_name(temp_name);
        let file = File::create(temp_path.clone())
            .map_err(|e| err_msg!(IOError, "Error creating tails temp file: {}", e))?;
        let temp_handle = TempFile(&temp_path);
        let mut buf = BufWriter::new(file);
        let mut hasher = Sha256::default();
        let version = &[0u8, 2u8];
        buf.write(version)?;
        hasher.update(version);
        while let Some(tail) = generator.try_next()? {
            let tail_bytes = tail.to_bytes()?;
            buf.write(tail_bytes.as_slice())?;
            hasher.update(tail_bytes);
        }
        let mut file = buf
            .into_inner()
            .map_err(|err| err_msg!("Error flushing output file: {}", err))?;
        let tails_size = file.seek(SeekFrom::Current(0))?;
        let hash = base58::encode(hasher.finalize());
        let target_path = self.root_path.with_file_name(&hash);
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
