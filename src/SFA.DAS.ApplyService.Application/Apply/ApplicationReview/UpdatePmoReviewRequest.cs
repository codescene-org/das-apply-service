using MediatR;
using System;

namespace SFA.DAS.ApplyService.Application.Apply.ApplicationReview
{
    public class UpdatePmoReviewRequest : IRequest
    {
        public Guid ApplicationId { get; }
        public string PmoReviewStatus { get; }

        public UpdatePmoReviewRequest(Guid applicationId, string pmoReviewStatus)
        {
            ApplicationId = applicationId;
            PmoReviewStatus = pmoReviewStatus;
        }
    }
}
