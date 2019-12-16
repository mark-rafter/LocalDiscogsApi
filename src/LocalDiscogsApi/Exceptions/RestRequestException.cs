using System;
using System.Net.Http;

namespace LocalDiscogsApi.Exceptions
{
    public class RestRequestException : Exception
    {
        // todo: refactor how content is passed in
        public RestRequestException(HttpResponseMessage response, string content)
            : base(BuildMessage(response.RequestMessage, response, content))
        {
        }

        private static string BuildMessage(HttpRequestMessage request, HttpResponseMessage response, string content) =>
            $"{request?.Method?.ToString() ?? "Unknown"} Request: '{request?.RequestUri}' failed. " +
            $"Status Code: {response.StatusCode}. " +
            $"Reason Phrase: {response.ReasonPhrase}. " +
            $"Content: {content}.";
    }
}
