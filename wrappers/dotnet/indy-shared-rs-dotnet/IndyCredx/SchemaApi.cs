using indy_shared_rs_dotnet.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static indy_shared_rs_dotnet.Models.Structures;

namespace indy_shared_rs_dotnet.IndyCredx
{
    public class SchemaApi
    {
        /// <summary>
        /// Creates a new <see cref="Schema"/> object from provided parameters.
        /// </summary>
        /// <param name="originDid">Did of issuer.</param>
        /// <param name="schemaName">Schema name.</param>
        /// <param name="schemaVersion">Version of schema.</param>
        /// <param name="attrNames">Names of the schema attributes.</param>
        /// <param name="seqNo">Sequence number.</param>
        /// <exception cref="SharedRsException">Throws when any parameter is invalid.</exception>
        /// <exception cref="System.InvalidOperationException">Throws when <paramref name="attrNames"/> are empty.</exception>
        /// <returns>A new <see cref="Schema"/> object.</returns>
        public static async Task<Schema> CreateSchemaAsync(
            string originDid, 
            string schemaName, 
            string schemaVersion, 
            List<string> attrNames, 
            long seqNo = -1 //default None 
            )
        {
            IntPtr schemaObjectHandle = new IntPtr();
            int errorCode = NativeMethods.credx_create_schema(FfiStr.Create(originDid), FfiStr.Create(schemaName), FfiStr.Create(schemaVersion), FfiStrList.Create(attrNames), seqNo, ref schemaObjectHandle);

            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }

            Schema schemaObject = await CreateSchemaObjectAsync(schemaObjectHandle);
            return await Task.FromResult(schemaObject);
        }

        /// <summary>
        /// Creates a new <see cref="string"/> schema json from provided parameters.
        /// </summary>
        /// <param name="originDid">Did of issuer.</param>
        /// <param name="schemaName">Schema name.</param>
        /// <param name="schemaVersion">Version of schema.</param>
        /// <param name="attrNames">Names of the schema attributes.</param>
        /// <param name="seqNo">Sequence number.</param>
        /// <exception cref="SharedRsException">Throws when any parameter is invalid.</exception>
        /// <exception cref="System.InvalidOperationException">Throws when <paramref name="attrNames"/> are empty.</exception>
        /// <returns>A new <see cref="string"/> schema json.</returns>
        public static async Task<string> CreateSchemaJsonAsync(
            string originDid, 
            string schemaName, 
            string schemaVersion, 
            List<string> attrNames, 
            long seqNo = -1 //default None
            )
        {
            IntPtr schemaObjectHandle = new IntPtr();
            int errorCode = NativeMethods.credx_create_schema(FfiStr.Create(originDid), FfiStr.Create(schemaName), FfiStr.Create(schemaVersion), FfiStrList.Create(attrNames), seqNo, ref schemaObjectHandle);

            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }

            return await ObjectApi.ToJsonAsync(schemaObjectHandle);
        }

        /// <summary>
        /// Creates a new <see cref="Schema"/> object from json <see cref="string"/>.
        /// </summary>
        /// <param name="schemaJson">Json <see cref="string"/> representing a <see cref="Schema"/> object.</param>
        /// <exception cref="SharedRsException">Throws when provided <paramref name="schemaJson"/> is invalid.</exception>
        /// <exception cref="System.IndexOutOfRangeException">Throws when <paramref name="schemaJson"/> is empty.</exception>
        /// <returns>A new <see cref="Schema"/> object.</returns>
        public static async Task<Schema> CreateSchemaFromJsonAsync(string schemaJson)
        {
            IntPtr schemaObjectHandle = new IntPtr();
            int errorCode = NativeMethods.credx_schema_from_json(ByteBuffer.Create(schemaJson), ref schemaObjectHandle);

            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }

            Schema schemaObject = await CreateSchemaObjectAsync(schemaObjectHandle);
            return await Task.FromResult(schemaObject);
        }

        /// <summary>
        /// Returns the value of a requested attribute from a <see cref="Schema"/> (Only attribute name "id" is supported so far).
        /// </summary>
        /// <param name="schema">The schema from which the attribute is requested.</param>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <exception cref="SharedRsException">Throws if any parameter is invalid.</exception>
        /// <returns>The value of the requested <paramref name="attributeName"/> from the provided <paramref name="schema"/>.</returns>
        public static async Task<string> GetSchemaAttributeAsync(Schema schema, string attributeName)
        {
            string result = "";
            //note: only "id" as attributeName supported so far.
            int errorCode = NativeMethods.credx_schema_get_attribute(schema.Handle, FfiStr.Create(attributeName), ref result);

            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Returns the value of a requested attribute from a <see cref="Schema"/> (Only attribute name "id" is supported so far).
        /// </summary>
        /// <param name="schemaJson">The schema json from which the attribute is requested.</param>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <exception cref="SharedRsException">Throws if any parameter is invalid.</exception>
        /// <returns>The value of the requested <paramref name="attributeName"/> from the provided <paramref name="schema"/>.</returns>
        public static async Task<string> GetSchemaAttributeAsync(string schemaJson, string attributeName)
        {
            string result = "";
            //note: only "id" as attributeName supported so far.
            Schema schema = await CreateSchemaFromJsonAsync(schemaJson);
            int errorCode = NativeMethods.credx_schema_get_attribute(schema.Handle, FfiStr.Create(attributeName), ref result);

            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }

            return await Task.FromResult(result);
        }

        private static async Task<Schema> CreateSchemaObjectAsync(IntPtr objectHandle)
        {
            string schemaJson = await ObjectApi.ToJsonAsync(objectHandle);
            Schema schemaObject = JsonConvert.DeserializeObject<Schema>(schemaJson, Settings.JsonSettings);
            schemaObject.JsonString = schemaJson;
            schemaObject.Handle = objectHandle;
            return await Task.FromResult(schemaObject);
        }
    }
}