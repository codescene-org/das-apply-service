﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SFA.DAS.ApplyService.Configuration;
using SFA.DAS.ApplyService.Domain.Apply;
using SFA.DAS.ApplyService.InternalApi.Models.Roatp;

namespace SFA.DAS.ApplyService.InternalApi.Infrastructure
{
    public class InternalQnaApiClient:IInternalQnaApiClient
    {
        private readonly ILogger<InternalQnaApiClient> _logger;
        private readonly IQnaTokenService _tokenService;
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _environmentName;

        protected readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        public InternalQnaApiClient(IConfigurationService configurationService, ILogger<InternalQnaApiClient> logger, IQnaTokenService tokenService)
        {
            _logger = logger;
            _tokenService = tokenService;

            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(configurationService.GetConfig().Result.QnaApiAuthentication.ApiBaseAddress);
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.GetToken());

            _environmentName = configurationService.GetEnvironmentName();
        }
        public async Task<string> GetQuestionTag(Guid applicationId, string questionTag)
        {
            var response = await _httpClient.GetAsync($"Applications/{applicationId}/applicationData/{questionTag}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<string>();
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiError = JsonConvert.DeserializeObject<ApiError>(json);
                var apiErrorMessage = apiError?.Message ?? json;

                _logger.LogError($"Error in QnaApiClient.GetQuestionTag() - applicationId {applicationId} | questionTag : {questionTag} | StatusCode : {response.StatusCode} | ErrorMessage: { apiErrorMessage }");
                return null;
            }
        }
        public async Task<Page> GetPageBySectionNo(Guid applicationId, int sequenceNo, int sectionNo, string pageId)
        {
            var response = await _httpClient.GetAsync($"Applications/{applicationId}/sequences/{sequenceNo}/sections/{sectionNo}/pages/{pageId}");

            return await response.Content.ReadAsAsync<Page>();
        }
    }
}
