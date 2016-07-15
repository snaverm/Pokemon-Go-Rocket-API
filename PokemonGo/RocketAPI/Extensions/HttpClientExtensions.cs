using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.GeneratedCode;

namespace PokemonGo.RocketAPI.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<V> PostProto<T, V>(this HttpClient client, string url, T request) where T : IMessage<T> where V : IMessage<V>, new()
        {
            //Encode message and send
            var data = request.ToByteArray();
            var result = await client.PostAsync(url, new ByteArrayContent(data));

            //Decode message
            var responseData = await result.Content.ReadAsByteArrayAsync();
            var codedStream = new CodedInputStream(responseData);
            var decodedResponse = new V();
            decodedResponse.MergeFrom(codedStream);
                
            return decodedResponse;
        }
    }
}
