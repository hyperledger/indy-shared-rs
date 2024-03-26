using FluentAssertions;
using indy_shared_rs_dotnet.IndyCredx;
using indy_shared_rs_dotnet.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace indy_shared_rs_dotnet_test.IndyCredx
{
    public class CredentialApiTests
    {
        #region Tests for CreateCredentialAsync
        [Test, TestCase(TestName = "CreateCredentialAsync() creates a credential and revocation registry object.")]
        public async Task CreateCredentialAsyncWorks()
        {
            //Arrange
            List<string> attrNames = new() { "name", "age", "sex" };
            List<string> attrNamesRaw = new() { "Alex", "20", "male" };
            List<string> attrNamesEnc = await CredentialApi.EncodeCredentialAttributesAsync(attrNamesRaw);
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string proverDid = "VsKV7grR1BUE29mG2Fm2kX";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPathForRevocation = null;

            MasterSecret masterSecretObject = await MasterSecretApi.CreateMasterSecretAsync();

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            (CredentialDefinition credDefObject, CredentialDefinitionPrivate credDefPvtObject, CredentialKeyCorrectnessProof keyProofObject) =
                await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            string schemaId = await CredentialDefinitionApi.GetCredentialDefinitionAttributeAsync(credDefObject, "schema_id");
            CredentialOffer credOfferObject = await CredentialOfferApi.CreateCredentialOfferAsync(schemaId, credDefObject, keyProofObject);

            (CredentialRequest credRequestObject, CredentialRequestMetadata metaDataObject) =
                await CredentialRequestApi.CreateCredentialRequestAsync(proverDid, credDefObject, masterSecretObject, "testMasterSecretName", credOfferObject);

            (RevocationRegistryDefinition revRegDefObject, RevocationRegistryDefinitionPrivate revRegDefPvtObject, RevocationRegistry revRegObject, RevocationRegistryDelta revRegDeltaObject) =
                await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDefObject, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPathForRevocation);


            //Act
            (Credential credObject, RevocationRegistry revRegObjectNew, RevocationRegistryDelta revDeltaObject) =
                await CredentialApi.CreateCredentialAsync(credDefObject, credDefPvtObject, credOfferObject, credRequestObject,
                attrNames, attrNamesRaw, attrNamesEnc, revRegDefObject, revRegDefPvtObject, revRegObject, 1, new List<long> { 1 });

            //Assert
            credObject.Should().BeOfType(typeof(Credential));
            revRegObjectNew.Should().BeOfType(typeof(RevocationRegistry));
            //revDeltaObject.Should().BeOfType(typeof(RevocationRegistryDelta));
        }

        [Test, TestCase(TestName = "CreateCredentialAsync() with JSON input creates a credential and revocation registry object.")]
        public async Task CreateCredentialAsyncJsonWorks()
        {
            //Arrange
            List<string> attrNames = new() { "name", "age", "sex" };
            List<string> attrNamesRaw = new() { "Alex", "20", "male" };
            List<string> attrNamesEnc = await CredentialApi.EncodeCredentialAttributesAsync(attrNamesRaw);
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string proverDid = "VsKV7grR1BUE29mG2Fm2kX";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPathForRevocation = null;

            string masterSecretObjectJson = await MasterSecretApi.CreateMasterSecretJsonAsync();

            string schemaObjectJson = await SchemaApi.CreateSchemaJsonAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            (string credDefObjectJson, string credDefPvtObjectJson, string keyProofObjectJson) =
                await CredentialDefinitionApi.CreateCredentialDefinitionJsonAsync(issuerDid, schemaObjectJson, "tag", SignatureType.CL, true);

            string schemaId = await CredentialDefinitionApi.GetCredentialDefinitionAttributeAsync(credDefObjectJson, "schema_id");
            string credOfferObjectJson = await CredentialOfferApi.CreateCredentialOfferJsonAsync(schemaId, credDefObjectJson, keyProofObjectJson);

            (string credRequestObjectJson, string metaDataObjectJson) =
                await CredentialRequestApi.CreateCredentialRequestJsonAsync(proverDid, credDefObjectJson, masterSecretObjectJson, "testMasterSecretName", credOfferObjectJson);

            (string revRegDefObjectJson, string revRegDefPvtObjectJson, string revRegObjectJson, string revRegDeltaObjectJson) =
                await RevocationApi.CreateRevocationRegistryJsonAsync(issuerDid, credDefObjectJson, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPathForRevocation);

            //Act
            (string credObjectJson, string revRegObjectNewJson, string revDeltaObjectJson) =
                await CredentialApi.CreateCredentialAsync(credDefObjectJson, credDefPvtObjectJson, credOfferObjectJson, credRequestObjectJson,
                attrNames, attrNamesRaw, attrNamesEnc, revRegDefObjectJson, revRegDefPvtObjectJson, revRegObjectJson, 1, new List<long> { 1 });

            //Assert
            credObjectJson.Should().NotBeNullOrEmpty();
            revRegObjectNewJson.Should().NotBeNullOrEmpty();
            //revDeltaObject.Should().BeOfType(typeof(RevocationRegistryDelta));
        }

        [Test, TestCase(TestName = "CreateCredentialAsync() throws SharedRsException when attribute names do not match their values.")]
        public async Task CreateCredentialAsyncThrowsException()
        {
            //Arrange
            List<string> attrNames = new() { "name", "age", "sex" };
            List<string> attrNamesRaw = new() { "Alex"};
            List<string> attrNamesEnc = await CredentialApi.EncodeCredentialAttributesAsync(attrNamesRaw);
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string proverDid = "VsKV7grR1BUE29mG2Fm2kX";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPathForRevocation = null;

            MasterSecret masterSecretObject = await MasterSecretApi.CreateMasterSecretAsync();

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            (CredentialDefinition credDefObject, CredentialDefinitionPrivate credDefPvtObject, CredentialKeyCorrectnessProof keyProofObject) =
                await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            string schemaId = await CredentialDefinitionApi.GetCredentialDefinitionAttributeAsync(credDefObject, "schema_id");
            CredentialOffer credOfferObject = await CredentialOfferApi.CreateCredentialOfferAsync(schemaId, credDefObject, keyProofObject);

            (CredentialRequest credRequestObject, CredentialRequestMetadata metaDataObject) =
                await CredentialRequestApi.CreateCredentialRequestAsync(proverDid, credDefObject, masterSecretObject, "testMasterSecretName", credOfferObject);

            (RevocationRegistryDefinition revRegDefObject, RevocationRegistryDefinitionPrivate revRegDefPvtObject, RevocationRegistry revRegObject, RevocationRegistryDelta revRegDeltaObject) =
                await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDefObject, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPathForRevocation);

            //Act
            Func<Task> act = async () => await CredentialApi.CreateCredentialAsync(credDefObject, credDefPvtObject, credOfferObject, credRequestObject,
                attrNames, attrNamesRaw, attrNamesEnc, revRegDefObject, revRegDefPvtObject, revRegObject, 1, new List<long>() { 1 });

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }
        #endregion

        #region Tests for EncodeCredentialAttributesAsync
        [Test, TestCase(TestName = "EncodeCredentialAttributesAsync() with all arguments set returns encoded attribute.")]
        public async Task EncodeCredentialAttributesAsyncWorks()
        {
            //Arrange
            List<string> rawAttributes = new List<string>() { "test", "test2", "test3" };

            //Act
            List<string> result = await CredentialApi.EncodeCredentialAttributesAsync(rawAttributes);

            //Assert
            result.Should().NotBeNull();
        }

        [Test, TestCase(TestName = "EncodeCredentialAttributesAsync() throws InvalidOperationException when given an empty list.")]
        public async Task EncodeCredentialAttributesAsyncThrowsException()
        {
            //Arrange
            List<string> rawAttributes = new List<string>();

            //Act
            Func<Task> act = async () => await CredentialApi.EncodeCredentialAttributesAsync(rawAttributes);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
        #endregion

        #region Tests for ProcessCredentialAsync
        [Test, TestCase(TestName = "ProcessCredentialAsync() creates a credential object.")]
        public async Task ProcessCredentialAsync()
        {
            //Arrange
            List<string> attrNames = new() { "name", "age", "sex" };
            List<string> attrNamesRaw = new() { "Alex", "20", "male" };
            List<string> attrNamesEnc = await CredentialApi.EncodeCredentialAttributesAsync(attrNamesRaw);
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string proverDid = "VsKV7grR1BUE29mG2Fm2kX";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPathForRevocation = null;

            MasterSecret masterSecretObject = await MasterSecretApi.CreateMasterSecretAsync();

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            (CredentialDefinition credDefObject, CredentialDefinitionPrivate credDefPvtObject, CredentialKeyCorrectnessProof keyProofObject) =
                await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            string schemaId = await CredentialDefinitionApi.GetCredentialDefinitionAttributeAsync(credDefObject, "schema_id");
            CredentialOffer credOfferObject = await CredentialOfferApi.CreateCredentialOfferAsync(schemaId, credDefObject, keyProofObject);

            (CredentialRequest credRequestObject, CredentialRequestMetadata metaDataObject) =
                await CredentialRequestApi.CreateCredentialRequestAsync(proverDid, credDefObject, masterSecretObject, "testMasterSecretName", credOfferObject);

            (RevocationRegistryDefinition revRegDefObject, RevocationRegistryDefinitionPrivate revRegDefPvtObject, RevocationRegistry revRegObject, _) =
                await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDefObject, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPathForRevocation);


            (Credential credObject, _, _) =
                await CredentialApi.CreateCredentialAsync(credDefObject, credDefPvtObject, credOfferObject, credRequestObject,
                attrNames, attrNamesRaw, attrNamesEnc, revRegDefObject, revRegDefPvtObject, revRegObject, 1, new List<long>() { 1 });

            //Act
            Credential credObjectProcessed =
                await CredentialApi.ProcessCredentialAsync(credObject, metaDataObject, masterSecretObject, credDefObject, revRegDefObject);

            //Assert
            credObjectProcessed.Should().BeOfType(typeof(Credential));
        }

        [Test, TestCase(TestName = "ProcessCredentialAsync() with JSON input creates a credential object.")]
        public async Task ProcessCredentialAsync2()
        {
            //Arrange
            List<string> attrNames = new() { "name", "age", "sex" };
            List<string> attrNamesRaw = new() { "Alex", "20", "male" };
            List<string> attrNamesEnc = await CredentialApi.EncodeCredentialAttributesAsync(attrNamesRaw);
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string proverDid = "VsKV7grR1BUE29mG2Fm2kX";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPathForRevocation = null;

            string masterSecretObjectJson = await MasterSecretApi.CreateMasterSecretJsonAsync();

            string schemaObjectJson = await SchemaApi.CreateSchemaJsonAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            (string credDefObjectJson, string credDefPvtObjectJson, string keyProofObjectJson) =
                await CredentialDefinitionApi.CreateCredentialDefinitionJsonAsync(issuerDid, schemaObjectJson, "tag", SignatureType.CL, true);

            string schemaId = await CredentialDefinitionApi.GetCredentialDefinitionAttributeAsync(credDefObjectJson, "schema_id");
            string credOfferObjectJson = await CredentialOfferApi.CreateCredentialOfferJsonAsync(schemaId, credDefObjectJson, keyProofObjectJson);

            (string credRequestObjectJson, string metaDataObjectJson) =
                await CredentialRequestApi.CreateCredentialRequestJsonAsync(proverDid, credDefObjectJson, masterSecretObjectJson, "testMasterSecretName", credOfferObjectJson);

            (string revRegDefObjectJson, string revRegDefPvtObjectJson, string revRegObjectJson, _) =
                await RevocationApi.CreateRevocationRegistryJsonAsync(issuerDid, credDefObjectJson, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPathForRevocation);

            (string credObjectJson, _, _) =
                await CredentialApi.CreateCredentialAsync(credDefObjectJson, credDefPvtObjectJson, credOfferObjectJson, credRequestObjectJson,
                attrNames, attrNamesRaw, attrNamesEnc, revRegDefObjectJson, revRegDefPvtObjectJson, revRegObjectJson, 1, new List<long>() { 1 });

            //Act
            string credObjectProcessedJson =
                await CredentialApi.ProcessCredentialAsync(credObjectJson, metaDataObjectJson, masterSecretObjectJson, credDefObjectJson, revRegDefObjectJson);

            //Assert
            credObjectProcessedJson.Should().NotBeNullOrEmpty();
        }

        [Test, TestCase(TestName = "ProcessCredentialAsync() throws SharedRsException when maste secret does not match credential.")]
        public async Task ProcessCredentialAsyncThrowsException()
        {
            //Arrange
            List<string> attrNames = new() { "name", "age", "sex" };
            List<string> attrNamesRaw = new() { "Alex", "20", "male" };
            List<string> attrNamesEnc = await CredentialApi.EncodeCredentialAttributesAsync(attrNamesRaw);
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string proverDid = "VsKV7grR1BUE29mG2Fm2kX";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPathForRevocation = null;

            MasterSecret masterSecretObject = await MasterSecretApi.CreateMasterSecretAsync();
            MasterSecret masterSecretObject2 = await MasterSecretApi.CreateMasterSecretAsync();

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            (CredentialDefinition credDefObject, CredentialDefinitionPrivate credDefPvtObject, CredentialKeyCorrectnessProof keyProofObject) =
                await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            string schemaId = await CredentialDefinitionApi.GetCredentialDefinitionAttributeAsync(credDefObject, "schema_id");
            CredentialOffer credOfferObject = await CredentialOfferApi.CreateCredentialOfferAsync(schemaId, credDefObject, keyProofObject);

            (CredentialRequest credRequestObject, CredentialRequestMetadata metaDataObject) =
                await CredentialRequestApi.CreateCredentialRequestAsync(proverDid, credDefObject, masterSecretObject, "testMasterSecretName", credOfferObject);

            (RevocationRegistryDefinition revRegDefObject, RevocationRegistryDefinitionPrivate revRegDefPvtObject, RevocationRegistry revRegObject, _) =
                await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDefObject, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPathForRevocation);

            (Credential credObject, RevocationRegistry _,RevocationRegistryDelta _) =
                await CredentialApi.CreateCredentialAsync(credDefObject, credDefPvtObject, credOfferObject, credRequestObject,
                attrNames, attrNamesRaw, attrNamesEnc, revRegDefObject, revRegDefPvtObject, revRegObject, 1, new List<long>() { 1 });

            //Act
            Func<Task> act = async () => await CredentialApi.ProcessCredentialAsync(credObject, metaDataObject, masterSecretObject2, credDefObject, revRegDefObject);

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }
        #endregion

        #region Tests for GetCredentialAttributeAsync
        [Test, TestCase(TestName = "GetCredentialAttributeAsync() works for attribute names: schema_id, cred_def_id, rev_reg_id.")]
        public async Task GetCredentialAttributeAsync()
        {
            //Arrange
            List<string> attrNames = new() { "name", "age", "sex" };
            List<string> attrNamesRaw = new() { "Alex", "20", "male" };
            List<string> attrNamesEnc = await CredentialApi.EncodeCredentialAttributesAsync(attrNamesRaw);
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string proverDid = "VsKV7grR1BUE29mG2Fm2kX";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPathForRevocation = null;
            MasterSecret masterSecretObject = await MasterSecretApi.CreateMasterSecretAsync();

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            (CredentialDefinition credDefObject, CredentialDefinitionPrivate credDefPvtObject, CredentialKeyCorrectnessProof keyProofObject) =
                await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            string schemaId = await CredentialDefinitionApi.GetCredentialDefinitionAttributeAsync(credDefObject, "schema_id");
            CredentialOffer credOfferObject = await CredentialOfferApi.CreateCredentialOfferAsync(schemaId, credDefObject, keyProofObject);

            (CredentialRequest credRequestObject, CredentialRequestMetadata metaDataObject) =
                await CredentialRequestApi.CreateCredentialRequestAsync(proverDid, credDefObject, masterSecretObject, "testMasterSecretName", credOfferObject);

            (RevocationRegistryDefinition revRegDefObject, RevocationRegistryDefinitionPrivate revRegDefPvtObject, RevocationRegistry revRegObject, RevocationRegistryDelta revRegDeltaObject) =
                await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDefObject, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPathForRevocation);

            (Credential credObject, RevocationRegistry revRegObjectNew, RevocationRegistryDelta revDeltaObject) =
                await CredentialApi.CreateCredentialAsync(credDefObject, credDefPvtObject, credOfferObject, credRequestObject,
                attrNames, attrNamesRaw, attrNamesEnc, revRegDefObject, revRegDefPvtObject, revRegObject, 1, new List<long>() { 1 });

            //Act
            //note: only attribute "schema_id", "cred_def_id", "rev_reg_id", "rev_reg_index" supported so far.
            string attrSchemaId = await CredentialApi.GetCredentialAttributeAsync(credObject, "schema_id");
            string attrCredDefId = await CredentialApi.GetCredentialAttributeAsync(credObject, "cred_def_id");
            string attrRevRegId = await CredentialApi.GetCredentialAttributeAsync(credObject, "rev_reg_id");
            string attrRevRegIndex = await CredentialApi.GetCredentialAttributeAsync(credObject, "rev_reg_index");
            //string attrDefault = await CredentialApi.GetCredentialAttributeAsync(credObject, "default");

            //Assert
            attrSchemaId.Should().Be(credObject.SchemaId);
            attrCredDefId.Should().Be(credObject.CredentialDefinitionId);
            attrRevRegId.Should().Be(credObject.RevocationRegistryId);
            attrRevRegIndex.Should().Be(credObject.Signature.RCredential.I.ToString());
            //attrDefault.Should().Be("");
        }

        [Test, TestCase(TestName = "GetCredentialAttributeAsync() with JSON inputs works for attribute names: schema_id, cred_def_id, rev_reg_id.")]
        public async Task GetCredentialAttributeJsonAsync()
        {
            //Arrange
            List<string> attrNames = new() { "name", "age", "sex" };
            List<string> attrNamesRaw = new() { "Alex", "20", "male" };
            List<string> attrNamesEnc = await CredentialApi.EncodeCredentialAttributesAsync(attrNamesRaw);
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string proverDid = "VsKV7grR1BUE29mG2Fm2kX";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPathForRevocation = null;
            string masterSecretObject = await MasterSecretApi.CreateMasterSecretJsonAsync();

            string schemaObject = await SchemaApi.CreateSchemaJsonAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            (string credDefObject, string credDefPvtObject, string keyProofObject) =
                await CredentialDefinitionApi.CreateCredentialDefinitionJsonAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            string schemaId = await CredentialDefinitionApi.GetCredentialDefinitionAttributeAsync(credDefObject, "schema_id");
            string credOfferObject = await CredentialOfferApi.CreateCredentialOfferJsonAsync(schemaId, credDefObject, keyProofObject);

            (string credRequestObject, string metaDataObject) =
                await CredentialRequestApi.CreateCredentialRequestJsonAsync(proverDid, credDefObject, masterSecretObject, "testMasterSecretName", credOfferObject);

            (string revRegDefObject, string revRegDefPvtObject, string revRegObject, string revRegDeltaObject) =
                await RevocationApi.CreateRevocationRegistryJsonAsync(issuerDid, credDefObject, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPathForRevocation);

            (string credObject, string revRegObjectNew, string revDeltaObject) =
                await CredentialApi.CreateCredentialAsync(credDefObject, credDefPvtObject, credOfferObject, credRequestObject,
                attrNames, attrNamesRaw, attrNamesEnc, revRegDefObject, revRegDefPvtObject, revRegObject, 1, new List<long>() { 1 });

            //Act
            //note: only attribute "schema_id", "cred_def_id", "rev_reg_id", "rev_reg_index" supported so far.
            string attrSchemaId = await CredentialApi.GetCredentialAttributeAsync(credObject, "schema_id");
            string attrCredDefId = await CredentialApi.GetCredentialAttributeAsync(credObject, "cred_def_id");
            string attrRevRegId = await CredentialApi.GetCredentialAttributeAsync(credObject, "rev_reg_id");
            string attrRevRegIndex = await CredentialApi.GetCredentialAttributeAsync(credObject, "rev_reg_index");

            //Assert
            attrSchemaId.Should().Be(JObject.Parse(credObject)["schema_id"].ToString());
            attrCredDefId.Should().Be(JObject.Parse(credObject)["cred_def_id"].ToString());
            attrRevRegId.Should().Be(JObject.Parse(credObject)["rev_reg_id"].ToString());
            attrRevRegIndex.Should().Be(JObject.Parse(JObject.Parse(JObject.Parse(credObject)["signature"].ToString())["r_credential"].ToString())["i"].ToString());

        }

        [Test, TestCase(TestName = "GetCredentialAttributeAsync() throws SharedRsException when given empty attribute name.")]
        public async Task GetCredentialAttributeAsyncThrowsException()
        {
            //Arrange
            string attributeName = "";
            List<string> attrNames = new() { "name", "age", "sex" };
            List<string> attrNamesRaw = new() { "Alex", "20", "male" };
            List<string> attrNamesEnc = await CredentialApi.EncodeCredentialAttributesAsync(attrNamesRaw);
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string proverDid = "VsKV7grR1BUE29mG2Fm2kX";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPathForRevocation = null;
            MasterSecret masterSecretObject = await MasterSecretApi.CreateMasterSecretAsync();

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            (CredentialDefinition credDefObject, CredentialDefinitionPrivate credDefPvtObject, CredentialKeyCorrectnessProof keyProofObject) =
                await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            string schemaId = await CredentialDefinitionApi.GetCredentialDefinitionAttributeAsync(credDefObject, "schema_id");
            CredentialOffer credOfferObject = await CredentialOfferApi.CreateCredentialOfferAsync(schemaId, credDefObject, keyProofObject);

            (CredentialRequest credRequestObject, CredentialRequestMetadata metaDataObject) =
                await CredentialRequestApi.CreateCredentialRequestAsync(proverDid, credDefObject, masterSecretObject, "testMasterSecretName", credOfferObject);

            (RevocationRegistryDefinition revRegDefObject, RevocationRegistryDefinitionPrivate revRegDefPvtObject, RevocationRegistry revRegObject, RevocationRegistryDelta revRegDeltaObject) =
                await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDefObject, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPathForRevocation);

            (Credential credObject, RevocationRegistry revRegObjectNew, RevocationRegistryDelta revDeltaObject) =
                await CredentialApi.CreateCredentialAsync(credDefObject, credDefPvtObject, credOfferObject, credRequestObject,
                attrNames, attrNamesRaw, attrNamesEnc, revRegDefObject, revRegDefPvtObject, revRegObject, 1, new List<long>() { 1 });

            //Act
            Func<Task> act = async () => await CredentialApi.GetCredentialAttributeAsync(credObject, attributeName);

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }

        [Test, TestCase(TestName = "GetCredentialAttributeAsync() with JSON inputs throws SharedRsException when given empty attribute name.")]
        public async Task GetCredentialAttributeJsonAsyncThrowsException()
        {
            //Arrange
            string attributeName = "";

            List<string> attrNames = new() { "name", "age", "sex" };
            List<string> attrNamesRaw = new() { "Alex", "20", "male" };
            List<string> attrNamesEnc = await CredentialApi.EncodeCredentialAttributesAsync(attrNamesRaw);
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string proverDid = "VsKV7grR1BUE29mG2Fm2kX";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPathForRevocation = null;
            string masterSecretObject = await MasterSecretApi.CreateMasterSecretJsonAsync();

            string schemaObject = await SchemaApi.CreateSchemaJsonAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            (string credDefObject, string credDefPvtObject, string keyProofObject) =
                await CredentialDefinitionApi.CreateCredentialDefinitionJsonAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            string schemaId = await CredentialDefinitionApi.GetCredentialDefinitionAttributeAsync(credDefObject, "schema_id");
            string credOfferObject = await CredentialOfferApi.CreateCredentialOfferJsonAsync(schemaId, credDefObject, keyProofObject);

            (string credRequestObject, string metaDataObject) =
                await CredentialRequestApi.CreateCredentialRequestJsonAsync(proverDid, credDefObject, masterSecretObject, "testMasterSecretName", credOfferObject);

            (string revRegDefObject, string revRegDefPvtObject, string revRegObject, string revRegDeltaObject) =
                await RevocationApi.CreateRevocationRegistryJsonAsync(issuerDid, credDefObject, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPathForRevocation);

            (string credObject, string revRegObjectNew, string revDeltaObject) =
                await CredentialApi.CreateCredentialAsync(credDefObject, credDefPvtObject, credOfferObject, credRequestObject,
                attrNames, attrNamesRaw, attrNamesEnc, revRegDefObject, revRegDefPvtObject, revRegObject, 1, new List<long>() { 1 });

            //Act
            Func<Task> act = async () => await CredentialApi.GetCredentialAttributeAsync(credObject, attributeName);

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();

        }
        #endregion
    }
}