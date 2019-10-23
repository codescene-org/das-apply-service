﻿namespace SFA.DAS.ApplyService.Web.Controllers.Roatp
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using SFA.DAS.ApplyService.Web.Infrastructure;
    using System.Threading.Tasks;
    using Application.Apply.Roatp;
    using Domain.Apply;
    using Domain.CharityCommission;
    using Domain.CompaniesHouse;
    using Domain.Roatp;
    using Domain.Ukrlp;
    using global::AutoMapper;
    using InternalApi.Types.CharityCommission;
    using Session;
    using ViewModels.Roatp;
    using Validators;
    using Microsoft.AspNetCore.Authorization;
    using SFA.DAS.ApplyService.InternalApi.Types;
    using SFA.DAS.ApplyService.Web.Resources;
    using System.Collections.Generic;

    [Authorize]
    public class RoatpApplicationPreambleController : Controller
    {
        private readonly ILogger<RoatpApplicationPreambleController> _logger;
        private readonly IRoatpApiClient _roatpApiClient;
        private readonly IUkrlpApiClient _ukrlpApiClient;
        private readonly ISessionService _sessionService;
        private readonly ICompaniesHouseApiClient _companiesHouseApiClient;
        private readonly ICharityCommissionApiClient _charityCommissionApiClient;
        private readonly IOrganisationApiClient _organisationApiClient;
        private readonly IUsersApiClient _usersApiClient;
        
        private const string ApplicationDetailsKey = "Roatp_Application_Details";
        
        private string[] StatusOnlyCompanyNumberPrefixes = new[] { "IP", "SP", "IC", "SI", "NP", "NV", "RC", "SR", "NR", "NO" };

        private string[] ExcludedCharityCommissionPrefixes = new[] {"SC", "NI"};

        public RoatpApplicationPreambleController(ILogger<RoatpApplicationPreambleController> logger, IRoatpApiClient roatpApiClient, 
                                                  IUkrlpApiClient ukrlpApiClient, ISessionService sessionService, 
                                                  ICompaniesHouseApiClient companiesHouseApiClient, 
                                                  ICharityCommissionApiClient charityCommissionApiClient,
                                                  IOrganisationApiClient organisationApiClient,
                                                  IUsersApiClient usersApiClient)
        {
            _logger = logger;
            _roatpApiClient = roatpApiClient;
            _ukrlpApiClient = ukrlpApiClient;
            _sessionService = sessionService;
            _companiesHouseApiClient = companiesHouseApiClient;
            _charityCommissionApiClient = charityCommissionApiClient;
            _organisationApiClient = organisationApiClient;
            _usersApiClient = usersApiClient;
        }

        [Route("terms-conditions-making-application")]
        public async Task<IActionResult> TermsAndConditions()
        {
            return View("~/Views/Roatp/TermsAndConditions.cshtml");
        }

        [Route("not-accept-terms-conditions")]
        public async Task<IActionResult> TermsAndConditionsNotAgreed()
        {
            return View("~/Views/Roatp/TermsAndConditionsNotAgreed.cshtml");
        }

        [Route("enter-uk-provider-reference-number")]
        public async Task<IActionResult> EnterApplicationUkprn(string ukprn)
        {
            var model = new SearchByUkprnViewModel();
            if (!String.IsNullOrWhiteSpace(ukprn))
            {
                model.UKPRN = ukprn;
            }
            return View("~/Views/Roatp/EnterApplicationUkprn.cshtml", model);
        }

        [Route("search-by-ukprn")]
        [HttpPost]
        public async Task<IActionResult> SearchByUkprn(SearchByUkprnViewModel model)
        {
            long ukprn = 0;
            string validationMessage = string.Empty;
            if (String.IsNullOrWhiteSpace(model.UKPRN))
            {
                validationMessage = UkprnValidationMessages.MissingUkprn;
            }
            else
            {
                bool isValidUkprn = UkprnValidator.IsValidUkprn(model.UKPRN, out ukprn);
                if (!isValidUkprn)
                {
                    validationMessage = UkprnValidationMessages.InvalidUkprn;
                }
            }

            if (!String.IsNullOrEmpty(validationMessage))
            {
                ModelState.AddModelError(nameof(model.UKPRN), validationMessage);
                TempData["ShowErrors"] = true;

                return View("~/Views/Roatp/EnterApplicationUkprn.cshtml", model);
            }
            
            var ukrlpLookupResults = await _ukrlpApiClient.GetTrainingProviderByUkprn(ukprn);

            if (!ukrlpLookupResults.Success)
            {
                return RedirectToAction("UkrlpNotAvailable");
            }

            if (ukrlpLookupResults.Results.Any())
            {
                var applicationDetails = new ApplicationDetails
                {
                    UKPRN = ukprn,
                    UkrlpLookupDetails = ukrlpLookupResults.Results.FirstOrDefault()
                };

                _sessionService.Set(ApplicationDetailsKey, applicationDetails);
                
                return RedirectToAction("ConfirmOrganisation");
            }
            else
            {
                var applicationDetails = new ApplicationDetails
                {
                    UKPRN = ukprn
                };

                _sessionService.Set(ApplicationDetailsKey, applicationDetails);
                return RedirectToAction("UkprnNotFound");
            }
        }

        [Route("confirm-organisations-details")]
        public async Task<IActionResult> ConfirmOrganisation()
        {
            var applicationDetails = _sessionService.Get<ApplicationDetails>(ApplicationDetailsKey);
            
            var viewModel = new UkprnSearchResultsViewModel
            {
                ProviderDetails = applicationDetails.UkrlpLookupDetails,
                UKPRN = applicationDetails.UkrlpLookupDetails.UKPRN
            };

            return View("~/Views/Roatp/ConfirmOrganisation.cshtml", viewModel);
        }
       
        [Route("uk-provider-reference-number-not-found")]
        public async Task<IActionResult> UkprnNotFound()
        {
            var applicationDetails = _sessionService.Get<ApplicationDetails>(ApplicationDetailsKey);

            var viewModel = new UkprnSearchResultsViewModel
            {
                UKPRN = applicationDetails.UKPRN.ToString()
            };

            return View("~/Views/Roatp/UkprnNotFound.cshtml", viewModel);
        }

        [Route("company-not-found")]
        public async Task<IActionResult> CompanyNotFound()
        {
            var applicationDetails = _sessionService.Get<ApplicationDetails>(ApplicationDetailsKey);

            var viewModel = new UkprnSearchResultsViewModel
            {
                UKPRN = applicationDetails.UKPRN.ToString(),
                ProviderDetails = applicationDetails.UkrlpLookupDetails
            };

            return View("~/Views/Roatp/CompanyNotFound.cshtml", viewModel);
        }

        [Route("charity-not-found")]
        public async Task<IActionResult> CharityNotFound()
        {
            var applicationDetails = _sessionService.Get<ApplicationDetails>(ApplicationDetailsKey);

            var viewModel = new UkprnSearchResultsViewModel
            {
                UKPRN = applicationDetails.UKPRN.ToString(),
                ProviderDetails = applicationDetails.UkrlpLookupDetails
            };

            return View("~/Views/Roatp/CharityNotFound.cshtml", viewModel);
        }
        
        [Route("start-application")]
        [HttpPost]
        public async Task<IActionResult> StartApplication(SelectApplicationRouteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ApplicationRoutes = await GetApplicationRoutesForOrganisation();

                return View("~/Views/Roatp/SelectApplicationRoute.cshtml", model);
            }

            if (model.ApplicationRouteId == ApplicationRoute.EmployerProviderApplicationRoute)
            {
                var applicationDetails = _sessionService.Get<ApplicationDetails>(ApplicationDetailsKey);
                var viewModel = new EmployerLevyStatusViewModel
                {
                    UKPRN = applicationDetails.UKPRN.ToString(),
                    ApplicationRouteId = model.ApplicationRouteId
                };
                return await ConfirmLevyStatus(viewModel);
            }

            return await StartRoatpApplication(model);
        }

        [Route("organisation-levy-paying-employer")]
        public async Task<IActionResult> ConfirmLevyStatus(EmployerLevyStatusViewModel model)
        {
            var applicationDetails = _sessionService.Get<ApplicationDetails>(ApplicationDetailsKey);
            if (applicationDetails.ApplicationRoute == null)
            {
                applicationDetails.ApplicationRoute = new ApplicationRoute { Id = model.ApplicationRouteId };
                _sessionService.Set(ApplicationDetailsKey, applicationDetails);
            }

            return View("~/Views/Roatp/ConfirmLevyStatus.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitLevyStatus(EmployerLevyStatusViewModel model)
        {
            var applicationDetails = _sessionService.Get<ApplicationDetails>(ApplicationDetailsKey);

            if (!ModelState.IsValid)
            {
                return View("~/Views/Roatp/ConfirmLevyStatus.cshtml", model);
            }
            
            applicationDetails.LevyPayingEmployer = model.LevyPayingEmployer;
            _sessionService.Set(ApplicationDetailsKey, applicationDetails);

            if (applicationDetails.LevyPayingEmployer == "Y")
            {
                var selectApplicationRouteModel = new SelectApplicationRouteViewModel
                {
                    ApplicationRouteId = applicationDetails.ApplicationRoute.Id
                };
                return await StartRoatpApplication(selectApplicationRouteModel);
            }
            return await IneligibleNonLevy();
        }

        [Route("organisation-cannot-apply-employer")]
        public async Task<IActionResult> IneligibleNonLevy()
        {
            return View("~/Views/Roatp/IneligibleNonLevy.cshtml", new EmployerProviderContinueApplicationViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmNonLevyContinue(EmployerProviderContinueApplicationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Roatp/IneligibleNonLevy.cshtml", model);
            }

            if (model.ContinueWithApplication == "Y")
            {
                return RedirectToAction("SelectApplicationRoute");
            }

            return RedirectToAction("NonLevyAbandonedApplication");
        }
        
        [Route("chosen-not-apply-roatp")]
        public async Task<IActionResult> NonLevyAbandonedApplication()
        {
            return View("~/Views/Roatp/NonLevyAbandonedApplication.cshtml");
        }

        [Route("ukrlp-unavailable")]
        public async Task<IActionResult> UkrlpNotAvailable()
        {
            return View("~/Views/Roatp/UkrlpNotAvailable.cshtml");
        }

        [Route("companies-house-unavailable")]
        public async Task<IActionResult> CompaniesHouseNotAvailable()
        {
            return View("~/Views/Roatp/CompaniesHouseNotAvailable.cshtml");
        }

        [Route("charity-commission-unavailable")]
        public async Task<IActionResult> CharityCommissionNotAvailable()
        {
            return View("~/Views/Roatp/CharityCommissionNotAvailable.cshtml");
        }

        [Route("choose-provider-route")]
        public async Task<IActionResult> SelectApplicationRoute()
        {
            var model = new SelectApplicationRouteViewModel();
            var applicationRoutes = await GetApplicationRoutesForOrganisation();

            model.ApplicationRoutes = applicationRoutes;

            var applicationDetails = _sessionService.Get<ApplicationDetails>(ApplicationDetailsKey);
            if (applicationDetails.ApplicationRoute != null)
            {
                model.ApplicationRouteId = applicationDetails.ApplicationRoute.Id;
            }

            return View("~/Views/Roatp/SelectApplicationRoute.cshtml", model);
        }

        public async Task<IActionResult> VerifyOrganisationDetails()
        {
            var applicationDetails = _sessionService.Get<ApplicationDetails>(ApplicationDetailsKey);
            var providerDetails = applicationDetails.UkrlpLookupDetails;
            CompaniesHouseSummary companyDetails = null;
            Charity charityDetails = null;

            if (providerDetails.VerifiedByCompaniesHouse)
            {
                var companiesHouseVerification = providerDetails.VerificationDetails.FirstOrDefault(x =>
                        x.VerificationAuthority == VerificationAuthorities.CompaniesHouseAuthority);

                companyDetails = await _companiesHouseApiClient.GetCompanyDetails(companiesHouseVerification.VerificationId);

                if ((companyDetails.Directors == null || companyDetails.Directors.Count == 0)
                    && (companyDetails.PersonsSignificationControl == null ||
                        companyDetails.PersonsSignificationControl.Count == 0))
                {
                    companyDetails.ManualEntryRequired = true;
                }
                
                if (companyDetails.Status == CompaniesHouseSummary.ServiceUnavailable)
                {
                    return RedirectToAction("CompaniesHouseNotAvailable");
                }

                if (companyDetails.Status == CompaniesHouseSummary.CompanyStatusNotFound)
                {
                    return RedirectToAction("CompanyNotFound");
                }
                
                if (!CompaniesHouseValidator.CompaniesHouseStatusValid(companyDetails.CompanyNumber, companyDetails.Status))
                {
                    return RedirectToAction("CompanyNotFound");
                }

                applicationDetails.CompanySummary = companyDetails;
            }

            if (applicationDetails.UkrlpLookupDetails.VerifiedbyCharityCommission)
            {
                var charityCommissionVerification = providerDetails.VerificationDetails.FirstOrDefault(x =>
                    x.VerificationAuthority == VerificationAuthorities.CharityCommissionAuthority);

                int charityNumber;
                string verificationId = charityCommissionVerification.VerificationId;
                if (verificationId.Contains("-"))
                {
                    verificationId = verificationId.Substring(0, verificationId.IndexOf("-"));
                }

                if (IsEnglandAndWalesCharityCommissionNumber(verificationId))
                {
                    bool isValidCharityNumber = int.TryParse(verificationId, out charityNumber);
                    if (!isValidCharityNumber)
                    {
                        return RedirectToAction("CharityNotFound");
                    }

                    var charityApiResponse = await _charityCommissionApiClient.GetCharityDetails(charityNumber);

                    if (!charityApiResponse.Success)
                    {
                        return RedirectToAction("CharityCommissionNotAvailable");
                    } 
                    charityDetails = charityApiResponse.Response;

                    if (charityDetails == null || !charityDetails.IsActivelyTrading)
                    {
                        return RedirectToAction("CharityNotFound");
                    }
                    
                    applicationDetails.CharitySummary = Mapper.Map<CharityCommissionSummary>(charityDetails);
                }
                else
                {
                    applicationDetails.CharitySummary = new CharityCommissionSummary
                    {
                        TrusteeManualEntryRequired = true
                    };
                }
            }

            var roatpRegisterStatus = await _roatpApiClient.GetOrganisationRegisterStatus(applicationDetails.UKPRN);

            applicationDetails.RoatpRegisterStatus = roatpRegisterStatus;
            
            _sessionService.Set(ApplicationDetailsKey, applicationDetails);


            if (ProviderEligibleToChangeRoute(roatpRegisterStatus))
            {
                return RedirectToAction("ProviderAlreadyOnRegister");
            }

            return RedirectToAction("SelectApplicationRoute");           
        }

        private async Task<IActionResult> StartRoatpApplication(SelectApplicationRouteViewModel model)
        {
            var applicationDetails = _sessionService.Get<ApplicationDetails>(ApplicationDetailsKey);
            applicationDetails.ApplicationRoute = new ApplicationRoute { Id = model.ApplicationRouteId };

            var user = await _usersApiClient.GetUserBySignInId(User.GetSignInId());

            applicationDetails.CreatedBy = user.Id;

            var createOrganisationRequest = Mapper.Map<CreateOrganisationRequest>(applicationDetails);

            var organisation = await _organisationApiClient.Create(createOrganisationRequest, user.Id);

            _sessionService.Set(ApplicationDetailsKey, applicationDetails);

            if (!user.IsApproved)
            {
                await _usersApiClient.ApproveUser(user.Id);
            }

            return RedirectToAction("Applications", "RoatpApplication", new { applicationType = ApplicationTypes.RegisterTrainingProviders });
		}
		
        [Route("already-on-roatp")]
        public async Task<IActionResult> ProviderAlreadyOnRegister()
        {
            var applicationDetails = _sessionService.Get<ApplicationDetails>(ApplicationDetailsKey);

            var providerRoutes = await _roatpApiClient.GetApplicationRoutes();

            var existingProviderRoute = providerRoutes.FirstOrDefault(x => x.Id == applicationDetails.RoatpRegisterStatus.ProviderTypeId);

            var model = new ChangeProviderRouteViewModel
            {
                UKPRN = applicationDetails.UKPRN.ToString(),
                CurrentProviderType = existingProviderRoute
            };

            return View("~/Views/Roatp/ProviderAlreadyOnRegister.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeProviderRoute(ChangeProviderRouteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var applicationDetails = _sessionService.Get<ApplicationDetails>(ApplicationDetailsKey);

                var providerRoutes = await _roatpApiClient.GetApplicationRoutes();

                var existingProviderRoute = providerRoutes.FirstOrDefault(x => x.Id == applicationDetails.RoatpRegisterStatus.ProviderTypeId);

                model = new ChangeProviderRouteViewModel
                {
                    UKPRN = applicationDetails.UKPRN.ToString(),
                    CurrentProviderType = existingProviderRoute
                };

                return View("~/Views/Roatp/ProviderAlreadyOnRegister.cshtml", model);
            }

            if (model.ChangeApplicationRoute != "Y")
            {                            
                return RedirectToAction("ChosenToRemainOnRegister", model);
            }
            else
            {
                return RedirectToAction("SelectApplicationRoute");
            }
        }
        
        [Route("chosen-stay-on-roatp")]
        public async Task<IActionResult> ChosenToRemainOnRegister()
        {
            var applicationDetails = _sessionService.Get<ApplicationDetails>(ApplicationDetailsKey);

            var providerRoutes = await _roatpApiClient.GetApplicationRoutes();

            var existingProviderRoute = providerRoutes.FirstOrDefault(x => x.Id == applicationDetails.RoatpRegisterStatus.ProviderTypeId);

            var model = new ChangeProviderRouteViewModel
            {
                UKPRN = applicationDetails.UKPRN.ToString(),
                CurrentProviderType = existingProviderRoute
            };

            return View("~/Views/Roatp/ChosenToRemainOnRegister.cshtml", model);
        }

        private bool ProviderEligibleToChangeRoute(OrganisationRegisterStatus roatpRegisterStatus)
        {
            if (roatpRegisterStatus.UkprnOnRegister 
                && (roatpRegisterStatus.StatusId == OrganisationStatus.Active 
                || roatpRegisterStatus.StatusId == OrganisationStatus.ActiveNotTakingOnApprentices
                || roatpRegisterStatus.StatusId == OrganisationStatus.Onboarding))
            {
                return true;
            }

            return false;
        }

        private bool IsEnglandAndWalesCharityCommissionNumber(string charityNumber)
        {
            if (String.IsNullOrWhiteSpace(charityNumber))
            {
                return false;
            }

            foreach (var prefix in ExcludedCharityCommissionPrefixes)
            {
                if (charityNumber.ToUpper().StartsWith(prefix))
                {
                    return false;
                }
            }

            return true;
        }
               
        private async Task<List<ApplicationRoute>> GetApplicationRoutesForOrganisation()
        {
            var applicationRoutes = (await _roatpApiClient.GetApplicationRoutes()).ToList();

            var applicationDetails = _sessionService.Get<ApplicationDetails>(ApplicationDetailsKey);
            if (applicationDetails != null 
                && applicationDetails.RoatpRegisterStatus !=null 
                && applicationDetails.RoatpRegisterStatus.UkprnOnRegister
                && applicationDetails.RoatpRegisterStatus.StatusId != OrganisationStatus.Removed)
            {
                var existingRoute = applicationRoutes.FirstOrDefault(x => x.Id == applicationDetails.RoatpRegisterStatus.ProviderTypeId);
                if (existingRoute != null)
                {
                    applicationRoutes.Remove(existingRoute);
                }
            }

            return applicationRoutes;
        }

    }
}
