using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.ApplyService.Application.Apply.GetApplications
{
    public class GetApplicationsByStatusHandler : IRequestHandler<GetApplicationsByStatusRequest, List<Domain.Entities.Application>>
    {
        private readonly IApplyRepository _applyRepository;

        public GetApplicationsByStatusHandler(IApplyRepository applyRepository)
        {
            _applyRepository = applyRepository;
        }

        public async Task<List<Domain.Entities.Application>> Handle(GetApplicationsByStatusRequest request, CancellationToken cancellationToken)
        {

            return await _applyRepository.GetApplicationsByStatus(request.Status);
        }
    }
}
