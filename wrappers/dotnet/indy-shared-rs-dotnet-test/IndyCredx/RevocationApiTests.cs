using FluentAssertions;
using indy_shared_rs_dotnet.IndyCredx;
using indy_shared_rs_dotnet.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace indy_shared_rs_dotnet_test.IndyCredx
{
    public class RevocationApiTests
    {
        #region Tests for CreateRevocationRegistry
        [Test, TestCase(TestName = "CreateRevocationRegistryAsync() returns a CredentialDefintion, CredentialDefinitionPrivate and CredentialKeyCorrectnessProof object.")]
        public async Task CreateRevocationRegistryAsyncWorks()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            (CredentialDefinition credDef, _, _) =
                await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            //Act
            (RevocationRegistryDefinition revRegDefObject, RevocationRegistryDefinitionPrivate revRegDefPvtObject, RevocationRegistry revRegObject, RevocationRegistryDelta revRegDeltaObject) = await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDef, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);

            //Assert
            revRegDefObject.Should().BeOfType(typeof(RevocationRegistryDefinition));
            revRegDefPvtObject.Should().BeOfType(typeof(RevocationRegistryDefinitionPrivate));
            revRegObject.Should().BeOfType(typeof(RevocationRegistry));
            revRegDeltaObject.Should().BeOfType(typeof(RevocationRegistryDelta));
        }

        [Test, TestCase(TestName = "CreateRevocationRegistryAsync() throws SharedRsException when provided credential definition is invalid.")]
        public async Task CreateRevocationRegistryAsyncThrowsException()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            (CredentialDefinition credDef, _, _) =
                await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            //Act
            Func<Task> act = async () => await RevocationApi.CreateRevocationRegistryAsync(issuerDid, new(), "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);

            //Assert
            await act.Should().ThrowAsync<SharedRsException>(); 
        }

        [Test, TestCase(TestName = "CreateRevocationRegistryJsonAsync() returns a CredentialDefintion, CredentialDefinitionPrivate and CredentialKeyCorrectnessProof object.")]
        public async Task CreateRevocationRegistryJsonAsyncWorks()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            string schemaObject = await SchemaApi.CreateSchemaJsonAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            (string credDef, _, _) = await CredentialDefinitionApi.CreateCredentialDefinitionJsonAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            //Act
            (string revRegDefObject, string revRegDefPvtObject, string revRegObject, string revRegDeltaObject) = await RevocationApi.CreateRevocationRegistryJsonAsync(issuerDid, credDef, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);
            
            //Assert
            JObject.Parse(revRegDefObject)["ver"].Should().NotBeNull();
            JObject.Parse(revRegDefPvtObject)["value"].Should().NotBeNull();
            JObject.Parse(revRegObject)["ver"].Should().NotBeNull();
            JObject.Parse(revRegDeltaObject)["ver"].Should().NotBeNull();
        }

        [Test, TestCase(TestName = "CreateRevocationRegistryJsonAsync() throws SharedRsException when provided credential definition is invalid.")]
        public async Task CreateRevocationRegistryJsonAsyncThrowsException()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string testTailsPath = null;

            //Act
            Func<Task> act = async () => await RevocationApi.CreateRevocationRegistryJsonAsync(issuerDid, "{}", "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }
        #endregion

        #region Tests for CreateRevocationRegistryFromJson
        [Test, TestCase(TestName = "CreateRevocationRegistryFromJsonAsync() works.")]
        public async Task CreateRevocationRegistryFromJsonAsyncWorks()
        {
            //Arrange
            string revRegJson = "{" +
                "\"ver\":\"1.0\"," +
                "\"value\":{" +
                    "\"accum\":\"21 121B0EBFF6EFDCD588B810E6ACF0394EC5D0EC1F44CA0987BFED06D73658566FF 21 1229800B78BFC8C1DF553C48D4A32F13D116A85494146D8727B0ABFD0C249F06E 6 8372FF1535B689AD7F4DB98C8A0FFCE387074208FE55DC9A94628A4FF0E921EE 4 364D0DF6297A4C8B1AF4B0B33EB80528D6D1559ED633D2106F3930152C7FE700 6 74AE0C197DD8E241B5F77DFD4CDF219A80091C4CFB869D72F8EDC22C4562E0F8 4 2BA80EE5693D063BBBA422DCCCB0C6CE4354378C9B81FCAE92C609F0A01C1D1E\"" +
                    "}" +
                "}";

            //Act
            var expected = await RevocationApi.CreateRevocationRegistryFromJsonAsync(revRegJson);

            //Assert
            expected.Should().BeOfType<RevocationRegistry>();
        }

        [Test, TestCase(TestName = "CreateRevocationRegistryFromJsonAsync() throws IndexOutOfRangeException when provided with an empty json string ")]
        public async Task CreateRevocationRegistryFromJsonAsyncThrowsExceptionForEmptyString()
        {
            //Arrange
            string revRegJson = "";

            //Act
            Func<Task> act = async () => await RevocationApi.CreateRevocationRegistryFromJsonAsync(revRegJson);

            //Assert
            await act.Should().ThrowAsync<IndexOutOfRangeException>();
        }

        [Test, TestCase(TestName = "CreateRevocationRegistryFromJsonAsync() throws SharedRsException when provided with invalid json string.")]
        public async Task CreateRevocationRegistryFromJsonAsyncThrowsExceptionForInvalidString()
        {
            //Arrange
            string revRegJson = "{}";

            //Act
            Func<Task> act = async () => await RevocationApi.CreateRevocationRegistryFromJsonAsync(revRegJson);

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }
        #endregion

        #region Tests for CreateRevocationRegistryDefinitionFromJson
        [Test, TestCase(TestName = "CreateRevocationRegistryDefinitionFromJsonAsync() creates a new revocation registry definition.")]
        public async Task CreateRevocationRegistryDefinitionFromJsonAsyncWorks()
        {
            //Arrange
            string revRegDefJson = "{" +
                "\"ver\":\"1.0\"," +
                "\"id\":\"NcYxiDXkpYi6ov5FcYDi1e:4:NcYxiDXkpYi6ov5FcYDi1e:3:CL:NcYxiDXkpYi6ov5FcYDi1e:2:gvt:1.0:tag:CL_ACCUM:test_tag\"," +
                "\"revocDefType\":\"CL_ACCUM\"," +
                "\"tag\":\"test_tag\"," +
                "\"credDefId\":\"NcYxiDXkpYi6ov5FcYDi1e:3:CL:NcYxiDXkpYi6ov5FcYDi1e:2:gvt:1.0:tag\"," +
                "\"value\":{" +
                    "\"issuanceType\":\"ISSUANCE_BY_DEFAULT\"," +
                    "\"maxCredNum\":99," +
                    "\"publicKeys\":{" +
                        "\"accumKey\":{" +
                            "\"z\":\"1 1ED957F7A41A2658EF1F38EAA0BC42B02A69A14A7D831006347BACF10123BAAB 1 0DCDE63B936D01D0B33BC14F79E2C0FF4759A9F860D2DA3912968182F04E91E3 1 05670AD5CBB12BD0FB2D8746A2D6A82E355F6FFC26C3E8D0394301C0AC640D98 1 18ACCA9600E5C4B3A1F5A790AA278E2E568256A9567ADCFF3FF986C466D32176 1 0412F1D141E3A29667817119D722F6685B4EFEF4F2FA2869BDAA613D0D7546B9 1 1D78496DF847407CE9087F221B5C7D0F65D4FEF52D684B495459B49FA157AE70 1 23125A66615F780D73C4495B8B65CE33002290FA72D0AB4FA6E0D53E3FF798B3 1 0C478C4AC57DD659FA536DEE7D5495BE3D3FE75614BA85EA35B9A046A1465264 1 21D532399BD3BD023CF527C80377827B2C59261F0F116A5D7DA5FDCC34B36303 1 10D98FA3CFBB0BD980ACE39CF5CFC9644F24C84168A737E9146C810BC0CD8169 1 0AF66A1A2DF030C9272F610DAC676B5DB6164AA22B4C3D7595A45B3D2FC50F00 1 1D8D584AA99D0ABC8A1C0B9C55EACFCB8F822D617508640C9E49BEDA86ED6995\"" +
                            "}" +
                        "}," +
                    "\"tailsHash\":\"52Hw3nM2S38dYU744kuVxJbsaipQ5fDDUVYjjK52B6DP\"," +
                    "\"tailsLocation\":\"\"" +
                "}" +
            "}";

            //Act
            var expected = await RevocationApi.CreateRevocationRegistryDefinitionFromJsonAsync(revRegDefJson);

            //Assert
            expected.Should().BeOfType<RevocationRegistryDefinition>();
        }

        [Test, TestCase(TestName = "CreateRevocationRegistryDefinitionFromJsonAsync() throws IndexOutOfRangeException when provided with empty json string.")]
        public async Task CreateRevocationRegistryDefinitionFromJsonAsyncThrowsExceptionForEmptyString()
        {
            //Arrange
            string revRegDefJson = "";

            //Act
            Func<Task> act = async () => await RevocationApi.CreateRevocationRegistryDefinitionFromJsonAsync(revRegDefJson);

            //Assert
            await act.Should().ThrowAsync<IndexOutOfRangeException>();
        }

        [Test, TestCase(TestName = "CreateRevocationRegistryDefinitionFromJsonAsync() throws SharedRsException when provided with invalid json string.")]
        public async Task CreateRevocationRegistryDefinitionFromJsonAsyncThrowsExceptionForInvalidString()
        {
            //Arrange
            string revRegDefJson = "{}";

            //Act
            Func<Task> act = async () => await RevocationApi.CreateRevocationRegistryDefinitionFromJsonAsync(revRegDefJson);

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }
        #endregion

        #region Tests for UpdateRevocationRegistry
        [Test, TestCase(TestName = "UpdateRevocationRegistryAsync() works.")]
        public async Task UpdateRevocationRegistryWorks()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            (CredentialDefinition credDef, _, _) =
                await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            (RevocationRegistryDefinition revRegDefObject, _, RevocationRegistry tmpRevRegObject, _) =
                await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDef, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);

            List<long> issuedList = new List<long> { 0 };
            List<long> revokedList = new List<long> { 0 };

            //Act
            (RevocationRegistry revRegObject, RevocationRegistryDelta revRegDeltaObject) =
                await RevocationApi.UpdateRevocationRegistryAsync(
                    revRegDefObject,
                    tmpRevRegObject,
                    issuedList,
                    revokedList,
                    revRegDefObject.Value.TailsLocation
                    );

            //Assert
            revRegObject.Value.Should().NotBeSameAs(tmpRevRegObject.Value);
            revRegDeltaObject.Should().NotBeNull();
        }

        [Test, TestCase(TestName = "UpdateRevocationRegistryAsync() throws SharedRsException when revocation registry is invalid.")]
        public async Task UpdateRevocationRegistryThrowsException()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            (CredentialDefinition credDef, _, _) =
                await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            (RevocationRegistryDefinition revRegDefObject, _, RevocationRegistry tmpRevRegObject, _) =
                await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDef, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);

            List<long> issuedList = new List<long>() { 0 };
            List<long> revokedList = new List<long>() { 0 };

            //Act
            Func<Task> act = async () => await RevocationApi.UpdateRevocationRegistryAsync(
                    revRegDefObject,
                    new(),
                    issuedList,
                    revokedList,
                    revRegDefObject.Value.TailsLocation
                    );

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }

        [Test, TestCase(TestName = "UpdateRevocationRegistryAsync() works.")]
        public async Task UpdateRevocationRegistryWorksWithJson()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            string schemaObject = await SchemaApi.CreateSchemaJsonAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            (string credDef, _, _) = await CredentialDefinitionApi.CreateCredentialDefinitionJsonAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            (string revRegDefJson, _, string tmpRevRegJson, _) =
                await RevocationApi.CreateRevocationRegistryJsonAsync(issuerDid, credDef, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);
            RevocationRegistryDefinition revRegDefObject = await RevocationApi.CreateRevocationRegistryDefinitionFromJsonAsync(revRegDefJson);
            List<long> issuedList = new List<long> { 0 };
            List<long> revokedList = new List<long> { 0 };

            //Act
            (string revRegJson, string revRegDeltaJson) =
                await RevocationApi.UpdateRevocationRegistryAsync(
                    revRegDefJson,
                    tmpRevRegJson,
                    issuedList,
                    revokedList,
                    revRegDefObject.Value.TailsLocation);


            //Assert
            revRegJson.Should().NotBeEmpty();
            revRegDeltaJson.Should().NotBeEmpty();
        }

        [Test, TestCase(TestName = "UpdateRevocationRegistryAsync() throws SharedRsException when revocation registry is invalid.")]
        public async Task UpdateRevocationRegistryThrowsExceptionWithJson()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            string schemaObject = await SchemaApi.CreateSchemaJsonAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            (string credDef, _, _) =
                await CredentialDefinitionApi.CreateCredentialDefinitionJsonAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            (string revRegDefObject, _, string tmpRevRegObject, _) =
                await RevocationApi.CreateRevocationRegistryJsonAsync(issuerDid, credDef, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);

            List<long> issuedList = new List<long>() { 0 };
            List<long> revokedList = new List<long>() { 0 };

            //Act
            Func<Task> act = async () => await RevocationApi.UpdateRevocationRegistryAsync(
                    revRegDefObject,
                    "",
                    issuedList,
                    revokedList,
                    ""
                    );

            //Assert
            await act.Should().ThrowAsync<Exception>();
        }
        #endregion

        #region Tests for RevokeCredentialAsync
        [Test, TestCase(TestName = "RevokeCredentialAsync() works.")]
        public async Task RevokeCredentialWorks()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            (CredentialDefinition credDef,
                _,
                _) = await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            (RevocationRegistryDefinition revRegDefObject,
                _,
                RevocationRegistry tmpRevRegObject,
                _) = await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDef, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);

            //Act
            (_, RevocationRegistryDelta actual) =
                await RevocationApi.RevokeCredentialAsync(
                    revRegDefObject,
                    tmpRevRegObject,
                    0,
                    revRegDefObject.Value.TailsLocation
                    );

            //Assert
            actual.Value.PrevAccum.Should().NotBeNull();
            actual.Value.Revoked.Should().HaveCount(1);
        }

        [Test, TestCase(TestName = "RevokeCredential() throws SharedRsException when revocation registry is invalid.")]
        public async Task RevokeCredentialThrowsException()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            (CredentialDefinition credDef,
                _,
                _) = await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            (RevocationRegistryDefinition revRegDefObject,
                _,
                RevocationRegistry tmpRevRegObject,
                _) = await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDef, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);

            //Act
            Func<Task> act = async () => await RevocationApi.RevokeCredentialAsync(
                    revRegDefObject,
                    new(),
                    0,
                    revRegDefObject.Value.TailsLocation
                    );

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }

        [Test, TestCase(TestName = "RevokeCredentialAsync() works.")]
        public async Task RevokeCredentialWorksWithJson()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            string schemaObject = await SchemaApi.CreateSchemaJsonAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            (string credDef,
                _,
                _) = await CredentialDefinitionApi.CreateCredentialDefinitionJsonAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            (string revRegDefJson,
                _,
                string tmpRevRegJson,
                _) = await RevocationApi.CreateRevocationRegistryJsonAsync(issuerDid, credDef, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);

            RevocationRegistryDefinition revRegDefObject = await RevocationApi.CreateRevocationRegistryDefinitionFromJsonAsync(revRegDefJson);

            //Act
            (_, string deltaJson) =
                await RevocationApi.RevokeCredentialAsync(
                    revRegDefJson,
                    tmpRevRegJson,
                    0,
                    revRegDefObject.Value.TailsLocation
                    );

            RevocationRegistryDelta actual = JsonConvert.DeserializeObject<RevocationRegistryDelta>(deltaJson);

            //Assert
            actual.Value.PrevAccum.Should().NotBeNull();
            actual.Value.Revoked.Should().HaveCount(1);
        }

        [Test, TestCase(TestName = "RevokeCredential() throws SharedRsException when revocation registry is invalid.")]
        public async Task RevokeCredentialThrowsExceptionWithJson()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            string schemaObject = await SchemaApi.CreateSchemaJsonAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            (string credDef,
                _,
                _) = await CredentialDefinitionApi.CreateCredentialDefinitionJsonAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            (string revRegDefObject,
                _,
                string tmpRevRegObject,
                _) = await RevocationApi.CreateRevocationRegistryJsonAsync(issuerDid, credDef, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);

            //Act
            Func<Task> act = async () => await RevocationApi.RevokeCredentialAsync(
                    revRegDefObject,
                    "",
                    0,
                    ""
                    );

            //Assert
            await act.Should().ThrowAsync<Exception>();
        }
        #endregion

        #region Tests for MergeRevocationRegistryDeltas
        [Test, TestCase(TestName = "MergeRevocationRegistryAsync() works.")]
        public async Task MergeRevocationRegistryDeltasWorks()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            (CredentialDefinition credDef, _, _) =
                await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            (RevocationRegistryDefinition revRegDefObject, _, RevocationRegistry revRegObject, _) =
                await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDef, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);

            List<long> issuedList1 = new List<long> { 0, 2 };
            List<long> issuedList2 = new List<long> { 0, 3 };
            List<long> revokedList1 = new List<long> { 0, 2 };
            List<long> revokedList2 = new List<long> { 0, 3 };

            (_, RevocationRegistryDelta delta1) = await RevocationApi.UpdateRevocationRegistryAsync(
                revRegDefObject,
                revRegObject,
                issuedList1,
                revokedList1,
                revRegDefObject.Value.TailsLocation
                );
            (_, RevocationRegistryDelta delta2) = await RevocationApi.UpdateRevocationRegistryAsync(
                revRegDefObject,
                revRegObject,
                issuedList2,
                revokedList2,
                revRegDefObject.Value.TailsLocation
                );

            //Act
            RevocationRegistryDelta actual = await RevocationApi.MergeRevocationRegistryDeltasAsync(delta1, delta2);

            //Assert
            actual.Value.Revoked.Should().HaveCount(1);
            actual.Value.Revoked.Contains(2).Should().BeTrue();
        }

        [Test, TestCase(TestName = "MergeRevocationRegistryDeltaAsync() throws SharedRsException when one delta is invalid.")]
        public async Task MergeRevocationRegistryDeltasASyncThrowsException()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            (CredentialDefinition credDef, _, _) =
                await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            (RevocationRegistryDefinition revRegDefObject, _, RevocationRegistry revRegObject, _) =
                await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDef, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);

            List<long> issuedList1 = new List<long> { 0, 2 };
            List<long> issuedList2 = new List<long> { 0, 3 };
            List<long> revokedList1 = new List<long> { 0, 2 };
            List<long> revokedList2 = new List<long> { 0, 3 };

            (_, RevocationRegistryDelta delta1) = await RevocationApi.UpdateRevocationRegistryAsync(
                revRegDefObject,
                revRegObject,
                issuedList1,
                revokedList1,
                revRegDefObject.Value.TailsLocation
                );
            (_, RevocationRegistryDelta delta2) = await RevocationApi.UpdateRevocationRegistryAsync(
                revRegDefObject,
                revRegObject,
                issuedList2,
                revokedList2,
                revRegDefObject.Value.TailsLocation
                );

            //Act
            Func<Task> act = async () => await RevocationApi.MergeRevocationRegistryDeltasAsync(delta1, new());

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }

        [Test, TestCase(TestName = "MergeRevocationRegistryAsync() works.")]
        public async Task MergeRevocationRegistryDeltasWorksWithJson()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            string schemaObject = await SchemaApi.CreateSchemaJsonAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            (string credDef, _, _) =
                await CredentialDefinitionApi.CreateCredentialDefinitionJsonAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            (string revRegDefJson, _, string revRegJson, _) =
                await RevocationApi.CreateRevocationRegistryJsonAsync(issuerDid, credDef, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);

            RevocationRegistryDefinition revRegDefObject = await RevocationApi.CreateRevocationRegistryDefinitionFromJsonAsync(revRegDefJson);

            List<long> issuedList1 = new List<long> { 0, 2 };
            List<long> issuedList2 = new List<long> { 0, 3 };
            List<long> revokedList1 = new List<long> { 0, 2 };
            List<long> revokedList2 = new List<long> { 0, 3 };

            (_, string delta1) = await RevocationApi.UpdateRevocationRegistryAsync(
                revRegDefJson,
                revRegJson,
                issuedList1,
                revokedList1,
                revRegDefObject.Value.TailsLocation
                );
            (_, string delta2) = await RevocationApi.UpdateRevocationRegistryAsync(
                revRegDefJson,
                revRegJson,
                issuedList2,
                revokedList2,
                revRegDefObject.Value.TailsLocation
                );

            //Act
            string deltaJson = await RevocationApi.MergeRevocationRegistryDeltasAsync(delta1, delta2);

            RevocationRegistryDelta actual = JsonConvert.DeserializeObject<RevocationRegistryDelta>(deltaJson);

            //Assert
            actual.Value.Revoked.Should().HaveCount(1);
            actual.Value.Revoked.Contains(2).Should().BeTrue();
        }

        [Test, TestCase(TestName = "MergeRevocationRegistryDeltaAsync() throws SharedRsException when one delta is invalid.")]
        public async Task MergeRevocationRegistryDeltasASyncThrowsExceptionWithJson()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            string schemaObject = await SchemaApi.CreateSchemaJsonAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            (string credDef, _, _) =
                await CredentialDefinitionApi.CreateCredentialDefinitionJsonAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            (string revRegDefJson, _, string revRegJson, _) =
                await RevocationApi.CreateRevocationRegistryJsonAsync(issuerDid, credDef, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);

            RevocationRegistryDefinition revRegDefObject = await RevocationApi.CreateRevocationRegistryDefinitionFromJsonAsync(revRegDefJson);

            List<long> issuedList1 = new List<long> { 0, 2 };
            List<long> issuedList2 = new List<long> { 0, 3 };
            List<long> revokedList1 = new List<long> { 0, 2 };
            List<long> revokedList2 = new List<long> { 0, 3 };

            (_, string delta1) = await RevocationApi.UpdateRevocationRegistryAsync(
                revRegDefJson,
                revRegJson,
                issuedList1,
                revokedList1,
                revRegDefObject.Value.TailsLocation
                );

            //Act
            Func<Task> act = async () => await RevocationApi.MergeRevocationRegistryDeltasAsync(delta1, "{}");

            //Assert
            await act.Should().ThrowAsync<Exception>();
        }
        #endregion

        #region Tests for CreateOrUpdateRevocationState
        [Test, TestCase(TestName = "CreateOrUpdateRevocationStateAsync() works.")]
        public async Task CreateOrUpdateRevocationStateAsyncWorks()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            (CredentialDefinition credDef,
                _,
                _) = await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);


            (RevocationRegistryDefinition revRegDefObject, _, _, RevocationRegistryDelta revRegDeltaObject) =
                await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDef, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);

            CredentialRevocationState revState = new();

            //Act
            CredentialRevocationState actual = await RevocationApi.CreateOrUpdateRevocationStateAsync(
                revRegDefObject,
                revRegDeltaObject,
                0,
                0,
                revRegDefObject.Value.TailsLocation,
                revState
                );

            //Assert
            actual.Should().NotBeNull();
        }

        [Test, TestCase(TestName = "CreateOrUpdateRevocationStateAsync() throws SharedRsException when revocation registry delta is invalid.")]
        public async Task CreateOrUpdateRevocationStateAsyncThrowsException()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            (CredentialDefinition credDef,
                _,
                _) = await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);


            (RevocationRegistryDefinition revRegDefObject, _, _, RevocationRegistryDelta revRegDeltaObject) =
                await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDef, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);

            CredentialRevocationState revState = new();

            //Act
            Func<Task> act = async () => await RevocationApi.CreateOrUpdateRevocationStateAsync(
                revRegDefObject,
                new(),
                0,
                0,
                revRegDefObject.Value.TailsLocation,
                revState
                );

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }

        [Test, TestCase(TestName = "CreateOrUpdateRevocationStateAsync() works.")]
        public async Task CreateOrUpdateRevocationStateAsyncWorksWithJson()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            (CredentialDefinition credDef,
                _,
                _) = await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);


            (RevocationRegistryDefinition revRegDefObject, _, _, RevocationRegistryDelta revRegDeltaObject) =
                await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDef, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);

            CredentialRevocationState revState = new();

            //Act
            CredentialRevocationState actual = await RevocationApi.CreateOrUpdateRevocationStateAsync(
                revRegDefObject,
                revRegDeltaObject,
                0,
                0,
                revRegDefObject.Value.TailsLocation,
                revState
                );

            //Assert
            actual.Should().NotBeNull();
        }

        [Test, TestCase(TestName = "CreateOrUpdateRevocationStateAsync() throws SharedRsException when revocation registry delta is invalid.")]
        public async Task CreateOrUpdateRevocationStateAsyncThrowsExceptionWithJson()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            (CredentialDefinition credDef,
                _,
                _) = await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);


            (RevocationRegistryDefinition revRegDefObject, _, _, RevocationRegistryDelta revRegDeltaObject) =
                await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDef, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);

            CredentialRevocationState revState = new();

            //Act
            Func<Task> act = async () => await RevocationApi.CreateOrUpdateRevocationStateAsync(
                revRegDefObject,
                new(),
                0,
                0,
                revRegDefObject.Value.TailsLocation,
                revState
                );

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }
        #endregion

        #region Tests for CreateRevocationStateFromJson
        [Test, TestCase(TestName = "CreateRevocationStateFromJsonAsync() returns a CredentialRevocationState object if provided with a valid json string.")]
        public async Task CreateRevocationStateFromJsonAsyncWorks()
        {
            //Arrange
            string revStateJson = "{" +
                "\"Handle\":9," +
                "\"witness\":{" +
                    "\"omega\":\"21 12CD27F6902B0C605053D21C277B62B2625802AECB16B41C45113CD3DA8A03A0D 21 13AEF810B5457092EC814EB84ECE38DD159A36D224551B051312410497A55A134 6 77FD09EE7F36E02EE33F475F95A06D2F13B8C1B3FDB5AD135CFD92E67CCB5EB2 4 37976F8859E86691D601415504DD4473F969C27FDB655787BCCA778FEC2F9C13 6 6D551EC893C94FB1347556ECA88226446184C48D97EE99B9437238E4687C0C2A 4 16917F5C8DE3FB1855737C204E57B1ED23AC27E238751BF182F5D47A78841884\"" +
                    "}," +
                    "\"rev_reg\":{" +
                        "\"accum\":\"21 12CD27F6902B0C605053D21C277B62B2625802AECB16B41C45113CD3DA8A03A0D 21 13AEF810B5457092EC814EB84ECE38DD159A36D224551B051312410497A55A134 6 77FD09EE7F36E02EE33F475F95A06D2F13B8C1B3FDB5AD135CFD92E67CCB5EB2 4 37976F8859E86691D601415504DD4473F969C27FDB655787BCCA778FEC2F9C13 6 6D551EC893C94FB1347556ECA88226446184C48D97EE99B9437238E4687C0C2A 4 16917F5C8DE3FB1855737C204E57B1ED23AC27E238751BF182F5D47A78841884\"" +
                    "}," +
                    "\"timestamp\":0" +
                "}";


            //Act
            CredentialRevocationState actual = await RevocationApi.CreateRevocationStateFromJsonAsync(revStateJson);

            //Assert
            actual.Should().BeOfType<CredentialRevocationState>();
        }

        [Test, TestCase(TestName = "CreateRevocationStateFromJsonAsync() throws IndexOutOfRangeException when provided with an empty json string.")]
        public async Task CreateRevocationStateFromJsonAsyncThrowsExceptionsForEmptyString()
        {
            //Arrange
            string revStateJson = "";

            //Act
            Func<Task> act = async () => await RevocationApi.CreateRevocationStateFromJsonAsync(revStateJson);

            //Assert
            await act.Should().ThrowAsync<IndexOutOfRangeException>();
        }

        [Test, TestCase(TestName = "CreateRevocationStateFromJsonAsync() throws SharedRsException when provided with an invalid json string.")]
        public async Task CreateRevocationStateFromJsonAsyncThrowsExceptionsForInvalidString()
        {
            //Arrange
            string revStateJson = "{}";

            //Act
            Func<Task> act = async () => await RevocationApi.CreateRevocationStateFromJsonAsync(revStateJson);

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }
        #endregion

        #region Tests for GetRevocationRegistryDefinitionAttribute
        [Test, TestCase(TestName = "GetRevocationRegistryDefinitionAttributeAsync() works for attribute name 'id'.")]
        public async Task GetRevocationRegistryDefinitionAttributeAsyncWorks()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            (CredentialDefinition credDef,
                _,
                _) = await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            (RevocationRegistryDefinition revRegDefObject,
                _,
                _,
                _) = await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDef, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);

            //Act
            string actual = await RevocationApi.GetRevocationRegistryDefinitionAttributeAsync(
                revRegDefObject,
                "id"
                );

            //Assert
            actual.Should().NotBeNull();
        }

        [Test, TestCase(TestName = "GetRevocationRegistryDefinitionAttributeAsync() throws SharedRsException for invalid attribute name.")]
        public async Task GetRevocationDefinitionAttributeAsyncThrowsException()
        {
            //Arrange
            string attributeName = "test";
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string testTailsPath = null;

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            (CredentialDefinition credDef,
                _,
                _) = await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            (RevocationRegistryDefinition revRegDefObject,
                _,
                _,
                _) = await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDef, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPath);

            //Act
            Func<Task> act = async () => await RevocationApi.GetRevocationRegistryDefinitionAttributeAsync(revRegDefObject, attributeName);

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }
        #endregion
    }
}
