using System;
using System.Collections.Generic;

namespace indy_shared_rs_dotnet.Models
{
    public class CredentialRevocationConfig
    {
        public IntPtr RevRegDefObjectHandle;
        public IntPtr RevRegDefPvtObjectHandle;
        public IntPtr RevRegObjectHandle;
        public long RegIdx;
        public List<long> RegUsed;
        public string TailsPath;
    }
}