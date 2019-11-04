using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.ApplyService.Application.Apply.ApplicationReview
{
    public class UpdateAddressChecksHandler : IRequestHandler<UpdateAddressChecksRequest>
    {
        private readonly IApplyRepository _applyRepository;

        public UpdateAddressChecksHandler(IApplyRepository applyRepository)
        {
            _applyRepository = applyRepository;
        }

        public async Task<Unit> Handle(UpdateAddressChecksRequest request, CancellationToken cancellationToken)
        {
            await _applyRepository.UpdateAddressChecks(request.ApplicationId, request.AddressChecks);

            return Unit.Value;
        }
    }
}
