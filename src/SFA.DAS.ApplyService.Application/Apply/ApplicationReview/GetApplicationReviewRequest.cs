using MediatR;
using System;
using Entities = SFA.DAS.ApplyService.Domain.Entities;

namespace SFA.DAS.ApplyService.Application.Apply.ApplicationReview
{
    public class GetApplicationReviewRequest : IRequest<Entities.ApplicationReview>
    {
        public Guid ApplicationId { get; }

        public GetApplicationReviewRequest(Guid applicationId)
        {
            ApplicationId = applicationId;
        }
    }
}
