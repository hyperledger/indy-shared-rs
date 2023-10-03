#[cfg(feature = "serde")]
#[macro_use]
extern crate serde;

#[cfg(all(feature = "serde", test))]
#[macro_use]
extern crate serde_json;

mod error;

#[macro_use]
mod validation;

pub mod did;
pub mod keys;
pub mod qualifiable;
pub mod utils;

pub use self::{
    error::{ConversionError, ValidationError},
    qualifiable::Qualifiable,
    validation::Validatable,
};

#[cfg(any(feature = "cl", feature = "cl_native"))]
pub use anoncreds_clsignatures;

#[cfg(feature = "anoncreds")]
/// Type definitions related Indy credential issuance and verification
pub mod anoncreds;

#[cfg(feature = "merkle_tree")]
/// Patricia Merkle tree support
pub mod merkle_tree;

mod identifiers;

pub use identifiers::cred_def::*;
pub use identifiers::rev_reg::*;
pub use identifiers::schema::*;

#[cfg(any(feature = "rich_schema", test))]
pub use identifiers::rich_schema::*;

pub use identifiers::DELIMITER as IDENT_DELIMITER;
