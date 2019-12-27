namespace SFA.DAS.ApplyService.Web.Infrastructure
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using Polly;
    using Polly.Extensions.Http;
    using Polly.Retry;
    using SFA.DAS.ApplyService.Configuration;
    using SFA.DAS.QnA.Api.Types;
    using SFA.DAS.QnA.Api.Types.Page;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public class QnaApiClient : IQnaApiClient
    {
        private readonly ILogger<QnaApiClient> _logger;
        private readonly IQnaTokenService _tokenService;
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly RetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly string _environmentName;

        protected readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        public QnaApiClient(IConfigurationService configurationService, ILogger<QnaApiClient> logger, IQnaTokenService tokenService)
        {
            _logger = logger;
            _tokenService = tokenService;
            _retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                    retryAttempt)));

            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(configurationService.GetConfig().Result.QnaApiAuthentication.ApiBaseAddress);
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.GetToken());

            _environmentName = configurationService.GetEnvironmentName();
        }

        public async Task<StartApplicationResponse> StartApplication(StartApplicationRequest startApplicationRequest)
        {
            var response = await _httpClient.PostAsJsonAsync($"/Applications/Start", startApplicationRequest);

            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<StartApplicationResponse>(json);
            }
            else
            {
                var apiError = JsonConvert.DeserializeObject<ApiError>(json);
                var apiErrorMessage = apiError?.Message ?? json;

                _logger.LogError($"Error Starting Application in QnA. UserReference : {startApplicationRequest?.UserReference} | WorkflowType : {startApplicationRequest?.WorkflowType} | ApplicationData : {startApplicationRequest?.ApplicationData} | StatusCode : {response.StatusCode} | Response: {apiErrorMessage}");
                return new StartApplicationResponse { ApplicationId = Guid.Empty };
            }
        }

        public async Task<object> GetApplicationData(Guid applicationId)
        {
            var response = await _httpClient.GetAsync($"Applications/{applicationId}/applicationData");

            return await response.Content.ReadAsAsync<object>();
        }


        public async Task<List<Sequence>> GetSequences(Guid applicationId)
        {
            var response = await _httpClient.GetAsync($"Applications/{applicationId}/Sequences");

            return await response.Content.ReadAsAsync<List<Sequence>>();
        }

        public async Task<Sequence> GetSequence(Guid applicationId, Guid sequenceId)
        {
            var response = await _httpClient.GetAsync($"Applications/{applicationId}/Sequences/{sequenceId}");

            return await response.Content.ReadAsAsync<Sequence>();
        }

        public async Task<Sequence> GetSequenceBySequenceNo(Guid applicationId, int sequenceNo)
        {
            var response = await _httpClient.GetAsync($"Applications/{applicationId}/Sequences/{sequenceNo}");

            return await response.Content.ReadAsAsync<Sequence>();
        }


        public async Task<List<Section>> GetSections(Guid applicationId, Guid sequenceId)
        {
            var response = await _httpClient.GetAsync($"Applications/{applicationId}/Sequences/{sequenceId}/sections");

            return await response.Content.ReadAsAsync<List<Section>>();
        }

        public async Task<Section> GetSection(Guid applicationId, Guid sectionId)
        {
            var response = await _httpClient.GetAsync($"Applications/{applicationId}/sections/{sectionId}");

            return await response.Content.ReadAsAsync<Section>();
        }

        public async Task<Section> GetSectionBySectionNo(Guid applicationId, int sequenceNo, int sectionNo)
        {
            var response = await _httpClient.GetAsync($"Applications/{applicationId}/sequences/{sequenceNo}/sections/{sectionNo}");

            return await response.Content.ReadAsAsync<Section>();
        }


        public async Task<Page> GetPage(Guid applicationId, Guid sectionId, string pageId)
        {
            var response = await _httpClient.GetAsync($"Applications/{applicationId}/sections/{sectionId}/pages/{pageId}");

            return await response.Content.ReadAsAsync<Page>();
        }

        public async Task<Page> GetPageBySectionNo(Guid applicationId, int sequenceNo, int sectionNo, string pageId)
        {
            var response = await _httpClient.GetAsync($"Applications/{applicationId}/sequences/{sequenceNo}/sections/{sectionNo}/pages/{pageId}");

            return await response.Content.ReadAsAsync<Page>();
        }


        public async Task<Answer> GetAnswer(Guid applicationId, Guid sectionId, string pageId, string questionId)
        {
            var pageContainingQuestion = await GetPage(applicationId, sectionId, pageId);

            if (pageContainingQuestion?.Questions != null)
            {
                foreach (var question in pageContainingQuestion.Questions)
                {
                    if (question.QuestionId == questionId && pageContainingQuestion.PageOfAnswers != null)
                    {
                        foreach (var pageOfAnswers in pageContainingQuestion.PageOfAnswers)
                        {
                            var pageAnswer = pageOfAnswers.Answers.FirstOrDefault(x => x.QuestionId == questionId);

                            if (pageAnswer != null)
                            {
                                return pageAnswer;
                            }
                        }
                    }
                }
            }

            return new Answer { QuestionId = questionId };
        }

        public async Task<Answer> GetAnswerByTag(Guid applicationId, string questionTag, string questionId = null)
        {
            var answer = new Answer { QuestionId = questionId };

            var applicationDataJson = await GetApplicationData(applicationId);

            if (applicationDataJson != null)
            {
                var applicationData = JObject.Parse(applicationDataJson.ToString());

                if (applicationData != null)
                {
                    var answerData = applicationData[questionTag];
                    if (answerData != null)
                    {
                        answer.Value = answerData.Value<string>();
                    }
                }
            }

            return answer;
        }

        
        public async Task<SetPageAnswersResponse> UpdatePageAnswers(Guid applicationId, Guid sectionId, string pageId, List<Answer> answers)
        {
            // TODO: RENAME METHOD TO SetPageAnswers ^~~~~~~
            var response = await _httpClient.PostAsJsonAsync(
                        $"/Applications/{applicationId}/sections/{sectionId}/pages/{pageId}",
                        answers);

            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<SetPageAnswersResponse>(json);
            }
            else
            {
                var apiError = JsonConvert.DeserializeObject<ApiError>(json);
                var apiErrorMessage = apiError?.Message ?? json;

                _logger.LogError($"Error Updating Page Answers into QnA. Application: {applicationId} | SectionId: {sectionId} | PageId: {pageId} | StatusCode : {response.StatusCode} | Response: {apiErrorMessage}");

                var validationErrorMessage = "Cannot save answers at this time. Please contact your system administrator.";

                if (!_environmentName.EndsWith("PROD", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Show API error message if outside of PROD and PREPROD environments
                    validationErrorMessage = apiErrorMessage;
                }

                var validationError = new KeyValuePair<string, string>(string.Empty, validationErrorMessage);
                return new SetPageAnswersResponse { ValidationPassed = false, ValidationErrors = new List<KeyValuePair<string, string>> { validationError } };
            }
        }

        public async Task<AddPageAnswerResponse> AddPageAnswerToMultipleAnswerPage(Guid applicationId, Guid sectionId, string pageId, List<Answer> answer)
        {
            // Not used. May need in future. See how EPAO Assessor Service does it
            throw new NotImplementedException();
        }

        public async Task<Page> RemovePageAnswerFromMultipleAnswerPage(Guid applicationId, Guid sectionId, string pageId, Guid answerId)
        {
            // Not used. May need in future. See how EPAO Assessor Service does it
            throw new NotImplementedException();
        }


        public async Task<SetPageAnswersResponse> Upload(Guid applicationId, Guid sectionId, string pageId, IFormFileCollection files)
        {
            // TODO: RENAME METHOD TO UploadFiles ^~~~~~~
            var formDataContent = new MultipartFormDataContent();

            if (files is null || files.Count < 1)
            {
                // This is so QnA knows there are no files
                formDataContent = new MultipartFormDataContent { Headers = { ContentLength = 0 } };
            }
            else
            {
                foreach (var file in files)
                {
                    var fileContent = new StreamContent(file.OpenReadStream())
                    { 
                        Headers = {
                            ContentLength = file.Length,
                            ContentType = new MediaTypeHeaderValue(file.ContentType)
                        } 
                    };

                    formDataContent.Add(fileContent, file.Name, file.FileName);
                }
            }

            var response = await _httpClient.PostAsync(
                    $"/applications/{applicationId}/sections/{sectionId}/pages/{pageId}/upload",
                    formDataContent);

            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<SetPageAnswersResponse>(json);
            }
            else
            {
                var apiError = JsonConvert.DeserializeObject<ApiError>(json);
                var apiErrorMessage = apiError?.Message ?? json;

                _logger.LogError($"Error Uploading files into QnA. Application: {applicationId} | SectionId: {sectionId} | PageId: {pageId} | StatusCode : {response.StatusCode} | Response: {apiErrorMessage}");

                var validationErrorMessage = "Cannot upload files at this time. Please contact your system administrator.";

                if (!_environmentName.EndsWith("PROD", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Show API error message if outside of PROD and PREPROD environments
                    validationErrorMessage = apiErrorMessage;
                }

                var validationError = new KeyValuePair<string, string>(string.Empty, validationErrorMessage);
                return new SetPageAnswersResponse { ValidationPassed = false, ValidationErrors = new List<KeyValuePair<string, string>> { validationError } };
            }
        }


        public async Task<HttpResponseMessage> DownloadFile(Guid applicationId, Guid sectionId, string pageId, string questionId, string filename)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/applications/{applicationId}/sections/{sectionId}/pages/{pageId}/questions/{questionId}/download/{filename}"))
            {
                return await RequestToDownloadFile(request, $"Could not download file {filename}");
            }
        }

        public async Task DeleteFile(Guid applicationId, Guid sectionId, string pageId, string questionId, string filename)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Delete, $"/applications/{applicationId}/sections/{sectionId}/pages/{pageId}/questions/{questionId}/download/{filename}"))
            {
                await Delete(request);
            }
        }


        public async Task<SkipPageResponse> SkipPage(Guid applicationId, Guid sectionId, string pageId)
        {
            var response = await _httpClient.PostAsJsonAsync($"Applications/{applicationId}/sections/{sectionId}/pages/{pageId}/skip", new object());

            return await response.Content.ReadAsAsync<SkipPageResponse>();
        }

        public async Task<SkipPageResponse> SkipPageBySectionNo(Guid applicationId, int sequenceNo, int sectionNo, string pageId)
        {
            var response = await _httpClient.PostAsJsonAsync(
                    $"Applications/{applicationId}/sequences/{sequenceNo}/sections/{sectionNo}/pages/{pageId}/skip", new object());

            return await response.Content.ReadAsAsync<SkipPageResponse>();
        }


        private async Task<HttpResponseMessage> RequestToDownloadFile(HttpRequestMessage request, string message = null)
        {
            HttpRequestMessage clonedRequest = null;

            var result = await _retryPolicy.ExecuteAsync(async () =>
            {
                clonedRequest = new HttpRequestMessage(request.Method, request.RequestUri);
                clonedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.GetToken());

                return await _httpClient.SendAsync(clonedRequest);

            });

            if (result.StatusCode == HttpStatusCode.OK)
            {
                return result;
            }

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                if (message == null)
                {
                    if (!request.RequestUri.IsAbsoluteUri)
                        message = "Could not find " + request.RequestUri;
                    else
                        message = "Could not find " + request.RequestUri.PathAndQuery;
                }

                RaiseResponseError(message, clonedRequest, result);
            }

            RaiseResponseError(clonedRequest, result);

            return result;
        }


        private static void RaiseResponseError(string message, HttpRequestMessage failedRequest, HttpResponseMessage failedResponse)
        {
            if (failedResponse.StatusCode == HttpStatusCode.NotFound)
            {
                throw new Exception(message, CreateRequestException(failedRequest, failedResponse));
            }

            throw CreateRequestException(failedRequest, failedResponse);
        }

        private static void RaiseResponseError(HttpRequestMessage failedRequest, HttpResponseMessage failedResponse)
        {
            throw CreateRequestException(failedRequest, failedResponse);
        }

        private static HttpRequestException CreateRequestException(HttpRequestMessage failedRequest, HttpResponseMessage failedResponse)
        {
            return new HttpRequestException(
                string.Format($"The Client request for {{0}} {{1}} failed. Response Status: {{2}}, Response Body: {{3}}",
                    failedRequest.Method.ToString().ToUpperInvariant(),
                    failedRequest.RequestUri,
                    (int)failedResponse.StatusCode,
                    failedResponse.Content.ReadAsStringAsync().Result));
        }


        private async Task Delete(HttpRequestMessage requestMessage)
        {
            HttpRequestMessage clonedRequest = null;
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                clonedRequest = new HttpRequestMessage(requestMessage.Method, requestMessage.RequestUri);
                clonedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.GetToken());

                return await _httpClient.SendAsync(clonedRequest);

            });

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new HttpRequestException();
            }
        }
    }
}
