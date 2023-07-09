#[cfg(any(feature = "cl", feature = "cl_native"))]
macro_rules! cl_type {
    ($ident:ident) => {
        $crate::anoncreds_clsignatures::$ident
    };
}

#[cfg(not(any(feature = "cl", feature = "cl_native")))]
macro_rules! cl_type {
    ($ident:path) => {
        serde_json::Value
    };
}
