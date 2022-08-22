using FluentAssertions;
using indy_shared_rs_dotnet.IndyCredx;
using NUnit.Framework;
using System.Threading.Tasks;

namespace indy_shared_rs_dotnet_test.IndyCredx
{
    public class ErrorApiTests
    {
        [Test]
        [TestCase(TestName = "GetCurrentErrorAsync returns the json of an empty error.")]
        public async Task GetCurrentError()
        {
            //Arrange

            //Act
            string expected = "{\"code\":0,\"message\":null}";
            string actual = await ErrorApi.GetCurrentErrorAsync();

            //Assert
            actual.Should().Be(expected);
        }
    }
}