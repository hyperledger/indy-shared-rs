using FluentAssertions;
using indy_shared_rs_dotnet.IndyCredx;
using indy_shared_rs_dotnet.Models;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace indy_shared_rs_dotnet_test.IndyCredx
{
    internal class ObjectApiTests
    {
        #region Tests for GetTypeNameAsync
        [Test, TestCase(TestName = "GetTypeNameAsync() returns correct type name.")]
        public async Task GetTypeNameWorks()
        {
            //Arrange
            MasterSecret secretObject = await MasterSecretApi.CreateMasterSecretAsync();

            //Act
            string actual = await ObjectApi.GetTypeNameAsync(secretObject.Handle);

            //Assert
            actual.Should().Be("MasterSecret");

        }

        [Test, TestCase(TestName = "GetTypeNameAsync() throws SharedRsException if object handle is invalid.")]
        public async Task GetTypeNameThrowsException()
        {
            //Arrange

            //Act
            Func<Task> act = async () => await ObjectApi.GetTypeNameAsync(new IntPtr());

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }
        #endregion

        #region Tests for FreeObjectAsync
        [Test, TestCase(TestName = "FreeObjectAsync() works with toJsonAsync() throwing when trying to access json from an invalid handle.")]
        public async Task FreeObjectAsyncWorks()
        {
            //Arrange
            MasterSecret secretObject = await MasterSecretApi.CreateMasterSecretAsync();
            string testJson = await ObjectApi.ToJsonAsync(secretObject.Handle);
            //Act
            await ObjectApi.FreeObjectAsync(secretObject.Handle);
            Func<Task> actual = async () => await ObjectApi.ToJsonAsync(secretObject.Handle);
            //Assert
            testJson.Should().NotBe("");
            await actual.Should().ThrowAsync<Exception>();

        }
        #endregion

        #region Tests for ToJsonAsync
        [Test, TestCase(TestName = "ToJsonAsync() converts an object handle to a correct json.")]
        public async Task ToJsonWorks()
        {
            string expected = "{" +
               "\"ver\":\"1.0\"," +
               "\"id\":\"55GkHamhTU1ZbTbV2ab9DE:2:schema name:schema version\"," +
               "\"name\":\"schema name\"," +
               "\"version\":\"schema version\"," +
               "\"attrNames\":[\"attr\"]," +
               "\"seqNo\":15" +
               "}";
            Schema schemaObject = await SchemaApi.CreateSchemaFromJsonAsync(expected);

            //Act
            string actual = await ObjectApi.ToJsonAsync(schemaObject.Handle);

            //Assert
            actual.Should().BeEquivalentTo(expected);
        }
        #endregion
    }
}