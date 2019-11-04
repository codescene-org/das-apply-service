using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.ApplyService.Application.Apply.ApplicationReview
{
    public class UpdateLegalChecksHandler : IRequestHandler<UpdateLegalChecksRequest>
    {
        private readonly IApplyRepository _applyRepository;

        public UpdateLegalChecksHandler(IApplyRepository applyRepository)
        {
            _applyRepository = applyRepository;
        }

        public async Task<Unit> Handle(UpdateLegalChecksRequest request, CancellationToken cancellationToken)
        {
            await _applyRepository.UpdateLegalChecks(request.ApplicationId, request.LegalChecks);

            return Unit.Value;
        }
    }
}
