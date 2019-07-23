namespace SFA.DAS.ApplyService.Web.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Domain.Roatp;
    using Microsoft.Extensions.Logging;
    using SFA.DAS.ApplyService.Configuration;

    public class RoatpApiClient : IRoatpApiClient
    {
        private readonly ILogger<RoatpApiClient> _logger;
        private static readonly HttpClient _httpClient = new HttpClient();

        public RoatpApiClient(IConfigurationService configurationService, ILogger<RoatpApiClient> logger)
        {
            _logger = logger;
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(configurationService.GetConfig().Result.InternalApi.Uri);
            }
        }

        public async Task<IEnumerable<Domain.Roatp.ApplicationRoute>> GetApplicationRoutes()
        {
            return await (await _httpClient.GetAsync($"/all-roatp-routes")).Content
                .ReadAsAsync<IEnumerable<ApplicationRoute>>();
        }

        public async Task<OrganisationRegisterStatus> UkprnOnRegister(long ukprn)
        {
            return await (await _httpClient.GetAsync($"/ukprn-on-register?ukprn={ukprn}")).Content
                .ReadAsAsync<OrganisationRegisterStatus>();
        }
        
    }
}