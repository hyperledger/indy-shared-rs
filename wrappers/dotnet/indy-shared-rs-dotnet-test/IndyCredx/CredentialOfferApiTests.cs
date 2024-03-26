using FluentAssertions;
using indy_shared_rs_dotnet.IndyCredx;
using indy_shared_rs_dotnet.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace indy_shared_rs_dotnet_test.IndyCredx
{
    public class CredentialOfferApiTests
    {
        #region Tests for CreateCredentialOfferAsync
        [Test, TestCase(TestName = "CreateCredentialOfferAsync() returns CredentialOffer object.")]
        public async Task CreateCredentialOfferWorks()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            (CredentialDefinition credDef, _, CredentialKeyCorrectnessProof keyProof) =
                await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            //Act
            string schemaId = await CredentialDefinitionApi.GetCredentialDefinitionAttributeAsync(credDef, "schema_id");
            CredentialOffer testObject = await CredentialOfferApi.CreateCredentialOfferAsync(schemaId, credDef, keyProof);

            //Assert
            testObject.Should().BeOfType(typeof(CredentialOffer));
        }

        [Test, TestCase(TestName = "CreateCredentialOfferAsync() with json input returns CredentialOffer object.")]
        public async Task CreateCredentialOfferJsonWorks()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";

            string schemaObjectJson = await SchemaApi.CreateSchemaJsonAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            (string credDef, _, string keyProof) =
                await CredentialDefinitionApi.CreateCredentialDefinitionJsonAsync(issuerDid, schemaObjectJson, "tag", SignatureType.CL, true);

            //Act
            string schemaId = await CredentialDefinitionApi.GetCredentialDefinitionAttributeAsync(credDef, "schema_id");
            string testObject = await CredentialOfferApi.CreateCredentialOfferJsonAsync(schemaId, credDef, keyProof);

            //Assert
            testObject.Should().NotBeNullOrEmpty();
        }

        private static IEnumerable<TestCaseData> CreateCredentialOfferCases()
        {
            yield return new TestCaseData(false, false, false)
                .SetName("CreateCredentialOfferAsync() throws Exception if all arguments are null.");
            yield return new TestCaseData(false, true, true)
                .SetName("CreateCredentialOfferAsync() throws SharedRsException if SchemaId is null.");
            yield return new TestCaseData(true, false, true)
                .SetName("CreateCredentialOfferAsync() throws SharedRsException if CredentialDefinition is null.");
            yield return new TestCaseData(true, true, false)
                .SetName("CreateCredentialOfferAsync() throws SharedRsException if CredentialKeyCorrectnessProof is null.");
        }

        [Test, TestCaseSource(nameof(CreateCredentialOfferCases))]
        public async Task CreateCredentialOfferThrowsException(bool hasSchemaId, bool hasCredDef, bool hasKeyProof)
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            string schemaId = null;
            CredentialDefinition credDef = null;
            CredentialKeyCorrectnessProof keyProof = null;

            if (hasSchemaId)
            {
                (CredentialDefinition tmpCredDef, CredentialDefinitionPrivate tmpCredDefPrivate, CredentialKeyCorrectnessProof tmpKeyProof) = await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);
                schemaId = await CredentialDefinitionApi.GetCredentialDefinitionAttributeAsync(tmpCredDef, "schema_id");
            }
            if (hasCredDef)
            {
                (CredentialDefinition tmpCredDef, CredentialDefinitionPrivate tmpCredDefPrivate, CredentialKeyCorrectnessProof tmpKeyProof) = await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);
                credDef = tmpCredDef;
            }
            if (hasKeyProof)
            {
                (CredentialDefinition tmpCredDef, CredentialDefinitionPrivate tmpCredDefPrivate, CredentialKeyCorrectnessProof tmpKeyProof) = await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);
                keyProof = tmpKeyProof;
            }

            //Act
            Func<Task> act = async () => await CredentialOfferApi.CreateCredentialOfferAsync(schemaId, credDef, keyProof);

            //Assert
            await act.Should().ThrowAsync<Exception>();
        }
        #endregion
    }
}