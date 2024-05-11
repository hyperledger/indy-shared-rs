using indy_shared_rs_dotnet.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using static indy_shared_rs_dotnet.Models.Structures;

namespace indy_shared_rs_dotnet.IndyCredx
{
    public static class CredentialRequestApi
    {
        /// <summary>
        /// Creates a new <see cref="CredentialRequest"/> from <see cref="CredentialDefinition"/>.
        /// </summary>
        /// <param name="proverDid">Prover DID.</param>
        /// <param name="credentialDefinition">Credential definition.</param>
        /// <param name="masterSecret">New master secret.</param>
        /// <param name="masterSecretId">Id of master secret.</param>
        /// <param name="credentialOffer">Credential offer.</param>
        /// <exception cref="SharedRsException">Throws if any argument is invalid.</exception>
        /// <returns>New <see cref="CredentialRequest"/> and its <see cref="CredentialRequestMetadata"/>.</returns>
        public static async Task<(CredentialRequest, CredentialRequestMetadata)> CreateCredentialRequestAsync(
            string proverDid,
            CredentialDefinition credentialDefinition,
            MasterSecret masterSecret,
            string masterSecretId,
            CredentialOffer credentialOffer)
        {
            IntPtr requestHandle = new IntPtr();
            IntPtr metadataHandle = new IntPtr();
            int errorCode = NativeMethods.credx_create_credential_request(
                FfiStr.Create(proverDid),
                credentialDefinition.Handle,
                masterSecret.Handle,
                FfiStr.Create(masterSecretId),
                credentialOffer.Handle,
                ref requestHandle,
                ref metadataHandle);

            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }
            CredentialRequest requestObject = await CreateCredentialRequestObject(requestHandle);
            CredentialRequestMetadata metadataObject = await CreateCredentialRequestMetadataObject(metadataHandle);
            return (requestObject, metadataObject);
        }

        /// <summary>
        /// Creates a new <see cref="CredentialRequest"/> from <see cref="CredentialDefinition"/>.
        /// </summary>
        /// <param name="proverDid">Prover DID.</param>
        /// <param name="credentialDefinition">Credential definition.</param>
        /// <param name="masterSecret">New master secret.</param>
        /// <param name="masterSecretId">Id of master secret.</param>
        /// <param name="credentialOffer">Credential offer.</param>
        /// <exception cref="SharedRsException">Throws if any argument is invalid.</exception>
        /// <returns>New <see cref="CredentialRequest"/> as JSON string and its <see cref="CredentialRequestMetadata"/> as JSON string.</returns>
        public static async Task<(string, string)> CreateCredentialRequestJsonAsync(
            string proverDid,
            string credentialDefinitionJson,
            string masterSecretJson,
            string masterSecretId,
            string credentialOfferJson)
        {
            IntPtr credDefObjectHandle = new IntPtr();
            IntPtr masterSecretObjectHandle = new IntPtr();
            IntPtr credentialOfferObjectHandle = new IntPtr();

            _ = NativeMethods.credx_credential_definition_from_json(ByteBuffer.Create(credentialDefinitionJson), ref credDefObjectHandle);
            _ = NativeMethods.credx_master_secret_from_json(ByteBuffer.Create(masterSecretJson), ref masterSecretObjectHandle);
            _ = NativeMethods.credx_credential_offer_from_json(ByteBuffer.Create(credentialOfferJson), ref credentialOfferObjectHandle);

            IntPtr requestHandle = new IntPtr();
            IntPtr metadataHandle = new IntPtr();
            int errorCode = NativeMethods.credx_create_credential_request(
                FfiStr.Create(proverDid),
                credDefObjectHandle,
                masterSecretObjectHandle,
                FfiStr.Create(masterSecretId),
                credentialOfferObjectHandle,
                ref requestHandle,
                ref metadataHandle);

            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }
            string requestJson = await ObjectApi.ToJsonAsync(requestHandle);
            string metadataJson = await ObjectApi.ToJsonAsync(metadataHandle);

            return (requestJson, metadataJson);
        }

        #region private methods
        private static async Task<CredentialRequest> CreateCredentialRequestObject(IntPtr objectHandle)
        {
            string credReqJson = await ObjectApi.ToJsonAsync(objectHandle);
            CredentialRequest requestObject = JsonConvert.DeserializeObject<CredentialRequest>(credReqJson, Settings.JsonSettings);
            requestObject.JsonString = credReqJson;
            requestObject.Handle = objectHandle;
            return await Task.FromResult(requestObject);
        }
        private static async Task<CredentialRequestMetadata> CreateCredentialRequestMetadataObject(IntPtr objectHandle)
        {
            string credMetadataJson = await ObjectApi.ToJsonAsync(objectHandle);
            CredentialRequestMetadata requestObject = JsonConvert.DeserializeObject<CredentialRequestMetadata>(credMetadataJson, Settings.JsonSettings);
            requestObject.JsonString = credMetadataJson;
            requestObject.Handle = objectHandle;
            return await Task.FromResult(requestObject);
        }
        #endregion
    }
}