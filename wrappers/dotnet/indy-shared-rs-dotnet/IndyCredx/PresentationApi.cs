using indy_shared_rs_dotnet.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static indy_shared_rs_dotnet.Models.Structures;

namespace indy_shared_rs_dotnet.IndyCredx
{
    public static class PresentationApi
    {
        /// <summary>
        /// Creates a new <see cref="Presentation"/> object from parameters.
        /// </summary>
        /// <param name="presentationRequest">Presentation request.</param>
        /// <param name="credentialEntries">Credential entries.</param>
        /// <param name="credentialProofs">Credential proofs.</param>
        /// <param name="selfAttestNames">Names of self attested attributes.</param>
        /// <param name="selfAttestValues">Values of self attested attributes.</param>
        /// <param name="masterSecret">Master secret.</param>
        /// <param name="schemas">Corresponding schemas.</param>
        /// <param name="credDefs">Credential definitions.</param>
        /// <exception cref="SharedRsException">Throws when any parameters are invalid.</exception>
        /// <returns>New <see cref="Presentation"/> object.</returns>
        public static async Task<Presentation> CreatePresentationAsync(
            PresentationRequest presentationRequest,
            List<CredentialEntry> credentialEntries,
            List<CredentialProof> credentialProofs,
            List<string> selfAttestNames,
            List<string> selfAttestValues,
            MasterSecret masterSecret,
            List<Schema> schemas,
            List<CredentialDefinition> credDefs)
        {
            IntPtr presentationObjectHandle = new IntPtr();
            List<IntPtr> schemaHandles = (from schema in schemas
                                        select schema.Handle).ToList();
            List<IntPtr> credDefHandles = (from credDef in credDefs
                                         select credDef.Handle).ToList();

            int errorCode = NativeMethods.credx_create_presentation(
                presentationRequest.Handle,
                FfiCredentialEntryList.Create(credentialEntries),
                FfiCredentialProveList.Create(credentialProofs),
                FfiStrList.Create(selfAttestNames),
                FfiStrList.Create(selfAttestValues),
                masterSecret.Handle,
                FfiUIntList.Create(schemaHandles),
                FfiUIntList.Create(credDefHandles),
                ref presentationObjectHandle);

            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }

            Presentation presentationObject = await CreatePresentationObject(presentationObjectHandle);
            return await Task.FromResult(presentationObject);
        }

        /// <summary>
        /// Creates a new <see cref="Presentation"/> object from parameters.
        /// </summary>
        /// <param name="presentationRequestJson">Presentation request JSON.</param>
        /// <param name="credentialEntryJsons">Credential entry JSONs.</param>
        /// <param name="credentialProofJsons">Credential proof JSONs.</param>
        /// <param name="selfAttestNames">Names of self attested attributes.</param>
        /// <param name="selfAttestValues">Values of self attested attributes.</param>
        /// <param name="masterSecretJson">Master secret JSON.</param>
        /// <param name="schemaJsons">Corresponding schema JSONs.</param>
        /// <param name="credDefJsons">Credential definition JSONs.</param>
        /// <exception cref="SharedRsException">Throws when any parameters are invalid.</exception>
        /// <returns>New <see cref="Presentation"/> JSON.</returns>
        public static async Task<string> CreatePresentationAsync(
            string presentationRequestJson,
            List<string> credentialEntryJsons,
            List<string> credentialProofJsons,
            List<string> selfAttestNames,
            List<string> selfAttestValues,
            string masterSecretJson,
            List<string> schemaJsons,
            List<string> credDefJsons)
        {
            IntPtr presentationObjectHandle = new IntPtr();
            List<CredentialEntry> credentialEntries = new List<CredentialEntry>();
            List<CredentialProof> credentialProofs = new List<CredentialProof>();
            IntPtr presentationRequestHandle = new IntPtr();
            IntPtr masterSecretHandle = new IntPtr();
            List<IntPtr> schemaHandles = new List<IntPtr>();
            List<IntPtr> credDefHandles = new List<IntPtr>();

            foreach (string credentialEntryJson in credentialEntryJsons)
            {
                credentialEntries.Add(JsonConvert.DeserializeObject<CredentialEntry>(credentialEntryJson));
            }
            foreach (string credentialProofJson in credentialProofJsons)
            {
                credentialProofs.Add(JsonConvert.DeserializeObject<CredentialProof>(credentialProofJson));
            }
            _ = NativeMethods.credx_presentation_request_from_json(ByteBuffer.Create(presentationRequestJson), ref presentationRequestHandle);
            _ = NativeMethods.credx_master_secret_from_json(ByteBuffer.Create(masterSecretJson), ref masterSecretHandle);
            foreach(string schemaJson in schemaJsons)
            {
                IntPtr newSchemaHandle = new IntPtr();
                _ = NativeMethods.credx_schema_from_json(ByteBuffer.Create(schemaJson), ref newSchemaHandle);
                schemaHandles.Add(newSchemaHandle);
            }
            foreach (string credDefJson in credDefJsons)
            {
                IntPtr newCredDefHandle = new IntPtr();
                _ = NativeMethods.credx_credential_definition_from_json(ByteBuffer.Create(credDefJson), ref newCredDefHandle);
                credDefHandles.Add(newCredDefHandle);
            }
            
            int errorCode = NativeMethods.credx_create_presentation(
                presentationRequestHandle,
                FfiCredentialEntryList.Create(credentialEntries),
                FfiCredentialProveList.Create(credentialProofs),
                FfiStrList.Create(selfAttestNames),
                FfiStrList.Create(selfAttestValues),
                masterSecretHandle,
                FfiUIntList.Create(schemaHandles),
                FfiUIntList.Create(credDefHandles),
                ref presentationObjectHandle);

            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }

            string presentationJson = await ObjectApi.ToJsonAsync(presentationObjectHandle);
            return await Task.FromResult(presentationJson);
        }

        /// <summary>
        /// Creates a <see cref="Presentation"/> object from json <see cref="string"/>.
        /// </summary>
        /// <param name="presentationJson">Json string of presentation object.</param>
        /// <exception cref="IndexOutOfRangeException">Throws when <paramref name="presentationJson"/> is empty.</exception>
        /// <exception cref="SharedRsException">Throws if <paramref name="presentationJson"/> is an invalid json object.</exception>
        /// <returns>New <see cref="Presentation"/> object.</returns>
        public static async Task<Presentation> CreatePresentationFromJsonAsync(string presentationJson)
        {
            IntPtr presentationObjectHandle = new IntPtr();
            int errorCode = NativeMethods.credx_presentation_from_json(ByteBuffer.Create(presentationJson), ref presentationObjectHandle);
            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }
            Presentation presentationObject = await CreatePresentationObject(presentationObjectHandle);
            return await Task.FromResult(presentationObject);
        }

        /// <summary>
        /// Verifies that a presentation matches its request.
        /// </summary>
        /// <param name="presentation">Presentation to verify.</param>
        /// <param name="presentationRequest">Request to verify the <paramref name="presentation"/> object with.</param>
        /// <param name="schemas">Corresponding schemas.</param>
        /// <param name="credentialDefinitions"></param>
        /// <param name="revocationRegistryDefinitions"></param>
        /// <param name="revocationRegistryEntries"></param>
        /// <exception cref="SharedRsException">Throws if any parameter is invalid.</exception>
        /// <returns>True if provided <see cref="Presentation"/> can be verified, false if not.</returns>
        public static async Task<bool> VerifyPresentationAsync(
            Presentation presentation,
            PresentationRequest presentationRequest,
            List<Schema> schemas,
            List<CredentialDefinition> credentialDefinitions,
            List<RevocationRegistryDefinition> revocationRegistryDefinitions,
            List<RevocationRegistryEntry> revocationRegistryEntries)
        {
            byte verify = 0;
            List<IntPtr> schemaHandles =
                (from schema in schemas select schema.Handle).ToList();
            List<IntPtr> credDefHandles =
                (from credDef in credentialDefinitions select credDef.Handle).ToList();
            List<IntPtr> revRegDefHandles =
                (from revRegDef in revocationRegistryDefinitions select revRegDef.Handle).ToList();

            int errorCode = NativeMethods.credx_verify_presentation(
                presentation.Handle,
                presentationRequest.Handle,
                FfiUIntList.Create(schemaHandles),
                FfiUIntList.Create(credDefHandles),
                FfiUIntList.Create(revRegDefHandles),
                FfiRevocationEntryList.Create(revocationRegistryEntries),
                ref verify);

            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }

            return await Task.FromResult(Convert.ToBoolean(verify));
        }

        /// <summary>
        /// Verifies that a presentation matches its request.
        /// </summary>
        /// <param name="presentationJson">Presentation to verify.</param>
        /// <param name="presentationRequestJson">Request to verify the <paramref name="presentationJson"/> object with.</param>
        /// <param name="schemaJsons">Corresponding schemas.</param>
        /// <param name="credentialDefinitionJsons"></param>
        /// <param name="revocationRegistryDefinitionJsons"></param>
        /// <param name="revocationRegistryEntryJsons"></param>
        /// <exception cref="SharedRsException">Throws if any parameter is invalid.</exception>
        /// <returns>True if provided <see cref="Presentation"/> can be verified, false if not.</returns>
        public static async Task<bool> VerifyPresentationAsync(
            string presentationJson,
            string presentationRequestJson,
            List<string> schemaJsons,
            List<string> credentialDefinitionJsons,
            List<string> revocationRegistryDefinitionJsons,
            List<string> revocationRegistryEntryJsons)
        {
            byte verify = 0;

            IntPtr presentationHandle = new IntPtr();
            IntPtr presentationRequestHandle = new IntPtr();
            List<IntPtr> schemaHandles = new List<IntPtr>();
            List<IntPtr> credDefHandles = new List<IntPtr>();
            List<IntPtr> revRegDefHandles = new List<IntPtr>();
            List<RevocationRegistryEntry> revocationRegistryEntries = new List<RevocationRegistryEntry>();

            _ = NativeMethods.credx_presentation_from_json(ByteBuffer.Create(presentationJson), ref presentationHandle);
            _ = NativeMethods.credx_presentation_request_from_json(ByteBuffer.Create(presentationRequestJson), ref presentationRequestHandle);
            foreach (string schemaJson in schemaJsons)
            {
                IntPtr newSchemaHandle = new IntPtr();
                _ = NativeMethods.credx_schema_from_json(ByteBuffer.Create(schemaJson), ref newSchemaHandle);
                schemaHandles.Add(newSchemaHandle);
            }
            foreach (string credentialDefinitionJson in credentialDefinitionJsons)
            {
                IntPtr newCredentialDefinitionHandle = new IntPtr();
                _ = NativeMethods.credx_credential_definition_from_json(ByteBuffer.Create(credentialDefinitionJson), ref newCredentialDefinitionHandle);
                credDefHandles.Add(newCredentialDefinitionHandle);
            }
            foreach (string revocationRegistryDefinitionJson in revocationRegistryDefinitionJsons)
            {
                IntPtr newRevocationRegistryDefinitionHandle = new IntPtr();
                _ = NativeMethods.credx_revocation_registry_definition_from_json(ByteBuffer.Create(revocationRegistryDefinitionJson), ref newRevocationRegistryDefinitionHandle);
                revRegDefHandles.Add(newRevocationRegistryDefinitionHandle);
            }
            foreach (string revocationRegistryEntryJson in revocationRegistryEntryJsons)
            {
                revocationRegistryEntries.Add(JsonConvert.DeserializeObject<RevocationRegistryEntry>(revocationRegistryEntryJson));
            }

            int errorCode = NativeMethods.credx_verify_presentation(
                presentationHandle,
                presentationRequestHandle,
                FfiUIntList.Create(schemaHandles),
                FfiUIntList.Create(credDefHandles),
                FfiUIntList.Create(revRegDefHandles),
                FfiRevocationEntryList.Create(revocationRegistryEntries),
                ref verify);

            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }

            return await Task.FromResult(Convert.ToBoolean(verify));
        }

        private static async Task<Presentation> CreatePresentationObject(IntPtr objectHandle)
        {
            string presentationJson = await ObjectApi.ToJsonAsync(objectHandle);
            Presentation presentationObject = JsonConvert.DeserializeObject<Presentation>(presentationJson, Settings.JsonSettings);

            presentationObject.JsonString = presentationJson;
            presentationObject.Handle = objectHandle;
            return await Task.FromResult(presentationObject);
        }
    }
}