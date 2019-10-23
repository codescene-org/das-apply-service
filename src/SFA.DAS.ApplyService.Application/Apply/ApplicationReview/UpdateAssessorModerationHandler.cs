using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.ApplyService.Application.Apply.ApplicationReview
{
    public class UpdateAssessorModerationHandler : IRequestHandler<UpdateAssessorModerationRequest>
    {
        private readonly IApplyRepository _applyRepository;

        public UpdateAssessorModerationHandler(IApplyRepository applyRepository)
        {
            _applyRepository = applyRepository;
        }

        public async Task<Unit> Handle(UpdateAssessorModerationRequest request, CancellationToken cancellationToken)
        {
            await _applyRepository.UpdateApplicationReviewAssessorModeration(request.ApplicationId, request.AssessorModerationStatus);

            return Unit.Value;
        }
    }
}
