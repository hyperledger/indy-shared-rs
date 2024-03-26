using FluentAssertions;
using indy_shared_rs_dotnet.IndyCredx;
using indy_shared_rs_dotnet.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace indy_shared_rs_dotnet_test.IndyCredx
{
    public class CredentialDefinitionApiTests
    {
        #region Tests for CreateCredentialDefinitionAsync
        [Test, TestCase(TestName = "CreateCredentialDefinition() returns a CredentialDefintion, CredentialDefinitionPrivate and CredentialKeyCorrectnessProof object.")]
        public async Task CreateCredentialDefinitionWorks()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";

            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            //Act
            (CredentialDefinition credDef, CredentialDefinitionPrivate credDefPvt, CredentialKeyCorrectnessProof keyProof) =
                await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            //Assert
            credDef.Should().BeOfType(typeof(CredentialDefinition));
            credDefPvt.Should().BeOfType(typeof(CredentialDefinitionPrivate));
            keyProof.Should().BeOfType(typeof(CredentialKeyCorrectnessProof));
        }

        private static IEnumerable<TestCaseData> CreateCredentialDefinitionCases()
        {
            yield return new TestCaseData(null, null, null, null)
                .SetName("CreateCredentialDefinition() throws SharedRsException if all arguments are null.");
            yield return new TestCaseData(null, "tag", SignatureType.CL, true)
                .SetName("CreateCredentialDefinition() throws SharedRsException if issuerDid is null.");
            yield return new TestCaseData("NcYxiDXkpYi6ov5FcYDi1e", null, SignatureType.CL, true)
                .SetName("CreateCredentialDefinition() throws SharedRsException if tag is null.");
            yield return new TestCaseData("NcYxiDXkpYi6ov5FcYDi1e", "tag", 99, true)
                .SetName("CreateCredentialDefinition() throws SharedRsException if signatureType is invalid.");
        }

        [Test, TestCaseSource(nameof(CreateCredentialDefinitionCases))]
        public async Task CreateCredentialDefinitionThrowsException(string issuerDid, string tag, SignatureType signatureType, bool supportRevocation)
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string schemaIssuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            Schema schemaObject = await SchemaApi.CreateSchemaAsync(schemaIssuerDid, schemaName, schemaVersion, attrNames, 0);

            //Act
            Func<Task> act = async () => await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, tag, signatureType, supportRevocation);

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }
        #endregion

        #region Tests for CreateCredentialDefinitionJsonAsync
        [Test, TestCase(TestName = "CreateCredentialDefinitionJsonAsync() with JSON inputs returns a CredentialDefintion, CredentialDefinitionPrivate and CredentialKeyCorrectnessProof object.")]
        public async Task CreateCredentialDefinitionJsonWorks()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";

            string schemaObjectJson = await SchemaApi.CreateSchemaJsonAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            //Act
            (string credDef, string credDefPvt, string keyProof) =
                await CredentialDefinitionApi.CreateCredentialDefinitionJsonAsync(issuerDid, schemaObjectJson, "tag", SignatureType.CL, true);

            //Assert
            credDef.Should().NotBeNullOrEmpty();
            credDefPvt.Should().NotBeNullOrEmpty();
            keyProof.Should().NotBeNullOrEmpty();
        }

        private static IEnumerable<TestCaseData> CreateCredentialDefinitionJsonCases()
        {
            yield return new TestCaseData(null, null, null, null)
                .SetName("CreateCredentialDefinition() throws SharedRsException if all arguments are null.");
            yield return new TestCaseData(null, "tag", SignatureType.CL, true)
                .SetName("CreateCredentialDefinition() throws SharedRsException if issuerDid is null.");
            yield return new TestCaseData("NcYxiDXkpYi6ov5FcYDi1e", null, SignatureType.CL, true)
                .SetName("CreateCredentialDefinition() throws SharedRsException if tag is null.");
            yield return new TestCaseData("NcYxiDXkpYi6ov5FcYDi1e", "tag", 99, true)
                .SetName("CreateCredentialDefinition() throws SharedRsException if signatureType is invalid.");
        }

        [Test, TestCaseSource(nameof(CreateCredentialDefinitionJsonCases))]
        public async Task CreateCredentialDefinitionJsonThrowsException(string issuerDid, string tag, SignatureType signatureType, bool supportRevocation)
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string schemaIssuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            string schemaObjectJson = await SchemaApi.CreateSchemaJsonAsync(schemaIssuerDid, schemaName, schemaVersion, attrNames, 0);

            //Act
            Func<Task> act = async () => await CredentialDefinitionApi.CreateCredentialDefinitionJsonAsync(issuerDid, schemaObjectJson, tag, signatureType, supportRevocation);

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }
        #endregion

        #region Tests for GetCredentialDefinitionAttributeAsync
        private static IEnumerable<TestCaseData> GetCredentialDefinitionAttributeCases()
        {
            yield return new TestCaseData("schema_id", "NcYxiDXkpYi6ov5FcYDi1e:2:gvt:1.0")
                .SetName("GetCredentialDefinitionAttribute() returns correct schema_id.");
            yield return new TestCaseData("id", "NcYxiDXkpYi6ov5FcYDi1e:3:CL:NcYxiDXkpYi6ov5FcYDi1e:2:gvt:1.0:tag")
                .SetName("GetCredentialDefinitionAttribute() returns correct id.");
        }

        [Test, TestCaseSource(nameof(GetCredentialDefinitionAttributeCases))]
        public async Task GetCredentialDefinitionAttributeAsyncWorks(string tag, string expected)
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            (CredentialDefinition credDefObject, _, _) =
                await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            //Act
            string actual = await CredentialDefinitionApi.GetCredentialDefinitionAttributeAsync(credDefObject, tag);

            //Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Test, TestCase(TestName = "GetCredentialDefinitionAttributeAsync() throws SharedRsException when requested attribute name is invalid.")]
        public async Task GetCredentialDefinitionAttributeAsyncThrowsException()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            Schema schemaObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            (CredentialDefinition credDefObject, _, _) =
                await CredentialDefinitionApi.CreateCredentialDefinitionAsync(issuerDid, schemaObject, "tag", SignatureType.CL, true);

            //Act
            Func<Task> act = async() => await CredentialDefinitionApi.GetCredentialDefinitionAttributeAsync(credDefObject, "blubb");

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }
        #endregion

        #region Tests for CreateCredentialDefinitionFromJsonAsync
        [Test, TestCase(TestName = "CreateCredentialDefinitionFromJsonAsync() creates a definition object from valid json string.")]
        public async Task CreateCredentialDefinitionFromJsonAsyncWorks()
        {
            //Arrange
            string id = "NcYxiDXkpYi6ov5FcYDi1e:3:CL: NcYxiDXkpYi6ov5FcYDi1e:2:gvt:1.0:tag";
            string schemaId = "NcYxiDXkpYi6ov5FcYDi1e:2:gvt:1.0";
            string credDefJson = "{\"ver\":\"1.0\"," +
                $"\"id\":\"{id}\"," +
                $"\"schemaId\":\"{schemaId}\"," +
                "\"type\":\"CL\"," +
                "\"tag\":\"tag\"," +
                "\"value\":{" +
                    "\"primary\":{" +
                        "\"n\":\"88387630363291843445917765608396190961936798680309230745671211332893825896256956632971757671753705574484208863030184880855030850942242534751802411442721896441679849048742184835339568883308819821816756752013580700149722071044285971450799992036021264563441685620635464677834167024263591219020786592373101536485833386932947814653293311141530156364119092292495613625341859715381978889867727158916906179837627651428189772922357950777617739379011795799369115712334147062247911621171957614036173827107675662531728411461770501226843957705975688750446748680253992178942140533916098147123488285175144028175000381409475644240901\"," +
                        "\"s\":\"23181829245453727262202752281869574466997297268473315163437924688010834120667879137929582313725263582274244726141912423294983209986819081973713483853098340413685713879935589142941001950130629521124563635844486653867924691705265114047012918128178803470051438713247537720402063017120311791198234276383248916687617299305940822383542507248339847705445852589580659390481970066842815756085631860798751735059280229823248472720567194787088590129852405279563397137548698562171659275770291739725317756451644421369232002042071689018272701988757306856027908355210838758308330395153467793134715096579746247739641273504200830477650\"," +
                        "\"r\":{" +
                            "\"sex\":\"25253652917203064399819595584467714526083381148613195464053336927939530313859621665757609206032627844228993414064446258879967021265109358376304793236325545667114282530376986269397001888016521473410729465003618461585152122687661583138193628474537856222295530589635414132277163950257929764692489121136849608099075354448141918841539919604512689923146783602397866950973603547610811187401246160345956279995732450097935526982519042897294964528586595301379133023673899073289300971591476448959034122464471391189889976661980662406744399022741003101024138333110651904344956533562049530030805805449367076886521350270921884520470\"," +
                            "\"age\":\"47676529198538252732247368863637385849850935640371349393131712257381015020628130032555969721612792240660424086678789407943537709186709800437915853697023251980007212649761512018166599400778839106760847505324791957139725282647620994121584350814202228947257569529752441137901005093827616577017981574634883760871863243055780288944772802807251055503424792588883418542562504691090037213997583001502557168854607854987501578380252168545675348099338407215670817187006745546597834903809322492588685364367409852051757414141226319912631427260452424993657590629940640588930913142045328632147990918438449333447777520649642249427792\"," +
                            "\"master_secret\":\"45315227521069448830895695179698582600953136568156283013845222308204171278036967216415387359767192663897538508889497403603530298212272044029330441294864167683862250652483752927750151110443719363922551896571816007264474525541102332399613187636576698944273497181249230876806749937181225807387856377822579631736171920626601593213836664776163566975963387156974153445316140580843501719491577933562840640761148823040434668660615870249017744423187957865508757903205975250256340233132551377927467915793919184437349775575542094135365065867127368203874391565485604323180184360286950228803843784781446633213083805270458683372209\"," +
                            "\"gender\":\"24684257775360646450714929514591833019172878520119295845967789351253710904225931159671923010634287492434083098410200694822374546825437540683540882948076121206417518270639224292872387612594401815217154901336340921436184165335310541315342386673829969827881014672978554951266332075662084059710829101404796338639389634479652864134298975470474118001468543258325740843729872962999919075675628699118602191598204860003864033867359716147016317586686748557348330056445233626992295913027771088038347933956334647359254723702924652294776972729655730902107149885209885303180384561632355763045538783172814009768248031846024443020656\"" +
                        "}," +
                        "\"rctxt\":\"30984260297016823694511391969319952340642332028359234246589500986717834568399687005478823467716767793946083836487626719635952605024364418347126836089687603573070617778373916924323275533183746044853829041409442913149252090550061593847724774639970437926149762060050168894717439407238611662061851976759150812314317108327751846393585746667018085888390559303034415533989751647466696983431756366547805592758980110261556598660530912650688219393208587235019766622808211002331128032234278118827162455512847611191283629847398028126533300762196341789872532241363466263211239684956977925791389627676219299629545263329674021433562\"," +
                        "\"z\":\"29630666204701479372167205184249690934738764780518497975791316550250670144960536997564168074927599343545099029170276737943435846097983078067495091211949848232423701558035249306516316180392108973831059692286498156990436057139570808093223195964258980645219479604415601522482378991347615549536530233838415855919446440394820306676311225365358205724871623316124892466081600218188535409964468879206380755678966775062513049033049098398512530437574428644752868646503189532788600331752073134729403669516649629141659075763149349184270317447695442386365529397551695765652997224544239404445588637511437043974095597037000684356397\"" +
                    "}," +
                    "\"revocation\":{" +
                        "\"g\":\"1 1DB2BBADD518370F86D6EA8BA682582ABFE38422E15BDB4918608C7729622608 1 157BD51C9AF4C475887C105299652D6D5CC44B4C60A9DBE6FA2FAF02AEC0F264 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\"," +
                        "\"g_dash\":\"1 18EFFCD4774B81165AE89B49410C59ADBB5AFCA72F049EEB6FAF9E803B5F1891 1 1BFBA9EED3E841C7B524478B39FB3CCB188C5167680E802C52BE1DAC3A9E7D12 1 03D30A73D35A85536936CB1D9FBC69DE4B1C2A18A2BAD0383ACE511FEC5FEAD2 1 04F3CAC2179E47D572CFFA9F21EABDC4D4D008FC05EE0432B255FA97DDF5453E 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8 1 0000000000000000000000000000000000000000000000000000000000000000\"," +
                        "\"h\":\"1 04B54511A419FC0C6B7A95566D570DD101F95A1E93323BE090F7C3232AC758F3 1 237FBB0AC636B13F2E7B2FD9895FD563A906A111834476119F9B3E3526891BA7 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\"," +
                        "\"h0\":\"1 211CA54DA2E8761B99D037E0229B8A755B23595F467C064DA983C3176AA640EF 1 08CD545A2AAFE7C7C91C98C9BB9D9A1DD49CE9D8E9E50810118F949151BD5CC5 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\"," +
                        "\"h1\":\"1 211121B7EB2C3498DEDE19AD7792E08A3F252444B21101693740EAA31A9FB161 1 0DD4A7D2A370DC4E2673033CE707D8914E8B31ED1D587485BA6E423528C7D972 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\"," +
                        "\"h2\":\"1 164DBA3F6EE780C24B37E835820C812D71260C26B591A10ABEA0961326DA3605 1 1B21B179E1F006F166214E4364E0A0945591E9BA14B6CD32DD894E9ED491BA04 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\"," +
                        "\"htilde\":\"1 1EDA2DBB5BC25274E08CC1323B54A9D367CA482013CFB586011DFE69819E9621 1 23474BEB427FA6FE1C52D79849EB9497F358525F5F046AB49D57060A09436D02 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\"," +
                        "\"h_cap\":\"1 1CDF2671AF5846622963367011EDF39D3D26EF1DC01171A85C29551B322A0B9A 1 10DDE09376B12169E1DDEDF0376273316482FF228C3563B9AF028C6735F7626A 1 21E047A5EC8CFB843AD1CC3E2DC6F8FF1F425130B804F1B9FC63534E74F725A2 1 0329C55B4A32C7DA767DD03060FF66D7B4D1ED5D08FA593C685B9A0CC9846CE7 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8 1 0000000000000000000000000000000000000000000000000000000000000000\"," +
                        "\"u\":\"1 220036478FA95A461A7CE1BE3A5B0692ABB2EBCC7C42E2E71A6D30ECF5A3EEA5 1 14BE5BCBBFC3B1DCB320CE5F743FB2668CC1ED5D8B506F1D48A5FA9EA950FE98 1 24A93EE2EC82E6D6B2CD1E8D30DC0D03BE6211198867C6DF6CF8D2F5EDE54B86 1 110108140A367FCC8596F3CF11101D41D2E29C152FAFB352037129BE7939378D 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8 1 0000000000000000000000000000000000000000000000000000000000000000\"," +
                        "\"pk\":\"1 1030E026FAECEAD5E0A6AC9635676F41DBF86940C99DF5F1815501A02D67BE7A 1 20DEA367DBDBE743F835D4BFBCC80C31638A7ED9CE7E9789B0B252FADF64F17F 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8\"," +
                        "\"y\":\"1 21C54EE80A69D9DC919E3B82B280175B48C0D4BF867A7BC276F077A9951A8011 1 120D517BC98448B445ABDA0361FC4B049699F7EDA6845F38D772BF092F582CDB 1 0D4060CBB03404F6E561393A2E36F126323A19FC8B4221802B7447D5E4B583A4 1 1F86927F314B45A810DBE0FD176A9F67B047B9C40268018EB72B9ED937B164F5 2 095E45DDF417D05FB10933FFC63D474548B7FFFF7888802F07FFFFFF7D07A8A8 1 0000000000000000000000000000000000000000000000000000000000000000\"" +
                    "}" +
                "}" +
            "}";

            //Act
            var actual = await CredentialDefinitionApi.CreateCredentialDefinitionFromJsonAsync(credDefJson);

            //Assert
            actual.Should().BeOfType<CredentialDefinition>();
            actual.CredentialDefinitionId.Should().Be(id);
            actual.SchemaId.Should().Be(schemaId);
        }

        [Test, TestCase(TestName = "CreateCredentialDefinitionFromJsonAsync() throws SharedRsException when the provided json string is invalid.")]
        public async Task CreateCredentialDefinitionFromJsonAsyncThrowsException()
        {
            //Arrange
            string credDefJson = "{}";

            //Act
            Func<Task> act = async () => await CredentialDefinitionApi.CreateCredentialDefinitionFromJsonAsync(credDefJson);

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }
        #endregion
    }
}