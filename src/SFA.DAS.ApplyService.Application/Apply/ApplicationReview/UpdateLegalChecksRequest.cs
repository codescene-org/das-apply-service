using MediatR;
using SFA.DAS.ApplyService.Domain.Entities;
using System;

namespace SFA.DAS.ApplyService.Application.Apply.ApplicationReview
{
    public class UpdateLegalChecksRequest : IRequest
    {
        public Guid ApplicationId { get; }
        public LegalChecks LegalChecks { get; }

        public UpdateLegalChecksRequest(Guid applicationId, LegalChecks legalChecks)
        {
            ApplicationId = applicationId;
            LegalChecks = legalChecks;
        }
    }
}
