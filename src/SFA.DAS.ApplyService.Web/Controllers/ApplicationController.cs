using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApplyService.Application.Apply.GetPage;
using SFA.DAS.ApplyService.Application.Apply.Validation;
using SFA.DAS.ApplyService.Configuration;
using SFA.DAS.ApplyService.Domain.Apply;
using SFA.DAS.ApplyService.Domain.Entities;
using SFA.DAS.ApplyService.Session;
using SFA.DAS.ApplyService.Web.Infrastructure;
using SFA.DAS.ApplyService.Web.ViewModels;

namespace SFA.DAS.ApplyService.Web.Controllers
{
    [Authorize]
    public class ApplicationController : Controller
    {
        private readonly IApplicationApiClient _apiClient;
        private readonly ILogger<ApplicationController> _logger;
        private readonly ISessionService _sessionService;
        private readonly IConfigurationService _configService;

        public ApplicationController(IApplicationApiClient apiClient, ILogger<ApplicationController> logger, ISessionService sessionService, IConfigurationService configService)
        {
            _apiClient = apiClient;
            _logger = logger;
            _sessionService = sessionService;
            _configService = configService;
        }

        [HttpGet("/Applications")]
        public async Task<IActionResult> Applications()
        {
            var user = _sessionService.Get("LoggedInUser");
            _logger.LogInformation($"Got LoggedInUser from Session: {user}");

            var applications = await _apiClient.GetApplicationsFor(User.GetUserId());

            if (!applications.Any())
            {
                return View("~/Views/Application/Declaration.cshtml");
            }
            else if (applications.Count() > 1)
            {
                return View(applications);
            }
            else
            {
                var application = applications.First();

                if (application.ApplicationStatus == ApplicationStatus.FeedbackAdded)
                {
                    return View("~/Views/Application/FeedbackIntro.cshtml", application.Id);
                }
                else if (application.ApplicationStatus == ApplicationStatus.Rejected)
                {
                    return View(applications);
                }
                else if (application.ApplicationStatus == ApplicationStatus.Approved)
                {
                    return View(applications);
                }

                return RedirectToAction("SequenceSignPost", new {applicationId = application.Id});
            }
        }

        [HttpPost("/Applications")]
        public async Task<IActionResult> StartApplication()
        {
            await _apiClient.StartApplication(User.GetUserId());

            return RedirectToAction("Applications");
        }

        [HttpGet("/Applications/{applicationId}/Sequence")]
        public async Task<IActionResult> Sequence(Guid applicationId)
        {
            // Break this out into a "Signpost" action.
            var sequence = await _apiClient.GetSequence(applicationId, User.GetUserId());
            var sequenceVm = new SequenceViewModel(sequence, applicationId, null);
            return View(sequenceVm);
        }

        [HttpGet("/Applications/{applicationId}")]
        public async Task<IActionResult> SequenceSignPost(Guid applicationId)
        {
            var application = await _apiClient.GetApplication(applicationId);
            if (application.ApplicationStatus == ApplicationStatus.Approved)
            {
                return View("~/Views/Application/Approved.cshtml", application);
            }

            if (application.ApplicationStatus == ApplicationStatus.Rejected)
            {
                return View("~/Views/Application/Rejected.cshtml", application);
            }

            var sequence = await _apiClient.GetSequence(applicationId, User.GetUserId());

            StandardApplicationData applicationData = null;

            if (application.ApplicationData != null)
            {
                applicationData = new StandardApplicationData
                {
                    StandardName = application.ApplicationData.StandardName
                };
            }

            // Only go to search if application hasn't got a selected standard?
            if (sequence.SequenceId == SequenceId.Stage1)
            {
                return RedirectToAction("Sequence", new {applicationId});
                //return View("", sequence);
            }
            else if (sequence.SequenceId == SequenceId.Stage2 && string.IsNullOrWhiteSpace(applicationData?.StandardName))
            {
                //return RedirectToAction("Search", "Standard", new {applicationId});
                return View("~/Views/Application/Stage2Intro.cshtml", applicationId);
            }
            else if (sequence.SequenceId == SequenceId.Stage2)
            {
                return RedirectToAction("Sequence", new {applicationId});
            }

            throw new BadRequestException("Section does not have a valid DisplayType");
        }

        [HttpGet("/Applications/{applicationId}/Sequences/{sequenceId}/Sections/{sectionId}")]
        public async Task<IActionResult> Section(Guid applicationId, int sequenceId, int sectionId)
        {
            var section = await _apiClient.GetSection(applicationId, sequenceId, sectionId, User.GetUserId());

            if (section.Status != ApplicationSectionStatus.Draft)
            {
                return RedirectToAction("Sequence", new { applicationId = applicationId });
            }

            switch(section?.DisplayType)
            {
                case null:
                case SectionDisplayType.Pages:
                    return View("~/Views/Application/Section.cshtml", section);
                case SectionDisplayType.Questions:
                    return View("~/Views/Application/Section.cshtml", section);
                case SectionDisplayType.PagesWithSections:
                    return View("~/Views/Application/PagesWithSections.cshtml", section);
                default:
                    throw new BadRequestException("Section does not have a valid DisplayType");

            }
        }

        [HttpGet("/Application/{applicationId}/Sequences/{sequenceId}/Sections/{sectionId}/Pages/{pageId}")]
        public async Task<IActionResult> Page(Guid applicationId, int sequenceId, int sectionId, string pageId, string redirectAction)
        {
            var canUpdate = await CanUpdateApplication(applicationId, sequenceId, sectionId, User.GetUserId());
            if (!canUpdate)
            {
                return RedirectToAction("Sequence", new { applicationId });
            }

            var page = await _apiClient.GetPage(applicationId, sequenceId, sectionId, pageId, User.GetUserId());
            page = await GetDataFedOptions(page);

            var returnUrl = Request.Headers["Referer"].ToString();
            var pageVm = new PageViewModel(applicationId, sequenceId, sectionId, pageId, page, redirectAction, returnUrl, null);
     
            ProcessPageVmQuestionsForStandardName(pageVm.Questions, applicationId);

            if (page != null && page.AllowMultipleAnswers)
            {
                return View("~/Views/Application/Pages/MultipleAnswers.cshtml", pageVm);
            }

            return View("~/Views/Application/Pages/Index.cshtml", pageVm);
        }

        private async Task<bool> CanUpdateApplication(Guid applicationId, int sequenceId, int? sectionId, Guid userId)
        {
            bool isEditable = false;

            var sequence = await _apiClient.GetSequence(applicationId, userId);

            if(sequence?.Status != null)
            {
                isEditable = sequence.Status == ApplicationSequenceStatus.Draft || sequence.Status == ApplicationSequenceStatus.FeedbackAdded;
            }

            if(!isEditable && sectionId.HasValue)
            {
                var section = await _apiClient.GetSection(applicationId, sequenceId, sectionId.Value, userId);

                if (section?.Status != null)
                {
                    isEditable = section.Status == ApplicationSectionStatus.Draft;
                }
            }

            return isEditable;
        }

        private async Task<Page> GetDataFedOptions(Page page)
        {
            if (page != null)
            {
                foreach (var question in page.Questions)
                {
                    if (question.Input.Type.StartsWith("DataFed_"))
                    {
                        var questionOptions = await _apiClient.GetQuestionDataFedOptions(question.Input.DataEndpoint);
                        // Get data from API using question.Input.DataEndpoint
                        question.Input.Options = questionOptions;
                        question.Input.Type = question.Input.Type.Replace("DataFed_", "");
                    }
                }
            }

            return page;
        }

        private void ProcessPageVmQuestionsForStandardName(List<QuestionViewModel> pageVmQuestions, Guid applicationId)
         {
            if (pageVmQuestions == null) return;

             var placeholderString = "StandardName";
             var isPlaceholderPresent = false;

             foreach (var question in pageVmQuestions)
             
                 if (question.Label.Contains($"[{placeholderString}]") ||
                    question.Hint.Contains($"[{placeholderString}]") ||
                     question.QuestionBodyText.Contains($"[{placeholderString}]") ||
                     question.ShortLabel.Contains($"[{placeholderString}]")
                    )
                    isPlaceholderPresent=true;

             if (!isPlaceholderPresent) return;

             var application = _apiClient.GetApplication(applicationId).Result;
             var standardName = application?.ApplicationData?.StandardName;


             if (string.IsNullOrEmpty(standardName)) standardName = "the standard to be selected";

            foreach (var question in pageVmQuestions)
             {
                question.Label = question.Label?.Replace($"[{placeholderString}]", standardName);
                question.Hint = question.Hint?.Replace($"[{placeholderString}]", standardName);
                question.QuestionBodyText = question.QuestionBodyText?.Replace($"[{placeholderString}]", standardName);
                question.ShortLabel = question.Label?.Replace($"[{placeholderString}]", standardName);
            }     
         }

        [HttpPost("/Application/{applicationId}/Sequences/{sequenceId}/Sections/{sectionId}/Pages/{pageId}/Multi")]
        public async Task<IActionResult> SaveAnswersMulti(Guid applicationId, int sequenceId, int sectionId, string pageId, string redirectAction, string __formAction)
        {
            var canUpdate = await CanUpdateApplication(applicationId, sequenceId, sectionId, User.GetUserId());
            if (!canUpdate)
            {
                return RedirectToAction("Sequence", new { applicationId });
            }
            
            if (__formAction == "Add")
            {
                return await SaveAnswers(applicationId, sequenceId, sectionId, pageId, redirectAction);
            }

            if (__formAction == "Save")
            {     

                var answers = new List<Answer>();
                GetAnswersFromForm(answers);
                var inputEnteredRegex = new System.Text.RegularExpressions.Regex(@"\w+");
                var applyValidationRulesOnSaveAndContinue = await CheckIfValidationRequiredOnSaveAndContinue(applicationId, sequenceId, sectionId, pageId, answers, inputEnteredRegex);

                if (applyValidationRulesOnSaveAndContinue)
                {
                    if (answers.Any(a => inputEnteredRegex.IsMatch(a.Value)))
                    {
                        var invalidSaveResult =
                            await SaveAnswers(applicationId, sequenceId, sectionId, pageId, redirectAction);

                        if (!ModelState.IsValid) return invalidSaveResult;
                    }
                }
            }

            // If we got to here then all is well
            if (redirectAction == "Feedback")
            {
                return RedirectToAction("Feedback", new { applicationId });
            }
            else
            {
                var thisPage = await _apiClient.GetPage(applicationId, sequenceId, sectionId, pageId, User.GetUserId());
                if (thisPage.PageOfAnswers.Any())
                {
                    var next = thisPage.Next.FirstOrDefault();
                    if (next == null)
                    {
                        return RedirectToAction("Section", "Application", new { applicationId, sectionId = thisPage.SectionId });
                    }

                    if (next.Action == "NextPage")
                    {
                        return RedirectToAction("Page", new { applicationId, sequenceId = thisPage.SequenceId, sectionId = thisPage.SectionId, pageId = next.ReturnId, redirectaction = redirectAction });
                    }

                    return next.Action == "ReturnToSection"
                        ? RedirectToAction("Section", "Application", new { applicationId, sectionId = next.ReturnId })
                        : RedirectToAction("Sequence", "Application", new { applicationId });
                }

                return RedirectToAction("Page", new { applicationId, sequenceId = thisPage.SequenceId, sectionId = thisPage.SectionId, pageId = thisPage.PageId, redirectaction = redirectAction });
            }
        }

        private async Task<bool> CheckIfValidationRequiredOnSaveAndContinue(Guid applicationId, int sequenceId, int sectionId,
            string pageId, List<Answer> answers, Regex inputEnteredRegex)
        {
            var oneOrMoreAnswerEntered = false;

            foreach (var answer in answers)
            {
                if (answer.QuestionId == "RedirectAction") continue;
                if (!inputEnteredRegex.IsMatch(answer.Value)) continue;
                oneOrMoreAnswerEntered = true;
                break;
            }

            if (oneOrMoreAnswerEntered) return true;
           
            var page = await _apiClient.GetPage(applicationId, sequenceId, sectionId, pageId, User.GetUserId());
            var hasAnswersAlready = page.PageOfAnswers.Any();
            return !hasAnswersAlready;
        }

        [HttpPost("/Application/{applicationId}/Sequences/{sequenceId}/Sections/{sectionId}/Pages/{pageId}")]
        public async Task<IActionResult> SaveAnswers(Guid applicationId, int sequenceId, int sectionId, string pageId, string redirectAction)
        {
            var canUpdate = await CanUpdateApplication(applicationId, sequenceId, sectionId, User.GetUserId());
            if (!canUpdate)
            {
                return RedirectToAction("Sequence", new { applicationId });
            }
            
            var userId = User.GetUserId();

            var page = await _apiClient.GetPage(applicationId, sequenceId, sectionId, pageId, userId);


            var errorMessages = new List<ValidationErrorDetail>();
            var answers = new List<Answer>();
            
            var fileValidationPassed = FileValidationPassed(answers, page, errorMessages);
            GetAnswersFromForm(answers);

            var updatePageResult = await _apiClient.UpdatePageAnswers(applicationId, userId, sequenceId, sectionId, pageId, answers);


            if (updatePageResult.ValidationPassed && fileValidationPassed)
            {
                await UploadFilesToStorage(applicationId, sequenceId, sectionId, pageId, userId);

                if (updatePageResult.Page.AllowMultipleAnswers)
                {
                    return RedirectToAction("Page", new {applicationId, sequenceId = updatePageResult.Page.SequenceId,
                            sectionId = updatePageResult.Page.SectionId, pageId = updatePageResult.Page.PageId, redirectAction});
                }

                if (redirectAction == "Feedback")
                {
                    return RedirectToAction("Feedback", new {applicationId});
                }

                var nextActions = updatePageResult.Page.Next;

                if (nextActions.Count == 1)
                {
                    var pageNext = nextActions[0];
                    if (pageNext.Action == "NextPage" && pageNext.ConditionMet)
                    {
                        return RedirectToAction("Page", new {applicationId, sequenceId = updatePageResult.Page.SequenceId,
                            sectionId = updatePageResult.Page.SectionId, pageId = pageNext.ReturnId, redirectAction});
                    }

                    return pageNext.Action == "ReturnToSection"
                        ? RedirectToAction("Section", "Application", new {applicationId, sequenceId = updatePageResult.Page.SequenceId, sectionId = pageNext.ReturnId})
                        : RedirectToAction("Sequence", "Application", new {applicationId});
                }

                var nextConditionMet = nextActions.FirstOrDefault(na => na.ConditionMet);
                if (nextConditionMet == null) return RedirectToAction("Sequence", "Application", new {applicationId});
                
                if (nextConditionMet.Action == "NextPage")
                {
                    return RedirectToAction("Page", new {applicationId, sequenceId = updatePageResult.Page.SequenceId,
                        sectionId = updatePageResult.Page.SectionId, pageId = nextConditionMet.ReturnId, redirectAction});
                }

                return nextConditionMet.Action == "ReturnToSequence"
                    ? RedirectToAction("Section", "Application", new {applicationId, sequenceId = updatePageResult.Page.SequenceId, sectionId = nextConditionMet.ReturnId})
                    : RedirectToAction("Sequence", "Application", new {applicationId});
            }

            if (updatePageResult.ValidationErrors != null)
            {
                foreach (var error in updatePageResult.ValidationErrors)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                    errorMessages.Add(new ValidationErrorDetail(error.Key, error.Value));
                }
            }
            var returnUrl = Request.Headers["Referer"].ToString();

            var newPage = await GetDataFedOptions(updatePageResult.Page);


            var pageVm = new PageViewModel(applicationId, sequenceId, sectionId, pageId, newPage, redirectAction, returnUrl, errorMessages);


            if (page.AllowMultipleAnswers)
            {
                return View("~/Views/Application/Pages/MultipleAnswers.cshtml", pageVm);
            }

            return View("~/Views/Application/Pages/Index.cshtml", pageVm);
        }

        private async Task UploadFilesToStorage(Guid applicationId, int sequenceId, int sectionId, string pageId, Guid userId)
        {
            if (HttpContext.Request.Form.Files.Any())
            {
                await _apiClient.Upload(applicationId, userId.ToString(), sequenceId, sectionId, pageId,
                    HttpContext.Request.Form.Files);
            }
        }

        private void GetAnswersFromForm(List<Answer> answers)
        {
            foreach (var keyValuePair in HttpContext.Request.Form.Where(f => !f.Key.StartsWith("__")))
            {
                answers.Add(new Answer() {QuestionId = keyValuePair.Key, Value = keyValuePair.Value});
            }
        }

        private bool FileValidationPassed(List<Answer> answers, Page page, List<ValidationErrorDetail> errorMessages)
        {
            var fileValidationPassed = true;
            if (!HttpContext.Request.Form.Files.Any()) return true;

            foreach (var file in HttpContext.Request.Form.Files)
            {
                
                var typeValidation = page.Questions.First(q => q.QuestionId == file.Name).Input.Validations.FirstOrDefault(v => v.Name == "FileType");
                if (typeValidation != null)
                {
                    var extension = typeValidation.Value.ToString().Split(",", StringSplitOptions.RemoveEmptyEntries)[0];
                    var mimeType = typeValidation.Value.ToString().Split(",", StringSplitOptions.RemoveEmptyEntries)[1];

                    if (file.FileName.Substring(file.FileName.IndexOf(".") + 1, (file.FileName.Length - 1) - file.FileName.IndexOf(".")).ToLower() != extension || file.ContentType.ToLower() != mimeType)
                    {
                        ModelState.AddModelError(file.Name, typeValidation.ErrorMessage);
                        errorMessages.Add(new ValidationErrorDetail(file.Name, typeValidation.ErrorMessage));
                        fileValidationPassed = false;
                    }
                    else
                    {
                        // Only add to answers if type validation passes.
                        answers.Add(new Answer() {QuestionId = file.Name, Value = file.FileName});
                    }
                }
                else
                {
                    // Only add to answers if type validation passes.
                    answers.Add(new Answer() {QuestionId = file.Name, Value = file.FileName});
                }
            }

            return fileValidationPassed;
        }

        [HttpGet("Application/{applicationId}/Sequence/{sequenceId}/Section/{sectionId}/Page/{pageId}/Question/{questionId}/{filename}/Download")]
        
        //[HttpGet("/Application/{applicationId}/Page/{pageId}/Question/{questionId}/File/{filename}/Download")]
        public async Task<IActionResult> Download(Guid applicationId, int sequenceId, int sectionId, string pageId, string questionId, string filename)
        {
            var userId = User.GetUserId();

            var fileInfo = await _apiClient.FileInfo(applicationId, userId, sequenceId, sectionId, pageId, questionId, filename);
            
            var file = await _apiClient.Download(applicationId, userId, sequenceId,sectionId, pageId, questionId, filename);

            var fileStream = await file.Content.ReadAsStreamAsync();
            
            return File(fileStream, fileInfo.ContentType, fileInfo.Filename);
        }

        [HttpGet("Application/{applicationId}/Sequence/{sequenceId}/Section/{sectionId}/Page/{pageId}/Question/{questionId}/{redirectAction}/Delete")]
        public async Task<IActionResult> DeleteFile(Guid applicationId, int sequenceId, int sectionId, string pageId, string questionId, string redirectAction)
        {
            await _apiClient.DeleteFile(applicationId, User.GetUserId(), sequenceId, sectionId, pageId, questionId);
            
            return RedirectToAction("Page", new {applicationId, sequenceId, sectionId, pageId, redirectAction});
        }


        [HttpPost("/Applications/Submit")]
        public async Task<IActionResult> Submit(Guid applicationId, int sequenceId)
        {
            var canUpdate = await CanUpdateApplication(applicationId, sequenceId, null, User.GetUserId());
            if (!canUpdate)
            {
                return RedirectToAction("Sequence", new { applicationId });
            }
            
            await _apiClient.Submit(applicationId, sequenceId, User.GetUserId(), User.GetEmail());
            return RedirectToAction("Submitted", new {applicationId});
        }

        [HttpPost("/Application/DeleteAnswer")]
        public async Task<IActionResult> DeleteAnswer(Guid applicationId, int sequenceId, int sectionId, string pageId, Guid answerId, string redirectAction)
        {
            await _apiClient.DeleteAnswer(applicationId, sequenceId, sectionId, pageId, answerId, User.GetUserId());
            
            return RedirectToAction("Page", new {applicationId, sequenceId, sectionId, pageId, redirectAction});
        }

        [HttpGet("/Application/{applicationId}/Feedback")]
        public async Task<IActionResult> Feedback(Guid applicationId)
        {
            var sequence = await _apiClient.GetSequence(applicationId, User.GetUserId());

            return View("~/Views/Application/Feedback.cshtml", sequence);
        }

        [HttpGet("/Application/{applicationId}/Submitted")]
        public async Task<IActionResult> Submitted(Guid applicationId)
        {
            var application = await _apiClient.GetApplication(applicationId);
            var config = await _configService.GetConfig();
            return View("~/Views/Application/Submitted.cshtml", new SubmittedViewModel
            {
                ReferenceNumber = application.ApplicationData.ReferenceNumber,
                FeedbackUrl = config.FeedbackUrl,
                StandardName = application?.ApplicationData?.StandardName
            });
        }
    }
}