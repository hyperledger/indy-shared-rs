using indy_shared_rs_dotnet.Models;
using System;
using System.Text;
using System.Threading.Tasks;
using static indy_shared_rs_dotnet.Models.Structures;

namespace indy_shared_rs_dotnet.IndyCredx
{
    public static class ObjectApi
    {
        /// <summary>
        /// Returns the typename of an object <see cref="string"/> representation from its handle.
        /// </summary>
        /// <param name="objectHandle">The handle of the specific object.</param>
        /// <exception cref="SharedRsException">Throws when <paramref name="objectHandle"/> is invalid.</exception>
        /// <returns>The typename of the object.</returns>
        public static async Task<string> GetTypeNameAsync(IntPtr objectHandle)
        {
            return await ObjectGetTypeNameAsync(objectHandle);
        }

        /// <summary>
        /// Removes handle from object.
        /// </summary>
        /// <param name="objectHandle">Object of which the handle is to be removed.</param>
        public static Task FreeObjectAsync(IntPtr objectHandle)
        {
            NativeMethods.credx_object_free(objectHandle);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns the json <see cref="string"/> representation of an object from its handle.
        /// </summary>
        /// <param name="objectHandle">The handle of the specific object.</param>
        /// <returns>The json serialization of the object.</returns>
        public static unsafe Task<string> ToJsonAsync(IntPtr objectHandle)
        {
            ByteBuffer byteBuffer = ObjectGetJsonAsync(objectHandle).GetAwaiter().GetResult();
            string decoded = DecodeToStringAsync(byteBuffer).GetAwaiter().GetResult();
            return Task.FromResult(decoded);
        }

        private static async Task<string> ObjectGetTypeNameAsync(IntPtr handle)
        {
            string result = "";
            int errorCode = NativeMethods.credx_object_get_type_name(handle, ref result);
            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }
            return await Task.FromResult(result);
        }

        private static unsafe Task<ByteBuffer> ObjectGetJsonAsync(IntPtr handle)
        {
            ByteBuffer result = new ByteBuffer()
            {
                len = 0,
                value = null
            };

            int errorCode = NativeMethods.credx_object_get_json(handle, ref result);
            if (errorCode != 0)
            {
                string error = ErrorApi.GetCurrentErrorAsync().GetAwaiter().GetResult();
                throw SharedRsException.FromSdkError(error);
            }
            return Task.FromResult(result);
        }

        private static unsafe Task<string> DecodeToStringAsync(ByteBuffer byteBuffer)
        {
            char[] charArray = new char[byteBuffer.len];
            UTF8Encoding utf8Decoder = new UTF8Encoding(true, true);

            fixed (char* char_ptr = &charArray[0])
            {
                _ = utf8Decoder.GetChars(byteBuffer.value, (int)byteBuffer.len, char_ptr, (int)byteBuffer.len);
            }
            return Task.FromResult(new string(charArray));
        }
    }
}