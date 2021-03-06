using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.ApplyService.Configuration;
using SFA.DAS.ApplyService.Domain.Apply;
using SFA.DAS.ApplyService.Domain.Entities;
using SFA.DAS.ApplyService.Session;
using SFA.DAS.ApplyService.Web.Infrastructure;
using SFA.DAS.ApplyService.Web.Validators;
using SFA.DAS.ApplyService.Web.ViewModels;

namespace SFA.DAS.ApplyService.Web.Controllers
{
    using SFA.DAS.ApplyService.Web.ViewModels.Roatp;
    using System.Collections.Generic;
    using System.Linq;

    public class UsersController : Controller
    {
        private readonly IUsersApiClient _usersApiClient;
        private readonly IApplicationApiClient _applicationApiClient;
        private readonly ISessionService _sessionService;
        private readonly ILogger<UsersController> _logger;
        private readonly IConfigurationService _config;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly CreateAccountValidator _createAccountValidator;
        private readonly IOrganisationApiClient _organisationApiClient;

        private const string TrainingProviderOrganisationType = "TrainingProvider";


        public UsersController(IUsersApiClient usersApiClient, ISessionService sessionService, ILogger<UsersController> logger, 
                               IConfigurationService config, IHttpContextAccessor contextAccessor, 
                               CreateAccountValidator createAccountValidator, IApplicationApiClient applicationApiClient,
                               IOrganisationApiClient organisationApiClient)
        { 
            _usersApiClient = usersApiClient;
            _applicationApiClient = applicationApiClient;
            _sessionService = sessionService;
            _logger = logger;
            _config = config;
            _contextAccessor = contextAccessor;
            _createAccountValidator = createAccountValidator;
            _applicationApiClient = applicationApiClient;
            _organisationApiClient = organisationApiClient;
        }
        
        [HttpGet]
        public IActionResult CreateAccount()
        {
            var vm = new CreateAccountViewModel();
            return View(vm);
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateAccount(CreateAccountViewModel vm)
        {
            _createAccountValidator.Validate(vm);

            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            
            var inviteSuccess = await _usersApiClient.InviteUser(vm);

            _sessionService.Set("NewAccount", vm);

            return inviteSuccess ? RedirectToAction("InviteSent") : RedirectToAction("Error", "Home");
            
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            return Challenge(new AuthenticationProperties() {RedirectUri = Url.Action("PostSignIn", "Users")},
                OpenIdConnectDefaults.AuthenticationScheme);
        }
        
        [HttpGet]
        public async Task<IActionResult> SignOut()
        {
            _contextAccessor.HttpContext.Session.Clear();
            foreach (var cookie in _contextAccessor.HttpContext.Request.Cookies.Keys)
            {
                _contextAccessor.HttpContext.Response.Cookies.Delete(cookie);
            }

            if (string.IsNullOrEmpty(_contextAccessor.HttpContext.User.FindFirstValue("display_name")))
            {
                var authenticationProperties = new AuthenticationProperties
                {
                    RedirectUri = Url.Action("Index", "Home")
                };
                return SignOut(authenticationProperties, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult InviteSent()
        {
            var viewModel = _sessionService.Get<CreateAccountViewModel>("NewAccount");

            if (viewModel?.Email is null)
            {
                RedirectToAction("CreateAccount");
            }

            return View(viewModel);
        }

        public async Task<IActionResult> PostSignIn()
        {
            var user = await _usersApiClient.GetUserBySignInId(User.GetSignInId());
           
            if (user is null)
            {
                return RedirectToAction("NotSetUp");
            }
            else if (user.ApplyOrganisationId is null)
            {
                return RedirectToAction("EnterApplicationUkprn", "RoatpApplicationPreamble");
            }

            var organisation = await _organisationApiClient.GetByUser(user.Id);

            var selectedApplicationType = ApplicationTypes.EndpointAssessor;
            if (organisation.OrganisationType == TrainingProviderOrganisationType)
            {
                selectedApplicationType = ApplicationTypes.RegisterTrainingProviders;
            }           
            else
            {
                var org = await _applicationApiClient.GetOrganisationByUserId(user.Id);

                if (org != null)
                {
                    _logger.LogInformation($"Setting OrganisationName in Session: {org.Name}");
                    _sessionService.Set("OrganisationName", $"{org.Name}");
                }
            }
            
            return RedirectToAction("Applications", "RoatpApplication", new { applicationType = selectedApplicationType });

        }

        [HttpGet("/Users/SignedOut")]
        public IActionResult SignedOut()
        {
            return View();
        }

        public IActionResult NotSetUp()
        {
            return View();
        }

        [HttpGet]
        [Route("first-time-apprenticeship-service")]
        public IActionResult ExistingAccount()
        {
            return View(new ExistingAccountViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmExistingAccount(ExistingAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ErrorMessages = new List<ValidationErrorDetail>();

                var modelErrors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var modelError in modelErrors)
                {
                    model.ErrorMessages.Add(new ValidationErrorDetail
                    {
                        Field = "ExistingAccount",
                        ErrorMessage = modelError.ErrorMessage
                    });
                }

                return View("~/Views/Users/ExistingAccount.cshtml", model);
            }

            if (model.FirstTimeSignin == "Y")
            {
                return RedirectToAction("CreateAccount");
            }

            return RedirectToAction("SignIn");
        }
    }
}