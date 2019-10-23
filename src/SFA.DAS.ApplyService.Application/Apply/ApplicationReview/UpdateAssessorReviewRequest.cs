using MediatR;
using SFA.DAS.ApplyService.Domain.Entities;
using System;

namespace SFA.DAS.ApplyService.Application.Apply.ApplicationReview
{
    public class UpdateAssessorReviewRequest : IRequest
    {
        public Guid ApplicationId { get; }
        public AssessorReviewNo ReviewNo { get; }
        public string AssessorReviewStatus { get; }

        public UpdateAssessorReviewRequest(Guid applicationId, AssessorReviewNo reviewNo, string assessorReviewStatus)
        {
            ApplicationId = applicationId;
            ReviewNo = reviewNo;
            AssessorReviewStatus = assessorReviewStatus;
        }
    }
}
