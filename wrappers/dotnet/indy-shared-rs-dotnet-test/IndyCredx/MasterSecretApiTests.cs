using FluentAssertions;
using indy_shared_rs_dotnet.IndyCredx;
using indy_shared_rs_dotnet.Models;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace indy_shared_rs_dotnet_test.IndyCredx
{
    public class MasterSecretApiTests
    {
        [Test]
        [TestCase(TestName = "CreateMasterSecret does not throw an exception.")]
        public async Task CreateMasterSecretNoThrow()
        {
            //Arrange

            //Act
            Func<Task> act = async () => { await MasterSecretApi.CreateMasterSecretAsync(); };

            //Assert
            await act.Should().NotThrowAsync();
        }

        [Test]
        [TestCase(TestName = "CreateMasterSecret works.")]
        public async Task CreateMasterSecretWorks()
        {
            //Arrange

            //Act
            MasterSecret testObject = await MasterSecretApi.CreateMasterSecretAsync();

            //Assert
            testObject.Should().BeOfType(typeof(MasterSecret));
            testObject.Value.Ms.Should().NotBeNull();
        }

        [Test]
        [TestCase(TestName = "CreateMasterSecret returns a master secret as JSON string.")]
        public async Task CreateMasterSecretJsonWorks()
        {
            //Arrange

            //Act
            string testObject = await MasterSecretApi.CreateMasterSecretJsonAsync();

            //Assert
            testObject.Should().NotBeNullOrEmpty();
        }
    }
}