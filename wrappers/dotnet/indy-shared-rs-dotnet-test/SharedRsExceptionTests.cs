using FluentAssertions;
using indy_shared_rs_dotnet.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace indy_shared_rs_dotnet_test
{
    public class SharedRsExceptionTests
    {
        private static IEnumerable<TestCaseData> CreateErrorCodeCases()
        {
            yield return new TestCaseData("message matching to rust errorCode", "0", "Success")
                .SetName("SharedRsExceptions contains ErrorCode 'Success' text after parsing the code to string.");
            yield return new TestCaseData("message matching to rust errorCode", "1", "Input")
                .SetName("SharedRsExceptions contains ErrorCode 'Input' text after parsing the code to string.");
            yield return new TestCaseData("message matching to rust errorCode", "2", "IOError")
                .SetName("SharedRsExceptions contains ErrorCode 'IOError' text after parsing the code to string.");
            yield return new TestCaseData("message matching to rust errorCode", "3", "InvalidState")
                .SetName("SharedRsExceptions contains ErrorCode 'InvalidState' text after parsing the code to string.");
            yield return new TestCaseData("message matching to rust errorCode", "4", "Unexpected")
                .SetName("SharedRsExceptions contains ErrorCode 'Unexpected' text after parsing the code to string.");
            yield return new TestCaseData("message matching to rust errorCode", "5", "CredentialRevoked")
                .SetName("SharedRsExceptions contains ErrorCode 'CredentialRevoked' text after parsing the code to string.");
            yield return new TestCaseData("message matching to rust errorCode", "6", "InvalidUserRevocId")
                .SetName("SharedRsExceptions contains ErrorCode 'InvalidUserRevocId' text after parsing the code to string.");
            yield return new TestCaseData("message matching to rust errorCode", "7", "ProofRejected")
                .SetName("SharedRsExceptions contains ErrorCode 'ProofRejected' text after parsing the code to string.");
            yield return new TestCaseData("message matching to rust errorCode", "8", "RevocationRegistryFull")
                .SetName("SharedRsExceptions contains ErrorCode 'RevocationRegistryFull' text after parsing the code to string.");
            yield return new TestCaseData("no message", "99", "Unknown error code")
                .SetName("SharedRsExceptions contains 'Unknown error code' text after trying to parse an unknown errorCode.");
            yield return new TestCaseData("no message", "xyz", "An unknown error code was received.")
                .SetName("SharedRsExceptions contains 'An unknown error code was received' text after trying to parse an non integer errorCode.");
        }

        [Test, TestCaseSource(nameof(CreateErrorCodeCases))]
        public Task SharedRsExceptionsRightMessages(string testMessage, string errorCode, string expected)
        {
            //Arrange
            string testErrorMessage = $"{{\"code\":\"{errorCode}\",\"message\":\"{testMessage}\"}}";

            //Act
            SharedRsException testException = SharedRsException.FromSdkError(testErrorMessage);
            string actual;
            if (errorCode != "xyz")
            {
                actual = testException.Message.Substring(1, expected.Length);
            }
            else
            {
                actual = testException.Message;
            }

            //Assert
            actual.Should().Be(expected);
            return Task.CompletedTask;
        }
    }
}