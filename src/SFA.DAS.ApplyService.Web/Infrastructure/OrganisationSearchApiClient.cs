﻿using SFA.DAS.ApplyService.Configuration;
using SFA.DAS.ApplyService.InternalApi.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace SFA.DAS.ApplyService.Web.Infrastructure
{
    public class OrganisationSearchApiClient
    {
        private readonly ILogger<OrganisationSearchApiClient> _logger;
        private readonly ITokenService _tokenService;
        private static readonly HttpClient _httpClient = new HttpClient();

        public OrganisationSearchApiClient(IConfigurationService configurationService, ILogger<OrganisationSearchApiClient> logger, ITokenService tokenService)
        {
            _logger = logger;
            _tokenService = tokenService;
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(configurationService.GetConfig().Result.InternalApi.Uri);
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.GetToken());
        }

        public async Task<IEnumerable<OrganisationSearchResult>> SearchOrganisation(string searchTerm)
        {
            return await (await _httpClient.GetAsync($"/OrganisationSearch?searchTerm={searchTerm}")).Content.ReadAsAsync<IEnumerable<OrganisationSearchResult>>();
        }

        public async Task<OrganisationSearchResult> GetOrganisationByEmail(string email)
        {
            _logger.LogInformation($"Calling OrganisationSearch/email from: {_httpClient.BaseAddress}/OrganisationSearch/email/{email}");
            
            var httpResponseMessage = await _httpClient.GetAsync($"/OrganisationSearch/email/{WebUtility.UrlEncode(email)}");

            var responseAsString = await httpResponseMessage.Content.ReadAsStringAsync();
            
            _logger.LogInformation($"Content received from OrganisationSearch/email: {responseAsString}");

            return JsonConvert.DeserializeObject<OrganisationSearchResult>(responseAsString);
            
//            return await httpResponseMessage.Content
//                .ReadAsAsync<OrganisationSearchResult>();
        }

        public async Task<IEnumerable<OrganisationType>> GetOrganisationTypes()
        {
            var types = await (await _httpClient.GetAsync($"/OrganisationTypes")).Content.ReadAsAsync<IEnumerable<OrganisationType>>();

            return types?.OrderBy(t => t.Type.Equals("Public Sector", StringComparison.InvariantCultureIgnoreCase)).AsEnumerable();
        }

        public async Task<bool> IsCompanyActivelyTrading(string companyNumber)
        {
            return await (await _httpClient.GetAsync($"/OrganisationSearch/{companyNumber}/isActivelyTrading")).Content.ReadAsAsync<bool>();
        }
    }
}
