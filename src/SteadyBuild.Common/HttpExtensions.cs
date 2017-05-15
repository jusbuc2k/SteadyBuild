using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild
{
    public static class HttpExtensions
    {
        /// <summary>
        /// Deserializes the given JSON http response content into the given object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <returns></returns>
        public async static Task<T> ReadJsonAsAsync<T>(this HttpContent content)
        {
            var json = await content.ReadAsStringAsync();
            var serializer = new Newtonsoft.Json.JsonSerializer();
            T result;

            using (var reader = new System.IO.StreamReader(await content.ReadAsStreamAsync()))
            {
                result = (T)serializer.Deserialize(reader, typeof(T));
            }

            return result;            
        }
    }
}
