using MediatR;
using System.Collections.Generic;

namespace SFA.DAS.ApplyService.Application.Apply.GetApplications
{
    public class GetApplicationsByStatusRequest : IRequest<List<Domain.Entities.Application>>
    {
        public string Status { get; }

        public GetApplicationsByStatusRequest(string status)
        {
            Status = status;
        }
    }
}
