﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Reflection;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApplyService.Application;
using SFA.DAS.ApplyService.Application.Apply;
using SFA.DAS.ApplyService.Application.Apply.Validation;
using SFA.DAS.ApplyService.Application.Organisations;
using SFA.DAS.ApplyService.Application.Interfaces;
using SFA.DAS.ApplyService.Application.Users;
using SFA.DAS.ApplyService.Application.Users.CreateAccount;
using SFA.DAS.ApplyService.Configuration;
using SFA.DAS.ApplyService.Data;
using SFA.DAS.ApplyService.DfeSignIn;
using SFA.DAS.ApplyService.InternalApi.Infrastructure;
using SFA.DAS.ApplyService.Encryption;
using SFA.DAS.ApplyService.Session;
using SFA.DAS.ApplyService.Storage;
using StructureMap;
using SFA.DAS.ApplyService.Application.Email;
using SFA.DAS.ApplyService.Application.Apply.GetAnswers;

namespace SFA.DAS.ApplyService.InternalApi
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _configuration;
        private const string _serviceName = "SFA.DAS.ApplyService";
        private const string _version = "1.0";

        private readonly IApplyConfig _applyConfig;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            _env = env;
            _configuration = configuration;
            
            _applyConfig = new ConfigurationService(_env, _configuration["EnvironmentName"], _configuration["ConfigurationStorageConnectionString"], _version, _serviceName).GetConfig().GetAwaiter().GetResult();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });

            services.AddHttpClient<AssessorServiceApiClient>("AssessorServiceApiClient", config =>
            {
                config.BaseAddress = new Uri(_applyConfig.AssessorServiceApiAuthentication.ApiBaseAddress); //  "http://localhost:59022"
                config.DefaultRequestHeaders.Add("Accept", "Application/json");
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            services.AddHttpClient<ProviderRegisterApiClient>("ProviderRegisterApiClient", config =>
            {
                config.BaseAddress = new Uri(_applyConfig.ProviderRegisterApiAuthentication.ApiBaseAddress); //  "https://findapprenticeshiptraining-api.sfa.bis.gov.uk"
                config.DefaultRequestHeaders.Add("Accept", "Application/json");
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            services.AddHttpClient<ReferenceDataApiClient>("ReferenceDataApiClient", config =>
            {
                config.BaseAddress = new Uri(_applyConfig.ReferenceDataApiAuthentication.ApiBaseAddress); //  "https://at-refdata.apprenticeships.sfa.bis.gov.uk/api"
                config.DefaultRequestHeaders.Add("Accept", "Application/json");
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            services.AddHttpClient<CompaniesHouseApiClient>("CompaniesHouseApiClient", config =>
            {
                config.BaseAddress = new Uri(_applyConfig.CompaniesHouseApiAuthentication.ApiBaseAddress); //  "https://api.companieshouse.gov.uk"
                config.DefaultRequestHeaders.Add("Accept", "Application/json");
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("en-GB");
                options.SupportedCultures = new List<CultureInfo> { new CultureInfo("en-GB") };
                options.SupportedUICultures = new List<CultureInfo> { new CultureInfo("en-GB") };
                options.RequestCultureProviders.Clear();
            });
            
            
            IMvcBuilder mvcBuilder;
            if (_env.IsDevelopment())
                mvcBuilder = services.AddMvc(opt => { opt.Filters.Add(new AllowAnonymousFilter()); });
            else
                mvcBuilder = services.AddMvc();

            mvcBuilder.SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddFluentValidation(fvc => fvc.RegisterValidatorsFromAssemblyContaining<Startup>());
            
            services.AddDistributedMemoryCache();

            ConfigureIOC(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            MappingStartup.AddMappings();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            app.UseRequestLocalization();
            app.UseSecurityHeaders();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
        
        private void ConfigureIOC(IServiceCollection services)
        {
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
                
            services.AddSingleton<IConfigurationService>(sp => new ConfigurationService(
                 sp.GetService<IHostingEnvironment>(),
                 _configuration["EnvironmentName"],
                 _configuration["ConfigurationStorageConnectionString"],
                 _version,
                 _serviceName));

            services.AddTransient<IValidatorFactory, ValidatorFactory>();

            services.AddTransient<IValidator, DateOfBirthNotInFutureValidator>();
            services.AddTransient<IValidator, DateOfBirthValidator>();
            services.AddTransient<IValidator, DateNotInFutureValidator>();
            services.AddTransient<IValidator, DateValidator>();
            services.AddTransient<IValidator, EmailAddressIsValidValidator>();
            services.AddTransient<IValidator, MaxLengthValidator>();
            services.AddTransient<IValidator, MaxWordCountValidator>();
            services.AddTransient<IValidator, NullValidator>();
            services.AddTransient<IValidator, RegexValidator>();
            services.AddTransient<IValidator, RegisteredCharityNumberValidator>();
            services.AddTransient<IValidator, RequiredValidator>();

            services.AddTransient<IContactRepository,ContactRepository>();
            services.AddTransient<IApplyRepository,ApplyRepository>();
            services.AddTransient<IOrganisationRepository,OrganisationRepository>();
            services.AddTransient<IDfeSignInService,DfeSignInService>();
            services.AddTransient<IGetAnswersService, GetAnswersService>();

            services.AddTransient<IEmailService, EmailService.EmailService>();
            services.AddTransient<IEmailTemplateRepository, EmailTemplateRepository>();

            // NOTE: These are SOAP Services. Their client interfaces are contained within the generated Proxy code.
            services.AddTransient<CharityCommissionService.ISearchCharitiesV1SoapClient,CharityCommissionService.SearchCharitiesV1SoapClient>();
            services.AddTransient<CharityCommissionApiClient,CharityCommissionApiClient>();
            // End of SOAP Services

            services.AddTransient<IKeyProvider,PlaceholderKeyProvider>();
            services.AddTransient<IStorageService,StorageService>();
            services.AddTransient<IEncryptionService, EncryptionService>();
    
            services.AddMediatR(typeof(CreateAccountHandler).GetTypeInfo().Assembly);
        }
    }
}