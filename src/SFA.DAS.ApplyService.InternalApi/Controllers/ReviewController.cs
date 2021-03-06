using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.ApplyService.Application.Apply.Review.Return;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SFA.DAS.ApplyService.InternalApi.Controllers
{
    [Authorize]
     public class ReviewController : Controller
    {
        ///////////////////////////////////////////////////////////
        // TODO: THIS WILL NEED RE-WRITING FOR NEW RoATP PROCESS
        ///////////////////////////////////////////////////////////

        private readonly IMediator _mediator;

        public ReviewController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Review/Applications/{applicationId}/Sequences/{sequenceId}/Return")]
        public async Task Return(Guid applicationId, int sequenceId, [FromBody] ReturnApplicationRequest request)
        {
            await _mediator.Send(new ReturnRequest(applicationId, sequenceId, request.ReturnType));
        }
    }

    public class ReturnApplicationRequest
    {
        public string ReturnType { get; set; }
    }

    public class EvaluateSectionRequest
    {
        public bool IsSectionComplete { get; set; }
    }
}