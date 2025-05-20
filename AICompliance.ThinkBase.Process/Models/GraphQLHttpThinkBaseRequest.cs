// Copyright (c) 2025 AI Compliance inc. Licensed under the MIT License.
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using System.Net.Http.Headers;
using System.Text;

namespace AICompliance.ThinkBase.Process.Models
{
    public class GraphQLHttpThinkBaseRequest : GraphQLHttpRequest
    {
        public string apikey { get; set; } = string.Empty;

        public override HttpRequestMessage ToHttpRequestMessage(GraphQLHttpClientOptions options, IGraphQLJsonSerializer serializer)
        {
            if (apikey is null )
                throw new ArgumentException(nameof(apikey));

            var message = new HttpRequestMessage(HttpMethod.Post, options.EndPoint)
            {
                Content = new StringContent(serializer.SerializeToString(this), Encoding.UTF8, options.MediaType)
            };
            message.Headers.Authorization = new AuthenticationHeaderValue("x-api-key", $"{apikey}");
            message.Headers.Add("x-api-key", $"{apikey}");
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/graphql-response+json"));
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            message.Headers.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
            return message;

        }
    }
}
