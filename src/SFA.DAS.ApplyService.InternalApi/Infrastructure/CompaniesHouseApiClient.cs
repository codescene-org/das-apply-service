﻿using AutoMapper;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApplyService.Configuration;
using SFA.DAS.ApplyService.InternalApi.Models.CompaniesHouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.ApplyService.InternalApi.Infrastructure
{
    /// <summary>
    /// Companies House API docs are located at: https://developer.companieshouse.gov.uk/api/docs/index.html
    /// There is a Web-Friendly version located at: https://beta.companieshouse.gov.uk/
    /// </summary>
    public class CompaniesHouseApiClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<CompaniesHouseApiClient> _logger;
        private readonly IApplyConfig _config;

        public CompaniesHouseApiClient(HttpClient client, ILogger<CompaniesHouseApiClient> logger, IConfigurationService configurationService)
        {
            _client = client;
            _logger = logger;
            _config = configurationService.GetConfig().Result;
        }

        private AuthenticationHeaderValue GetBasicAuthHeader()
        {
            var bytes = Encoding.ASCII.GetBytes($"{_config.CompaniesHouseApiAuthentication.ApiKey}:");
            var token = Convert.ToBase64String(bytes);
            return new AuthenticationHeaderValue("Basic", token);
        }

        private async Task<T> Get<T>(string uri)
        {
            _client.DefaultRequestHeaders.Authorization = GetBasicAuthHeader();

            using (var response = await _client.GetAsync(new Uri(uri, UriKind.Relative)))
            {
                return await response.Content.ReadAsAsync<T>();
            }
        }

        public async Task GetTheDetails(string companyNumber)
        {
            // Company Details
            //Registered Address
            //Company status
            //Company type
            //Nature of business <-- Sic codes
            //Parent company <-- not getting this
            //Incorporation date
            // Previous company names??? date period from - to  <-- is this something we want

            // Financial Accounts
            //Year end date <-- this is in company details
            //Abridge accounts <-- not sure if this is in company details. <-- last account date
            //Confirmation statement <-- this is in company details



            // Current Officers
            // Name
            // Address
            // Date of birth <-- only month & year

            // People with substantial control
            // assuming same officer/directors details as above??

            // Name of disqualified directors
            // no easy endpoint. may have to iterate through officers

            // Percentage of shares <-- not available 
            //      "natures_of_control": [
            //  "ownership-of-shares-25-to-50-percent-as-person"
            //]

            await Task.CompletedTask;
        }

        public async Task<Types.CompaniesHouse.Company> GetCompanyDetails(string companyNumber)
        {
            _logger.LogInformation($"Searching Companies House - Company Details. Company Number: {companyNumber}");
            var apiResponse = await Get<CompanyDetails>($"/company/{companyNumber}");
            return Mapper.Map<CompanyDetails, Types.CompaniesHouse.Company> (apiResponse);
        }

        public async Task<IEnumerable<Types.CompaniesHouse.Officer>> GetOfficers(string companyNumber, bool activeOnly = true)
        {
            _logger.LogInformation($"Searching Companies House - Officers. Company Number: {companyNumber}");
            var apiResponse = await Get<OfficerList>($"/company/{companyNumber}/officers?items_per_page=100");

            var items = activeOnly ? apiResponse.items.Where(i => i.resigned_on is null) : apiResponse.items;
            return Mapper.Map<IEnumerable<Officer>, IEnumerable<Types.CompaniesHouse.Officer>> (items);
        }

        private async Task<IEnumerable<Types.CompaniesHouse.Disqualification>> GetOfficerDisqualifications(string officerId)
        {
            _logger.LogInformation($"Searching Companies House - Natural Officer's Disqualifications. Officer Id: {officerId}");
            var apiResponseNatural = await Get<DisqualificationList>($"/disqualified-officers/natural/{officerId}");

            _logger.LogInformation($"Searching Companies House - Corporate Officer's Disqualifications. Officer Id: {officerId}");
            var apiResponseCorporate = await Get<DisqualificationList>($"/disqualified-officers/corporate/{officerId}");

            var disqualifications = new List<Disqualification>();

            if(apiResponseNatural?.disqualifications != null)
            {
                disqualifications.AddRange(apiResponseNatural.disqualifications);
            }
            if (apiResponseCorporate?.disqualifications != null)
            {
                disqualifications.AddRange(apiResponseCorporate.disqualifications);
            }

            return Mapper.Map<IEnumerable<Disqualification>, IEnumerable<Types.CompaniesHouse.Disqualification>> (disqualifications);
        }

        public async Task<IEnumerable<Types.CompaniesHouse.PersonWithSignificantControl>> GetPeopleWithSignificantControl(string companyNumber, bool activeOnly = true)
        {
            _logger.LogInformation($"Searching Companies House - People With Significant Control. Company Number: {companyNumber}");
            var apiResponse = await Get<PersonWithSignificantControlList>($"/company/{companyNumber}/persons-with-significant-control?items_per_page=100");

            var items = activeOnly ? apiResponse.items.Where(i => i.ceased_on is null) : apiResponse.items;
            return Mapper.Map<IEnumerable<PersonWithSignificantControl>, IEnumerable<Types.CompaniesHouse.PersonWithSignificantControl>>(items);
        }

        public async Task<IEnumerable<dynamic>> GetCharges(string companyNumber)
        {
            _logger.LogInformation($"Searching Companies House - Charges. Company Number: {companyNumber}");
            var apiResponse = await Get<ChargeList>($"/company/{companyNumber}/charges");
            return apiResponse.items;
            //return Mapper.Map<IEnumerable<Charge>, IEnumerable<Types.CompaniesHouse.Charge>> (apiResponse.items);
        }

        public async Task<dynamic> GetInsolvencyDetails(string companyNumber)
        {
            _logger.LogInformation($"Searching Companies House - Insolvency. Company Number: {companyNumber}");
            var apiResponse = await Get<InsolvencyDetails>($"/company/{companyNumber}/insolvency");
            return apiResponse;
            //return Mapper.Map<InsolvencyDetails, InsolvencyDetails> (apiResponse);
        }
    }
}
