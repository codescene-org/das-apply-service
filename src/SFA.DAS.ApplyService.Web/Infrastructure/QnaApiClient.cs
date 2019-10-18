﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using SFA.DAS.ApplyService.Application.Apply;
using SFA.DAS.ApplyService.Configuration;
using SFA.DAS.ApplyService.Domain.Apply;
using SFA.DAS.ApplyService.Domain.Entities;
using SFA.DAS.QnA.Api.Types;
using StartApplicationResponse = SFA.DAS.ApplyService.Application.Apply.StartApplicationResponse;

namespace SFA.DAS.ApplyService.Web.Infrastructure
{
    using Newtonsoft.Json.Linq;
    using SFA.DAS.ApplyService.Application.Apply.UpdatePageAnswers;

    public class QnaApiClient : IQnaApiClient
    {
        private readonly ILogger<QnaApiClient> _logger;
        private readonly IQnaTokenService _tokenService;
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly RetryPolicy<HttpResponseMessage> _retryPolicy;


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
        }

        public async Task<Answer> GetAnswer(Guid applicationId, Guid sectionId, string pageId, string questionId)
        {
            var pageContainingQuestion = await GetPage(applicationId, sectionId, pageId);

            foreach (var question in pageContainingQuestion.Questions)
            {
                if (question.QuestionId == questionId)
                {
                    if (pageContainingQuestion.PageOfAnswers != null && pageContainingQuestion.PageOfAnswers.Count > 0)
                    {
                        foreach (var pageOfAnswers in pageContainingQuestion.PageOfAnswers)
                        {
                            var pageAnswer =
                                pageOfAnswers.Answers.FirstOrDefault(x => x.QuestionId == questionId);
                            if (pageAnswer != null)
                            {
                                return await Task.FromResult(pageAnswer);
                            }
                        }
                    }
                }
            }

            return await Task.FromResult(new Answer { QuestionId = questionId, Value = string.Empty });
        }

        public async Task<Answer> GetAnswerByTag(Guid applicationId, string questionTag)
        {
            var answer = new Answer();
            var applicationDataJson = await (await _httpClient.GetAsync(
                    $"Applications/{applicationId}/applicationData")
                )
                .Content.ReadAsAsync<object>();
            var applicationData = JObject.Parse(applicationDataJson.ToString());
            if (applicationData != null)
            {
                var answerData = applicationData[questionTag];
                if (answerData != null)
                {
                    answer.Value = answerData.Value<string>();
                }
            }

            return await Task.FromResult(answer);
        }

        public async Task<UploadPageAnswersResult> Upload(Guid applicationId, Guid sectionId, string pageId, IFormFileCollection files)
        {

            var formDataContent = new MultipartFormDataContent();
            foreach (var file in files)
            {
                var fileContent = new StreamContent(file.OpenReadStream())
                    { Headers = { ContentLength = file.Length, ContentType = new MediaTypeHeaderValue(file.ContentType) } };
                formDataContent.Add(fileContent, file.Name, file.FileName);
            }

            return await (await _httpClient.PostAsync(
                    $"/applications/{applicationId}/sections/{sectionId}/pages/{pageId}/upload",
                    formDataContent)).Content
                .ReadAsAsync<UploadPageAnswersResult>();
        }

      

        public async Task<Page> GetPage(Guid applicationId, Guid sectionId, string pageId)
        {
            return await(await _httpClient.GetAsync(
                    $"Applications/{applicationId}/sections/{sectionId}/pages/{pageId}")
                )
                .Content.ReadAsAsync<Page>();
        }

        public async Task<ApplicationSection> GetSection(Guid applicationId, Guid sectionId)
        {
            return await (await _httpClient.GetAsync(
                    $"Applications/{applicationId}/sections/{sectionId}")
                )
                .Content.ReadAsAsync<ApplicationSection>();
        }

        public async Task<IEnumerable<ApplicationSection>> GetSections(Guid applicationId, Guid sequenceId)
        {
            return await(await _httpClient.GetAsync(
                    $"Applications/{applicationId}/Sequences/{sequenceId}/sections")
                )
                .Content.ReadAsAsync<IEnumerable<ApplicationSection>>();
        }

        public async Task<ApplicationSequence> GetSequence(Guid applicationId, Guid sequenceId)
        {
            return await(await _httpClient.GetAsync(
                    $"Applications/{applicationId}/Sequences/{sequenceId}")
                )
                .Content.ReadAsAsync<ApplicationSequence>();
        }

        public async Task<IEnumerable<ApplicationSequence>> GetSequences(Guid applicationId)
        {
            return await(await _httpClient.GetAsync(
                    $"Applications/{applicationId}/Sequences")
                )
                .Content.ReadAsAsync<IEnumerable<ApplicationSequence>>();
        }

        public async Task<StartApplicationResponse> StartApplication(string userReference, string workflowType, string applicationData)
        {
            var startApplicationRequest = new StartQnaApplicationRequest
            {
                UserReference = userReference,
                WorkflowType = workflowType,
                ApplicationData = applicationData
            };

            return await(await _httpClient.PostAsJsonAsync(
                    "/Applications/Start",
                    startApplicationRequest)).Content
                .ReadAsAsync<StartApplicationResponse>();
        }

        public async Task<SetPageAnswersResponse> UpdatePageAnswers(Guid applicationId, Guid sectionId, string pageId, List<Answer> answers)
        {
            return await (await _httpClient.PostAsJsonAsync(
                        $"/Applications/{applicationId}/sections/{sectionId}/pages/{pageId}",
                        answers)).Content
                    .ReadAsAsync<SetPageAnswersResponse>();
        }

        public async Task<HttpResponseMessage> DownloadFile(Guid applicationId, Guid sectionId, string pageId, string questionId, string fileName)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/applications/{applicationId}/sections/{sectionId}/pages/{pageId}/questions/{questionId}/download/{fileName}"))
            {
                return await RequestToDownloadFile(request,
                    $"Could not download file {fileName}");
            }
        }

        public async Task DeleteFile(Guid applicationId, Guid sectionId, string pageId, string questionId, string fileName)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Delete, $"/applications/{applicationId}/sections/{sectionId}/pages/{pageId}/questions/{questionId}/download/{fileName}"))
            {
                await Delete(request);
            }
        }

        protected async Task<HttpResponseMessage> RequestToDownloadFile(HttpRequestMessage request, string message = null)
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


        protected static void RaiseResponseError(string message, HttpRequestMessage failedRequest, HttpResponseMessage failedResponse)
        {
            if (failedResponse.StatusCode == HttpStatusCode.NotFound)
            {
                throw new Exception(message, CreateRequestException(failedRequest, failedResponse));
            }

            throw CreateRequestException(failedRequest, failedResponse);
        }

        protected static void RaiseResponseError(HttpRequestMessage failedRequest, HttpResponseMessage failedResponse)
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


        protected async Task Delete(HttpRequestMessage requestMessage)
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
