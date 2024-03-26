using indy_shared_rs_dotnet.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static indy_shared_rs_dotnet.Models.Structures;

namespace indy_shared_rs_dotnet.IndyCredx
{
    public static class PresentationRequestApi
    {
        /// <summary>
        /// Generates a new <see cref="string"/> representation of a random nonce.
        /// </summary>
        /// <exception cref="SharedRsException">Throws when nonce can not be generated.</exception>
        /// <returns>New nonce.</returns>
        public static async Task<string> GenerateNonceAsync()
        {
            string result = "";
            int errorCode = NativeMethods.credx_generate_nonce(ref result);
            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }
            return await Task.FromResult(result);
        }

        /// <summary>
        /// Creates a new <see cref="PresentationRequest"/> object from a provided json <see cref="string"/>.
        /// </summary>
        /// <param name="presReqJson">Json string representing a presentation request object.</param>
        /// <exception cref="SharedRsException">Throws when <paramref name="presReqJson"/> is invalid.</exception>
        /// <exception cref="IndexOutOfRangeException">Throws when <paramref name="presReqJson"/> is empty.</exception>
        /// <returns>A new <see cref="PresentationRequest"/>.</returns>
        public static async Task<PresentationRequest> CreatePresReqFromJsonAsync(string presReqJson)
        {
            IntPtr presReqObjectHandle = new IntPtr();
            int errorCode = NativeMethods.credx_presentation_request_from_json(ByteBuffer.Create(presReqJson), ref presReqObjectHandle);
            if (errorCode != 0)
            {
                string error = await ErrorApi.GetCurrentErrorAsync();
                throw SharedRsException.FromSdkError(error);
            }
            PresentationRequest presReq = await CreatePresentationRequestObject(presReqObjectHandle);
            return await Task.FromResult(presReq);
        }

        private static async Task<PresentationRequest> CreatePresentationRequestObject(IntPtr objectHandle)
        {
            string presReqJson = await ObjectApi.ToJsonAsync(objectHandle);
            PresentationRequest presentationRequestObject = JsonConvert.DeserializeObject<PresentationRequest>(presReqJson, Settings.JsonSettings);
            presentationRequestObject.JsonString = presReqJson;

            presentationRequestObject.RequestedAttributes = new Dictionary<string, AttributeInfo>();
            presentationRequestObject.RequestedPredicates = new Dictionary<string, PredicateInfo>();

            JObject presReqJObject = JObject.Parse(presReqJson);

            JToken requestedAttributes = presReqJObject["requested_attributes"];
            foreach (JToken attribute in requestedAttributes)
            {
                string key = attribute.Path.Split('.')[1];
                foreach (JToken element in attribute)
                {
                    try
                    {
                        AttributeInfo info = new AttributeInfo()
                        {
                            Name = element["name"].Value<string>()
                        };
                        if (element["names"] != null)
                        {
                            info.Names = element["names"].ToObject<List<string>>();
                        }
                        info.Restrictions = CreateAttributeFilterList(element["restrictions"]);
                        info.NonRevoked = element["non_revoked"].ToObject<NonRevokedInterval>(); ;
                        presentationRequestObject.RequestedAttributes.Add(key, info);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            JToken requestedPredicates = presReqJObject["requested_predicates"];
            foreach (JToken predicate in requestedPredicates)
            {
                string key = predicate.Path.Split('.')[1];
                foreach (JToken element in predicate)
                {
                    try
                    {
                        PredicateInfo info = new PredicateInfo()
                        {
                            Name = element["name"].Value<string>(),
                            PredicateType = ParsePredicateType(element["p_type"].Value<string>()),
                            PredicateValue = element["p_value"].Value<int>()
                        };
                        if (element["non_revoked"] != null)
                        {
                            info.NonRevoked = element["non_revoked"].ToObject<NonRevokedInterval>();
                        }
                        info.Restrictions = CreateAttributeFilterList(element["restrictions"]);
                        presentationRequestObject.RequestedPredicates.Add(key, info);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            presentationRequestObject.Handle = objectHandle;
            return await Task.FromResult(presentationRequestObject);
        }

        private static List<AttributeFilter> CreateAttributeFilterList(JToken restrictionsElement)
        {
            List<AttributeFilter> filterList = new List<AttributeFilter>();
            if (restrictionsElement.HasValues)
            {
                foreach (JObject restriction in restrictionsElement["$or"].Children<JObject>())
                {
                    IEnumerable<JProperty> properties = restriction.ToString().Contains("$and") ? (IEnumerable<JProperty>)restriction["$and"].Children<JObject>().Properties() : restriction.Properties();
                    string filterJson = "{";
                    foreach (JProperty res in properties)
                    {
                        filterJson += "\"" + res.Name + "\": \"" + res.Value.ToString() + "\",";
                    }
                    filterJson += "}";


                    AttributeFilter filter = JsonConvert.DeserializeObject<AttributeFilter>(filterJson);
                    // Only add filter, if at least one property is not null.
                    if (filter.GetType().GetProperties().Select(prop => prop.GetValue(filter)).Any(value => value != null))
                    {
                        filterList.Add(filter);
                    }
                }
            }
            return filterList;
        }

        private static PredicateTypes ParsePredicateType(string type)
        {
            switch (type)
            {
                case "<": 
                    return PredicateTypes.LT;
                case "<=": 
                    return PredicateTypes.LE;
                case ">": 
                    return PredicateTypes.GT;
                default: 
                    return PredicateTypes.GE;
            }
        }
    }
}