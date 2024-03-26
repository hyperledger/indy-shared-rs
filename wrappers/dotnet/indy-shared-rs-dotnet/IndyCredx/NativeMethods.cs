using System;
using System.Runtime.InteropServices;
using static indy_shared_rs_dotnet.Models.Structures;

namespace indy_shared_rs_dotnet.IndyCredx
{
    internal static class NativeMethods
    {
        #region Error
        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_get_current_error(ref string errorJson);
        #endregion

        #region Mod
        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_set_default_logger();
        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern string credx_version();
        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_buffer_free(ByteBuffer byteBuffer);
        #endregion

        #region PresReq
        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_generate_nonce(ref string nonce);
        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_presentation_request_from_json(ByteBuffer presentationRequestJson, ref IntPtr presentationRequestObjectHandle);
        #endregion

        #region CredentialDefinition
        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_create_credential_definition(FfiStr originDid, IntPtr schemaObjectHandle, FfiStr tag, FfiStr signatureType, byte supportRevocation,
                                                                         ref IntPtr credDefObjectHandle, ref IntPtr credDefPvtObjectHandle, ref IntPtr keyProofObjectHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_credential_definition_get_attribute(IntPtr credDefObjectHandle, FfiStr attributeName, ref string result);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_credential_definition_from_json(ByteBuffer credDefJson, ref IntPtr credDefObjectHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_credential_definition_private_from_json(ByteBuffer credDefJson, ref IntPtr credDefPrivObjectHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_key_correctness_proof_from_json(ByteBuffer keyCorrectnessProofJson, ref IntPtr keyCorrectnessProofObjectHandle);
        #endregion

        #region CredentialOffer 
        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_create_credential_offer(FfiStr schemaId, IntPtr credDefObjectHandle, IntPtr keyProofObjectHandle, ref IntPtr credOfferHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_credential_offer_from_json(ByteBuffer credOfferJson, ref IntPtr credOfferObjectHandle);
        #endregion

        #region CredentialRequest
        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_create_credential_request(FfiStr proverDid, IntPtr credDefObjectHandle, IntPtr masterSecretObjectHandle, FfiStr masterSecretId, IntPtr credOfferObjectHandle, ref IntPtr credReqObjectHandle, ref IntPtr credReqMetaObjectHandle);
        
        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_credential_request_from_json(ByteBuffer credReqJson, ref IntPtr credReqObjectHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_credential_request_metadata_from_json(ByteBuffer credReqMetaJson, ref IntPtr credReqMetaObjectHandle);
        #endregion

        #region Credential
        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_create_credential(
            IntPtr credDefObjectHandle,
            IntPtr credDefPrivateObjectHandle,
            IntPtr credOfferObjectHandle,
            IntPtr credRequestObjectHandle,
            FfiStrList attrNames,
            FfiStrList attrRawValues,
            FfiStrList attrEncValues,
            FfiCredRevInfo revocation,
            ref IntPtr credObjectHandle,
            ref IntPtr revRegObjectHandle,
            ref IntPtr revDeltaObjectHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_encode_credential_attributes(FfiStrList attrRawValues, ref string result);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_process_credential(IntPtr credObjectHandle, IntPtr credReqObjectHandle, IntPtr masterSecretObjectHandle, IntPtr credDefObjectHandle, IntPtr revRegDefObjectHandle, ref IntPtr resultObjectHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_credential_from_json(ByteBuffer credJson, ref IntPtr credObjectHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_credential_get_attribute(IntPtr credObjectHandle, FfiStr attributeName, ref string result);
        #endregion

        #region MasterSecret
        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_create_master_secret(ref IntPtr masterSecretObjectHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_master_secret_from_json(ByteBuffer masterSecretJson, ref IntPtr masterSecretObjectHandle);
        #endregion

        #region Presentation
        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_create_presentation(
            IntPtr presReqObjectHandle,
            FfiCredentialEntryList credentials,
            FfiCredentialProveList credentialsProof,
            FfiStrList selfAttestNames,
            FfiStrList selfAttestValues,
            IntPtr masterSecret,
            FfiUIntList schemas,
            FfiUIntList credDefs,
            ref IntPtr presentationObjectHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_presentation_from_json(ByteBuffer presentationJson, ref IntPtr presentationObjectHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_verify_presentation(
            IntPtr presObjectHandle,
            IntPtr presReqObjectHandle,
            FfiUIntList schemaObjectHandles,
            FfiUIntList credDefObjectHandles,
            FfiUIntList revRegDefObjectHandles,
            FfiRevocationEntryList revRegEntries,
            ref byte verifyResult);
        #endregion

        #region Revocation
        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_create_revocation_registry(
            FfiStr originDid,
            IntPtr credDefObjectHandle,
            FfiStr tag,
            FfiStr revRegType,
            FfiStr issuanceType,
            long maxCredNumber,
            FfiStr tailsDirPath,
            ref IntPtr regDefObjectHandle,
            ref IntPtr regDefPvtObjectHandle,
            ref IntPtr regEntryObjectHandle,
            ref IntPtr regInitDeltaObjectHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_revocation_registry_from_json(ByteBuffer revRegJson, ref IntPtr revRegObjectHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_revocation_registry_definition_from_json(ByteBuffer revRegJson, ref IntPtr revRegObjectHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_revocation_registry_definition_private_from_json(ByteBuffer revRegDefPvtJson, ref IntPtr revRegDefPvtObjectHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_update_revocation_registry(
            IntPtr revRegDefObjectHandle,
            IntPtr revRegObjectHandle,
            FfiLongList issued,
            FfiLongList revoked,
            FfiStr tailsPath,
            ref IntPtr revRegUpdatedObjectHandle,
            ref IntPtr revRegDeltaObjectHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_revoke_credential(
            IntPtr revRegDefObjectHandle,
            IntPtr revRegObjectHandle,
            long credRevIdx,
            FfiStr tailsPath,
            ref IntPtr revRegUpdatedObjectHandle,
            ref IntPtr revRegDeltaObjectHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_revocation_registry_definition_get_attribute(
            IntPtr regDefObjectHandle,
            FfiStr attributeName,
            ref string result);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_merge_revocation_registry_deltas(
            IntPtr revRegDelta1ObjectHandle,
            IntPtr revRegDelta2ObjectHandle,
            ref IntPtr revRegDeltaNewObjectHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_create_or_update_revocation_state(
            IntPtr revRegDefObjectHandle,
            IntPtr revRegDeltaObjectHandle,
            long revRegIndex,
            long timestamp,
            FfiStr tailsPath,
            IntPtr revStateObjectHandle,
            ref IntPtr revStateNewObjectHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_revocation_state_from_json(ByteBuffer credentialRevocationStateJson, ref IntPtr credentialRevocationStateObjectHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_revocation_registry_delta_from_json(ByteBuffer credentialRevocationRegistryDeltaJson, ref IntPtr credentialRevocationRegistryDeltaObjectHandle);
        #endregion

        #region Schema
        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int credx_create_schema(FfiStr originDid, FfiStr schemaName, FfiStr schemaVersion, FfiStrList attrNames, long seqNo, ref IntPtr schemaObjectHandle);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_schema_get_attribute(IntPtr schemaObjectHandle, FfiStr attributeName, ref string result);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_schema_from_json(ByteBuffer schemaJson, ref IntPtr schemaObjectHandle);

        #endregion

        #region ObjectHandle
        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_object_get_type_name(IntPtr objectHandle, ref string result);

        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int credx_object_get_json(IntPtr objectHandle, ref ByteBuffer result);
        [DllImport(Consts.CREDX_LIB_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern void credx_object_free(IntPtr objectHandle);
        #endregion
    }
}