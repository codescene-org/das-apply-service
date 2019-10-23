using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.ApplyService.Application.Apply.ApplicationReview
{
    public class UpdateAssessorReviewHandler : IRequestHandler<UpdateAssessorReviewRequest>
    {
        private readonly IApplyRepository _applyRepository;

        public UpdateAssessorReviewHandler(IApplyRepository applyRepository)
        {
            _applyRepository = applyRepository;
        }

        public async Task<Unit> Handle(UpdateAssessorReviewRequest request, CancellationToken cancellationToken)
        {
            switch(request.ReviewNo)
            {
                case Domain.Entities.AssessorReviewNo.Review1:
                    await _applyRepository.UpdateApplicationReviewAssessor1Review(request.ApplicationId, request.AssessorReviewStatus);
                    break;
                case Domain.Entities.AssessorReviewNo.Review2:
                    await _applyRepository.UpdateApplicationReviewAssessor2Review(request.ApplicationId, request.AssessorReviewStatus);
                    break;
                default:
                    throw new NotImplementedException($"Cannot handle reviewNo:{request.ReviewNo}");
            }

            return Unit.Value;
        }
    }
}
