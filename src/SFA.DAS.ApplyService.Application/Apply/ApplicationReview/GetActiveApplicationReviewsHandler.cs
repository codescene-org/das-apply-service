using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Entities = SFA.DAS.ApplyService.Domain.Entities;

namespace SFA.DAS.ApplyService.Application.Apply.ApplicationReview
{
    public class GetActiveApplicationReviewsHandler : IRequestHandler<GetActiveApplicationReviewsRequest, List<Entities.ApplicationReview>>
    {
        private readonly IApplyRepository _applyRepository;

        public GetActiveApplicationReviewsHandler(IApplyRepository applyRepository)
        {
            _applyRepository = applyRepository;
        }

        public Task<List<Entities.ApplicationReview>> Handle(GetActiveApplicationReviewsRequest request, CancellationToken cancellationToken)
        {
            return _applyRepository.GetActiveApplicationReviews();
        }
    }
}
