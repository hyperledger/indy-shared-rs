using FluentAssertions;
using indy_shared_rs_dotnet.IndyCredx;
using indy_shared_rs_dotnet.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace indy_shared_rs_dotnet_test.IndyCredx
{
    public class PresentationApiTests
    {
        #region Tests for CreatePresentationAsync
        [Test, TestCase(TestName = "CreatePresentationAsync() returns a presentation object of a self attestable proof when given the propper arguments.")]
        public async Task CreatePresentationAsyncOfSelfAttestProof()
        {
            //Arrange
            string nonce = await PresentationRequestApi.GenerateNonceAsync();
            long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            string presReqJson = "{" +
                "\"name\": \"proof\"," +
                "\"version\": \"1.0\", " +
                $"\"nonce\": \"{nonce}\"," +
                "\"requested_attributes\": " +
                "{" +
                    "\"reft\": " +
                    "{" +
                        "\"name\":\"attr\"," +
                        "\"value\":\"myValue\"," +
                        "\"names\": [], " +
                        "\"non_revoked\":" +
                        "{ " +
                            $"\"from\": {timestamp}, " +
                            $"\"to\": {timestamp}" +
                        "}" +
                    "}" +
                "}," +
                "\"revealed_attrs_groups\": {}," +
                "\"requested_predicates\": " +
                "{" +
                    "\"light\": " +
                    "{" +
                        "\"name\":\"pred\"," +
                        "\"p_type\":\">=\"," +
                        "\"p_value\":18," +
                        "\"non_revoked\":" +
                        "{ " +
                            $"\"from\": {timestamp}, " +
                            $"\"to\": {timestamp}" +
                        "}," +
                        "\"restrictions\":" +
                        "[" +
                            "{\"schema_name\": \"blubb\"," +
                            "\"schema_version\": \"1.0\"}," +
                            "{\"cred_def_id\": \"blubb2\"," +
                            "\"schema_version\": \"2.0\"}," +
                            "{\"not_an_attribute\": \"should Fail\"}" +
                        "]" +
                    "}" +
                "}," +
                "\"non_revoked\": " +
                "{ " +
                    $"\"from\": {timestamp}," +
                    $"\"to\": {timestamp}" +
                "}," +
                "\"ver\": \"1.0\"" +
                "}";
            PresentationRequest presReqObject = await PresentationRequestApi.CreatePresReqFromJsonAsync(presReqJson);

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
            (CredentialRequest credRequestObject, _) =
                await CredentialRequestApi.CreateCredentialRequestAsync(proverDid, credDefObject, masterSecretObject, "testMasterSecretName", credOfferObject);

            (RevocationRegistryDefinition revRegDefObject, RevocationRegistryDefinitionPrivate revRegDefPvtObject, RevocationRegistry revRegObject, RevocationRegistryDelta revRegDeltaObject) =
                await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDefObject, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPathForRevocation);
            (Credential credObject, _, _) =
                await CredentialApi.CreateCredentialAsync(credDefObject, credDefPvtObject, credOfferObject, credRequestObject,
                attrNames, attrNamesRaw, attrNamesEnc, revRegDefObject, revRegDefPvtObject, revRegObject, 1, new List<long>() { 1 });

            CredentialRevocationState emptyRevocationState = new() { Handle = new IntPtr() };
            CredentialRevocationState credRevRegState = await RevocationApi.CreateOrUpdateRevocationStateAsync(
                revRegDefObject,
                revRegDeltaObject,
                credObject.Signature.RCredential.I,
                timestamp,
                revRegDefObject.Value.TailsLocation,
                emptyRevocationState);

            List<CredentialEntry> credentialEntries = new()
            {
                CredentialEntry.CreateCredentialEntry(credObject, timestamp, credRevRegState)
                //with empty timestamp and revState
                //new CredentialEntry(credObject,0, null)
            };

            List<CredentialProof> credentialProofs = new()
            {
                new CredentialProof
                {
                    EntryIndex = 1,
                    IsPredicate = Convert.ToByte(false),
                    Referent = "testReferent",
                    Reveal = Convert.ToByte(true)
                }
            };

            List<string> selfAttestNames = new()
            {
                "testSelfAttestName1"
            };

            List<string> selfAttestValues = new()
            {
                "testSelfAttestName1"
            };

            MasterSecret masterSecret = await MasterSecretApi.CreateMasterSecretAsync();

            List<Schema> schemas = new()
            {
                schemaObject
            };

            List<CredentialDefinition> credentialDefinitions = new()
            {
                credDefObject
            };

            //Act
            Presentation actual = await PresentationApi.CreatePresentationAsync(
                presReqObject,
                credentialEntries,
                credentialProofs,
                selfAttestNames,
                selfAttestValues,
                masterSecret,
                schemas,
                credentialDefinitions
                );

            //Assert
            _ = actual.Should().BeOfType(typeof(Presentation));
        }

        [Test, TestCase(TestName = "CreatePresentationAsync() with invalid presentation request handle throws.")]
        public async Task CreatePresentationAsyncOfSelfAttestProofThrows()
        {
            //Arrange
            string nonce = await PresentationRequestApi.GenerateNonceAsync();
            long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            string presReqJson = "{" +
                "\"name\": \"proof\"," +
                "\"version\": \"1.0\", " +
                $"\"nonce\": \"{nonce}\"," +
                "\"requested_attributes\": " +
                "{" +
                    "\"reft\": " +
                    "{" +
                        "\"name\":\"attr\"," +
                        "\"value\":\"myValue\"," +
                        "\"names\": [], " +
                        "\"non_revoked\":" +
                        "{ " +
                            $"\"from\": {timestamp}, " +
                            $"\"to\": {timestamp}" +
                        "}" +
                    "}" +
                "}," +
                "\"revealed_attrs_groups\": {}," +
                "\"requested_predicates\": " +
                "{" +
                    "\"light\": " +
                    "{" +
                        "\"name\":\"pred\"," +
                        "\"p_type\":\">=\"," +
                        "\"p_value\":18," +
                        "\"non_revoked\":" +
                        "{ " +
                            $"\"from\": {timestamp}, " +
                            $"\"to\": {timestamp}" +
                        "}," +
                        "\"restrictions\":" +
                        "[" +
                            "{\"schema_name\": \"blubb\"," +
                            "\"schema_version\": \"1.0\"}," +
                            "{\"cred_def_id\": \"blubb2\"," +
                            "\"schema_version\": \"2.0\"}," +
                            "{\"not_an_attribute\": \"should Fail\"}" +
                        "]" +
                    "}" +
                "}," +
                "\"non_revoked\": " +
                "{ " +
                    $"\"from\": {timestamp}," +
                    $"\"to\": {timestamp}" +
                "}," +
                "\"ver\": \"1.0\"" +
                "}";
            PresentationRequest presReqObject = await PresentationRequestApi.CreatePresReqFromJsonAsync(presReqJson);
            presReqObject.Handle = new IntPtr();

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

            CredentialRevocationState emptyRevocationState = new() { Handle = new IntPtr() };
            CredentialRevocationState credRevRegState = await RevocationApi.CreateOrUpdateRevocationStateAsync(
                revRegDefObject,
                revRegDeltaObject,
                credObject.Signature.RCredential.I,
                timestamp,
                revRegDefObject.Value.TailsLocation,
                emptyRevocationState);

            List<CredentialEntry> credentialEntries = new()
            {
                CredentialEntry.CreateCredentialEntry(credObject, timestamp, credRevRegState)
                //with empty timestamp and revState
                //new CredentialEntry(credObject,0, null)
            };

            List<CredentialProof> credentialProofs = new()
            {
                new CredentialProof
                {
                    EntryIndex = 1,
                    IsPredicate = Convert.ToByte(false),
                    Referent = "testReferent",
                    Reveal = Convert.ToByte(true)
                }
            };

            List<string> selfAttestNames = new()
            {
                "testSelfAttestName1"
            };

            List<string> selfAttestValues = new()
            {
                "testSelfAttestName1"
            };

            MasterSecret masterSecret = await MasterSecretApi.CreateMasterSecretAsync();

            List<Schema> schemas = new()
            {
                schemaObject
            };

            List<CredentialDefinition> credentialDefinitions = new()
            {
                credDefObject
            };

            //Act
            Func<Task<Presentation>> func = async () => await PresentationApi.CreatePresentationAsync(
                presReqObject,
                credentialEntries,
                credentialProofs,
                selfAttestNames,
                selfAttestValues,
                masterSecret,
                schemas,
                credentialDefinitions
                );

            //Assert
            _ = await func.Should().ThrowAsync<Exception>();
        }

        [Test, TestCase(TestName = "CreatePresentationAsync() returns a presentation object as JSON string of a self attestable proof when given the propper arguments.")]
        public async Task CreatePresentationJsonAsyncOfSelfAttestProof()
        {
            //Arrange
            string nonce = await PresentationRequestApi.GenerateNonceAsync();
            long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            string presReqJson = "{" +
                "\"name\": \"proof\"," +
                "\"version\": \"1.0\", " +
                $"\"nonce\": \"{nonce}\"," +
                "\"requested_attributes\": " +
                "{" +
                    "\"reft\": " +
                    "{" +
                        "\"name\":\"attr\"," +
                        "\"value\":\"myValue\"," +
                        "\"names\": [], " +
                        "\"non_revoked\":" +
                        "{ " +
                            $"\"from\": {timestamp}, " +
                            $"\"to\": {timestamp}" +
                        "}" +
                    "}" +
                "}," +
                "\"revealed_attrs_groups\": {}," +
                "\"requested_predicates\": " +
                "{" +
                    "\"light\": " +
                    "{" +
                        "\"name\":\"pred\"," +
                        "\"p_type\":\">=\"," +
                        "\"p_value\":18," +
                        "\"non_revoked\":" +
                        "{ " +
                            $"\"from\": {timestamp}, " +
                            $"\"to\": {timestamp}" +
                        "}," +
                        "\"restrictions\":" +
                        "[" +
                            "{\"schema_name\": \"blubb\"," +
                            "\"schema_version\": \"1.0\"}," +
                            "{\"cred_def_id\": \"blubb2\"," +
                            "\"schema_version\": \"2.0\"}," +
                            "{\"not_an_attribute\": \"should Fail\"}" +
                        "]" +
                    "}" +
                "}," +
                "\"non_revoked\": " +
                "{ " +
                    $"\"from\": {timestamp}," +
                    $"\"to\": {timestamp}" +
                "}," +
                "\"ver\": \"1.0\"" +
                "}";

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
            (CredentialRequest credRequestObject, _) =
                await CredentialRequestApi.CreateCredentialRequestAsync(proverDid, credDefObject, masterSecretObject, "testMasterSecretName", credOfferObject);

            (RevocationRegistryDefinition revRegDefObject, RevocationRegistryDefinitionPrivate revRegDefPvtObject, RevocationRegistry revRegObject, RevocationRegistryDelta revRegDeltaObject) =
                await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDefObject, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPathForRevocation);
            (Credential credObject, _, _) =
                await CredentialApi.CreateCredentialAsync(credDefObject, credDefPvtObject, credOfferObject, credRequestObject,
                attrNames, attrNamesRaw, attrNamesEnc, revRegDefObject, revRegDefPvtObject, revRegObject, 1, new List<long>() { 1 });

            CredentialRevocationState emptyRevocationState = new() { Handle = new IntPtr() };
            CredentialRevocationState credRevRegState = await RevocationApi.CreateOrUpdateRevocationStateAsync(
                revRegDefObject,
                revRegDeltaObject,
                credObject.Signature.RCredential.I,
                timestamp,
                revRegDefObject.Value.TailsLocation,
                emptyRevocationState);



            List<string> credentialEntries = new()
            {
               JsonConvert.SerializeObject(CredentialEntry.CreateCredentialEntry(credObject, timestamp, credRevRegState))
                //with empty timestamp and revState
                //new CredentialEntry(credObject,0, null)
            };

            List<string> credentialProofs = new()
            {
                JsonConvert.SerializeObject(new CredentialProof
                {
                    EntryIndex = 1,
                    IsPredicate = Convert.ToByte(false),
                    Referent = "testReferent",
                    Reveal = Convert.ToByte(true)
                })
            };

            List<string> selfAttestNames = new()
            {
                "testSelfAttestName1"
            };

            List<string> selfAttestValues = new()
            {
                "testSelfAttestName1"
            };

            string masterSecret = await MasterSecretApi.CreateMasterSecretJsonAsync();

            List<string> schemas = new()
            {
                schemaObject.JsonString
            };

            List<string> credentialDefinitions = new()
            {
                credDefObject.JsonString
            };

            //Act
            string actual = await PresentationApi.CreatePresentationAsync(
                presReqJson,
                credentialEntries,
                credentialProofs,
                selfAttestNames,
                selfAttestValues,
                masterSecret,
                schemas,
                credentialDefinitions
                );

            //Assert
            _ = actual.Should().NotBeNullOrEmpty();
        }

        [Test, TestCase(TestName = "CreatePresentationAsync() with JSON input and invalid presentation request handle throws.")]
        public async Task CreatePresentationJsonAsyncOfSelfAttestProofThrows()
        {
            //Arrange
            string nonce = await PresentationRequestApi.GenerateNonceAsync();
            long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            string presReqJson = "{" +
                "\"name\": \"proof\"," +
                "\"version\": \"1.0\", " +
                $"\"nonce\": \"{nonce}\"," +
                "\"requested_attributes\": " +
                "{" +
                    "\"reft\": " +
                    "{" +
                        "\"name\":\"attr\"," +
                        "\"value\":\"myValue\"," +
                        "\"names\": [], " +
                        "\"non_revoked\":" +
                        "{ " +
                            $"\"from\": {timestamp}, " +
                            $"\"to\": {timestamp}" +
                        "}" +
                    "}" +
                "}," +
                "\"revealed_attrs_groups\": {}," +
                "\"requested_predicates\": " +
                "{" +
                    "\"light\": " +
                    "{" +
                        "\"name\":\"pred\"," +
                        "\"p_type\":\">=\"," +
                        "\"p_value\":18," +
                        "\"non_revoked\":" +
                        "{ " +
                            $"\"from\": {timestamp}, " +
                            $"\"to\": {timestamp}" +
                        "}," +
                        "\"restrictions\":" +
                        "[" +
                            "{\"schema_name\": \"blubb\"," +
                            "\"schema_version\": \"1.0\"}," +
                            "{\"cred_def_id\": \"blubb2\"," +
                            "\"schema_version\": \"2.0\"}," +
                            "{\"not_an_attribute\": \"should Fail\"}" +
                        "]" +
                    "}" +
                "}," +
                "\"non_revoked\": " +
                "{ " +
                    $"\"from\": {timestamp}," +
                    $"\"to\": {timestamp}" +
                "}," +
                "\"ver\": \"1.0\"" +
                "}";

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
            (CredentialRequest credRequestObject, _) =
                await CredentialRequestApi.CreateCredentialRequestAsync(proverDid, credDefObject, masterSecretObject, "testMasterSecretName", credOfferObject);

            (RevocationRegistryDefinition revRegDefObject, RevocationRegistryDefinitionPrivate revRegDefPvtObject, RevocationRegistry revRegObject, RevocationRegistryDelta revRegDeltaObject) =
                await RevocationApi.CreateRevocationRegistryAsync(issuerDid, credDefObject, "test_tag", RegistryType.CL_ACCUM, IssuerType.ISSUANCE_BY_DEFAULT, 99, testTailsPathForRevocation);
            (Credential credObject, _, _) =
                await CredentialApi.CreateCredentialAsync(credDefObject, credDefPvtObject, credOfferObject, credRequestObject,
                attrNames, attrNamesRaw, attrNamesEnc, revRegDefObject, revRegDefPvtObject, revRegObject, 1, new List<long>() { 1 });

            CredentialRevocationState emptyRevocationState = new() { Handle = new IntPtr() };
            CredentialRevocationState credRevRegState = await RevocationApi.CreateOrUpdateRevocationStateAsync(
                revRegDefObject,
                revRegDeltaObject,
                credObject.Signature.RCredential.I,
                timestamp,
                revRegDefObject.Value.TailsLocation,
                emptyRevocationState);



            List<string> credentialEntries = new()
            {
               JsonConvert.SerializeObject(CredentialEntry.CreateCredentialEntry(credObject, timestamp, credRevRegState))
                //with empty timestamp and revState
                //new CredentialEntry(credObject,0, null)
            };

            List<string> credentialProofs = new()
            {
                JsonConvert.SerializeObject(new CredentialProof
                {
                    EntryIndex = 1,
                    IsPredicate = Convert.ToByte(false),
                    Referent = "testReferent",
                    Reveal = Convert.ToByte(true)
                })
            };

            List<string> selfAttestNames = new()
            {
                "testSelfAttestName1"
            };

            List<string> selfAttestValues = new()
            {
                "testSelfAttestName1"
            };

            string masterSecret = await MasterSecretApi.CreateMasterSecretJsonAsync();

            List<string> schemas = new()
            {
                "{}"
            };

            List<string> credentialDefinitions = new()
            {
                credDefObject.JsonString
            };

            //Act
            Func<Task<string>> func = async () => await PresentationApi.CreatePresentationAsync(
                presReqJson,
                credentialEntries,
                credentialProofs,
                selfAttestNames,
                selfAttestValues,
                masterSecret,
                schemas,
                credentialDefinitions
                );

            //Assert
            _ = await func.Should().ThrowAsync<Exception>();
        }
        #endregion

        #region Tests for CreatePresentationFromJsonAsync
        [Test, TestCase(TestName = "CreatePresentationFromJsonAsync() works.")]
        public async Task CreatePresentationFromJsonAsyncWorks()
        {
            //Arrange
            string presentationJson = "{\"proof\":{\"proofs\":[{\"primary_proof\":{\"eq_proof\":{\"revealed_attrs\":{\"attr\":\"72155939486846849509759369733266486982821795810448245423168957390607644363272\"},\"a_prime\":\"22787947532665702942166390265565099820863799587030609859364101947347429956006178433736182956558680217451360098070400415842618416586813209500267111752188923151298858777205206383545858632930505476551131457277377652820989619031780149993288820195069310942122913730689119829340750366182236664539501596260514894650983980623701649527364749330998285260704338497630597378327025502438431858516849741910716399356638178657965616831942802640713754838111099765928053264003133303245178307245238269379969657844386541201731429479010183162613731023271140443137949920801708151362109823177071910561747744940225633896803721317669782358421\",\"e\":\"69124141745661451232422071833447747082810057151848226659717004723245660321643866206894483809434372484419787675271515845575181681885576485\",\"v\":\"502483187879133716417096480006386205327995711270888126778869121855278753425593998422963838651347769608503283807649516512880075823986911920658063448216074566307723181816414360454851464182162626545924978908908754080967539118089711545270393292866557588250787116479546341674975985851318791108768074114121210355659608509113854257493677851780112011053516065048781059761361613409853017535033453997896416355511480069057755205220463158986451933726626781714019352189004060699441752283027199173688877273426479931600413051842909026732192148129355030099736087987835032879264302949802085989635543207730170481446080695187580698323015118647125807401471446437413140532020293604881927856270725677607294897411796868074908977878774588588258946655011556630640214442071918831131351471397679508616542608467322074771091661348278136342901709870360635291451103144692305647239100786223515212569094457685892676825400988189774258596242950852693022055\",\"m\":{\"master_secret\":\"2307093772458771039711933057912384843500743528411547595991148149387405897463262936047925219917981120668180047862724912654604570684672680548843949939043365133411527271086042486203\"},\"m2\":\"3350007083872569628769486907745210896506907039493766347465000482213910452825573519528617062976602075306102068308540480111571831600569611213574129530137747\"},\"ge_proofs\":[]},\"non_revoc_proof\":{\"x_list\":{\"rho\":\"1C3AC92D0868D6AD9F124B88E0AA66ED2F3BBC2401AE52A4512CDEC68B82078D\",\"r\":\"0D0387C22AA96E9D00AC180BF461BA40809BE312BB0FC57B392B4660F8AC02C9\",\"r_prime\":\"156475DD528D2A7F6A7711479C40A634F76A82A0AD098DEFC957CF500ADDD6F6\",\"r_prime_prime\":\"1262D4B6EE055CB7B12AC9CCC5A8C5551DC1398D925162FD63B19A7E27E47601\",\"r_prime_prime_prime\":\"1FC79BA981C70CF3FDDBEE8F3908C63F195284DA250B9F4BF0B8C444F00A1DA5\",\"o\":\"0645F0F20692FC3EB7A8AC6DF8975475D69046A3A1AA2F9454309976628CC2D6\",\"o_prime\":\"0E17AE4D4A318BE3F22B8741A9CD401EEF8EDF2571234353DE93C71F91AEB868\",\"m\":\"07AA5FC3DC804398D1E7C087241D13E05F9920BB3E72AE903EF9780BEA53F516\",\"m_prime\":\"22F8D76B8EEC5B8E8AB10B00165CDCB1638733CEC1DAA82D8E2D1DF64EAEA8CE\",\"t\":\"10ACE2A8623DCB973DF484C9634FB4DC94BA9097F89ABA3C5863693F9EC5F223\",\"t_prime\":\"236B85805AD43E4EEA0DE3A03A764BF717805EC5AADEEAD2566B485B7CAC81D1\",\"m2\":\"1BA5E259DB355BC7D8A7DB2B76E6CA77F6BF7EFA08D19E7F533836005F6FBAB3\",\"s\":\"0B1339A60E431657FFDACA6D5C79CC964471B7595B4D34562AAD8E4648E78A82\",\"c\":\"16608A5E3549E73F2F6A32D54D8AE8B0B2DF36DDE1410876BFE20287CB069E63\"},\"c_list\":{\"e\":\"6 38732649A4C651FABABE98F14CF92AF6F2217F528540B3AC499D80D35D494055 4 3F966F47E48797FB2CEA9B6318782FA70B77880DABDCAF2E0C0EA99351EA842F 4 30738E44CA85F08F501F3E7D53D0F87DEB42862769EBD8B13D062A2F33432A93\",\"d\":\"6 474DE704E04EA60150D37B39FEF9C87908CA371D20A2C3509864591699723647 4 04180C1A54CFE8B2225F09DF98469B26DFE703D1E3596E9E17DA0596F9364DBA 4 2CD33754BC31C6DFC3D265B10255B6C768AD491EF4858DACF6464C95F449F3AB\",\"a\":\"6 4B639F55DECFBC5C3DCEF78E043F5056702E9A24FF62BEF53D5C658AEC998640 4 1E87C680AE0FE4C144C02C2C6D9BFBB6319C3F68DAD161FEE52FF7C9E3B7A5E4 4 1B3FDF399DD2CE9416CBCF9F2DF371CCDADDD26C60882AE6DF55921A6009D07A\",\"g\":\"6 42184EA20FFB2329573A01CD5DE5BA48968E8C6CC0840931B31B410AEFE66B87 4 30CF73E43C59393CEB521ADE04CBD1268039521080C2282B459C0A110866F3F0 4 1C52F59F00FEE09872C01AEAE0AB76A15AD2F00CCDB99B73BEC14EFB3BE7A75E\",\"w\":\"21 13A9D8DB787AD569B83F83E3E4D85D01C1FD50E61ABB41BBAF62CEAA157033180 21 12C955A1AB9842C62A7A3A023A6A34EA3B5C966780A00F5175B1CD5D25D87A1FF 6 6608A8E039569F56F4221EA7B4BDEF52DC0D9F073728700ADEA2F695F9DB4853 4 1AE01005403C489AC15C9349BC2D8D43B34D3C43D0D828F238DA4F742B35260B 6 52AB4701947804C7F3B17D7F22F35DE50483B588AF74C5EA122F508D2D634B74 4 073118B2C588D05F7DCDB3D8C2EA322663DAA589F081C36A28C6220B990793CB\",\"s\":\"21 11AA1CEE4FB15EEBAA34BF79C5BA75A0D22064288FE8D64CEEBBABF24F6D37903 21 113CE1CE8533D17F54BEC9490B3FD3601CDBFC8B04B0B8948F930EBA3E67C8CFF 6 55814737228E6B608EE44B894863605890FB81528C7A67FC0857B73FD9438F5B 4 309834112A9A043C0AF59BBC2BC47D72A258FDA266237FD780925D7AB9A6F43C 6 79E87F6D2789925E22F89F0E5E86B15E26B1259B2EAEE8A86AAF8A38D724A123 4 1A8ABBC77FABDF0C3F3223D9D81F51481E28967A33636CD598878F4483F6B6CA\",\"u\":\"21 12846FAFA8E2998E260BE514A492CA4101F0206BE1C46293F736BB027CBCD7D66 21 13581B75D34A2413C50178B4337DA5C8B4056639057DCCF2DDE6563E200BCCFC6 6 64EA09AD4A1E3C6BCC7EBFF9C3AAD7FEFDE17D2D12517FB3644C891D91B579FA 4 31C98A8C89C90961D814CC181F6036C810DDFC9C8D62703DBBC0EEFAEB5EE73B 6 720417974AA223B2375B13837CDB3D5CC2542EDBAF7B9192CB414CD40A191DF1 4 0BCDF714766E1EB0CF923BF54D954ABFF39A2BACACE00E4E2B73AA86B420E8F9\"}}}],\"aggregated_proof\":{\"c_hash\":\"57204176382419527374101370361393524337764538734535251618083733237783975439070\",\"c_list\":[[4,15,88,167,248,56,102,147,80,24,31,234,110,102,161,67,43,125,207,87,179,56,8,40,221,242,70,202,231,69,195,248,205,25,33,90,195,219,255,23,52,105,221,133,186,132,167,223,162,68,186,75,179,189,1,65,254,76,96,125,100,40,5,100,103,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[4,24,49,229,206,52,10,42,52,195,227,91,45,94,133,85,241,105,195,178,217,81,190,25,203,16,149,93,186,44,71,119,190,1,95,192,234,30,175,215,56,136,133,100,233,140,112,56,244,95,204,69,128,213,21,233,94,147,116,37,153,242,52,209,138,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[4,36,122,151,5,113,228,166,236,38,129,222,222,49,212,15,96,103,229,3,25,28,79,37,225,177,177,237,134,6,66,27,119,27,109,135,41,214,232,251,152,184,10,32,74,82,207,40,195,103,231,154,242,187,30,231,218,67,106,190,128,121,155,131,113,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[4,34,66,12,235,45,42,171,239,205,121,131,31,41,208,82,246,226,170,49,76,126,165,225,43,128,172,54,126,88,20,128,19,25,223,36,170,76,242,212,148,63,33,235,54,137,148,140,196,40,65,151,62,212,181,189,243,6,88,75,235,151,114,54,232,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[2,220,197,124,183,58,58,157,142,103,86,147,145,21,36,119,22,117,86,200,99,115,154,196,120,116,80,24,138,230,36,177,30,81,86,105,168,250,240,169,14,34,93,141,158,208,228,138,226,86,195,111,234,35,121,40,171,132,129,53,97,162,190,76,17,198,236,135,230,125,232,215,65,239,184,64,2,185,53,15,32,100,138,152,2,23,237,183,77,53,137,194,42,13,147,224,15,251,118,253,30,27,80,73,207,230,147,236,203,187,117,137,203,187,203,51,255,14,254,57,110,87,140,174,202,247,155,13],[6,4,32,235,21,24,190,211,121,184,190,97,209,193,204,252,84,165,232,105,66,242,136,146,55,150,198,158,138,75,9,42,20,94,45,147,73,55,47,29,238,14,37,159,3,138,207,149,38,169,47,201,12,26,72,119,103,113,204,39,9,237,140,171,30,138,231,25,58,230,106,18,39,185,248,42,210,252,84,207,80,156,11,152,33,1,126,84,180,236,37,83,219,45,36,100,8,178,133,234,236,77,114,247,114,116,96,227,221,115,73,209,254,192,44,154,95,138,75,225,140,115,158,153,117,247,46,120],[16,44,243,209,174,98,186,130,29,56,38,87,123,124,238,122,203,120,31,253,232,19,226,74,204,75,162,51,66,66,45,186,9,6,138,241,64,162,56,111,131,25,104,172,138,235,27,138,139,39,130,101,227,74,72,192,159,162,90,203,62,206,169,103,26,216,166,16,244,46,39,151,224,100,180,118,36,16,47,226,176,52,23,161,20,206,124,249,124,111,169,38,165,248,85,186,20,108,129,30,247,163,11,93,67,207,172,13,205,19,137,217,20,51,12,20,189,188,141,172,216,195,210,122,166,225,240,176],[180,131,235,194,141,158,164,112,75,71,68,181,71,3,222,45,131,147,72,120,36,190,183,145,119,59,94,109,178,83,240,117,200,139,183,234,120,1,110,141,136,165,42,78,208,133,128,142,129,225,90,76,185,141,133,255,168,176,156,198,205,174,109,95,175,11,53,125,245,234,195,119,40,130,113,52,218,73,76,203,177,234,76,126,241,151,118,123,75,50,61,64,142,89,4,43,204,101,133,151,134,232,55,175,118,177,0,30,39,220,62,41,191,69,69,176,24,176,190,231,153,46,1,224,26,77,218,181,22,139,97,9,101,122,140,126,141,227,30,235,59,32,28,156,131,249,221,32,2,105,209,58,36,113,168,135,218,59,229,111,101,184,187,104,186,9,42,160,169,62,171,119,200,240,34,22,132,162,120,81,95,241,137,102,7,179,13,132,28,103,203,185,8,79,241,36,7,197,92,180,229,209,207,178,255,134,226,94,183,236,213,184,164,53,139,255,206,186,94,215,173,161,15,160,235,255,163,132,248,68,184,47,160,1,57,39,255,49,113,237,48,199,71,242,219,32,146,28,55,139,17,40,149,67,9,149]]}},\"requested_proof\":{\"revealed_attrs\":{\"reft\":{\"sub_proof_index\":0,\"raw\":\"test\",\"encoded\":\"72155939486846849509759369733266486982821795810448245423168957390607644363272\"}},\"self_attested_attrs\":{},\"unrevealed_attrs\":{},\"predicates\":{}},\"identifiers\":[{\"schema_id\":\"55GkHamhTU1ZbTbV2ab9DE:2:schema name:schema version\",\"cred_def_id\":\"55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag\",\"rev_reg_id\":\"55GkHamhTU1ZbTbV2ab9DE:4:55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag:CL_ACCUM:default\",\"timestamp\":1656323735}]}";

            //Act
            Presentation actual = await PresentationApi.CreatePresentationFromJsonAsync(presentationJson);

            //Assert
            _ = actual.Should().BeOfType<Presentation>();
            _ = actual.Identifiers[0].CredentialDefinitionId.Should().Be("55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag");
        }

        [Test, TestCase(TestName = "CreatePresentationFromJsonAsync() throws IndexOutOfRangeException when given empty string.")]
        public async Task CreatePresentationFromJsonAsyncThrowsIndexOutOfBoundsException()
        {
            //Arrange
            string presentationJson = "";

            //Act
            Func<Task> act = async () => await PresentationApi.CreatePresentationFromJsonAsync(presentationJson);

            //Assert
            _ = await act.Should().ThrowAsync<IndexOutOfRangeException>();
        }

        [Test, TestCase(TestName = "CreatePresentationFromJsonAsync() throws SharedRsException when given invalid json string.")]
        public async Task CreatePresentationFromJsonAsyncThrowsSharedRsException()
        {
            //Arrange
            string presentationJson = "{}";

            //Act
            Func<Task> act = async () => await PresentationApi.CreatePresentationFromJsonAsync(presentationJson);

            //Assert
            _ = await act.Should().ThrowAsync<SharedRsException>();
        }
        #endregion

        #region Tests for VerifyPresentationAsync
        [Test, TestCase(TestName = "VerifyPresentationAsync() works.")]
        public async Task VerifyPresentationWorks()
        {
            //Arrange
            string nonce = "75097235631617494585946";
            long timestamp = 1656323735;
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string testTailsPathForRevocation = null;

            Schema schemaObject = await SchemaApi.CreateSchemaFromJsonAsync("{" +
               "\"ver\": \"1.0\"," +
               "\"id\": \"55GkHamhTU1ZbTbV2ab9DE:2:schema name:schema version\"," +
               "\"name\": \"schema name\"," +
               "\"version\": \"schema version\"," +
               "\"attrNames\": [\"attr\"]," +
               "\"seqNo\": 15" +
               "}");
            List<Schema> schemas = new() { schemaObject };

            string presReqJson = "{" +
            "\"name\": \"proof\"," +
            "\"version\": \"1.0\", " +
            $"\"nonce\": \"{nonce}\"," +
            "\"requested_attributes\": " +
                "{" +
                    "\"reft\": " +
                    "{" +
                        "\"name\": \"attr\"," +
                        "\"non_revoked\": " +
                        "{" +
                            $"\"from\": {timestamp}, " +
                            $"\"to\": {timestamp}" +
                        "}" +
                    "}" +
                "}," +
            "\"requested_predicates\": { }," +
            "\"ver\": \"1.0\"" +
            "}";
            PresentationRequest presReqObject = await PresentationRequestApi.CreatePresReqFromJsonAsync(presReqJson);

            string credDefJson = "{\"ver\":\"1.0\",\"id\":\"55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag\",\"schemaId\":\"15\",\"type\":\"CL\",\"tag\":\"tag\",\"value\":{\"primary\":{\"n\":\"84487835551039032285187155154911060483660350222398883306413615732874461803883807049444869314357050156687890484696882752158866570703691960502013865653643690888821722220736405782919735960755230175042942119509787097308421567876300005147107204196026086261303918012289983042328204552430813959949302734869806656279874589793840951290632815831311761788019595465070212590341317127385075800049878207575128216810613429868614248865074547315292251284915159151999933871168916872594390167708391421621228425918116044537974671504192144649804765393008843077539872571691651675561911758354029794542551288272701560143399651052167087750481\",\"s\":\"28558128648758027909305904351171703188330099292924797248574842793109489507264016530475517567631632922816270447442674993775079076136342650057568741312479074313908686874475078654401350487691696926149271362294936274559242219859505383427715620999464298355003971210313002128345321767575959219528532430269155639274747587292593452207105673695263222521323277842616410349603290362664657370067179177493187725056366137274260699343615520306336032240972903155558389304716505008244368995276114740488732419322360582041616827281207709706369246942275508659481785003071817812195736581875859513370666043191942935979341546224669755726592\",\"r\":{\"attr\":\"19374635229243697947050097026914899831804151710623390439699875709053261437490979212259490718039116411407119344652902355943963171792218723086630516063841949623590129382828843769738786513710334176054037579338207410777516896932526759088769893402563183018185612215867340692245277462458139565495698429412757356575226491664765036788474249115006091029994257607576855491435914614697429367514578757816940487787493706573109304351402336200840439292576613554345366341329612250384952870403073130659832480298734047175262840687349916307540764272919838633325633665460254326264312598782859300055717928967069882262403842787394139435894\",\"master_secret\":\"81622178607923918364721456204298773176565035371329841505071999320161445715140470066338770173307831504322432335318816064336064526133483910228038661838624698618588845927308454293473291897151646838241523303375565952307736856063762202291448059793289978319987542894438432492616678253629520076550282854666371806570750552098795289963378158905480562249765636399463659535901314787140796754551215402990460593947771202214698867632223596467677921849056698450129899546703013955829498161920514483760182816658783824691036213919954445215806393456079532166137528830442865329317066684497704520867308483692017792380043666972369996476648\"},\"rctxt\":\"12146311402700730909210821007084758312612633902400591088560201782774404663232896787734230361129748004241058753273490601823770520400256720606314268792396336044245755238750374415517749168755873962734625982190385924909156182838280338018890559958648961135704419623925386696159082001415714738757173419135327834089658501956546412643981617383898868811945190720171479924941322302800590599119428117660679419757785001036588261888810604510413383924652834880650784571036955954104556885805909446788668012071619521870517934764185543864410726697064950585970364606527370867045042988632693291566416288920563963182217099152392920623981\",\"z\":\"8659484373005269980033082471592982329289449003147153823387929755264589920438261740054893267116254145809205967722170852206137509022326400590521422584318195793318452699121075733292345177492016943238125091931931097404226756579606524593225823005706035955720119333209333659906090001548030910641905069127435973870268228855554477785824268588565396535099976521044309399187732772283806196540898387710340281460301731792116774708748625316697733261016999865199755587315775825661081423595514414352371280406326503074363945445577704946310124460634120397951995477382974597897391473236827620740751902305208021365042055236954223606712\"},\"revocation\":{\"g\":\"1 147F164871D36240487873028DC022B8B2D72551285179B6BA1963709B809E98 1 15B52C24CDDB4BF0B91AB9299C307970DA2039F99412370C7F4F10B5FA442CE4 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"g_dash\":\"1 010272B8327846397255FC5F6634ABF2A4838A907DC4E85CA41733F100C4C045 1 1E50A2C2046BEB577CCA2BF3E77377EA4539915C4E2631F16533535DBBB59078 1 19E0F2F978B69328250B646FB4E1C3761CB8505B0C051C16F0C62ADA10AC1EDE 1 24597FEEA245D53B3EBE6A3F21654ECE28FA6F4A4525B502D52A21C4D051E716 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8 1 0000000000000000000000000000000000000000000000000000000000000000\",\"h\":\"1 23C97E94D95C57F8ACF2E78E8860651DC4A5955D8FE75E20FA663AD3FBA088B3 1 0799BB348CAFD085E13174FE165E83C0672FF5FF3CF71A39F6684F35547E96F3 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"h0\":\"1 0D637EA93C50CE193DF1BF616334EFD6A41D4339912B68138E334F408B380387 1 0F3D675892139E06CD5A492B12FC9E859B4CA2674D976D6437CBFF3D7B380312 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"h1\":\"1 1F4DD6589D2D318AB752112A7631B23E507F33A45CE73CE1719038CE4CDDD7F0 1 0A2502C73B9DA105316BFA81BFA6EA9763B4E4BF85E8034936929A7DF9EB6202 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"h2\":\"1 1D3AD4848CEEEDAB17FDCF1AB2628375B0ACBB67FB5712D54BF52636DE050C49 1 11AB2ECB1FBA088AF2E7F19B06B19EDF12738DE62C42E5D2E5493A19CA231C7D 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"htilde\":\"1 04641618E8A51FE5266A70BECBAD852B9974250006E7083C157FBA1288004ABD 1 177B3E0114876224103FD395110C0F90A640E299352DB06853D76DB4E35B4172 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"h_cap\":\"1 142C106B302E4125072211F1A65E61B4335F39558E014523B406EBF0D964EC37 1 06EA720DCA4DA1CEE3E614E5DC14DC8C3B052D2B7B2F30881CAFCED4B08864F0 1 000B8EC003C2AE9CB68E11134E0ACAD8402CD0A968FF7C9AF0434BAA79D3E697 1 054FB7273A3E828DB80DEDA4F7821989F32AC082E935C5099876F6B31F953075 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8 1 0000000000000000000000000000000000000000000000000000000000000000\",\"u\":\"1 0FD51468053FD3188BE914065D6AACD9978B44F9CC594161C6B1BE6D6DEE28C2 1 23A9E07A6CFAF228C641FD4A6540E61599B8B66A18F44C89AB88E4AF529ABA5C 1 09B8D02402F8964A2103DB926C4E7AB0003127E6E622EE162B058C10B41C9148 1 13CBFA6415CFFAC0E834D769F1DDED9B0BB978AB3BA78A7A8608889995E1125B 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8 1 0000000000000000000000000000000000000000000000000000000000000000\",\"pk\":\"1 0F8B6D1969C96B11AC19D290242053EB870A69FBF488ED7B44BB040C921542F2 1 0563DDC59EE0530BCC8B3A73D1BE60403993E9476E2C15B7D1C6F2B3DF04A91E 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"y\":\"1 149AB72F3F5D2F5A2486EC0A95B35126923F0F846F7DB6952C2BA305CF9253BA 1 0F0FDFA8FD278190F1636CD0FA703A4EF94B8CDA5ADD5967A4C57E872919DF1F 1 1E363496C15D2A8E3BDA3343929630A3517040F0A056A578534BFF9BF2D8411C 1 23DABD532DF8CEC9FB3B0BFA3C13F0C28E8B0F1947702C05DDBB9E0C56C0FBAE 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8 1 0000000000000000000000000000000000000000000000000000000000000000\"}}}";
            CredentialDefinition credDefObject = await CredentialDefinitionApi.CreateCredentialDefinitionFromJsonAsync(credDefJson);
            List<CredentialDefinition> credentialDefinitions = new() { credDefObject };

            (RevocationRegistryDefinition tmpRevRegDefObject,
                _,
                _,
                _) =
                await RevocationApi.CreateRevocationRegistryAsync(
                    issuerDid,
                    credDefObject,
                    "tag",
                    RegistryType.CL_ACCUM,
                    IssuerType.ISSUANCE_BY_DEFAULT,
                    100,
                    testTailsPathForRevocation);
            string revRegDefJson = "{\"ver\":\"1.0\",\"id\":\"55GkHamhTU1ZbTbV2ab9DE:4:55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag:CL_ACCUM:default\",\"revocDefType\":\"CL_ACCUM\",\"tag\":\"default\",\"credDefId\":\"55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag\",\"value\":{\"issuanceType\":\"ISSUANCE_BY_DEFAULT\",\"maxCredNum\":100,\"publicKeys\":{\"accumKey\":{\"z\":\"1 191190AD6D246A0C08A3FD861111646892A92C95D693BD575D3BB7EA2B4DDCB8 1 1569C1150D398CF5D5681C3DFD5DEC4FD2AF4A9FACA1726DA228C9613F0F2A55 1 2461A8627CC49731DF8B333D18427B52DD50BCB0B726AC2CB85706AC2706F102 1 1B5BB5DBA793F19DD8CF3D0FE70AEFBFAFFB5EA3725FC518B50FECC0D3FA7C3B 1 0033682B8C64D970748A23913E7795931D095C10347999CB9D56AB2A1388645F 1 118B0D9DB80D569A0A66E8BA4C26C684BE300D8A43A126C1C59364A649837E00 1 1DFE562E117B1D0986FA749A34311217A29B8CE990390EE474223AA3BB6B5484 1 19E244A5D7CB172B43E03692F23A53E3CD05318CB653BE479CE6CE6BAE71C800 1 1F410C423EC051272438A187D417C6863BD1DAA252B5CE3A77D83D64F3606C41 1 08A422C160DCD70AD18AA1346FB1799309698F52B99280512C7F0AB6D80CD8DD 1 1F59F3BE1CDEA1A3D7A2BB6C4CDBE7E332D29F788A210DF136BB18CB15CBAD06 1 07A871CCC542447BE3398F31570DBA1B366E68C56F1D0141D1392FB0BE76DFFF\"}},\"tailsHash\":\"4kWpJmEMbyukHeLyV3AiounJRH22aaj2kHbQB742Ut6S\",\"tailsLocation\":\"" +
                $"{tmpRevRegDefObject.Value.TailsLocation.Replace("\\", "/")}" +
                "\"}}";
            RevocationRegistryDefinition revRegDefObject = await RevocationApi.CreateRevocationRegistryDefinitionFromJsonAsync(revRegDefJson);
            List<RevocationRegistryDefinition> revRegDefinitions = new() { revRegDefObject };

            string revRegJson = "{\"ver\":\"1.0\",\"value\":{\"accum\":\"21 12A1996B8EBA47C9E7CEA41963DF05F99FDB6E9B381B7C8C34BFD66BE01D1C04B 21 13FB3011CFC5A3BF41923A7F9FA3FA9A89F7169738370CB3386F3E4F4CB9FF29D 6 61492CFCB58CB8F4A17D1F5880C88FCBD6DE1348E05EC7692ACEB4C00592FED5 4 3256E2C0799403B0320EF693D67F48922A6E3B233766AE952CBE516F67F27DE8 6 876F21FBF7A1EE72C77DDE8D462C4195D0DD6CB4E3C76E9AD3948D38956DB6C7 4 26EED3F0DA48A5D5187FDA236C4B8FDAB6DF072CA7C1D18B4F6BFD81D72BE731\"}}";
            RevocationRegistry revRegObject = await RevocationApi.CreateRevocationRegistryFromJsonAsync(revRegJson);
            List<RevocationRegistryEntry> revRegistries = new()
            {
                new RevocationRegistryEntry
                {
                    DefEntryIdx = 0,
                    Entry = revRegObject.Handle,
                    Timestamp = timestamp
                }
            };

            string presentationJson = "{\"proof\":{\"proofs\":[{\"primary_proof\":{\"eq_proof\":{\"revealed_attrs\":{\"attr\":\"72155939486846849509759369733266486982821795810448245423168957390607644363272\"},\"a_prime\":\"22787947532665702942166390265565099820863799587030609859364101947347429956006178433736182956558680217451360098070400415842618416586813209500267111752188923151298858777205206383545858632930505476551131457277377652820989619031780149993288820195069310942122913730689119829340750366182236664539501596260514894650983980623701649527364749330998285260704338497630597378327025502438431858516849741910716399356638178657965616831942802640713754838111099765928053264003133303245178307245238269379969657844386541201731429479010183162613731023271140443137949920801708151362109823177071910561747744940225633896803721317669782358421\",\"e\":\"69124141745661451232422071833447747082810057151848226659717004723245660321643866206894483809434372484419787675271515845575181681885576485\",\"v\":\"502483187879133716417096480006386205327995711270888126778869121855278753425593998422963838651347769608503283807649516512880075823986911920658063448216074566307723181816414360454851464182162626545924978908908754080967539118089711545270393292866557588250787116479546341674975985851318791108768074114121210355659608509113854257493677851780112011053516065048781059761361613409853017535033453997896416355511480069057755205220463158986451933726626781714019352189004060699441752283027199173688877273426479931600413051842909026732192148129355030099736087987835032879264302949802085989635543207730170481446080695187580698323015118647125807401471446437413140532020293604881927856270725677607294897411796868074908977878774588588258946655011556630640214442071918831131351471397679508616542608467322074771091661348278136342901709870360635291451103144692305647239100786223515212569094457685892676825400988189774258596242950852693022055\",\"m\":{\"master_secret\":\"2307093772458771039711933057912384843500743528411547595991148149387405897463262936047925219917981120668180047862724912654604570684672680548843949939043365133411527271086042486203\"},\"m2\":\"3350007083872569628769486907745210896506907039493766347465000482213910452825573519528617062976602075306102068308540480111571831600569611213574129530137747\"},\"ge_proofs\":[]},\"non_revoc_proof\":{\"x_list\":{\"rho\":\"1C3AC92D0868D6AD9F124B88E0AA66ED2F3BBC2401AE52A4512CDEC68B82078D\",\"r\":\"0D0387C22AA96E9D00AC180BF461BA40809BE312BB0FC57B392B4660F8AC02C9\",\"r_prime\":\"156475DD528D2A7F6A7711479C40A634F76A82A0AD098DEFC957CF500ADDD6F6\",\"r_prime_prime\":\"1262D4B6EE055CB7B12AC9CCC5A8C5551DC1398D925162FD63B19A7E27E47601\",\"r_prime_prime_prime\":\"1FC79BA981C70CF3FDDBEE8F3908C63F195284DA250B9F4BF0B8C444F00A1DA5\",\"o\":\"0645F0F20692FC3EB7A8AC6DF8975475D69046A3A1AA2F9454309976628CC2D6\",\"o_prime\":\"0E17AE4D4A318BE3F22B8741A9CD401EEF8EDF2571234353DE93C71F91AEB868\",\"m\":\"07AA5FC3DC804398D1E7C087241D13E05F9920BB3E72AE903EF9780BEA53F516\",\"m_prime\":\"22F8D76B8EEC5B8E8AB10B00165CDCB1638733CEC1DAA82D8E2D1DF64EAEA8CE\",\"t\":\"10ACE2A8623DCB973DF484C9634FB4DC94BA9097F89ABA3C5863693F9EC5F223\",\"t_prime\":\"236B85805AD43E4EEA0DE3A03A764BF717805EC5AADEEAD2566B485B7CAC81D1\",\"m2\":\"1BA5E259DB355BC7D8A7DB2B76E6CA77F6BF7EFA08D19E7F533836005F6FBAB3\",\"s\":\"0B1339A60E431657FFDACA6D5C79CC964471B7595B4D34562AAD8E4648E78A82\",\"c\":\"16608A5E3549E73F2F6A32D54D8AE8B0B2DF36DDE1410876BFE20287CB069E63\"},\"c_list\":{\"e\":\"6 38732649A4C651FABABE98F14CF92AF6F2217F528540B3AC499D80D35D494055 4 3F966F47E48797FB2CEA9B6318782FA70B77880DABDCAF2E0C0EA99351EA842F 4 30738E44CA85F08F501F3E7D53D0F87DEB42862769EBD8B13D062A2F33432A93\",\"d\":\"6 474DE704E04EA60150D37B39FEF9C87908CA371D20A2C3509864591699723647 4 04180C1A54CFE8B2225F09DF98469B26DFE703D1E3596E9E17DA0596F9364DBA 4 2CD33754BC31C6DFC3D265B10255B6C768AD491EF4858DACF6464C95F449F3AB\",\"a\":\"6 4B639F55DECFBC5C3DCEF78E043F5056702E9A24FF62BEF53D5C658AEC998640 4 1E87C680AE0FE4C144C02C2C6D9BFBB6319C3F68DAD161FEE52FF7C9E3B7A5E4 4 1B3FDF399DD2CE9416CBCF9F2DF371CCDADDD26C60882AE6DF55921A6009D07A\",\"g\":\"6 42184EA20FFB2329573A01CD5DE5BA48968E8C6CC0840931B31B410AEFE66B87 4 30CF73E43C59393CEB521ADE04CBD1268039521080C2282B459C0A110866F3F0 4 1C52F59F00FEE09872C01AEAE0AB76A15AD2F00CCDB99B73BEC14EFB3BE7A75E\",\"w\":\"21 13A9D8DB787AD569B83F83E3E4D85D01C1FD50E61ABB41BBAF62CEAA157033180 21 12C955A1AB9842C62A7A3A023A6A34EA3B5C966780A00F5175B1CD5D25D87A1FF 6 6608A8E039569F56F4221EA7B4BDEF52DC0D9F073728700ADEA2F695F9DB4853 4 1AE01005403C489AC15C9349BC2D8D43B34D3C43D0D828F238DA4F742B35260B 6 52AB4701947804C7F3B17D7F22F35DE50483B588AF74C5EA122F508D2D634B74 4 073118B2C588D05F7DCDB3D8C2EA322663DAA589F081C36A28C6220B990793CB\",\"s\":\"21 11AA1CEE4FB15EEBAA34BF79C5BA75A0D22064288FE8D64CEEBBABF24F6D37903 21 113CE1CE8533D17F54BEC9490B3FD3601CDBFC8B04B0B8948F930EBA3E67C8CFF 6 55814737228E6B608EE44B894863605890FB81528C7A67FC0857B73FD9438F5B 4 309834112A9A043C0AF59BBC2BC47D72A258FDA266237FD780925D7AB9A6F43C 6 79E87F6D2789925E22F89F0E5E86B15E26B1259B2EAEE8A86AAF8A38D724A123 4 1A8ABBC77FABDF0C3F3223D9D81F51481E28967A33636CD598878F4483F6B6CA\",\"u\":\"21 12846FAFA8E2998E260BE514A492CA4101F0206BE1C46293F736BB027CBCD7D66 21 13581B75D34A2413C50178B4337DA5C8B4056639057DCCF2DDE6563E200BCCFC6 6 64EA09AD4A1E3C6BCC7EBFF9C3AAD7FEFDE17D2D12517FB3644C891D91B579FA 4 31C98A8C89C90961D814CC181F6036C810DDFC9C8D62703DBBC0EEFAEB5EE73B 6 720417974AA223B2375B13837CDB3D5CC2542EDBAF7B9192CB414CD40A191DF1 4 0BCDF714766E1EB0CF923BF54D954ABFF39A2BACACE00E4E2B73AA86B420E8F9\"}}}],\"aggregated_proof\":{\"c_hash\":\"57204176382419527374101370361393524337764538734535251618083733237783975439070\",\"c_list\":[[4,15,88,167,248,56,102,147,80,24,31,234,110,102,161,67,43,125,207,87,179,56,8,40,221,242,70,202,231,69,195,248,205,25,33,90,195,219,255,23,52,105,221,133,186,132,167,223,162,68,186,75,179,189,1,65,254,76,96,125,100,40,5,100,103,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[4,24,49,229,206,52,10,42,52,195,227,91,45,94,133,85,241,105,195,178,217,81,190,25,203,16,149,93,186,44,71,119,190,1,95,192,234,30,175,215,56,136,133,100,233,140,112,56,244,95,204,69,128,213,21,233,94,147,116,37,153,242,52,209,138,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[4,36,122,151,5,113,228,166,236,38,129,222,222,49,212,15,96,103,229,3,25,28,79,37,225,177,177,237,134,6,66,27,119,27,109,135,41,214,232,251,152,184,10,32,74,82,207,40,195,103,231,154,242,187,30,231,218,67,106,190,128,121,155,131,113,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[4,34,66,12,235,45,42,171,239,205,121,131,31,41,208,82,246,226,170,49,76,126,165,225,43,128,172,54,126,88,20,128,19,25,223,36,170,76,242,212,148,63,33,235,54,137,148,140,196,40,65,151,62,212,181,189,243,6,88,75,235,151,114,54,232,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[2,220,197,124,183,58,58,157,142,103,86,147,145,21,36,119,22,117,86,200,99,115,154,196,120,116,80,24,138,230,36,177,30,81,86,105,168,250,240,169,14,34,93,141,158,208,228,138,226,86,195,111,234,35,121,40,171,132,129,53,97,162,190,76,17,198,236,135,230,125,232,215,65,239,184,64,2,185,53,15,32,100,138,152,2,23,237,183,77,53,137,194,42,13,147,224,15,251,118,253,30,27,80,73,207,230,147,236,203,187,117,137,203,187,203,51,255,14,254,57,110,87,140,174,202,247,155,13],[6,4,32,235,21,24,190,211,121,184,190,97,209,193,204,252,84,165,232,105,66,242,136,146,55,150,198,158,138,75,9,42,20,94,45,147,73,55,47,29,238,14,37,159,3,138,207,149,38,169,47,201,12,26,72,119,103,113,204,39,9,237,140,171,30,138,231,25,58,230,106,18,39,185,248,42,210,252,84,207,80,156,11,152,33,1,126,84,180,236,37,83,219,45,36,100,8,178,133,234,236,77,114,247,114,116,96,227,221,115,73,209,254,192,44,154,95,138,75,225,140,115,158,153,117,247,46,120],[16,44,243,209,174,98,186,130,29,56,38,87,123,124,238,122,203,120,31,253,232,19,226,74,204,75,162,51,66,66,45,186,9,6,138,241,64,162,56,111,131,25,104,172,138,235,27,138,139,39,130,101,227,74,72,192,159,162,90,203,62,206,169,103,26,216,166,16,244,46,39,151,224,100,180,118,36,16,47,226,176,52,23,161,20,206,124,249,124,111,169,38,165,248,85,186,20,108,129,30,247,163,11,93,67,207,172,13,205,19,137,217,20,51,12,20,189,188,141,172,216,195,210,122,166,225,240,176],[180,131,235,194,141,158,164,112,75,71,68,181,71,3,222,45,131,147,72,120,36,190,183,145,119,59,94,109,178,83,240,117,200,139,183,234,120,1,110,141,136,165,42,78,208,133,128,142,129,225,90,76,185,141,133,255,168,176,156,198,205,174,109,95,175,11,53,125,245,234,195,119,40,130,113,52,218,73,76,203,177,234,76,126,241,151,118,123,75,50,61,64,142,89,4,43,204,101,133,151,134,232,55,175,118,177,0,30,39,220,62,41,191,69,69,176,24,176,190,231,153,46,1,224,26,77,218,181,22,139,97,9,101,122,140,126,141,227,30,235,59,32,28,156,131,249,221,32,2,105,209,58,36,113,168,135,218,59,229,111,101,184,187,104,186,9,42,160,169,62,171,119,200,240,34,22,132,162,120,81,95,241,137,102,7,179,13,132,28,103,203,185,8,79,241,36,7,197,92,180,229,209,207,178,255,134,226,94,183,236,213,184,164,53,139,255,206,186,94,215,173,161,15,160,235,255,163,132,248,68,184,47,160,1,57,39,255,49,113,237,48,199,71,242,219,32,146,28,55,139,17,40,149,67,9,149]]}},\"requested_proof\":{\"revealed_attrs\":{\"reft\":{\"sub_proof_index\":0,\"raw\":\"test\",\"encoded\":\"72155939486846849509759369733266486982821795810448245423168957390607644363272\"}},\"self_attested_attrs\":{},\"unrevealed_attrs\":{},\"predicates\":{}},\"identifiers\":[{\"schema_id\":\"55GkHamhTU1ZbTbV2ab9DE:2:schema name:schema version\",\"cred_def_id\":\"55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag\",\"rev_reg_id\":\"55GkHamhTU1ZbTbV2ab9DE:4:55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag:CL_ACCUM:default\",\"timestamp\":1656323735}]}";
            Presentation presentationObject = await PresentationApi.CreatePresentationFromJsonAsync(presentationJson);

            //Act
            bool actual = await PresentationApi.VerifyPresentationAsync(
                presentationObject,
                presReqObject,
                schemas,
                credentialDefinitions,
                revRegDefinitions,
                revRegistries);

            //Assert
            _ = actual.Should().BeTrue();
        }

        [Test, TestCase(TestName = "VerifyPresentationAsync() throws.")]
        public async Task VerifyPresentationThrows()
        {
            //Arrange
            string nonce = "75097235631617494585946";
            long timestamp = 1656323735;
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string testTailsPathForRevocation = null;

            Schema schemaObject = await SchemaApi.CreateSchemaFromJsonAsync("{" +
               "\"ver\": \"1.0\"," +
               "\"id\": \"55GkHamhTU1ZbTbV2ab9DE:2:schema name:schema version\"," +
               "\"name\": \"schema name\"," +
               "\"version\": \"schema version\"," +
               "\"attrNames\": [\"attr\"]," +
               "\"seqNo\": 15" +
               "}");
            List<Schema> schemas = new() { schemaObject };

            string presReqJson = "{" +
            "\"name\": \"proof\"," +
            "\"version\": \"1.0\", " +
            $"\"nonce\": \"{nonce}\"," +
            "\"requested_attributes\": " +
                "{" +
                    "\"reft\": " +
                    "{" +
                        "\"name\": \"attr\"," +
                        "\"non_revoked\": " +
                        "{" +
                            $"\"from\": {timestamp}, " +
                            $"\"to\": {timestamp}" +
                        "}" +
                    "}" +
                "}," +
            "\"requested_predicates\": { }," +
            "\"ver\": \"1.0\"" +
            "}";
            PresentationRequest presReqObject = await PresentationRequestApi.CreatePresReqFromJsonAsync(presReqJson);

            string credDefJson = "{\"ver\":\"1.0\",\"id\":\"55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag\",\"schemaId\":\"15\",\"type\":\"CL\",\"tag\":\"tag\",\"value\":{\"primary\":{\"n\":\"84487835551039032285187155154911060483660350222398883306413615732874461803883807049444869314357050156687890484696882752158866570703691960502013865653643690888821722220736405782919735960755230175042942119509787097308421567876300005147107204196026086261303918012289983042328204552430813959949302734869806656279874589793840951290632815831311761788019595465070212590341317127385075800049878207575128216810613429868614248865074547315292251284915159151999933871168916872594390167708391421621228425918116044537974671504192144649804765393008843077539872571691651675561911758354029794542551288272701560143399651052167087750481\",\"s\":\"28558128648758027909305904351171703188330099292924797248574842793109489507264016530475517567631632922816270447442674993775079076136342650057568741312479074313908686874475078654401350487691696926149271362294936274559242219859505383427715620999464298355003971210313002128345321767575959219528532430269155639274747587292593452207105673695263222521323277842616410349603290362664657370067179177493187725056366137274260699343615520306336032240972903155558389304716505008244368995276114740488732419322360582041616827281207709706369246942275508659481785003071817812195736581875859513370666043191942935979341546224669755726592\",\"r\":{\"attr\":\"19374635229243697947050097026914899831804151710623390439699875709053261437490979212259490718039116411407119344652902355943963171792218723086630516063841949623590129382828843769738786513710334176054037579338207410777516896932526759088769893402563183018185612215867340692245277462458139565495698429412757356575226491664765036788474249115006091029994257607576855491435914614697429367514578757816940487787493706573109304351402336200840439292576613554345366341329612250384952870403073130659832480298734047175262840687349916307540764272919838633325633665460254326264312598782859300055717928967069882262403842787394139435894\",\"master_secret\":\"81622178607923918364721456204298773176565035371329841505071999320161445715140470066338770173307831504322432335318816064336064526133483910228038661838624698618588845927308454293473291897151646838241523303375565952307736856063762202291448059793289978319987542894438432492616678253629520076550282854666371806570750552098795289963378158905480562249765636399463659535901314787140796754551215402990460593947771202214698867632223596467677921849056698450129899546703013955829498161920514483760182816658783824691036213919954445215806393456079532166137528830442865329317066684497704520867308483692017792380043666972369996476648\"},\"rctxt\":\"12146311402700730909210821007084758312612633902400591088560201782774404663232896787734230361129748004241058753273490601823770520400256720606314268792396336044245755238750374415517749168755873962734625982190385924909156182838280338018890559958648961135704419623925386696159082001415714738757173419135327834089658501956546412643981617383898868811945190720171479924941322302800590599119428117660679419757785001036588261888810604510413383924652834880650784571036955954104556885805909446788668012071619521870517934764185543864410726697064950585970364606527370867045042988632693291566416288920563963182217099152392920623981\",\"z\":\"8659484373005269980033082471592982329289449003147153823387929755264589920438261740054893267116254145809205967722170852206137509022326400590521422584318195793318452699121075733292345177492016943238125091931931097404226756579606524593225823005706035955720119333209333659906090001548030910641905069127435973870268228855554477785824268588565396535099976521044309399187732772283806196540898387710340281460301731792116774708748625316697733261016999865199755587315775825661081423595514414352371280406326503074363945445577704946310124460634120397951995477382974597897391473236827620740751902305208021365042055236954223606712\"},\"revocation\":{\"g\":\"1 147F164871D36240487873028DC022B8B2D72551285179B6BA1963709B809E98 1 15B52C24CDDB4BF0B91AB9299C307970DA2039F99412370C7F4F10B5FA442CE4 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"g_dash\":\"1 010272B8327846397255FC5F6634ABF2A4838A907DC4E85CA41733F100C4C045 1 1E50A2C2046BEB577CCA2BF3E77377EA4539915C4E2631F16533535DBBB59078 1 19E0F2F978B69328250B646FB4E1C3761CB8505B0C051C16F0C62ADA10AC1EDE 1 24597FEEA245D53B3EBE6A3F21654ECE28FA6F4A4525B502D52A21C4D051E716 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8 1 0000000000000000000000000000000000000000000000000000000000000000\",\"h\":\"1 23C97E94D95C57F8ACF2E78E8860651DC4A5955D8FE75E20FA663AD3FBA088B3 1 0799BB348CAFD085E13174FE165E83C0672FF5FF3CF71A39F6684F35547E96F3 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"h0\":\"1 0D637EA93C50CE193DF1BF616334EFD6A41D4339912B68138E334F408B380387 1 0F3D675892139E06CD5A492B12FC9E859B4CA2674D976D6437CBFF3D7B380312 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"h1\":\"1 1F4DD6589D2D318AB752112A7631B23E507F33A45CE73CE1719038CE4CDDD7F0 1 0A2502C73B9DA105316BFA81BFA6EA9763B4E4BF85E8034936929A7DF9EB6202 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"h2\":\"1 1D3AD4848CEEEDAB17FDCF1AB2628375B0ACBB67FB5712D54BF52636DE050C49 1 11AB2ECB1FBA088AF2E7F19B06B19EDF12738DE62C42E5D2E5493A19CA231C7D 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"htilde\":\"1 04641618E8A51FE5266A70BECBAD852B9974250006E7083C157FBA1288004ABD 1 177B3E0114876224103FD395110C0F90A640E299352DB06853D76DB4E35B4172 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"h_cap\":\"1 142C106B302E4125072211F1A65E61B4335F39558E014523B406EBF0D964EC37 1 06EA720DCA4DA1CEE3E614E5DC14DC8C3B052D2B7B2F30881CAFCED4B08864F0 1 000B8EC003C2AE9CB68E11134E0ACAD8402CD0A968FF7C9AF0434BAA79D3E697 1 054FB7273A3E828DB80DEDA4F7821989F32AC082E935C5099876F6B31F953075 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8 1 0000000000000000000000000000000000000000000000000000000000000000\",\"u\":\"1 0FD51468053FD3188BE914065D6AACD9978B44F9CC594161C6B1BE6D6DEE28C2 1 23A9E07A6CFAF228C641FD4A6540E61599B8B66A18F44C89AB88E4AF529ABA5C 1 09B8D02402F8964A2103DB926C4E7AB0003127E6E622EE162B058C10B41C9148 1 13CBFA6415CFFAC0E834D769F1DDED9B0BB978AB3BA78A7A8608889995E1125B 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8 1 0000000000000000000000000000000000000000000000000000000000000000\",\"pk\":\"1 0F8B6D1969C96B11AC19D290242053EB870A69FBF488ED7B44BB040C921542F2 1 0563DDC59EE0530BCC8B3A73D1BE60403993E9476E2C15B7D1C6F2B3DF04A91E 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"y\":\"1 149AB72F3F5D2F5A2486EC0A95B35126923F0F846F7DB6952C2BA305CF9253BA 1 0F0FDFA8FD278190F1636CD0FA703A4EF94B8CDA5ADD5967A4C57E872919DF1F 1 1E363496C15D2A8E3BDA3343929630A3517040F0A056A578534BFF9BF2D8411C 1 23DABD532DF8CEC9FB3B0BFA3C13F0C28E8B0F1947702C05DDBB9E0C56C0FBAE 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8 1 0000000000000000000000000000000000000000000000000000000000000000\"}}}";
            CredentialDefinition credDefObject = await CredentialDefinitionApi.CreateCredentialDefinitionFromJsonAsync(credDefJson);
            List<CredentialDefinition> credentialDefinitions = new() { credDefObject };

            (RevocationRegistryDefinition tmpRevRegDefObject,
                _,
                _,
                _) =
                await RevocationApi.CreateRevocationRegistryAsync(
                    issuerDid,
                    credDefObject,
                    "tag",
                    RegistryType.CL_ACCUM,
                    IssuerType.ISSUANCE_BY_DEFAULT,
                    100,
                    testTailsPathForRevocation);
            string revRegDefJson = "{\"ver\":\"1.0\",\"id\":\"55GkHamhTU1ZbTbV2ab9DE:4:55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag:CL_ACCUM:default\",\"revocDefType\":\"CL_ACCUM\",\"tag\":\"default\",\"credDefId\":\"55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag\",\"value\":{\"issuanceType\":\"ISSUANCE_BY_DEFAULT\",\"maxCredNum\":100,\"publicKeys\":{\"accumKey\":{\"z\":\"1 191190AD6D246A0C08A3FD861111646892A92C95D693BD575D3BB7EA2B4DDCB8 1 1569C1150D398CF5D5681C3DFD5DEC4FD2AF4A9FACA1726DA228C9613F0F2A55 1 2461A8627CC49731DF8B333D18427B52DD50BCB0B726AC2CB85706AC2706F102 1 1B5BB5DBA793F19DD8CF3D0FE70AEFBFAFFB5EA3725FC518B50FECC0D3FA7C3B 1 0033682B8C64D970748A23913E7795931D095C10347999CB9D56AB2A1388645F 1 118B0D9DB80D569A0A66E8BA4C26C684BE300D8A43A126C1C59364A649837E00 1 1DFE562E117B1D0986FA749A34311217A29B8CE990390EE474223AA3BB6B5484 1 19E244A5D7CB172B43E03692F23A53E3CD05318CB653BE479CE6CE6BAE71C800 1 1F410C423EC051272438A187D417C6863BD1DAA252B5CE3A77D83D64F3606C41 1 08A422C160DCD70AD18AA1346FB1799309698F52B99280512C7F0AB6D80CD8DD 1 1F59F3BE1CDEA1A3D7A2BB6C4CDBE7E332D29F788A210DF136BB18CB15CBAD06 1 07A871CCC542447BE3398F31570DBA1B366E68C56F1D0141D1392FB0BE76DFFF\"}},\"tailsHash\":\"4kWpJmEMbyukHeLyV3AiounJRH22aaj2kHbQB742Ut6S\",\"tailsLocation\":\"" +
                $"{tmpRevRegDefObject.Value.TailsLocation.Replace("\\", "/")}" +
                "\"}}";
            RevocationRegistryDefinition revRegDefObject = await RevocationApi.CreateRevocationRegistryDefinitionFromJsonAsync(revRegDefJson);
            List<RevocationRegistryDefinition> revRegDefinitions = new() { revRegDefObject };

            string revRegJson = "{\"ver\":\"1.0\",\"value\":{\"accum\":\"21 12A1996B8EBA47C9E7CEA41963DF05F99FDB6E9B381B7C8C34BFD66BE01D1C04B 21 13FB3011CFC5A3BF41923A7F9FA3FA9A89F7169738370CB3386F3E4F4CB9FF29D 6 61492CFCB58CB8F4A17D1F5880C88FCBD6DE1348E05EC7692ACEB4C00592FED5 4 3256E2C0799403B0320EF693D67F48922A6E3B233766AE952CBE516F67F27DE8 6 876F21FBF7A1EE72C77DDE8D462C4195D0DD6CB4E3C76E9AD3948D38956DB6C7 4 26EED3F0DA48A5D5187FDA236C4B8FDAB6DF072CA7C1D18B4F6BFD81D72BE731\"}}";
            RevocationRegistry revRegObject = await RevocationApi.CreateRevocationRegistryFromJsonAsync(revRegJson);
            List<RevocationRegistryEntry> revRegistries = new()
            {
                new RevocationRegistryEntry
                {
                    DefEntryIdx = 0,
                    Entry = new IntPtr(),
                    Timestamp = timestamp
                }
            };

            string presentationJson = "{\"proof\":{\"proofs\":[{\"primary_proof\":{\"eq_proof\":{\"revealed_attrs\":{\"attr\":\"72155939486846849509759369733266486982821795810448245423168957390607644363272\"},\"a_prime\":\"22787947532665702942166390265565099820863799587030609859364101947347429956006178433736182956558680217451360098070400415842618416586813209500267111752188923151298858777205206383545858632930505476551131457277377652820989619031780149993288820195069310942122913730689119829340750366182236664539501596260514894650983980623701649527364749330998285260704338497630597378327025502438431858516849741910716399356638178657965616831942802640713754838111099765928053264003133303245178307245238269379969657844386541201731429479010183162613731023271140443137949920801708151362109823177071910561747744940225633896803721317669782358421\",\"e\":\"69124141745661451232422071833447747082810057151848226659717004723245660321643866206894483809434372484419787675271515845575181681885576485\",\"v\":\"502483187879133716417096480006386205327995711270888126778869121855278753425593998422963838651347769608503283807649516512880075823986911920658063448216074566307723181816414360454851464182162626545924978908908754080967539118089711545270393292866557588250787116479546341674975985851318791108768074114121210355659608509113854257493677851780112011053516065048781059761361613409853017535033453997896416355511480069057755205220463158986451933726626781714019352189004060699441752283027199173688877273426479931600413051842909026732192148129355030099736087987835032879264302949802085989635543207730170481446080695187580698323015118647125807401471446437413140532020293604881927856270725677607294897411796868074908977878774588588258946655011556630640214442071918831131351471397679508616542608467322074771091661348278136342901709870360635291451103144692305647239100786223515212569094457685892676825400988189774258596242950852693022055\",\"m\":{\"master_secret\":\"2307093772458771039711933057912384843500743528411547595991148149387405897463262936047925219917981120668180047862724912654604570684672680548843949939043365133411527271086042486203\"},\"m2\":\"3350007083872569628769486907745210896506907039493766347465000482213910452825573519528617062976602075306102068308540480111571831600569611213574129530137747\"},\"ge_proofs\":[]},\"non_revoc_proof\":{\"x_list\":{\"rho\":\"1C3AC92D0868D6AD9F124B88E0AA66ED2F3BBC2401AE52A4512CDEC68B82078D\",\"r\":\"0D0387C22AA96E9D00AC180BF461BA40809BE312BB0FC57B392B4660F8AC02C9\",\"r_prime\":\"156475DD528D2A7F6A7711479C40A634F76A82A0AD098DEFC957CF500ADDD6F6\",\"r_prime_prime\":\"1262D4B6EE055CB7B12AC9CCC5A8C5551DC1398D925162FD63B19A7E27E47601\",\"r_prime_prime_prime\":\"1FC79BA981C70CF3FDDBEE8F3908C63F195284DA250B9F4BF0B8C444F00A1DA5\",\"o\":\"0645F0F20692FC3EB7A8AC6DF8975475D69046A3A1AA2F9454309976628CC2D6\",\"o_prime\":\"0E17AE4D4A318BE3F22B8741A9CD401EEF8EDF2571234353DE93C71F91AEB868\",\"m\":\"07AA5FC3DC804398D1E7C087241D13E05F9920BB3E72AE903EF9780BEA53F516\",\"m_prime\":\"22F8D76B8EEC5B8E8AB10B00165CDCB1638733CEC1DAA82D8E2D1DF64EAEA8CE\",\"t\":\"10ACE2A8623DCB973DF484C9634FB4DC94BA9097F89ABA3C5863693F9EC5F223\",\"t_prime\":\"236B85805AD43E4EEA0DE3A03A764BF717805EC5AADEEAD2566B485B7CAC81D1\",\"m2\":\"1BA5E259DB355BC7D8A7DB2B76E6CA77F6BF7EFA08D19E7F533836005F6FBAB3\",\"s\":\"0B1339A60E431657FFDACA6D5C79CC964471B7595B4D34562AAD8E4648E78A82\",\"c\":\"16608A5E3549E73F2F6A32D54D8AE8B0B2DF36DDE1410876BFE20287CB069E63\"},\"c_list\":{\"e\":\"6 38732649A4C651FABABE98F14CF92AF6F2217F528540B3AC499D80D35D494055 4 3F966F47E48797FB2CEA9B6318782FA70B77880DABDCAF2E0C0EA99351EA842F 4 30738E44CA85F08F501F3E7D53D0F87DEB42862769EBD8B13D062A2F33432A93\",\"d\":\"6 474DE704E04EA60150D37B39FEF9C87908CA371D20A2C3509864591699723647 4 04180C1A54CFE8B2225F09DF98469B26DFE703D1E3596E9E17DA0596F9364DBA 4 2CD33754BC31C6DFC3D265B10255B6C768AD491EF4858DACF6464C95F449F3AB\",\"a\":\"6 4B639F55DECFBC5C3DCEF78E043F5056702E9A24FF62BEF53D5C658AEC998640 4 1E87C680AE0FE4C144C02C2C6D9BFBB6319C3F68DAD161FEE52FF7C9E3B7A5E4 4 1B3FDF399DD2CE9416CBCF9F2DF371CCDADDD26C60882AE6DF55921A6009D07A\",\"g\":\"6 42184EA20FFB2329573A01CD5DE5BA48968E8C6CC0840931B31B410AEFE66B87 4 30CF73E43C59393CEB521ADE04CBD1268039521080C2282B459C0A110866F3F0 4 1C52F59F00FEE09872C01AEAE0AB76A15AD2F00CCDB99B73BEC14EFB3BE7A75E\",\"w\":\"21 13A9D8DB787AD569B83F83E3E4D85D01C1FD50E61ABB41BBAF62CEAA157033180 21 12C955A1AB9842C62A7A3A023A6A34EA3B5C966780A00F5175B1CD5D25D87A1FF 6 6608A8E039569F56F4221EA7B4BDEF52DC0D9F073728700ADEA2F695F9DB4853 4 1AE01005403C489AC15C9349BC2D8D43B34D3C43D0D828F238DA4F742B35260B 6 52AB4701947804C7F3B17D7F22F35DE50483B588AF74C5EA122F508D2D634B74 4 073118B2C588D05F7DCDB3D8C2EA322663DAA589F081C36A28C6220B990793CB\",\"s\":\"21 11AA1CEE4FB15EEBAA34BF79C5BA75A0D22064288FE8D64CEEBBABF24F6D37903 21 113CE1CE8533D17F54BEC9490B3FD3601CDBFC8B04B0B8948F930EBA3E67C8CFF 6 55814737228E6B608EE44B894863605890FB81528C7A67FC0857B73FD9438F5B 4 309834112A9A043C0AF59BBC2BC47D72A258FDA266237FD780925D7AB9A6F43C 6 79E87F6D2789925E22F89F0E5E86B15E26B1259B2EAEE8A86AAF8A38D724A123 4 1A8ABBC77FABDF0C3F3223D9D81F51481E28967A33636CD598878F4483F6B6CA\",\"u\":\"21 12846FAFA8E2998E260BE514A492CA4101F0206BE1C46293F736BB027CBCD7D66 21 13581B75D34A2413C50178B4337DA5C8B4056639057DCCF2DDE6563E200BCCFC6 6 64EA09AD4A1E3C6BCC7EBFF9C3AAD7FEFDE17D2D12517FB3644C891D91B579FA 4 31C98A8C89C90961D814CC181F6036C810DDFC9C8D62703DBBC0EEFAEB5EE73B 6 720417974AA223B2375B13837CDB3D5CC2542EDBAF7B9192CB414CD40A191DF1 4 0BCDF714766E1EB0CF923BF54D954ABFF39A2BACACE00E4E2B73AA86B420E8F9\"}}}],\"aggregated_proof\":{\"c_hash\":\"57204176382419527374101370361393524337764538734535251618083733237783975439070\",\"c_list\":[[4,15,88,167,248,56,102,147,80,24,31,234,110,102,161,67,43,125,207,87,179,56,8,40,221,242,70,202,231,69,195,248,205,25,33,90,195,219,255,23,52,105,221,133,186,132,167,223,162,68,186,75,179,189,1,65,254,76,96,125,100,40,5,100,103,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[4,24,49,229,206,52,10,42,52,195,227,91,45,94,133,85,241,105,195,178,217,81,190,25,203,16,149,93,186,44,71,119,190,1,95,192,234,30,175,215,56,136,133,100,233,140,112,56,244,95,204,69,128,213,21,233,94,147,116,37,153,242,52,209,138,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[4,36,122,151,5,113,228,166,236,38,129,222,222,49,212,15,96,103,229,3,25,28,79,37,225,177,177,237,134,6,66,27,119,27,109,135,41,214,232,251,152,184,10,32,74,82,207,40,195,103,231,154,242,187,30,231,218,67,106,190,128,121,155,131,113,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[4,34,66,12,235,45,42,171,239,205,121,131,31,41,208,82,246,226,170,49,76,126,165,225,43,128,172,54,126,88,20,128,19,25,223,36,170,76,242,212,148,63,33,235,54,137,148,140,196,40,65,151,62,212,181,189,243,6,88,75,235,151,114,54,232,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[2,220,197,124,183,58,58,157,142,103,86,147,145,21,36,119,22,117,86,200,99,115,154,196,120,116,80,24,138,230,36,177,30,81,86,105,168,250,240,169,14,34,93,141,158,208,228,138,226,86,195,111,234,35,121,40,171,132,129,53,97,162,190,76,17,198,236,135,230,125,232,215,65,239,184,64,2,185,53,15,32,100,138,152,2,23,237,183,77,53,137,194,42,13,147,224,15,251,118,253,30,27,80,73,207,230,147,236,203,187,117,137,203,187,203,51,255,14,254,57,110,87,140,174,202,247,155,13],[6,4,32,235,21,24,190,211,121,184,190,97,209,193,204,252,84,165,232,105,66,242,136,146,55,150,198,158,138,75,9,42,20,94,45,147,73,55,47,29,238,14,37,159,3,138,207,149,38,169,47,201,12,26,72,119,103,113,204,39,9,237,140,171,30,138,231,25,58,230,106,18,39,185,248,42,210,252,84,207,80,156,11,152,33,1,126,84,180,236,37,83,219,45,36,100,8,178,133,234,236,77,114,247,114,116,96,227,221,115,73,209,254,192,44,154,95,138,75,225,140,115,158,153,117,247,46,120],[16,44,243,209,174,98,186,130,29,56,38,87,123,124,238,122,203,120,31,253,232,19,226,74,204,75,162,51,66,66,45,186,9,6,138,241,64,162,56,111,131,25,104,172,138,235,27,138,139,39,130,101,227,74,72,192,159,162,90,203,62,206,169,103,26,216,166,16,244,46,39,151,224,100,180,118,36,16,47,226,176,52,23,161,20,206,124,249,124,111,169,38,165,248,85,186,20,108,129,30,247,163,11,93,67,207,172,13,205,19,137,217,20,51,12,20,189,188,141,172,216,195,210,122,166,225,240,176],[180,131,235,194,141,158,164,112,75,71,68,181,71,3,222,45,131,147,72,120,36,190,183,145,119,59,94,109,178,83,240,117,200,139,183,234,120,1,110,141,136,165,42,78,208,133,128,142,129,225,90,76,185,141,133,255,168,176,156,198,205,174,109,95,175,11,53,125,245,234,195,119,40,130,113,52,218,73,76,203,177,234,76,126,241,151,118,123,75,50,61,64,142,89,4,43,204,101,133,151,134,232,55,175,118,177,0,30,39,220,62,41,191,69,69,176,24,176,190,231,153,46,1,224,26,77,218,181,22,139,97,9,101,122,140,126,141,227,30,235,59,32,28,156,131,249,221,32,2,105,209,58,36,113,168,135,218,59,229,111,101,184,187,104,186,9,42,160,169,62,171,119,200,240,34,22,132,162,120,81,95,241,137,102,7,179,13,132,28,103,203,185,8,79,241,36,7,197,92,180,229,209,207,178,255,134,226,94,183,236,213,184,164,53,139,255,206,186,94,215,173,161,15,160,235,255,163,132,248,68,184,47,160,1,57,39,255,49,113,237,48,199,71,242,219,32,146,28,55,139,17,40,149,67,9,149]]}},\"requested_proof\":{\"revealed_attrs\":{\"reft\":{\"sub_proof_index\":0,\"raw\":\"test\",\"encoded\":\"72155939486846849509759369733266486982821795810448245423168957390607644363272\"}},\"self_attested_attrs\":{},\"unrevealed_attrs\":{},\"predicates\":{}},\"identifiers\":[{\"schema_id\":\"55GkHamhTU1ZbTbV2ab9DE:2:schema name:schema version\",\"cred_def_id\":\"55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag\",\"rev_reg_id\":\"55GkHamhTU1ZbTbV2ab9DE:4:55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag:CL_ACCUM:default\",\"timestamp\":1656323735}]}";
            Presentation presentationObject = await PresentationApi.CreatePresentationFromJsonAsync(presentationJson);

            //Act
            Func<Task<bool>> func = async () => await PresentationApi.VerifyPresentationAsync(
                presentationObject,
                presReqObject,
                schemas,
                credentialDefinitions,
                revRegDefinitions,
                revRegistries);

            //Assert
            _ = await func.Should().ThrowAsync<Exception>();
        }

        [Test, TestCase(TestName = "VerifyPresentationAsync() with JSON input works.")]
        public async Task VerifyPresentationJsonWorks()
        {
            //Arrange
            string nonce = "75097235631617494585946";
            long timestamp = 1656323735;
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string testTailsPathForRevocation = null;

            Schema schemaObject = await SchemaApi.CreateSchemaFromJsonAsync("{" +
               "\"ver\": \"1.0\"," +
               "\"id\": \"55GkHamhTU1ZbTbV2ab9DE:2:schema name:schema version\"," +
               "\"name\": \"schema name\"," +
               "\"version\": \"schema version\"," +
               "\"attrNames\": [\"attr\"]," +
               "\"seqNo\": 15" +
               "}");
            List<string> schemas = new() { schemaObject.JsonString };

            string presReqJson = "{" +
            "\"name\": \"proof\"," +
            "\"version\": \"1.0\", " +
            $"\"nonce\": \"{nonce}\"," +
            "\"requested_attributes\": " +
                "{" +
                    "\"reft\": " +
                    "{" +
                        "\"name\": \"attr\"," +
                        "\"non_revoked\": " +
                        "{" +
                            $"\"from\": {timestamp}, " +
                            $"\"to\": {timestamp}" +
                        "}" +
                    "}" +
                "}," +
            "\"requested_predicates\": { }," +
            "\"ver\": \"1.0\"" +
            "}";

            string credDefJson = "{\"ver\":\"1.0\",\"id\":\"55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag\",\"schemaId\":\"15\",\"type\":\"CL\",\"tag\":\"tag\",\"value\":{\"primary\":{\"n\":\"84487835551039032285187155154911060483660350222398883306413615732874461803883807049444869314357050156687890484696882752158866570703691960502013865653643690888821722220736405782919735960755230175042942119509787097308421567876300005147107204196026086261303918012289983042328204552430813959949302734869806656279874589793840951290632815831311761788019595465070212590341317127385075800049878207575128216810613429868614248865074547315292251284915159151999933871168916872594390167708391421621228425918116044537974671504192144649804765393008843077539872571691651675561911758354029794542551288272701560143399651052167087750481\",\"s\":\"28558128648758027909305904351171703188330099292924797248574842793109489507264016530475517567631632922816270447442674993775079076136342650057568741312479074313908686874475078654401350487691696926149271362294936274559242219859505383427715620999464298355003971210313002128345321767575959219528532430269155639274747587292593452207105673695263222521323277842616410349603290362664657370067179177493187725056366137274260699343615520306336032240972903155558389304716505008244368995276114740488732419322360582041616827281207709706369246942275508659481785003071817812195736581875859513370666043191942935979341546224669755726592\",\"r\":{\"attr\":\"19374635229243697947050097026914899831804151710623390439699875709053261437490979212259490718039116411407119344652902355943963171792218723086630516063841949623590129382828843769738786513710334176054037579338207410777516896932526759088769893402563183018185612215867340692245277462458139565495698429412757356575226491664765036788474249115006091029994257607576855491435914614697429367514578757816940487787493706573109304351402336200840439292576613554345366341329612250384952870403073130659832480298734047175262840687349916307540764272919838633325633665460254326264312598782859300055717928967069882262403842787394139435894\",\"master_secret\":\"81622178607923918364721456204298773176565035371329841505071999320161445715140470066338770173307831504322432335318816064336064526133483910228038661838624698618588845927308454293473291897151646838241523303375565952307736856063762202291448059793289978319987542894438432492616678253629520076550282854666371806570750552098795289963378158905480562249765636399463659535901314787140796754551215402990460593947771202214698867632223596467677921849056698450129899546703013955829498161920514483760182816658783824691036213919954445215806393456079532166137528830442865329317066684497704520867308483692017792380043666972369996476648\"},\"rctxt\":\"12146311402700730909210821007084758312612633902400591088560201782774404663232896787734230361129748004241058753273490601823770520400256720606314268792396336044245755238750374415517749168755873962734625982190385924909156182838280338018890559958648961135704419623925386696159082001415714738757173419135327834089658501956546412643981617383898868811945190720171479924941322302800590599119428117660679419757785001036588261888810604510413383924652834880650784571036955954104556885805909446788668012071619521870517934764185543864410726697064950585970364606527370867045042988632693291566416288920563963182217099152392920623981\",\"z\":\"8659484373005269980033082471592982329289449003147153823387929755264589920438261740054893267116254145809205967722170852206137509022326400590521422584318195793318452699121075733292345177492016943238125091931931097404226756579606524593225823005706035955720119333209333659906090001548030910641905069127435973870268228855554477785824268588565396535099976521044309399187732772283806196540898387710340281460301731792116774708748625316697733261016999865199755587315775825661081423595514414352371280406326503074363945445577704946310124460634120397951995477382974597897391473236827620740751902305208021365042055236954223606712\"},\"revocation\":{\"g\":\"1 147F164871D36240487873028DC022B8B2D72551285179B6BA1963709B809E98 1 15B52C24CDDB4BF0B91AB9299C307970DA2039F99412370C7F4F10B5FA442CE4 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"g_dash\":\"1 010272B8327846397255FC5F6634ABF2A4838A907DC4E85CA41733F100C4C045 1 1E50A2C2046BEB577CCA2BF3E77377EA4539915C4E2631F16533535DBBB59078 1 19E0F2F978B69328250B646FB4E1C3761CB8505B0C051C16F0C62ADA10AC1EDE 1 24597FEEA245D53B3EBE6A3F21654ECE28FA6F4A4525B502D52A21C4D051E716 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8 1 0000000000000000000000000000000000000000000000000000000000000000\",\"h\":\"1 23C97E94D95C57F8ACF2E78E8860651DC4A5955D8FE75E20FA663AD3FBA088B3 1 0799BB348CAFD085E13174FE165E83C0672FF5FF3CF71A39F6684F35547E96F3 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"h0\":\"1 0D637EA93C50CE193DF1BF616334EFD6A41D4339912B68138E334F408B380387 1 0F3D675892139E06CD5A492B12FC9E859B4CA2674D976D6437CBFF3D7B380312 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"h1\":\"1 1F4DD6589D2D318AB752112A7631B23E507F33A45CE73CE1719038CE4CDDD7F0 1 0A2502C73B9DA105316BFA81BFA6EA9763B4E4BF85E8034936929A7DF9EB6202 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"h2\":\"1 1D3AD4848CEEEDAB17FDCF1AB2628375B0ACBB67FB5712D54BF52636DE050C49 1 11AB2ECB1FBA088AF2E7F19B06B19EDF12738DE62C42E5D2E5493A19CA231C7D 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"htilde\":\"1 04641618E8A51FE5266A70BECBAD852B9974250006E7083C157FBA1288004ABD 1 177B3E0114876224103FD395110C0F90A640E299352DB06853D76DB4E35B4172 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"h_cap\":\"1 142C106B302E4125072211F1A65E61B4335F39558E014523B406EBF0D964EC37 1 06EA720DCA4DA1CEE3E614E5DC14DC8C3B052D2B7B2F30881CAFCED4B08864F0 1 000B8EC003C2AE9CB68E11134E0ACAD8402CD0A968FF7C9AF0434BAA79D3E697 1 054FB7273A3E828DB80DEDA4F7821989F32AC082E935C5099876F6B31F953075 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8 1 0000000000000000000000000000000000000000000000000000000000000000\",\"u\":\"1 0FD51468053FD3188BE914065D6AACD9978B44F9CC594161C6B1BE6D6DEE28C2 1 23A9E07A6CFAF228C641FD4A6540E61599B8B66A18F44C89AB88E4AF529ABA5C 1 09B8D02402F8964A2103DB926C4E7AB0003127E6E622EE162B058C10B41C9148 1 13CBFA6415CFFAC0E834D769F1DDED9B0BB978AB3BA78A7A8608889995E1125B 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8 1 0000000000000000000000000000000000000000000000000000000000000000\",\"pk\":\"1 0F8B6D1969C96B11AC19D290242053EB870A69FBF488ED7B44BB040C921542F2 1 0563DDC59EE0530BCC8B3A73D1BE60403993E9476E2C15B7D1C6F2B3DF04A91E 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"y\":\"1 149AB72F3F5D2F5A2486EC0A95B35126923F0F846F7DB6952C2BA305CF9253BA 1 0F0FDFA8FD278190F1636CD0FA703A4EF94B8CDA5ADD5967A4C57E872919DF1F 1 1E363496C15D2A8E3BDA3343929630A3517040F0A056A578534BFF9BF2D8411C 1 23DABD532DF8CEC9FB3B0BFA3C13F0C28E8B0F1947702C05DDBB9E0C56C0FBAE 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8 1 0000000000000000000000000000000000000000000000000000000000000000\"}}}";
            CredentialDefinition credDefObject = await CredentialDefinitionApi.CreateCredentialDefinitionFromJsonAsync(credDefJson);
            List<string> credentialDefinitions = new() { credDefObject.JsonString };

            (RevocationRegistryDefinition tmpRevRegDefObject,
                _,
                _,
                _) =
                await RevocationApi.CreateRevocationRegistryAsync(
                    issuerDid,
                    credDefObject,
                    "tag",
                    RegistryType.CL_ACCUM,
                    IssuerType.ISSUANCE_BY_DEFAULT,
                    100,
                    testTailsPathForRevocation);
            string revRegDefJson = "{\"ver\":\"1.0\",\"id\":\"55GkHamhTU1ZbTbV2ab9DE:4:55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag:CL_ACCUM:default\",\"revocDefType\":\"CL_ACCUM\",\"tag\":\"default\",\"credDefId\":\"55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag\",\"value\":{\"issuanceType\":\"ISSUANCE_BY_DEFAULT\",\"maxCredNum\":100,\"publicKeys\":{\"accumKey\":{\"z\":\"1 191190AD6D246A0C08A3FD861111646892A92C95D693BD575D3BB7EA2B4DDCB8 1 1569C1150D398CF5D5681C3DFD5DEC4FD2AF4A9FACA1726DA228C9613F0F2A55 1 2461A8627CC49731DF8B333D18427B52DD50BCB0B726AC2CB85706AC2706F102 1 1B5BB5DBA793F19DD8CF3D0FE70AEFBFAFFB5EA3725FC518B50FECC0D3FA7C3B 1 0033682B8C64D970748A23913E7795931D095C10347999CB9D56AB2A1388645F 1 118B0D9DB80D569A0A66E8BA4C26C684BE300D8A43A126C1C59364A649837E00 1 1DFE562E117B1D0986FA749A34311217A29B8CE990390EE474223AA3BB6B5484 1 19E244A5D7CB172B43E03692F23A53E3CD05318CB653BE479CE6CE6BAE71C800 1 1F410C423EC051272438A187D417C6863BD1DAA252B5CE3A77D83D64F3606C41 1 08A422C160DCD70AD18AA1346FB1799309698F52B99280512C7F0AB6D80CD8DD 1 1F59F3BE1CDEA1A3D7A2BB6C4CDBE7E332D29F788A210DF136BB18CB15CBAD06 1 07A871CCC542447BE3398F31570DBA1B366E68C56F1D0141D1392FB0BE76DFFF\"}},\"tailsHash\":\"4kWpJmEMbyukHeLyV3AiounJRH22aaj2kHbQB742Ut6S\",\"tailsLocation\":\"" +
                $"{tmpRevRegDefObject.Value.TailsLocation.Replace("\\", "/")}" +
                "\"}}";
            RevocationRegistryDefinition revRegDefObject = await RevocationApi.CreateRevocationRegistryDefinitionFromJsonAsync(revRegDefJson);
            List<string> revRegDefinitions = new() { revRegDefObject.JsonString };

            string revRegJson = "{\"ver\":\"1.0\",\"value\":{\"accum\":\"21 12A1996B8EBA47C9E7CEA41963DF05F99FDB6E9B381B7C8C34BFD66BE01D1C04B 21 13FB3011CFC5A3BF41923A7F9FA3FA9A89F7169738370CB3386F3E4F4CB9FF29D 6 61492CFCB58CB8F4A17D1F5880C88FCBD6DE1348E05EC7692ACEB4C00592FED5 4 3256E2C0799403B0320EF693D67F48922A6E3B233766AE952CBE516F67F27DE8 6 876F21FBF7A1EE72C77DDE8D462C4195D0DD6CB4E3C76E9AD3948D38956DB6C7 4 26EED3F0DA48A5D5187FDA236C4B8FDAB6DF072CA7C1D18B4F6BFD81D72BE731\"}}";
            RevocationRegistry revRegObject = await RevocationApi.CreateRevocationRegistryFromJsonAsync(revRegJson);
            List<string> revRegistries = new()
            {
                JsonConvert.SerializeObject(new RevocationRegistryEntry
                {
                    DefEntryIdx = 0,
                    Entry = revRegObject.Handle,
                    Timestamp = timestamp
                })
            };

            string presentationJson = "{\"proof\":{\"proofs\":[{\"primary_proof\":{\"eq_proof\":{\"revealed_attrs\":{\"attr\":\"72155939486846849509759369733266486982821795810448245423168957390607644363272\"},\"a_prime\":\"22787947532665702942166390265565099820863799587030609859364101947347429956006178433736182956558680217451360098070400415842618416586813209500267111752188923151298858777205206383545858632930505476551131457277377652820989619031780149993288820195069310942122913730689119829340750366182236664539501596260514894650983980623701649527364749330998285260704338497630597378327025502438431858516849741910716399356638178657965616831942802640713754838111099765928053264003133303245178307245238269379969657844386541201731429479010183162613731023271140443137949920801708151362109823177071910561747744940225633896803721317669782358421\",\"e\":\"69124141745661451232422071833447747082810057151848226659717004723245660321643866206894483809434372484419787675271515845575181681885576485\",\"v\":\"502483187879133716417096480006386205327995711270888126778869121855278753425593998422963838651347769608503283807649516512880075823986911920658063448216074566307723181816414360454851464182162626545924978908908754080967539118089711545270393292866557588250787116479546341674975985851318791108768074114121210355659608509113854257493677851780112011053516065048781059761361613409853017535033453997896416355511480069057755205220463158986451933726626781714019352189004060699441752283027199173688877273426479931600413051842909026732192148129355030099736087987835032879264302949802085989635543207730170481446080695187580698323015118647125807401471446437413140532020293604881927856270725677607294897411796868074908977878774588588258946655011556630640214442071918831131351471397679508616542608467322074771091661348278136342901709870360635291451103144692305647239100786223515212569094457685892676825400988189774258596242950852693022055\",\"m\":{\"master_secret\":\"2307093772458771039711933057912384843500743528411547595991148149387405897463262936047925219917981120668180047862724912654604570684672680548843949939043365133411527271086042486203\"},\"m2\":\"3350007083872569628769486907745210896506907039493766347465000482213910452825573519528617062976602075306102068308540480111571831600569611213574129530137747\"},\"ge_proofs\":[]},\"non_revoc_proof\":{\"x_list\":{\"rho\":\"1C3AC92D0868D6AD9F124B88E0AA66ED2F3BBC2401AE52A4512CDEC68B82078D\",\"r\":\"0D0387C22AA96E9D00AC180BF461BA40809BE312BB0FC57B392B4660F8AC02C9\",\"r_prime\":\"156475DD528D2A7F6A7711479C40A634F76A82A0AD098DEFC957CF500ADDD6F6\",\"r_prime_prime\":\"1262D4B6EE055CB7B12AC9CCC5A8C5551DC1398D925162FD63B19A7E27E47601\",\"r_prime_prime_prime\":\"1FC79BA981C70CF3FDDBEE8F3908C63F195284DA250B9F4BF0B8C444F00A1DA5\",\"o\":\"0645F0F20692FC3EB7A8AC6DF8975475D69046A3A1AA2F9454309976628CC2D6\",\"o_prime\":\"0E17AE4D4A318BE3F22B8741A9CD401EEF8EDF2571234353DE93C71F91AEB868\",\"m\":\"07AA5FC3DC804398D1E7C087241D13E05F9920BB3E72AE903EF9780BEA53F516\",\"m_prime\":\"22F8D76B8EEC5B8E8AB10B00165CDCB1638733CEC1DAA82D8E2D1DF64EAEA8CE\",\"t\":\"10ACE2A8623DCB973DF484C9634FB4DC94BA9097F89ABA3C5863693F9EC5F223\",\"t_prime\":\"236B85805AD43E4EEA0DE3A03A764BF717805EC5AADEEAD2566B485B7CAC81D1\",\"m2\":\"1BA5E259DB355BC7D8A7DB2B76E6CA77F6BF7EFA08D19E7F533836005F6FBAB3\",\"s\":\"0B1339A60E431657FFDACA6D5C79CC964471B7595B4D34562AAD8E4648E78A82\",\"c\":\"16608A5E3549E73F2F6A32D54D8AE8B0B2DF36DDE1410876BFE20287CB069E63\"},\"c_list\":{\"e\":\"6 38732649A4C651FABABE98F14CF92AF6F2217F528540B3AC499D80D35D494055 4 3F966F47E48797FB2CEA9B6318782FA70B77880DABDCAF2E0C0EA99351EA842F 4 30738E44CA85F08F501F3E7D53D0F87DEB42862769EBD8B13D062A2F33432A93\",\"d\":\"6 474DE704E04EA60150D37B39FEF9C87908CA371D20A2C3509864591699723647 4 04180C1A54CFE8B2225F09DF98469B26DFE703D1E3596E9E17DA0596F9364DBA 4 2CD33754BC31C6DFC3D265B10255B6C768AD491EF4858DACF6464C95F449F3AB\",\"a\":\"6 4B639F55DECFBC5C3DCEF78E043F5056702E9A24FF62BEF53D5C658AEC998640 4 1E87C680AE0FE4C144C02C2C6D9BFBB6319C3F68DAD161FEE52FF7C9E3B7A5E4 4 1B3FDF399DD2CE9416CBCF9F2DF371CCDADDD26C60882AE6DF55921A6009D07A\",\"g\":\"6 42184EA20FFB2329573A01CD5DE5BA48968E8C6CC0840931B31B410AEFE66B87 4 30CF73E43C59393CEB521ADE04CBD1268039521080C2282B459C0A110866F3F0 4 1C52F59F00FEE09872C01AEAE0AB76A15AD2F00CCDB99B73BEC14EFB3BE7A75E\",\"w\":\"21 13A9D8DB787AD569B83F83E3E4D85D01C1FD50E61ABB41BBAF62CEAA157033180 21 12C955A1AB9842C62A7A3A023A6A34EA3B5C966780A00F5175B1CD5D25D87A1FF 6 6608A8E039569F56F4221EA7B4BDEF52DC0D9F073728700ADEA2F695F9DB4853 4 1AE01005403C489AC15C9349BC2D8D43B34D3C43D0D828F238DA4F742B35260B 6 52AB4701947804C7F3B17D7F22F35DE50483B588AF74C5EA122F508D2D634B74 4 073118B2C588D05F7DCDB3D8C2EA322663DAA589F081C36A28C6220B990793CB\",\"s\":\"21 11AA1CEE4FB15EEBAA34BF79C5BA75A0D22064288FE8D64CEEBBABF24F6D37903 21 113CE1CE8533D17F54BEC9490B3FD3601CDBFC8B04B0B8948F930EBA3E67C8CFF 6 55814737228E6B608EE44B894863605890FB81528C7A67FC0857B73FD9438F5B 4 309834112A9A043C0AF59BBC2BC47D72A258FDA266237FD780925D7AB9A6F43C 6 79E87F6D2789925E22F89F0E5E86B15E26B1259B2EAEE8A86AAF8A38D724A123 4 1A8ABBC77FABDF0C3F3223D9D81F51481E28967A33636CD598878F4483F6B6CA\",\"u\":\"21 12846FAFA8E2998E260BE514A492CA4101F0206BE1C46293F736BB027CBCD7D66 21 13581B75D34A2413C50178B4337DA5C8B4056639057DCCF2DDE6563E200BCCFC6 6 64EA09AD4A1E3C6BCC7EBFF9C3AAD7FEFDE17D2D12517FB3644C891D91B579FA 4 31C98A8C89C90961D814CC181F6036C810DDFC9C8D62703DBBC0EEFAEB5EE73B 6 720417974AA223B2375B13837CDB3D5CC2542EDBAF7B9192CB414CD40A191DF1 4 0BCDF714766E1EB0CF923BF54D954ABFF39A2BACACE00E4E2B73AA86B420E8F9\"}}}],\"aggregated_proof\":{\"c_hash\":\"57204176382419527374101370361393524337764538734535251618083733237783975439070\",\"c_list\":[[4,15,88,167,248,56,102,147,80,24,31,234,110,102,161,67,43,125,207,87,179,56,8,40,221,242,70,202,231,69,195,248,205,25,33,90,195,219,255,23,52,105,221,133,186,132,167,223,162,68,186,75,179,189,1,65,254,76,96,125,100,40,5,100,103,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[4,24,49,229,206,52,10,42,52,195,227,91,45,94,133,85,241,105,195,178,217,81,190,25,203,16,149,93,186,44,71,119,190,1,95,192,234,30,175,215,56,136,133,100,233,140,112,56,244,95,204,69,128,213,21,233,94,147,116,37,153,242,52,209,138,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[4,36,122,151,5,113,228,166,236,38,129,222,222,49,212,15,96,103,229,3,25,28,79,37,225,177,177,237,134,6,66,27,119,27,109,135,41,214,232,251,152,184,10,32,74,82,207,40,195,103,231,154,242,187,30,231,218,67,106,190,128,121,155,131,113,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[4,34,66,12,235,45,42,171,239,205,121,131,31,41,208,82,246,226,170,49,76,126,165,225,43,128,172,54,126,88,20,128,19,25,223,36,170,76,242,212,148,63,33,235,54,137,148,140,196,40,65,151,62,212,181,189,243,6,88,75,235,151,114,54,232,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[2,220,197,124,183,58,58,157,142,103,86,147,145,21,36,119,22,117,86,200,99,115,154,196,120,116,80,24,138,230,36,177,30,81,86,105,168,250,240,169,14,34,93,141,158,208,228,138,226,86,195,111,234,35,121,40,171,132,129,53,97,162,190,76,17,198,236,135,230,125,232,215,65,239,184,64,2,185,53,15,32,100,138,152,2,23,237,183,77,53,137,194,42,13,147,224,15,251,118,253,30,27,80,73,207,230,147,236,203,187,117,137,203,187,203,51,255,14,254,57,110,87,140,174,202,247,155,13],[6,4,32,235,21,24,190,211,121,184,190,97,209,193,204,252,84,165,232,105,66,242,136,146,55,150,198,158,138,75,9,42,20,94,45,147,73,55,47,29,238,14,37,159,3,138,207,149,38,169,47,201,12,26,72,119,103,113,204,39,9,237,140,171,30,138,231,25,58,230,106,18,39,185,248,42,210,252,84,207,80,156,11,152,33,1,126,84,180,236,37,83,219,45,36,100,8,178,133,234,236,77,114,247,114,116,96,227,221,115,73,209,254,192,44,154,95,138,75,225,140,115,158,153,117,247,46,120],[16,44,243,209,174,98,186,130,29,56,38,87,123,124,238,122,203,120,31,253,232,19,226,74,204,75,162,51,66,66,45,186,9,6,138,241,64,162,56,111,131,25,104,172,138,235,27,138,139,39,130,101,227,74,72,192,159,162,90,203,62,206,169,103,26,216,166,16,244,46,39,151,224,100,180,118,36,16,47,226,176,52,23,161,20,206,124,249,124,111,169,38,165,248,85,186,20,108,129,30,247,163,11,93,67,207,172,13,205,19,137,217,20,51,12,20,189,188,141,172,216,195,210,122,166,225,240,176],[180,131,235,194,141,158,164,112,75,71,68,181,71,3,222,45,131,147,72,120,36,190,183,145,119,59,94,109,178,83,240,117,200,139,183,234,120,1,110,141,136,165,42,78,208,133,128,142,129,225,90,76,185,141,133,255,168,176,156,198,205,174,109,95,175,11,53,125,245,234,195,119,40,130,113,52,218,73,76,203,177,234,76,126,241,151,118,123,75,50,61,64,142,89,4,43,204,101,133,151,134,232,55,175,118,177,0,30,39,220,62,41,191,69,69,176,24,176,190,231,153,46,1,224,26,77,218,181,22,139,97,9,101,122,140,126,141,227,30,235,59,32,28,156,131,249,221,32,2,105,209,58,36,113,168,135,218,59,229,111,101,184,187,104,186,9,42,160,169,62,171,119,200,240,34,22,132,162,120,81,95,241,137,102,7,179,13,132,28,103,203,185,8,79,241,36,7,197,92,180,229,209,207,178,255,134,226,94,183,236,213,184,164,53,139,255,206,186,94,215,173,161,15,160,235,255,163,132,248,68,184,47,160,1,57,39,255,49,113,237,48,199,71,242,219,32,146,28,55,139,17,40,149,67,9,149]]}},\"requested_proof\":{\"revealed_attrs\":{\"reft\":{\"sub_proof_index\":0,\"raw\":\"test\",\"encoded\":\"72155939486846849509759369733266486982821795810448245423168957390607644363272\"}},\"self_attested_attrs\":{},\"unrevealed_attrs\":{},\"predicates\":{}},\"identifiers\":[{\"schema_id\":\"55GkHamhTU1ZbTbV2ab9DE:2:schema name:schema version\",\"cred_def_id\":\"55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag\",\"rev_reg_id\":\"55GkHamhTU1ZbTbV2ab9DE:4:55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag:CL_ACCUM:default\",\"timestamp\":1656323735}]}";

            //Act
            bool actual = await PresentationApi.VerifyPresentationAsync(
                presentationJson,
                presReqJson,
                schemas,
                credentialDefinitions,
                revRegDefinitions,
                revRegistries);

            //Assert
            _ = actual.Should().BeTrue();
        }

        [Test, TestCase(TestName = "VerifyPresentationAsync() with JSON input throws.")]
        public async Task VerifyPresentationJsonThrows()
        {
            //Arrange
            string nonce = "75097235631617494585946";
            long timestamp = 1656323735;
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string testTailsPathForRevocation = null;

            Schema schemaObject = await SchemaApi.CreateSchemaFromJsonAsync("{" +
               "\"ver\": \"1.0\"," +
               "\"id\": \"55GkHamhTU1ZbTbV2ab9DE:2:schema name:schema version\"," +
               "\"name\": \"schema name\"," +
               "\"version\": \"schema version\"," +
               "\"attrNames\": [\"attr\"]," +
               "\"seqNo\": 15" +
               "}");
            List<string> schemas = new() { schemaObject.JsonString };

            string presReqJson = "{" +
            "\"name\": \"proof\"," +
            "\"version\": \"1.0\", " +
            $"\"nonce\": \"{nonce}\"," +
            "\"requested_attributes\": " +
                "{" +
                    "\"reft\": " +
                    "{" +
                        "\"name\": \"attr\"," +
                        "\"non_revoked\": " +
                        "{" +
                            $"\"from\": {timestamp}, " +
                            $"\"to\": {timestamp}" +
                        "}" +
                    "}" +
                "}," +
            "\"requested_predicates\": { }," +
            "\"ver\": \"1.0\"" +
            "}";

            string credDefJson = "{\"ver\":\"1.0\",\"id\":\"55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag\",\"schemaId\":\"15\",\"type\":\"CL\",\"tag\":\"tag\",\"value\":{\"primary\":{\"n\":\"84487835551039032285187155154911060483660350222398883306413615732874461803883807049444869314357050156687890484696882752158866570703691960502013865653643690888821722220736405782919735960755230175042942119509787097308421567876300005147107204196026086261303918012289983042328204552430813959949302734869806656279874589793840951290632815831311761788019595465070212590341317127385075800049878207575128216810613429868614248865074547315292251284915159151999933871168916872594390167708391421621228425918116044537974671504192144649804765393008843077539872571691651675561911758354029794542551288272701560143399651052167087750481\",\"s\":\"28558128648758027909305904351171703188330099292924797248574842793109489507264016530475517567631632922816270447442674993775079076136342650057568741312479074313908686874475078654401350487691696926149271362294936274559242219859505383427715620999464298355003971210313002128345321767575959219528532430269155639274747587292593452207105673695263222521323277842616410349603290362664657370067179177493187725056366137274260699343615520306336032240972903155558389304716505008244368995276114740488732419322360582041616827281207709706369246942275508659481785003071817812195736581875859513370666043191942935979341546224669755726592\",\"r\":{\"attr\":\"19374635229243697947050097026914899831804151710623390439699875709053261437490979212259490718039116411407119344652902355943963171792218723086630516063841949623590129382828843769738786513710334176054037579338207410777516896932526759088769893402563183018185612215867340692245277462458139565495698429412757356575226491664765036788474249115006091029994257607576855491435914614697429367514578757816940487787493706573109304351402336200840439292576613554345366341329612250384952870403073130659832480298734047175262840687349916307540764272919838633325633665460254326264312598782859300055717928967069882262403842787394139435894\",\"master_secret\":\"81622178607923918364721456204298773176565035371329841505071999320161445715140470066338770173307831504322432335318816064336064526133483910228038661838624698618588845927308454293473291897151646838241523303375565952307736856063762202291448059793289978319987542894438432492616678253629520076550282854666371806570750552098795289963378158905480562249765636399463659535901314787140796754551215402990460593947771202214698867632223596467677921849056698450129899546703013955829498161920514483760182816658783824691036213919954445215806393456079532166137528830442865329317066684497704520867308483692017792380043666972369996476648\"},\"rctxt\":\"12146311402700730909210821007084758312612633902400591088560201782774404663232896787734230361129748004241058753273490601823770520400256720606314268792396336044245755238750374415517749168755873962734625982190385924909156182838280338018890559958648961135704419623925386696159082001415714738757173419135327834089658501956546412643981617383898868811945190720171479924941322302800590599119428117660679419757785001036588261888810604510413383924652834880650784571036955954104556885805909446788668012071619521870517934764185543864410726697064950585970364606527370867045042988632693291566416288920563963182217099152392920623981\",\"z\":\"8659484373005269980033082471592982329289449003147153823387929755264589920438261740054893267116254145809205967722170852206137509022326400590521422584318195793318452699121075733292345177492016943238125091931931097404226756579606524593225823005706035955720119333209333659906090001548030910641905069127435973870268228855554477785824268588565396535099976521044309399187732772283806196540898387710340281460301731792116774708748625316697733261016999865199755587315775825661081423595514414352371280406326503074363945445577704946310124460634120397951995477382974597897391473236827620740751902305208021365042055236954223606712\"},\"revocation\":{\"g\":\"1 147F164871D36240487873028DC022B8B2D72551285179B6BA1963709B809E98 1 15B52C24CDDB4BF0B91AB9299C307970DA2039F99412370C7F4F10B5FA442CE4 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"g_dash\":\"1 010272B8327846397255FC5F6634ABF2A4838A907DC4E85CA41733F100C4C045 1 1E50A2C2046BEB577CCA2BF3E77377EA4539915C4E2631F16533535DBBB59078 1 19E0F2F978B69328250B646FB4E1C3761CB8505B0C051C16F0C62ADA10AC1EDE 1 24597FEEA245D53B3EBE6A3F21654ECE28FA6F4A4525B502D52A21C4D051E716 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8 1 0000000000000000000000000000000000000000000000000000000000000000\",\"h\":\"1 23C97E94D95C57F8ACF2E78E8860651DC4A5955D8FE75E20FA663AD3FBA088B3 1 0799BB348CAFD085E13174FE165E83C0672FF5FF3CF71A39F6684F35547E96F3 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"h0\":\"1 0D637EA93C50CE193DF1BF616334EFD6A41D4339912B68138E334F408B380387 1 0F3D675892139E06CD5A492B12FC9E859B4CA2674D976D6437CBFF3D7B380312 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"h1\":\"1 1F4DD6589D2D318AB752112A7631B23E507F33A45CE73CE1719038CE4CDDD7F0 1 0A2502C73B9DA105316BFA81BFA6EA9763B4E4BF85E8034936929A7DF9EB6202 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"h2\":\"1 1D3AD4848CEEEDAB17FDCF1AB2628375B0ACBB67FB5712D54BF52636DE050C49 1 11AB2ECB1FBA088AF2E7F19B06B19EDF12738DE62C42E5D2E5493A19CA231C7D 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"htilde\":\"1 04641618E8A51FE5266A70BECBAD852B9974250006E7083C157FBA1288004ABD 1 177B3E0114876224103FD395110C0F90A640E299352DB06853D76DB4E35B4172 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"h_cap\":\"1 142C106B302E4125072211F1A65E61B4335F39558E014523B406EBF0D964EC37 1 06EA720DCA4DA1CEE3E614E5DC14DC8C3B052D2B7B2F30881CAFCED4B08864F0 1 000B8EC003C2AE9CB68E11134E0ACAD8402CD0A968FF7C9AF0434BAA79D3E697 1 054FB7273A3E828DB80DEDA4F7821989F32AC082E935C5099876F6B31F953075 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8 1 0000000000000000000000000000000000000000000000000000000000000000\",\"u\":\"1 0FD51468053FD3188BE914065D6AACD9978B44F9CC594161C6B1BE6D6DEE28C2 1 23A9E07A6CFAF228C641FD4A6540E61599B8B66A18F44C89AB88E4AF529ABA5C 1 09B8D02402F8964A2103DB926C4E7AB0003127E6E622EE162B058C10B41C9148 1 13CBFA6415CFFAC0E834D769F1DDED9B0BB978AB3BA78A7A8608889995E1125B 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8 1 0000000000000000000000000000000000000000000000000000000000000000\",\"pk\":\"1 0F8B6D1969C96B11AC19D290242053EB870A69FBF488ED7B44BB040C921542F2 1 0563DDC59EE0530BCC8B3A73D1BE60403993E9476E2C15B7D1C6F2B3DF04A91E 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\",\"y\":\"1 149AB72F3F5D2F5A2486EC0A95B35126923F0F846F7DB6952C2BA305CF9253BA 1 0F0FDFA8FD278190F1636CD0FA703A4EF94B8CDA5ADD5967A4C57E872919DF1F 1 1E363496C15D2A8E3BDA3343929630A3517040F0A056A578534BFF9BF2D8411C 1 23DABD532DF8CEC9FB3B0BFA3C13F0C28E8B0F1947702C05DDBB9E0C56C0FBAE 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8 1 0000000000000000000000000000000000000000000000000000000000000000\"}}}";
            CredentialDefinition credDefObject = await CredentialDefinitionApi.CreateCredentialDefinitionFromJsonAsync(credDefJson);
            List<string> credentialDefinitions = new() { credDefObject.JsonString };

            (RevocationRegistryDefinition tmpRevRegDefObject,
                _,
                _,
                _) =
                await RevocationApi.CreateRevocationRegistryAsync(
                    issuerDid,
                    credDefObject,
                    "tag",
                    RegistryType.CL_ACCUM,
                    IssuerType.ISSUANCE_BY_DEFAULT,
                    100,
                    testTailsPathForRevocation);
            string revRegDefJson = "{\"ver\":\"1.0\",\"id\":\"55GkHamhTU1ZbTbV2ab9DE:4:55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag:CL_ACCUM:default\",\"revocDefType\":\"CL_ACCUM\",\"tag\":\"default\",\"credDefId\":\"55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag\",\"value\":{\"issuanceType\":\"ISSUANCE_BY_DEFAULT\",\"maxCredNum\":100,\"publicKeys\":{\"accumKey\":{\"z\":\"1 191190AD6D246A0C08A3FD861111646892A92C95D693BD575D3BB7EA2B4DDCB8 1 1569C1150D398CF5D5681C3DFD5DEC4FD2AF4A9FACA1726DA228C9613F0F2A55 1 2461A8627CC49731DF8B333D18427B52DD50BCB0B726AC2CB85706AC2706F102 1 1B5BB5DBA793F19DD8CF3D0FE70AEFBFAFFB5EA3725FC518B50FECC0D3FA7C3B 1 0033682B8C64D970748A23913E7795931D095C10347999CB9D56AB2A1388645F 1 118B0D9DB80D569A0A66E8BA4C26C684BE300D8A43A126C1C59364A649837E00 1 1DFE562E117B1D0986FA749A34311217A29B8CE990390EE474223AA3BB6B5484 1 19E244A5D7CB172B43E03692F23A53E3CD05318CB653BE479CE6CE6BAE71C800 1 1F410C423EC051272438A187D417C6863BD1DAA252B5CE3A77D83D64F3606C41 1 08A422C160DCD70AD18AA1346FB1799309698F52B99280512C7F0AB6D80CD8DD 1 1F59F3BE1CDEA1A3D7A2BB6C4CDBE7E332D29F788A210DF136BB18CB15CBAD06 1 07A871CCC542447BE3398F31570DBA1B366E68C56F1D0141D1392FB0BE76DFFF\"}},\"tailsHash\":\"4kWpJmEMbyukHeLyV3AiounJRH22aaj2kHbQB742Ut6S\",\"tailsLocation\":\"" +
                $"{tmpRevRegDefObject.Value.TailsLocation.Replace("\\", "/")}" +
                "\"}}";
            RevocationRegistryDefinition revRegDefObject = await RevocationApi.CreateRevocationRegistryDefinitionFromJsonAsync(revRegDefJson);
            List<string> revRegDefinitions = new() { revRegDefObject.JsonString };

            string revRegJson = "{\"ver\":\"1.0\",\"value\":{\"accum\":\"21 12A1996B8EBA47C9E7CEA41963DF05F99FDB6E9B381B7C8C34BFD66BE01D1C04B 21 13FB3011CFC5A3BF41923A7F9FA3FA9A89F7169738370CB3386F3E4F4CB9FF29D 6 61492CFCB58CB8F4A17D1F5880C88FCBD6DE1348E05EC7692ACEB4C00592FED5 4 3256E2C0799403B0320EF693D67F48922A6E3B233766AE952CBE516F67F27DE8 6 876F21FBF7A1EE72C77DDE8D462C4195D0DD6CB4E3C76E9AD3948D38956DB6C7 4 26EED3F0DA48A5D5187FDA236C4B8FDAB6DF072CA7C1D18B4F6BFD81D72BE731\"}}";
            RevocationRegistry revRegObject = await RevocationApi.CreateRevocationRegistryFromJsonAsync(revRegJson);
            List<string> revRegistries = new()
            {
                JsonConvert.SerializeObject(new RevocationRegistryEntry
                {
                    DefEntryIdx = 0,
                    Entry = new IntPtr(),
                    Timestamp = timestamp
                })
            };

            string presentationJson = "{\"proof\":{\"proofs\":[{\"primary_proof\":{\"eq_proof\":{\"revealed_attrs\":{\"attr\":\"72155939486846849509759369733266486982821795810448245423168957390607644363272\"},\"a_prime\":\"22787947532665702942166390265565099820863799587030609859364101947347429956006178433736182956558680217451360098070400415842618416586813209500267111752188923151298858777205206383545858632930505476551131457277377652820989619031780149993288820195069310942122913730689119829340750366182236664539501596260514894650983980623701649527364749330998285260704338497630597378327025502438431858516849741910716399356638178657965616831942802640713754838111099765928053264003133303245178307245238269379969657844386541201731429479010183162613731023271140443137949920801708151362109823177071910561747744940225633896803721317669782358421\",\"e\":\"69124141745661451232422071833447747082810057151848226659717004723245660321643866206894483809434372484419787675271515845575181681885576485\",\"v\":\"502483187879133716417096480006386205327995711270888126778869121855278753425593998422963838651347769608503283807649516512880075823986911920658063448216074566307723181816414360454851464182162626545924978908908754080967539118089711545270393292866557588250787116479546341674975985851318791108768074114121210355659608509113854257493677851780112011053516065048781059761361613409853017535033453997896416355511480069057755205220463158986451933726626781714019352189004060699441752283027199173688877273426479931600413051842909026732192148129355030099736087987835032879264302949802085989635543207730170481446080695187580698323015118647125807401471446437413140532020293604881927856270725677607294897411796868074908977878774588588258946655011556630640214442071918831131351471397679508616542608467322074771091661348278136342901709870360635291451103144692305647239100786223515212569094457685892676825400988189774258596242950852693022055\",\"m\":{\"master_secret\":\"2307093772458771039711933057912384843500743528411547595991148149387405897463262936047925219917981120668180047862724912654604570684672680548843949939043365133411527271086042486203\"},\"m2\":\"3350007083872569628769486907745210896506907039493766347465000482213910452825573519528617062976602075306102068308540480111571831600569611213574129530137747\"},\"ge_proofs\":[]},\"non_revoc_proof\":{\"x_list\":{\"rho\":\"1C3AC92D0868D6AD9F124B88E0AA66ED2F3BBC2401AE52A4512CDEC68B82078D\",\"r\":\"0D0387C22AA96E9D00AC180BF461BA40809BE312BB0FC57B392B4660F8AC02C9\",\"r_prime\":\"156475DD528D2A7F6A7711479C40A634F76A82A0AD098DEFC957CF500ADDD6F6\",\"r_prime_prime\":\"1262D4B6EE055CB7B12AC9CCC5A8C5551DC1398D925162FD63B19A7E27E47601\",\"r_prime_prime_prime\":\"1FC79BA981C70CF3FDDBEE8F3908C63F195284DA250B9F4BF0B8C444F00A1DA5\",\"o\":\"0645F0F20692FC3EB7A8AC6DF8975475D69046A3A1AA2F9454309976628CC2D6\",\"o_prime\":\"0E17AE4D4A318BE3F22B8741A9CD401EEF8EDF2571234353DE93C71F91AEB868\",\"m\":\"07AA5FC3DC804398D1E7C087241D13E05F9920BB3E72AE903EF9780BEA53F516\",\"m_prime\":\"22F8D76B8EEC5B8E8AB10B00165CDCB1638733CEC1DAA82D8E2D1DF64EAEA8CE\",\"t\":\"10ACE2A8623DCB973DF484C9634FB4DC94BA9097F89ABA3C5863693F9EC5F223\",\"t_prime\":\"236B85805AD43E4EEA0DE3A03A764BF717805EC5AADEEAD2566B485B7CAC81D1\",\"m2\":\"1BA5E259DB355BC7D8A7DB2B76E6CA77F6BF7EFA08D19E7F533836005F6FBAB3\",\"s\":\"0B1339A60E431657FFDACA6D5C79CC964471B7595B4D34562AAD8E4648E78A82\",\"c\":\"16608A5E3549E73F2F6A32D54D8AE8B0B2DF36DDE1410876BFE20287CB069E63\"},\"c_list\":{\"e\":\"6 38732649A4C651FABABE98F14CF92AF6F2217F528540B3AC499D80D35D494055 4 3F966F47E48797FB2CEA9B6318782FA70B77880DABDCAF2E0C0EA99351EA842F 4 30738E44CA85F08F501F3E7D53D0F87DEB42862769EBD8B13D062A2F33432A93\",\"d\":\"6 474DE704E04EA60150D37B39FEF9C87908CA371D20A2C3509864591699723647 4 04180C1A54CFE8B2225F09DF98469B26DFE703D1E3596E9E17DA0596F9364DBA 4 2CD33754BC31C6DFC3D265B10255B6C768AD491EF4858DACF6464C95F449F3AB\",\"a\":\"6 4B639F55DECFBC5C3DCEF78E043F5056702E9A24FF62BEF53D5C658AEC998640 4 1E87C680AE0FE4C144C02C2C6D9BFBB6319C3F68DAD161FEE52FF7C9E3B7A5E4 4 1B3FDF399DD2CE9416CBCF9F2DF371CCDADDD26C60882AE6DF55921A6009D07A\",\"g\":\"6 42184EA20FFB2329573A01CD5DE5BA48968E8C6CC0840931B31B410AEFE66B87 4 30CF73E43C59393CEB521ADE04CBD1268039521080C2282B459C0A110866F3F0 4 1C52F59F00FEE09872C01AEAE0AB76A15AD2F00CCDB99B73BEC14EFB3BE7A75E\",\"w\":\"21 13A9D8DB787AD569B83F83E3E4D85D01C1FD50E61ABB41BBAF62CEAA157033180 21 12C955A1AB9842C62A7A3A023A6A34EA3B5C966780A00F5175B1CD5D25D87A1FF 6 6608A8E039569F56F4221EA7B4BDEF52DC0D9F073728700ADEA2F695F9DB4853 4 1AE01005403C489AC15C9349BC2D8D43B34D3C43D0D828F238DA4F742B35260B 6 52AB4701947804C7F3B17D7F22F35DE50483B588AF74C5EA122F508D2D634B74 4 073118B2C588D05F7DCDB3D8C2EA322663DAA589F081C36A28C6220B990793CB\",\"s\":\"21 11AA1CEE4FB15EEBAA34BF79C5BA75A0D22064288FE8D64CEEBBABF24F6D37903 21 113CE1CE8533D17F54BEC9490B3FD3601CDBFC8B04B0B8948F930EBA3E67C8CFF 6 55814737228E6B608EE44B894863605890FB81528C7A67FC0857B73FD9438F5B 4 309834112A9A043C0AF59BBC2BC47D72A258FDA266237FD780925D7AB9A6F43C 6 79E87F6D2789925E22F89F0E5E86B15E26B1259B2EAEE8A86AAF8A38D724A123 4 1A8ABBC77FABDF0C3F3223D9D81F51481E28967A33636CD598878F4483F6B6CA\",\"u\":\"21 12846FAFA8E2998E260BE514A492CA4101F0206BE1C46293F736BB027CBCD7D66 21 13581B75D34A2413C50178B4337DA5C8B4056639057DCCF2DDE6563E200BCCFC6 6 64EA09AD4A1E3C6BCC7EBFF9C3AAD7FEFDE17D2D12517FB3644C891D91B579FA 4 31C98A8C89C90961D814CC181F6036C810DDFC9C8D62703DBBC0EEFAEB5EE73B 6 720417974AA223B2375B13837CDB3D5CC2542EDBAF7B9192CB414CD40A191DF1 4 0BCDF714766E1EB0CF923BF54D954ABFF39A2BACACE00E4E2B73AA86B420E8F9\"}}}],\"aggregated_proof\":{\"c_hash\":\"57204176382419527374101370361393524337764538734535251618083733237783975439070\",\"c_list\":[[4,15,88,167,248,56,102,147,80,24,31,234,110,102,161,67,43,125,207,87,179,56,8,40,221,242,70,202,231,69,195,248,205,25,33,90,195,219,255,23,52,105,221,133,186,132,167,223,162,68,186,75,179,189,1,65,254,76,96,125,100,40,5,100,103,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[4,24,49,229,206,52,10,42,52,195,227,91,45,94,133,85,241,105,195,178,217,81,190,25,203,16,149,93,186,44,71,119,190,1,95,192,234,30,175,215,56,136,133,100,233,140,112,56,244,95,204,69,128,213,21,233,94,147,116,37,153,242,52,209,138,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[4,36,122,151,5,113,228,166,236,38,129,222,222,49,212,15,96,103,229,3,25,28,79,37,225,177,177,237,134,6,66,27,119,27,109,135,41,214,232,251,152,184,10,32,74,82,207,40,195,103,231,154,242,187,30,231,218,67,106,190,128,121,155,131,113,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[4,34,66,12,235,45,42,171,239,205,121,131,31,41,208,82,246,226,170,49,76,126,165,225,43,128,172,54,126,88,20,128,19,25,223,36,170,76,242,212,148,63,33,235,54,137,148,140,196,40,65,151,62,212,181,189,243,6,88,75,235,151,114,54,232,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[2,220,197,124,183,58,58,157,142,103,86,147,145,21,36,119,22,117,86,200,99,115,154,196,120,116,80,24,138,230,36,177,30,81,86,105,168,250,240,169,14,34,93,141,158,208,228,138,226,86,195,111,234,35,121,40,171,132,129,53,97,162,190,76,17,198,236,135,230,125,232,215,65,239,184,64,2,185,53,15,32,100,138,152,2,23,237,183,77,53,137,194,42,13,147,224,15,251,118,253,30,27,80,73,207,230,147,236,203,187,117,137,203,187,203,51,255,14,254,57,110,87,140,174,202,247,155,13],[6,4,32,235,21,24,190,211,121,184,190,97,209,193,204,252,84,165,232,105,66,242,136,146,55,150,198,158,138,75,9,42,20,94,45,147,73,55,47,29,238,14,37,159,3,138,207,149,38,169,47,201,12,26,72,119,103,113,204,39,9,237,140,171,30,138,231,25,58,230,106,18,39,185,248,42,210,252,84,207,80,156,11,152,33,1,126,84,180,236,37,83,219,45,36,100,8,178,133,234,236,77,114,247,114,116,96,227,221,115,73,209,254,192,44,154,95,138,75,225,140,115,158,153,117,247,46,120],[16,44,243,209,174,98,186,130,29,56,38,87,123,124,238,122,203,120,31,253,232,19,226,74,204,75,162,51,66,66,45,186,9,6,138,241,64,162,56,111,131,25,104,172,138,235,27,138,139,39,130,101,227,74,72,192,159,162,90,203,62,206,169,103,26,216,166,16,244,46,39,151,224,100,180,118,36,16,47,226,176,52,23,161,20,206,124,249,124,111,169,38,165,248,85,186,20,108,129,30,247,163,11,93,67,207,172,13,205,19,137,217,20,51,12,20,189,188,141,172,216,195,210,122,166,225,240,176],[180,131,235,194,141,158,164,112,75,71,68,181,71,3,222,45,131,147,72,120,36,190,183,145,119,59,94,109,178,83,240,117,200,139,183,234,120,1,110,141,136,165,42,78,208,133,128,142,129,225,90,76,185,141,133,255,168,176,156,198,205,174,109,95,175,11,53,125,245,234,195,119,40,130,113,52,218,73,76,203,177,234,76,126,241,151,118,123,75,50,61,64,142,89,4,43,204,101,133,151,134,232,55,175,118,177,0,30,39,220,62,41,191,69,69,176,24,176,190,231,153,46,1,224,26,77,218,181,22,139,97,9,101,122,140,126,141,227,30,235,59,32,28,156,131,249,221,32,2,105,209,58,36,113,168,135,218,59,229,111,101,184,187,104,186,9,42,160,169,62,171,119,200,240,34,22,132,162,120,81,95,241,137,102,7,179,13,132,28,103,203,185,8,79,241,36,7,197,92,180,229,209,207,178,255,134,226,94,183,236,213,184,164,53,139,255,206,186,94,215,173,161,15,160,235,255,163,132,248,68,184,47,160,1,57,39,255,49,113,237,48,199,71,242,219,32,146,28,55,139,17,40,149,67,9,149]]}},\"requested_proof\":{\"revealed_attrs\":{\"reft\":{\"sub_proof_index\":0,\"raw\":\"test\",\"encoded\":\"72155939486846849509759369733266486982821795810448245423168957390607644363272\"}},\"self_attested_attrs\":{},\"unrevealed_attrs\":{},\"predicates\":{}},\"identifiers\":[{\"schema_id\":\"55GkHamhTU1ZbTbV2ab9DE:2:schema name:schema version\",\"cred_def_id\":\"55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag\",\"rev_reg_id\":\"55GkHamhTU1ZbTbV2ab9DE:4:55GkHamhTU1ZbTbV2ab9DE:3:CL:15:tag:CL_ACCUM:default\",\"timestamp\":1656323735}]}";

            //Act
            Func<Task<bool>> func = async () => await PresentationApi.VerifyPresentationAsync(
                presentationJson,
                presReqJson,
                schemas,
                credentialDefinitions,
                revRegDefinitions,
                revRegistries);

            //Assert
            _ = await func.Should().ThrowAsync<Exception>();
        }
    }
    #endregion
}
