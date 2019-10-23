using MediatR;
using System;

namespace SFA.DAS.ApplyService.Application.Apply.ApplicationReview
{
    public class UpdateGatewayReviewRequest : IRequest
    {
        public Guid ApplicationId { get; }
        public string GatewayReviewStatus { get; }

        public UpdateGatewayReviewRequest(Guid applicationId, string gatewayReviewStatus)
        {
            ApplicationId = applicationId;
            GatewayReviewStatus = gatewayReviewStatus;
        }
    }
}
