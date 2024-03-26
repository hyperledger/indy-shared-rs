namespace indy_shared_rs_dotnet
{
    public static class ErrorCodeConverter
    {
        /// <summary>
        /// Converts the value of <see cref="ErrorCode"/> to the corresponding <see cref="string"/> representation for the backend.
        /// </summary>
        /// <returns>Matching <see cref="string"/> for each provided code to use in error messages.</returns>
        public static string ToErrorCodeString(this ErrorCode errorCode)
        {
            switch (errorCode)
            {
                case ErrorCode.Success: 
                    return "Success";
                case ErrorCode.Input: 
                    return "Input";
                case ErrorCode.IOError: 
                    return "IOError";
                case ErrorCode.InvalidState: 
                    return "InvalidState";
                case ErrorCode.Unexpected: 
                    return "Unexpected";
                case ErrorCode.CredentialRevoked: 
                    return "CredentialRevoked";
                case ErrorCode.InvalidUserRevocId: 
                    return "InvalidUserRevocId";
                case ErrorCode.ProofRejected: 
                    return "ProofRejected";
                case ErrorCode.RevocationRegistryFull: 
                    return "RevocationRegistryFull";
                default: 
                    return "Unknown error code";
            }
        }
    }

    /// <summary>
    /// The error codes defined in the backend.
    /// </summary>
    public enum ErrorCode
    {
        Success = 0,
        Input = 1,
        IOError = 2,
        InvalidState = 3,
        Unexpected = 4,
        CredentialRevoked = 5,
        InvalidUserRevocId = 6,
        ProofRejected = 7,
        RevocationRegistryFull = 8,
    }
}