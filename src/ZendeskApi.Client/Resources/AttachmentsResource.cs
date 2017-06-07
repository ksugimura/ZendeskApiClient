﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ZendeskApi.Contracts.Models;
using ZendeskApi.Contracts.Requests;
using ZendeskApi.Contracts.Responses;

namespace ZendeskApi.Client.Resources
{
    /// <summary>
    /// <see cref="https://developer.zendesk.com/rest_api/docs/core/attachments"/>
    /// </summary>
    public class AttachmentsResource : IAttachmentsResource
    {
        private const string AttachmentsResourceUri = "api/v2/attachments";
        private const string UploadsResourceUri = "api/v2/uploads";

        private readonly IZendeskApiClient _apiClient;
        private readonly ILogger _logger;

        private Func<ILogger, string, IDisposable> _loggerScope =
            LoggerMessage.DefineScope<string>("GroupsResource: {0}");

        public AttachmentsResource(IZendeskApiClient apiClient,
            ILogger logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<Upload> UploadAsync(UploadRequest request, string token = null)
        {
            using (_loggerScope(_logger, "UploadAsync"))
            using (var client = _apiClient.CreateClient())
            {
                var response = await client.PostAsBinaryAsync(
                    UploadsResourceUri + $"?filename={request.FileName}&token={token}", 
                    request.InputStream, 
                    request.FileName).ConfigureAwait(false);

                if (response.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    throw new HttpRequestException(
                        $"Status code retrieved was {response.StatusCode} and not a 201 as expected" +
                        Environment.NewLine +
                        "See: https://developer.zendesk.com/rest_api/docs/core/groups#create-groups");
                }

                return (await response.Content.ReadAsAsync<UploadResponse>()).Item;
            }
        }

        public async Task DeleteAsync(string token)
        {
            using (_loggerScope(_logger, $"DeleteAsync({token})"))
            using (var client = _apiClient.CreateClient(UploadsResourceUri))
            {
                var response = await client.DeleteAsync(token);

                if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
                {
                    throw new HttpRequestException(
                        $"Status code retrieved was {response.StatusCode} and not a 204 as expected" +
                        Environment.NewLine +
                        "See: https://developer.zendesk.com/rest_api/docs/core/attachments#delete-upload");
                }
            }
        }

    }
}
