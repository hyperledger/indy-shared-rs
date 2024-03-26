using indy_shared_rs_dotnet.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static indy_shared_rs_dotnet.Models.Structures;

namespace indy_shared_rs_dotnet.IndyCredx
{
    public static class CredentialOfferApi
    {
        /// <summary>
        /// Create a new <see cref="CredentialOffer"/> from <see cref="CredentialDefinition"/>.
        /// </summary>
        /// <param name="schemaId">Id of the corresponding schema.</param>
        /// <param name="credDefObject">Credential definition.</param>
        /// <param name="keyProofObject">Key correctness proof.</param>
        /// <exception cref="SharedRsException">Throws if any parameter is invalid.</exception>
        /// <returns>A new <see cref="CredentialOffer"/>.</returns>
        public static async Task<CredentialOffer> CreateCredentialOfferAsync(
            string schemaId,
            CredentialDefinition credDefObject,
            CredentialKeyCorrectnessProof keyProofObject)
        {
            IntPtr credOfferObjectHandle = new IntPtr();
            int errorCode = NativeMethods.credx_create_credential_offer(FfiStr.Create(schemaId), credDefObject.Handle, keyProofObject.Handle, ref credOfferObjectHandle);

            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }

            CredentialOffer credOfferObject = await CreateCredentialOfferObject(credOfferObjectHandle);
            return await Task.FromResult(credOfferObject);
        }

        /// <summary>
        /// Create a new <see cref="CredentialOffer"/> from <see cref="CredentialDefinition"/>.
        /// </summary>
        /// <param name="schemaId">Id of the corresponding schema.</param>
        /// <param name="credDefObjectJson">Credential definition as JSON string.</param>
        /// <param name="keyProofObjectJson">Key correctness proof as JSON string.</param>
        /// <exception cref="SharedRsException">Throws if any parameter is invalid.</exception>
        /// <returns>A new <see cref="CredentialOffer"/> as JSON string.</returns>
        public static async Task<string> CreateCredentialOfferJsonAsync(
            string schemaId,
            string credDefObjectJson,
            string keyProofObjectJson)
        {
            IntPtr credDefObjectHandle = new IntPtr();
            IntPtr keyProofObjecthandle = new IntPtr();
            _ = NativeMethods.credx_credential_definition_from_json(ByteBuffer.Create(credDefObjectJson), ref credDefObjectHandle);
            _ = NativeMethods.credx_key_correctness_proof_from_json(ByteBuffer.Create(keyProofObjectJson), ref keyProofObjecthandle);

            IntPtr credOfferObjectHandle = new IntPtr();
            int errorCode = NativeMethods.credx_create_credential_offer(FfiStr.Create(schemaId), credDefObjectHandle, keyProofObjecthandle, ref credOfferObjectHandle);

            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }

            string credOfferJson = await ObjectApi.ToJsonAsync(credOfferObjectHandle);
            return await Task.FromResult(credOfferJson);
        }

        #region private methods
        private static async Task<CredentialOffer> CreateCredentialOfferObject(IntPtr objectHandle)
        {
            string credOfferJson = await ObjectApi.ToJsonAsync(objectHandle);
            CredentialOffer credOfferObject = JsonConvert.DeserializeObject<CredentialOffer>(credOfferJson, Settings.JsonSettings);
            credOfferObject.JsonString = credOfferJson;
            credOfferObject.Handle = objectHandle;

            try
            {
                JObject jObj = JObject.Parse(credOfferJson);
                credOfferObject.KeyCorrectnessProof.XrCap = new List<KeyProofAttributeValue>();
                foreach (JToken ele in jObj["key_correctness_proof"]["xr_cap"])
                {
                    KeyProofAttributeValue attribute = new KeyProofAttributeValue(ele.First.ToString(), ele.Last.ToString());
                    credOfferObject.KeyCorrectnessProof.XrCap.Add(attribute);
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException("Could not find field xr_cap.", e);
            }

            return await Task.FromResult(credOfferObject);
        }
        #endregion
    }
}