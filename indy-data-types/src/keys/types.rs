use std::fmt::{self, Display, Formatter};
use std::ops::Deref;
use std::str::FromStr;

use crate::ConversionError;

pub const KEY_ENC_BASE58: &str = "base58";

pub const KEY_TYPE_ED25519: &str = "ed25519";
pub const KEY_TYPE_X25519: &str = "x25519";

/// Enum of known and unknown key types
#[derive(Clone, Debug, Default, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub enum KeyType {
    #[default]
    ED25519,
    X25519,
    Other(String),
}

impl KeyType {
    pub fn is_known(&self) -> bool {
        !matches!(self, Self::Other(_))
    }

    pub fn as_str(&self) -> &str {
        match self {
            Self::ED25519 => KEY_TYPE_ED25519,
            Self::X25519 => KEY_TYPE_X25519,
            Self::Other(t) => t.as_str(),
        }
    }
}

impl FromStr for KeyType {
    type Err = ConversionError;

    fn from_str(keytype: &str) -> Result<KeyType, ConversionError> {
        Ok(match keytype.to_ascii_lowercase().as_str() {
            KEY_TYPE_ED25519 => KeyType::ED25519,
            KEY_TYPE_X25519 => KeyType::X25519,
            _ => KeyType::Other(keytype.to_owned()),
        })
    }
}

impl Display for KeyType {
    fn fmt(&self, f: &mut Formatter<'_>) -> fmt::Result {
        f.write_str(self.as_str())
    }
}

impl std::ops::Deref for KeyType {
    type Target = str;
    fn deref(&self) -> &str {
        self.as_str()
    }
}

impl From<&str> for KeyType {
    fn from(value: &str) -> Self {
        Self::from_str(value).unwrap()
    }
}

impl From<String> for KeyType {
    fn from(value: String) -> Self {
        Self::from_str(&value).unwrap()
    }
}

/// Enum of known and unknown key encodings
#[derive(Clone, Debug, Default, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub enum KeyEncoding {
    #[default]
    BASE58,
    Other(String),
}

impl KeyEncoding {
    pub fn as_str(&self) -> &str {
        match self {
            Self::BASE58 => KEY_ENC_BASE58,
            Self::Other(e) => e.as_str(),
        }
    }
}

impl FromStr for KeyEncoding {
    type Err = ConversionError;

    fn from_str(keyenc: &str) -> Result<KeyEncoding, ConversionError> {
        Ok(match keyenc.to_ascii_lowercase().as_str() {
            KEY_ENC_BASE58 => KeyEncoding::BASE58,
            _ => KeyEncoding::Other(keyenc.to_owned()),
        })
    }
}

impl Display for KeyEncoding {
    fn fmt(&self, f: &mut Formatter<'_>) -> fmt::Result {
        f.write_str(self.as_str())
    }
}

impl Deref for KeyEncoding {
    type Target = str;
    fn deref(&self) -> &str {
        self.as_str()
    }
}

impl From<&str> for KeyEncoding {
    fn from(value: &str) -> Self {
        Self::from_str(value).unwrap()
    }
}

impl From<String> for KeyEncoding {
    fn from(value: String) -> Self {
        Self::from_str(&value).unwrap()
    }
}
