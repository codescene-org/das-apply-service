using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.ApplyService.Application.Apply.UpdatePageAnswers;
using SFA.DAS.ApplyService.Domain.Apply;
using SFA.DAS.ApplyService.Domain.Entities;

namespace SFA.DAS.ApplyService.Web.Infrastructure
{
    public interface IApplicationApiClient
    {
        Task<Page> GetPage(Guid applicationId, string pageId, Guid userId);

        Task<UpdatePageAnswersResult> UpdatePageAnswers(Guid applicationId, Guid userId, string pageId,
            List<Answer> answers);

        Task<Sequence> GetSequence(Guid applicationId, string sequenceId, Guid userId);

        Task<List<Sequence>> GetSequences(Guid applicationId, Guid userId);

        Task<List<Entity>> GetApplicationsFor(Guid userId);
    }
}