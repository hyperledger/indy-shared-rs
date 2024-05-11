using FluentAssertions;
using indy_shared_rs_dotnet.IndyCredx;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace indy_shared_rs_dotnet_test.IndyCredx
{
    public class ModApiTests
    {
        #region Tests for SetDefaultLoggerAsync
        [Test, TestCase(TestName = "SetDefaultLoggerAsync() does not throw an exception.")]
        public async Task SetDefaultLoggerAsyncWorks()
        {
            //Arrange

            //Act
            Func<Task> act = async () => await ModApi.SetDefaultLoggerAsync();

            //Assert
            await act.Should().NotThrowAsync<Exception>();
        }
        #endregion

        #region Tests for GetVersionAsync
        [Test, TestCase(TestName = "GetVersionAsync() returns a string that is not empty.")]
        public async Task GetVersion()
        {
            //Arrange

            //Act
            string actual = await ModApi.GetVersionAsync();

            //Assert
            actual.Should().NotBeEmpty();
        }
        #endregion
    }
}