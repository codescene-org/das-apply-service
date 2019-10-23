using MediatR;
using System;

namespace SFA.DAS.ApplyService.Application.Apply.ApplicationReview
{
    public class CreateApplicationReviewRequest : IRequest<Guid>
    {
        public Guid ApplicationId { get; }

        public CreateApplicationReviewRequest(Guid applicationId)
        {
            ApplicationId = applicationId;
        }
    }
}
