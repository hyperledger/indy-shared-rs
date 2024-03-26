using System;

namespace indy_shared_rs_dotnet.Models
{
    public class CredentialEntry
    {
        public IntPtr CredentialObjectHandle;
        public long Timestamp;
        public IntPtr RevStateObjectHandle;

        /** Timestamp and revocationStateObject are optional parameters. Either timestamp and revocation state must be presented, or neither.
         * ´param name="credentialObject" : Credential object
         *  param name="timestamp" : Value of -1 corresponds to None value
         *  param name="revocationStateObject" : CredentialRevocationState object
         **/
        public static CredentialEntry CreateCredentialEntry(Credential credentialObject, long timestamp = -1, CredentialRevocationState revocationStateObject = null)
        {
            CredentialEntry entry = new CredentialEntry();
            entry.CredentialObjectHandle = credentialObject.Handle;
            if (timestamp == 0 || revocationStateObject == null)
            {
                entry.Timestamp = -1;
                entry.RevStateObjectHandle = new IntPtr();
            }
            if (revocationStateObject != null)
            {
                entry.Timestamp = timestamp;
                entry.RevStateObjectHandle = revocationStateObject.Handle;
            }
            return entry;
        }
    }
}