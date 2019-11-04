using MediatR;
using SFA.DAS.ApplyService.Domain.Entities;
using System;

namespace SFA.DAS.ApplyService.Application.Apply.ApplicationReview
{
    public class UpdateAddressChecksRequest : IRequest
    {
        public Guid ApplicationId { get; }
        public AddressChecks AddressChecks { get; }

        public UpdateAddressChecksRequest(Guid applicationId, AddressChecks addressChecks)
        {
            ApplicationId = applicationId;
            AddressChecks = addressChecks;
        }
    }
}
