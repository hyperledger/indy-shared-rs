use std::marker::PhantomData;
use std::slice;

use ffi_support::FfiStr;

use crate::error::Result;

#[derive(Debug)]
#[repr(C)]
pub struct FfiList<'a, T> {
    count: i64,
    data: *const T,
    _pd: PhantomData<&'a ()>,
}

impl<'a, T> FfiList<'a, T> {
    #[inline]
    pub fn as_slice(&self) -> Result<&[T]> {
        if self.data.is_null() || self.count == 0 {
            Ok(&[])
        } else if self.count < 0 {
            return Err(err_msg!(Input, "Invalid index for result set"));
        } else {
            Ok(unsafe { slice::from_raw_parts(self.data, self.count as usize) })
        }
    }

    #[inline]
    pub fn try_collect<R>(&self, f: impl FnMut(&T) -> Result<R>) -> Result<Vec<R>> {
        self.as_slice()?.iter().map(f).collect()
    }

    #[inline]
    pub fn is_empty(&self) -> bool {
        self.len() == 0
    }

    #[inline]
    pub fn len(&self) -> i64 {
        if self.data.is_null() {
            0
        } else {
            self.count
        }
    }
}

pub type FfiStrList<'a> = FfiList<'a, FfiStr<'a>>;

impl<'a> FfiStrList<'a> {
    pub fn to_string_vec(&self) -> Result<Vec<String>> {
        self.try_collect(|s| {
            Ok(s.as_opt_str()
                .ok_or_else(|| err_msg!("Expected non-empty string"))?
                .to_string())
        })
    }
}
