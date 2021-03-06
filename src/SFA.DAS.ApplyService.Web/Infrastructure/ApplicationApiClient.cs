using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApplyService.Application.Apply;
using SFA.DAS.ApplyService.Application.Apply.Start;
using SFA.DAS.ApplyService.Application.Apply.Submit;
using SFA.DAS.ApplyService.Configuration;
using SFA.DAS.ApplyService.Domain.Apply;
using SFA.DAS.ApplyService.Domain.Entities;
using SFA.DAS.ApplyService.InternalApi.Types;

namespace SFA.DAS.ApplyService.Web.Infrastructure
{
    using SFA.DAS.ApplyService.Application.Apply.Roatp;
    using SFA.DAS.ApplyService.Domain.Roatp;

    public class ApplicationApiClient : IApplicationApiClient
    {
        private readonly ILogger<ApplicationApiClient> _logger;
        private readonly ITokenService _tokenService;
        private static readonly HttpClient _httpClient = new HttpClient();

        public ApplicationApiClient(IConfigurationService configurationService, ILogger<ApplicationApiClient> logger, ITokenService tokenService)
        {
            _logger = logger;
            _tokenService = tokenService;
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(configurationService.GetConfig().Result.InternalApi.Uri);
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.GetToken());
        }

        public async Task<Guid> StartApplication(StartApplicationRequest startApplicationRequest)
        {
            var httpResponse = await _httpClient.PostAsJsonAsync("/Application/Start", startApplicationRequest);
            var startApplicationResponse = await httpResponse.Content.ReadAsAsync<Guid>();
            return startApplicationResponse;
        }

        public async Task<bool> SubmitApplication(SubmitApplicationRequest submitApplicationRequest)
        {
            var httpResponse = await _httpClient.PostAsJsonAsync("/Application/Submit", submitApplicationRequest);
            var submitApplicationResponse = await httpResponse.Content.ReadAsAsync<bool>();
            return submitApplicationResponse;
        }

        public async Task<bool> ChangeProviderRoute(ChangeProviderRouteRequest changeProviderRouteRequest)
        {
            var httpResponse = await _httpClient.PostAsJsonAsync("/Application/ChangeProviderRoute", changeProviderRouteRequest);
            var changeProviderRouteResponse = await httpResponse.Content.ReadAsAsync<bool>();
            return changeProviderRouteResponse;
        }

        public async Task<Domain.Entities.Apply> GetApplication(Guid applicationId)
        {
            return await (await _httpClient.GetAsync($"Application/{applicationId}")).Content
                .ReadAsAsync<Domain.Entities.Apply>();
        }

        public async Task<List<Domain.Entities.Apply>> GetApplications(Guid userId, bool createdBy)
        {
            if (!createdBy)
            {
                return await (await _httpClient.GetAsync($"/Applications/{userId}/Organisation")).Content
                .ReadAsAsync<List<Domain.Entities.Apply>>();
            }

            return await (await _httpClient.GetAsync($"/Applications/{userId}")).Content
                .ReadAsAsync<List<Domain.Entities.Apply>>();
        }

        public async Task<string> GetApplicationStatus(Guid applicationId, int standardCode)
        {
            return await(await _httpClient.GetAsync(
                $"Application/{applicationId}/standard/{standardCode}/check-status")).Content.ReadAsStringAsync();
        }

        public async Task<List<StandardCollation>> GetStandards()
        {
            return await(await _httpClient.GetAsync("all-standards")).Content.ReadAsAsync<List<StandardCollation>>();
        }

        public async Task<List<Option>> GetQuestionDataFedOptions(string dataEndpoint)
        {
            return await(await _httpClient.GetAsync($"QuestionOptions/{dataEndpoint}")).Content.ReadAsAsync<List<Option>>();
        }

        public async Task DeleteFile(Guid applicationId, Guid userId, int sequenceId, int sectionId, string pageId, string questionId)
        {
            await _httpClient.PostAsJsonAsync($"/DeleteFile", new {applicationId, userId, sequenceId, sectionId, pageId, questionId});
        }

        public async Task<Organisation> GetOrganisationByUserId(Guid userId)
        {
            return await(await _httpClient.GetAsync($"organisations/userId/{userId}")).Content.ReadAsAsync<Organisation>();
        }

        public async Task<Organisation> GetOrganisationByUkprn(string ukprn)
        {
            return await (await _httpClient.GetAsync($"organisations/ukprn/{ukprn}")).Content.ReadAsAsync<Organisation>();
        }

        public async Task<Organisation> GetOrganisationByName(string name)
        {
            return await (await _httpClient.GetAsync($"organisations/name/{WebUtility.UrlEncode(name)}")).Content.ReadAsAsync<Organisation>();
        }
        public async Task<IEnumerable<RoatpApplicationStatus>> GetExistingApplicationStatus(string ukprn)
        {
            return await (await _httpClient.GetAsync($"/Applications/Existing/{ukprn}")).Content.ReadAsAsync<IEnumerable<RoatpApplicationStatus>>();
        }

        public async Task<bool> UpdateApplicationStatus(Guid applicationId, string applicationStatus)
        {
            var httpResponse = await _httpClient.PostAsJsonAsync($"/Application/Status", new { applicationId, applicationStatus });
            return await httpResponse.Content.ReadAsAsync<bool>();            
        }
    }
}