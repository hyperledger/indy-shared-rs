use super::error::{catch_error, ErrorCode};
use super::object::ObjectHandle;
use crate::services::{prover::create_link_secret, types::LinkSecret};

#[no_mangle]
pub extern "C" fn credx_create_link_secret(link_secret_p: *mut ObjectHandle) -> ErrorCode {
    catch_error(|| {
        check_useful_c_ptr!(link_secret_p);
        let secret = ObjectHandle::create(create_link_secret()?)?;
        unsafe { *link_secret_p = secret };
        Ok(())
    })
}

impl_indy_object!(LinkSecret, "LinkSecret");
impl_indy_object_from_json!(LinkSecret, credx_link_secret_from_json);
