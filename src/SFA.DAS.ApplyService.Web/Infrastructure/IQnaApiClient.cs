namespace SFA.DAS.ApplyService.Web.Infrastructure
{
    using Microsoft.AspNetCore.Http;
    using SFA.DAS.QnA.Api.Types;
    using SFA.DAS.QnA.Api.Types.Page;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IQnaApiClient
    {
        Task<StartApplicationResponse> StartApplication(StartApplicationRequest startApplicationRequest);

        Task<object> GetApplicationData(Guid applicationId);

        Task<List<Sequence>> GetSequences(Guid applicationId);
        Task<Sequence> GetSequence(Guid applicationId, Guid sequenceId);
        Task<Sequence> GetSequenceBySequenceNo(Guid applicationId, int sequenceNo);

        Task<List<Section>> GetSections(Guid applicationId, Guid sequenceId);
        Task<Section> GetSection(Guid applicationId, Guid sectionId);
        Task<Section> GetSectionBySectionNo(Guid applicationId, int sequenceNo, int sectionNo);

        Task<Page> GetPage(Guid applicationId, Guid sectionId, string pageId);
        Task<Page> GetPageBySectionNo(Guid applicationId, int sequenceNo, int sectionNo, string pageId);

        Task<Answer> GetAnswer(Guid applicationId, Guid sectionId, string pageId, string questionId);
        Task<Answer> GetAnswerByTag(Guid applicationId, string questionTag, string questionId = null);

        Task<SetPageAnswersResponse> UpdatePageAnswers(Guid applicationId, Guid sectionId, string pageId, List<Answer> answers);
        Task<AddPageAnswerResponse> AddPageAnswerToMultipleAnswerPage(Guid applicationId, Guid sectionId, string pageId, List<Answer> answer);
        Task<Page> RemovePageAnswerFromMultipleAnswerPage(Guid applicationId, Guid sectionId, string pageId, Guid answerId);

        Task<SetPageAnswersResponse> Upload(Guid applicationId, Guid sectionId, string pageId, IFormFileCollection files);
        Task<HttpResponseMessage> DownloadFile(Guid applicationId, Guid sectionId, string pageId, string questionId, string filename);
        Task DeleteFile(Guid applicationId, Guid sectionId, string pageId, string questionId, string filename);

        Task<SkipPageResponse> SkipPage(Guid applicationId, Guid sectionId, string pageId);
        Task<SkipPageResponse> SkipPageBySectionNo(Guid applicationId, int sequenceNo, int sectionNo, string pageId);
    }
}
