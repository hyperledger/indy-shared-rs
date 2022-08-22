using indy_shared_rs_dotnet.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static indy_shared_rs_dotnet.Models.Structures;

namespace indy_shared_rs_dotnet.IndyCredx
{
    public static class CredentialDefinitionApi
    {
        /// <summary>
        /// Creates a new <see cref="CredentialDefinition"/> from schema and other parameters (only signatureType "CL" supported so far).
        /// </summary>
        /// <param name="originDid">Issuer DID.</param>
        /// <param name="schemaObject">Corresponding schema.</param>
        /// <param name="tag">Tag name.</param>
        /// <param name="signatureType">Type of the sginature.</param>
        /// <param name="supportRevocation">Flag if revocation is supported or not.</param>
        /// <exception cref="SharedRsException">Throws if any provided parameters are invalid.</exception>
        /// <returns>The new <see cref="CredentialDefinition"/>, <see cref="CredentialDefinitionPrivate"/> and <see cref="CredentialKeyCorrectnessProof"/>.</returns>
        public static async Task<(CredentialDefinition, CredentialDefinitionPrivate, CredentialKeyCorrectnessProof)> CreateCredentialDefinitionAsync(
            string originDid,
            Schema schemaObject,
            string tag,
            SignatureType signatureType,
            bool supportRevocation)
        {
            IntPtr credDefHandle = new IntPtr();
            IntPtr credDefPvtHandle = new IntPtr();
            IntPtr keyProofHandle = new IntPtr();
            int errorCode = NativeMethods.credx_create_credential_definition(
                FfiStr.Create(originDid),
                schemaObject.Handle,
                FfiStr.Create(tag),
                FfiStr.Create(signatureType.ToString()),
                Convert.ToByte(supportRevocation),
                ref credDefHandle,
                ref credDefPvtHandle,
                ref keyProofHandle);

            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }

            CredentialDefinition credDefObject = await CreateCredentialDefinitonObject(credDefHandle);
            CredentialDefinitionPrivate credDefPvtObject = await CreateCredentialDefinitonPrivateObject(credDefPvtHandle);
            CredentialKeyCorrectnessProof keyProofObject = await CreateCredentialKeyProofObject(keyProofHandle);
            return await Task.FromResult((credDefObject, credDefPvtObject, keyProofObject));
        }

        /// <summary>
        /// Creates a new <see cref="CredentialDefinition"/> as JSON string from schema and other parameters (only signatureType "CL" supported so far).
        /// </summary>
        /// <param name="originDid">Issuer DID.</param>
        /// <param name="schemaObjectJson">Corresponding schema.</param>
        /// <param name="tag">Tag name.</param>
        /// <param name="signatureType">Type of the sginature.</param>
        /// <param name="supportRevocation">Flag if revocation is supported or not.</param>
        /// <exception cref="SharedRsException">Throws if any provided parameters are invalid.</exception>
        /// <returns>The new <see cref="CredentialDefinition"/> as JSON string, <see cref="CredentialDefinitionPrivate"/> as JSON string and <see cref="CredentialKeyCorrectnessProof"/> as JSON String.</returns>
        public static async Task<(string, string, string)> CreateCredentialDefinitionJsonAsync(
            string originDid,
            string schemaObjectJson,
            string tag,
            SignatureType signatureType,
            bool supportRevocation)
        {
            IntPtr credDefHandle = new IntPtr();
            IntPtr credDefPvtHandle = new IntPtr();
            IntPtr keyProofHandle = new IntPtr();
            IntPtr schemaObjectHandle = new IntPtr();
            _ = NativeMethods.credx_schema_from_json(ByteBuffer.Create(schemaObjectJson), ref schemaObjectHandle);

            int errorCode = NativeMethods.credx_create_credential_definition(
                FfiStr.Create(originDid),
                schemaObjectHandle,
                FfiStr.Create(tag),
                FfiStr.Create(signatureType.ToString()),
                Convert.ToByte(supportRevocation),
                ref credDefHandle,
                ref credDefPvtHandle,
                ref keyProofHandle);

            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }

            string credDefObjectJson = await ObjectApi.ToJsonAsync(credDefHandle);
            string credDefPvtObjectJson = await ObjectApi.ToJsonAsync(credDefPvtHandle);
            string keyProofObjectJson = await ObjectApi.ToJsonAsync(keyProofHandle);

            return await Task.FromResult((credDefObjectJson, credDefPvtObjectJson, keyProofObjectJson));
        }

        /// <summary>
        /// Returns the value of a <see cref="CredentialDefinition"/> attribute (only the attribute names "id" and "schema_id" are supported so far).
        /// </summary>
        /// <param name="credDefObject">Definition to get the value from.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <exception cref="SharedRsException">Throws if <paramref name="attributeName"/> or <paramref name="credDefObject"/> are invalid.</exception>
        /// <returns>The value of the requested <paramref name="attributeName"/> from the provided <paramref name="credDefObject"/>.</returns>
        public static async Task<string> GetCredentialDefinitionAttributeAsync(CredentialDefinition credDefObject, string attributeName)
        {
            string result = "";
            int errorCode = NativeMethods.credx_credential_definition_get_attribute(credDefObject.Handle, FfiStr.Create(attributeName), ref result);

            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }
            return await Task.FromResult(result);
        }
        /// <summary>
        /// Returns the value of a <see cref="CredentialDefinition"/> attribute (only the attribute names "id" and "schema_id" are supported so far).
        /// </summary>
        /// <param name="credDefObjectJson">Definition as JSON string to get the value from.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <exception cref="SharedRsException">Throws if <paramref name="attributeName"/> or <paramref name="credDefObject"/> are invalid.</exception>
        /// <returns>The value of the requested <paramref name="attributeName"/> from the provided <paramref name="credDefObject"/>.</returns>
        public static async Task<string> GetCredentialDefinitionAttributeAsync(string credDefObjectJson, string attributeName)
        {
            IntPtr credDefHandle = new IntPtr();
            _ = NativeMethods.credx_credential_definition_from_json(ByteBuffer.Create(credDefObjectJson), ref credDefHandle);
            string result = "";
            int errorCode = NativeMethods.credx_credential_definition_get_attribute(credDefHandle, FfiStr.Create(attributeName), ref result);

            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }
            return await Task.FromResult(result);
        }

        #region private methods
        /// <summary>
        /// Creates a <see cref="CredentialDefinition"/> object from json <see cref="string"/>.
        /// </summary>
        /// <param name="credDefJson">Json string encoding a credential definition object.</param>
        /// <exception cref="SharedRsException">Throws when <paramref name="credDefJson"/> is invalid.</exception>
        /// <returns>The new <see cref="CredentialDefinition"/> object.</returns>
        public static async Task<CredentialDefinition> CreateCredentialDefinitionFromJsonAsync(string credDefJson)
        {
            IntPtr credDefHandle = new IntPtr();
            int errorCode = NativeMethods.credx_credential_definition_from_json(ByteBuffer.Create(credDefJson), ref credDefHandle);

            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }

            CredentialDefinition result = await CreateCredentialDefinitonObject(credDefHandle);
            return await Task.FromResult(result);
        }

        private static async Task<CredentialDefinition> CreateCredentialDefinitonObject(IntPtr objectHandle)
        {
            string credDefJson = await ObjectApi.ToJsonAsync(objectHandle);
            CredentialDefinition credDefObject = JsonConvert.DeserializeObject<CredentialDefinition>(credDefJson, Settings.JsonSettings);
            credDefObject.JsonString = credDefJson;
            credDefObject.Handle = objectHandle;

            try
            {
                JObject jObj = JObject.Parse(credDefJson);
                credDefObject.Value.Primary.R = new List<KeyProofAttributeValue>();
                foreach (JToken ele in jObj["value"]["primary"]["r"])
                {
                    string[] attrFields = ele.ToString().Split(':');
                    KeyProofAttributeValue attribute = new KeyProofAttributeValue(JsonConvert.DeserializeObject<string>(attrFields[0]), JsonConvert.DeserializeObject<string>(attrFields[1]));
                    credDefObject.Value.Primary.R.Add(attribute);
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException("Could not find field r.", e);
            }
            return await Task.FromResult(credDefObject);
        }

        private static async Task<CredentialDefinitionPrivate> CreateCredentialDefinitonPrivateObject(IntPtr objectHandle)
        {
            string credDefPvtJson = await ObjectApi.ToJsonAsync(objectHandle);
            CredentialDefinitionPrivate credDefPvtObject = JsonConvert.DeserializeObject<CredentialDefinitionPrivate>(credDefPvtJson, Settings.JsonSettings);
            credDefPvtObject.JsonString = credDefPvtJson;
            credDefPvtObject.Handle = objectHandle;
            return await Task.FromResult(credDefPvtObject);
        }

        private static async Task<CredentialKeyCorrectnessProof> CreateCredentialKeyProofObject(IntPtr objectHandle)
        {
            string keyProofJson = await ObjectApi.ToJsonAsync(objectHandle);
            CredentialKeyCorrectnessProof keyProofObject = JsonConvert.DeserializeObject<CredentialKeyCorrectnessProof>(keyProofJson, Settings.JsonSettings);
            keyProofObject.JsonString = keyProofJson;
            keyProofObject.Handle = objectHandle;

            try
            {
                JObject jObj = JObject.Parse(keyProofJson);
                keyProofObject.XrCap = new List<KeyProofAttributeValue>();
                foreach (JToken ele in jObj["xr_cap"])
                {
                    KeyProofAttributeValue attribute = new KeyProofAttributeValue(ele.First.ToString(), ele.Last.ToString());
                    keyProofObject.XrCap.Add(attribute);
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException("Could not find field xr_cap.", e);
            }
            return await Task.FromResult(keyProofObject);
        }
        #endregion
    }
}