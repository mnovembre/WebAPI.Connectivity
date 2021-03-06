using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using PageJaunesResto.WebAPI.Connectivity.Framework.Helpers;

namespace PageJaunesResto.WebAPI.Connectivity.Framework.RequestCommands.RequestCommands.Http
{
    public class PostHttpRequestBuilderCommand : IRequestBuilderCommand
    {
        private readonly string _methodName;
        private readonly IRequestSerializer _requestSerializer;

        public PostHttpRequestBuilderCommand(string methodName, IRequestSerializer requestSerializer)
        {
            _methodName = methodName;
            _requestSerializer = requestSerializer;
        }

        public async Task<TReturnType> BuildRequest<TReturnType>(string url, params KeyValuePair<string, object>[] parameters)
        {
            var result = await MakeRequest(url, parameters);

            return _requestSerializer.DeserializeObject<TReturnType>(await result.Content.ReadAsStringAsync());
        }

        public async Task BuildRequest(string url, params KeyValuePair<string, object>[] parameters)
        {
            await MakeRequest(url, parameters);
        }

        private async Task<HttpResponseMessage> MakeRequest(string url, KeyValuePair<string, object>[] parameters)
        {
            var request = new HttpClient();
            Uri uri = new Uri(url);

            uri = new Uri(uri + _methodName.ToLower());


            if (parameters.Any())
                uri = UriBuildingHelpers.AttachParameters(uri,
                    parameters.Where(UriBuildingHelpers.IsSimpleType)
                        .Select(x => new KeyValuePair<string, string>(x.Key, UriBuildingHelpers.SimpleTypeToString(x))).ToArray());

            var postItem = parameters.FirstOrDefault(x => !UriBuildingHelpers.IsSimpleType(x));

            Debug.WriteLine(uri.ToString() + "\r\n " +
                            parameters.Aggregate(string.Empty, (x, y) => x + (y.Key + " " + y.Value + "\r\n")));

            var content = new StringContent(_requestSerializer.SerializeObject(postItem.Value), Encoding.UTF8, "application/json");

            return await request.PostAsync(uri, content);
        }

    }
}