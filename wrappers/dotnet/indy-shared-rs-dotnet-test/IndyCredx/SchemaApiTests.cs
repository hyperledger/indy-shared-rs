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
    internal class SchemaApiTests
    {
        #region Tests for CreateSchemaAsync
        [Test, TestCase(TestName = "CreateSchemaAsync() creates a valid schema object.")]
        public async Task CreateSchemaWorks()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";

            //Act
            Schema testObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            //Assert
            testObject.Should().BeOfType(typeof(Schema));
        }

        [Test, TestCase(TestName = "CreateSchemaAsync() throws a SharedRsException if no issuerDid is provided.")]
        public async Task CreateSchemaThrowsExceptionForMissingIssuerDid()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "";
            string schemaName = "gvt";
            string schemaVersion = "1.0";

            //Act
            Func<Task> act = async () => await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }

        [Test, TestCase(TestName = "CreateSchemaAsync() throws a InvalidOperationException if no attribute names are provided.")]
        public async Task CreateSchemaThrowsExceptionForMissingAttributeNames()
        {
            //Arrange
            List<string> attrNames = new() { };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";

            //Act
            Func<Task> act = async () => await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
        #endregion

        #region Tests for CreateSchemaAsync
        [Test, TestCase(TestName = "CreateSchemaJsonAsync() creates a valid schema object.")]
        public async Task CreateSchemaJsonAsyncWorks()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";

            //Act
            string testObject = await SchemaApi.CreateSchemaJsonAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            //Assert
            testObject.Should().BeOfType(typeof(string));
        }

        [Test, TestCase(TestName = "CreateSchemaJsonAsync() throws a SharedRsException if no issuerDid is provided.")]
        public async Task CreateSchemaJsonAsyncThrowsExceptionForMissingIssuerDid()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "";
            string schemaName = "gvt";
            string schemaVersion = "1.0";

            //Act
            Func<Task> act = async () => await SchemaApi.CreateSchemaJsonAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }

        [Test, TestCase(TestName = "CreateSchemaJsonAsync() throws a InvalidOperationException if no attribute names are provided.")]
        public async Task CreateSchemaJsonAsyncThrowsExceptionForMissingAttributeNames()
        {
            //Arrange
            List<string> attrNames = new() { };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";

            //Act
            Func<Task> act = async () => await SchemaApi.CreateSchemaJsonAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
        #endregion

        #region Tests for CreateSchemaFromJsonAsync
        [Test, TestCase(TestName = "CreateSchemaFromJsonAsync() returns a Schema object if a valid json string is provided.")]
        public async Task CreateSchemaFromJsonAsyncWorks()
        {
            //Arrange
            string schemaJson = "{" +
               "\"ver\":\"1.0\"," +
               "\"id\":\"55GkHamhTU1ZbTbV2ab9DE:2:schema name:schema version\"," +
               "\"name\":\"schema name\"," +
               "\"version\":\"schema version\"," +
               "\"attrNames\":[\"attr\"]," +
               "\"seqNo\":15" +
               "}";

            //Act
            Schema actual = await SchemaApi.CreateSchemaFromJsonAsync(schemaJson);

            //Assert
            actual.Should().BeOfType<Schema>();
            actual.Id.Should().Be("55GkHamhTU1ZbTbV2ab9DE:2:schema name:schema version");
        }

        [Test, TestCase(TestName = "CreateSchemaFromJsonAsync() throws a IndexOutOfRangeException if an empty json string is provided.")]
        public async Task CreateSchemaFromJsonAsyncThrowsExceptionForEmptyString()
        {
            //Arrange
            string schemaJson = "";

            //Act
            Func<Task> act = async () => await SchemaApi.CreateSchemaFromJsonAsync(schemaJson);

            //Assert
            await act.Should().ThrowAsync<IndexOutOfRangeException>();
        }

        [Test, TestCase(TestName = "CreateSchemaFromJsonAsync() throws a SharedRsException if an invalid json string is provided.")]
        public async Task CreateSchemaFromJsonAsyncThrowsExceptionForInvalidString()
        {
            //Arrange
            string schemaJson = "{}";

            //Act
            Func<Task> act = async () => await SchemaApi.CreateSchemaFromJsonAsync(schemaJson);

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }
        #endregion

        #region Tests for GetSchemaAttributeAsync
        [Test, TestCase(TestName = "GetSchemaAttributeAsync(Schema schema, string attributeName) works for supported attribute names.")]
        public async Task GetSchemaAttributeAsyncWorks()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            Schema testObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);
            string schemaJson = JsonConvert.SerializeObject(testObject);
            Console.WriteLine(schemaJson);
            //Act
            string schemaAttributeId = await SchemaApi.GetSchemaAttributeAsync(testObject, "id"); ////should return id string (only one supported in rust)

            //Assert
            schemaAttributeId.Should().Be(testObject.Id);
        }

        [Test, TestCase(TestName = "GetSchemaAttributeAsync(Schema schema, string attributeName) throws Exception if an unsupported attribute name is provided.")]
        public async Task GetSchemaAttributeAsyncThrowsException()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            Schema testObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            //Act
            Func<Task> act = async () => await SchemaApi.GetSchemaAttributeAsync(testObject, "version"); //should return "" -> not supported in rust

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }

        [Test, TestCase(TestName = "GetSchemaAttributeAsync(string schemaJson, string attributeName) works for supported attribute names.")]
        public async Task GetSchemaAttributeAsyncWorksWithSchemaJson()
        {
            //Arrange
            string testObject = "{\"id\":\"NcYxiDXkpYi6ov5FcYDi1e:2:gvt:1.0\",\"name\":\"gvt\",\"version\":\"1.0\",\"ver\":\"1.0\",\"attrNames\":[\"age\",\"gender\",\"sex\"],\"seqNo\":0}";

            //Act
            string schemaAttributeId = await SchemaApi.GetSchemaAttributeAsync(testObject, "id");
            string expected = JsonConvert.DeserializeObject<Schema>(testObject).Id;

            //Assert
            schemaAttributeId.Should().Be(expected);
        }

        [Test, TestCase(TestName = "GetSchemaAttributeAsync(string schemaJson, string attributeName) throws Exception if an unsupported attribute name is provided.")]
        public async Task GetSchemaAttributeAsyncThrowsExceptionWithSchemaJson()
        {
            //Arrange
            List<string> attrNames = new() { "gender", "age", "sex" };
            string issuerDid = "NcYxiDXkpYi6ov5FcYDi1e";
            string schemaName = "gvt";
            string schemaVersion = "1.0";
            Schema testObject = await SchemaApi.CreateSchemaAsync(issuerDid, schemaName, schemaVersion, attrNames, 0);

            //Act
            Func<Task> act = async () => await SchemaApi.GetSchemaAttributeAsync(testObject, "version"); //should return "" -> not supported in rust

            //Assert
            await act.Should().ThrowAsync<SharedRsException>();
        }
        #endregion
    }
}