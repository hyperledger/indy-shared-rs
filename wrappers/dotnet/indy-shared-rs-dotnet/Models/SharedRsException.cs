using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace indy_shared_rs_dotnet.Models
{
    public class SharedRsException : Exception
    {
        public SharedRsException(string message) : base(message)
        {

        }

        public static SharedRsException FromSdkError(string message)
        {
            string msg = JsonConvert.DeserializeObject<Dictionary<string, string>>(message)["message"];
            string errCode = JsonConvert.DeserializeObject<Dictionary<string, string>>(message)["code"];
            return int.TryParse(errCode, out int errCodeInt)
                ? new SharedRsException(
                    $"'{((ErrorCode)errCodeInt).ToErrorCodeString()}' error occured with ErrorCode '{errCode}' : {msg}.")
                : new SharedRsException("An unknown error code was received.");
        }
    }
}