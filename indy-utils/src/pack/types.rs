use crate::keys::{SignKey, VerKey};

use std::future::Future;

#[derive(Serialize, Deserialize, Debug, Clone, Eq, PartialEq)]
pub struct JWE {
    pub protected: String,
    pub iv: String,
    pub ciphertext: String,
    pub tag: String,
}

#[derive(Serialize, Deserialize, Debug, Clone, Eq, PartialEq)]
pub struct Recipient {
    pub encrypted_key: String,
    pub header: Header,
}

#[derive(Serialize, Deserialize, Debug, Clone, Eq, PartialEq)]
pub struct Header {
    pub kid: String,
    #[serde(default)]
    #[serde(skip_serializing_if = "Option::is_none")]
    pub iv: Option<String>,
    #[serde(default)]
    #[serde(skip_serializing_if = "Option::is_none")]
    pub sender: Option<String>,
}

#[derive(Serialize, Deserialize, Debug, Clone, Eq, PartialEq)]
pub struct Protected {
    pub enc: String,
    pub typ: String,
    pub alg: String,
    pub recipients: Vec<Recipient>,
}

pub trait KeyLookup: Sync {
    fn find<'f>(
        &'f self,
        key: &'f VerKey,
    ) -> std::pin::Pin<Box<dyn Future<Output = Option<SignKey>> + Send + 'f>>;
}

type KeyLookupCb<'a> = Box<dyn Fn(&VerKey) -> Option<SignKey> + Send + Sync + 'a>;

pub struct KeyLookupFn<'a> {
    cb: KeyLookupCb<'a>,
}

pub fn key_lookup_fn<'a, F>(cb: F) -> KeyLookupFn<'a>
where
    F: Fn(&VerKey) -> Option<SignKey> + Send + Sync + 'a,
{
    KeyLookupFn {
        cb: Box::new(cb) as KeyLookupCb,
    }
}

async fn lazy_lookup<'a>(
    cb: &Box<dyn Fn(&VerKey) -> Option<SignKey> + Send + Sync + 'a>,
    key: &VerKey,
) -> Option<SignKey> {
    cb(key)
}

impl<'a> KeyLookup for KeyLookupFn<'a> {
    fn find<'f>(
        &'f self,
        key: &'f VerKey,
    ) -> std::pin::Pin<Box<dyn Future<Output = Option<SignKey>> + Send + 'f>> {
        Box::pin(lazy_lookup(&self.cb, key))
    }
}
