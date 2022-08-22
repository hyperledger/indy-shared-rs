using indy_shared_rs_dotnet.Models;
using System.Threading.Tasks;

namespace indy_shared_rs_dotnet.IndyCredx
{
    public static class ModApi
    {
        /// <summary>
        /// Sets the default logger.
        /// </summary>
        /// <exception cref="SharedRsException">Throws when logger can not be set.</exception>
        public static async Task SetDefaultLoggerAsync()
        {
            int errorCode = NativeMethods.credx_set_default_logger();
            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }
        }

        /// <summary>
        /// Gets the current library version.
        /// </summary>
        /// <returns>Version number as <see cref="string"/> representation.</returns>
        public static Task<string> GetVersionAsync()
        {
            string result = NativeMethods.credx_version();
            return Task.FromResult(result);
        }
    }
}