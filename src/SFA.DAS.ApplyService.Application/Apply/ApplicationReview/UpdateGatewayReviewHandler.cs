using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.ApplyService.Application.Apply.ApplicationReview
{
    public class UpdateGatewayReviewHandler : IRequestHandler<UpdateGatewayReviewRequest>
    {
        private readonly IApplyRepository _applyRepository;

        public UpdateGatewayReviewHandler(IApplyRepository applyRepository)
        {
            _applyRepository = applyRepository;
        }

        public async Task<Unit> Handle(UpdateGatewayReviewRequest request, CancellationToken cancellationToken)
        {
            await _applyRepository.UpdateApplicationReviewGatewayReview(request.ApplicationId, request.GatewayReviewStatus);

            return Unit.Value;
        }
    }
}
