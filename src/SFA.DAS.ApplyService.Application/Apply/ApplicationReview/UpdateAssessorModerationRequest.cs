using MediatR;
using SFA.DAS.ApplyService.Domain.Entities;
using System;

namespace SFA.DAS.ApplyService.Application.Apply.ApplicationReview
{
    public class UpdateAssessorModerationRequest : IRequest
    {
        public Guid ApplicationId { get; }
        public string AssessorModerationStatus { get; }

        public UpdateAssessorModerationRequest(Guid applicationId, string assessorModerationStatus)
        {
            ApplicationId = applicationId;
            AssessorModerationStatus = assessorModerationStatus;
        }
    }
}
