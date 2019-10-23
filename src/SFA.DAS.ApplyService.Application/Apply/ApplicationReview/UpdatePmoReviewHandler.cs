using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.ApplyService.Application.Apply.ApplicationReview
{
    public class UpdatePmoReviewHandler : IRequestHandler<UpdatePmoReviewRequest>
    {
        private readonly IApplyRepository _applyRepository;

        public UpdatePmoReviewHandler(IApplyRepository applyRepository)
        {
            _applyRepository = applyRepository;
        }

        public async Task<Unit> Handle(UpdatePmoReviewRequest request, CancellationToken cancellationToken)
        {
            await _applyRepository.UpdateApplicationReviewPmoReview(request.ApplicationId, request.PmoReviewStatus);

            return Unit.Value;
        }
    }
}
