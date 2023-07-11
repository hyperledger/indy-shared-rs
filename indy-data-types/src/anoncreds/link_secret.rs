use std::fmt;

use serde::{Deserialize, Serialize};

use crate::anoncreds_clsignatures::{MasterSecret as ClMasterSecret, Prover as ClProver};
use crate::ConversionError;

#[derive(Serialize, Deserialize)]
pub struct LinkSecret {
    pub value: ClMasterSecret,
}

impl LinkSecret {
    #[cfg(any(feature = "cl", feature = "cl_native"))]
    #[inline]
    pub fn new() -> Result<Self, ConversionError> {
        let value = ClProver::new_master_secret().map_err(|err| {
            ConversionError::from_msg(format!("Error creating master secret: {}", err))
        })?;
        Ok(Self { value })
    }

    pub fn try_clone(&self) -> Result<Self, ConversionError> {
        Ok(Self {
            value: self.value.try_clone().map_err(|e| e.to_string())?,
        })
    }
}

impl fmt::Debug for LinkSecret {
    fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
        f.debug_tuple("LinkSecret")
            .field(if cfg!(test) { &self.value } else { &"<hidden>" })
            .finish()
    }
}
