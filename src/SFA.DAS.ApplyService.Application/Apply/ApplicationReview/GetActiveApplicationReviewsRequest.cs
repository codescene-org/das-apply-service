using MediatR;
using System.Collections.Generic;
using Entities = SFA.DAS.ApplyService.Domain.Entities;

namespace SFA.DAS.ApplyService.Application.Apply.ApplicationReview
{
    public class GetActiveApplicationReviewsRequest : IRequest<List<Entities.ApplicationReview>>
    { 
    }
}
