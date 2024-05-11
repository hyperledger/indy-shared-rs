using FluentAssertions;
using indy_shared_rs_dotnet.IndyCredx;
using indy_shared_rs_dotnet.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace indy_shared_rs_dotnet_test.IndyCredx
{
    public class CredentialRequestApiTests
    {
        #region Tests for CreateCredentialRequestAsync
        [Test, TestCase(TestName = "CreateCredentialRequestAsync() with all Arguments set returns a request and metadata.")]
        public async Task CreateCredentialRequestAsyncWorks()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string proverDid = "VsKV7grR1BUE29mG2Fm2kX";
            string schemaName = "gvt";
            string schemaVersion = "1.0";

            MasterSecret masterSecretObject = await MasterSecretApi.CreateMasterSecretAsync();
            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            (CredentialDefinition credDefObject, _, CredentialKeyCorrectnessProof keyProofObject) =
                await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);
            string schemaId = await CredentialDefinitionApi.GetCredentialDefinitionAttributeAsync(credDefObject, "schema_id");
            CredentialOffer credOfferObject = await CredentialOfferApi.CreateCredentialOfferAsync(schemaId, credDefObject, keyProofObject);

            //Act
            (CredentialRequest request, CredentialRequestMetadata metaData) = await CredentialRequestApi.CreateCredentialRequestAsync(proverDid, credDefObject, masterSecretObject, "testMasterSecretName", credOfferObject);

            //Assert
            request.Should().NotBeNull();
            request.Should().BeOfType(typeof(CredentialRequest));
            metaData.Should().NotBeNull();
            metaData.Should().BeOfType(typeof(CredentialRequestMetadata));
        }

        [Test, TestCase(TestName = "CreateCredentialRequestAsync() with all Arguments set returns a request and metadata.")]
        public async Task CreateCredentialRequestJsonAsyncWorks()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string proverDid = "VsKV7grR1BUE29mG2Fm2kX";
            string schemaName = "gvt";
            string schemaVersion = "1.0";

            string masterSecretObject = await MasterSecretApi.CreateMasterSecretJsonAsync();
            string schemaObject = await SchemaApi.CreateSchemaJsonAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            (string credDefObject, string _, string keyProofObject) =
                await CredentialDefinitionApi.CreateCredentialDefinitionJsonAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);
            string schemaId = await CredentialDefinitionApi.GetCredentialDefinitionAttributeAsync(credDefObject, "schema_id");
            string credOfferObject = await CredentialOfferApi.CreateCredentialOfferJsonAsync(schemaId, credDefObject, keyProofObject);

            //Act
            (string request, string metaData) = await CredentialRequestApi.CreateCredentialRequestJsonAsync(proverDid, credDefObject, masterSecretObject, "testMasterSecretName", credOfferObject);

            //Assert
            request.Should().NotBeNullOrEmpty();
            metaData.Should().NotBeNullOrEmpty();
        }

        private static IEnumerable<TestCaseData> CreateCredentialRequestCases()
        {
            yield return new TestCaseData(false, false, false, false, false)
                .SetName("CreateCredentialRequestAsync() throws Exception if all arguments are null.");
            yield return new TestCaseData(false, true, true, true, true)
                .SetName("CreateCredentialRequestAsync() throws Exception if proverDid is null.");
            yield return new TestCaseData(true, false, true, true, true)
                .SetName("CreateCredentialRequestAsync() throws Exception if credential defintion is null.");
            yield return new TestCaseData(true, true, false, true, true)
                .SetName("CreateCredentialRequestAsync() throws Exception if master secret is null.");
            yield return new TestCaseData(true, true, true, false, true)
                .SetName("CreateCredentialRequestAsync() throws Exception if master secret id is null.");
            yield return new TestCaseData(true, true, true, true, false)
                .SetName("CreateCredentialRequestAsync() throws Exception if credential offer is null.");
        }

        [Test, TestCaseSource(nameof(CreateCredentialRequestCases))]
        public async Task CreateCredentialRequestThrowsException(bool hasProverDid, bool hasCredDef, bool hasMasterSecret, bool hasMasterSecretId, bool hasCredentialOffer)
        {
            //Arrange
            string proverDid = null;
            CredentialDefinition credDefObject = null;
            MasterSecret masterSecretObject = null;
            string masterSecretId = null;
            CredentialOffer credOfferObject = null;

            if (hasProverDid)
            {
                proverDid = "VsKV7grR1BUE29mG2Fm2kX";
            }

            if (hasCredDef)
            {
                List<string> attrNames = new() { "gender", "age", "sex" };
                string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
                proverDid = "VsKV7grR1BUE29mG2Fm2kX";
                string schemaName = "gvt";
                string schemaVersion = "1.0";
                Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
                (CredentialDefinition tmpCredDef, _, _) =
                await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);
                credDefObject = tmpCredDef;
            }

            if (hasMasterSecret)
            {
                masterSecretObject = await MasterSecretApi.CreateMasterSecretAsync();
            }

            if (hasMasterSecretId)
            {
                masterSecretId = "testMasterSecretName";
            }

            if (hasCredentialOffer)
            {
                string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
                List<string> attrNames = new() { "gender", "age", "sex" };
                string schemaName = "gvt";
                string schemaVersion = "1.0";
                Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
                (CredentialDefinition tmpCredDef, _, CredentialKeyCorrectnessProof keyProofObject) =
                   await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);
                string schemaId = await CredentialDefinitionApi.GetCredentialDefinitionAttributeAsync(tmpCredDef, "schema_id");
                credOfferObject = await CredentialOfferApi.CreateCredentialOfferAsync(schemaId, tmpCredDef, keyProofObject);
            }

            //Act
            Func<Task> act = async () => await CredentialRequestApi.CreateCredentialRequestAsync(proverDid, credDefObject, masterSecretObject, masterSecretId, credOfferObject);

            //Assert
            await act.Should().ThrowAsync<Exception>();
        }
        #endregion
    }
}