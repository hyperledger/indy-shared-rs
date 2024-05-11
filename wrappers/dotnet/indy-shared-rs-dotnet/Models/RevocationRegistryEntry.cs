using System;

namespace indy_shared_rs_dotnet.Models
{
    public class RevocationRegistryEntry
    {
        public long DefEntryIdx { get; set; }
        public IntPtr Entry { get; set; }
        public long Timestamp { get; set; }
    }
}