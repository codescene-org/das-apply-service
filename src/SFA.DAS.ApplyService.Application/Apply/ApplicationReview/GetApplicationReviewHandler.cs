using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Entities = SFA.DAS.ApplyService.Domain.Entities;

namespace SFA.DAS.ApplyService.Application.Apply.ApplicationReview
{
    public class GetApplicationReviewHandler : IRequestHandler<GetApplicationReviewRequest, Entities.ApplicationReview>
    {
        private readonly IApplyRepository _applyRepository;

        public GetApplicationReviewHandler(IApplyRepository applyRepository)
        {
            _applyRepository = applyRepository;
        }

        public Task<Entities.ApplicationReview> Handle(GetApplicationReviewRequest request, CancellationToken cancellationToken)
        {
            return _applyRepository.GetApplicationReview(request.ApplicationId);
        }
    }
}
