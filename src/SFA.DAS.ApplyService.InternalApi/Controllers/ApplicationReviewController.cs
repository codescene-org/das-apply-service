using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.ApplyService.Application.Apply.ApplicationReview;
using SFA.DAS.ApplyService.Domain.Entities;
using SFA.DAS.ApplyService.InternalApi.Models.ApplyService;

namespace SFA.DAS.ApplyService.InternalApi.Controllers
{
    [Authorize]
    public class ApplicationReviewController : Controller
    {
        private readonly IMediator _mediator;

        public ApplicationReviewController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("ApplicationReview/CreateReview/{applicationId:Guid}")]
        public async Task<ActionResult<Guid>> CreateReview([FromRoute] Guid applicationId)
        {
            var request = new CreateApplicationReviewRequest(applicationId);

            var result = await _mediator.Send(request);

            return result;
        }

        [HttpGet("ApplicationReviews/Active")]
        public async Task<ActionResult<List<ApplicationReview>>> GetActiveApplicationReviews()
        {
            var request = new GetActiveApplicationReviewsRequest();

            var result = await _mediator.Send(request);

            return result;
        }

        [HttpGet("ApplicationReview/{applicationId:Guid}")]
        public async Task<ActionResult<ApplicationReview>> GetApplicationReview([FromRoute] Guid applicationId)
        {
            var request = new GetApplicationReviewRequest(applicationId);

            var result = await _mediator.Send(request);

            return result;
        }

        [HttpPost("ApplicationReview/{applicationId:Guid}/UpdateGatewayReview")]
        public async Task<ActionResult> UpdateGatewayReview([FromRoute] Guid applicationId, [FromBody] UpdateGatewayReviewModel model)
        {
            var request = new UpdateGatewayReviewRequest(applicationId, model.GatewayReviewStatus);

            await _mediator.Send(request);

            return Ok();
        }

        [HttpPost("ApplicationReview/{applicationId:Guid}/UpdatePmoReview")]
        public async Task<ActionResult> UpdatePmoReview([FromRoute] Guid applicationId, [FromBody] UpdatePmoReviewModel model)
        {
            var request = new UpdatePmoReviewRequest(applicationId, model.PmoReviewStatus);

            await _mediator.Send(request);

            return Ok();
        }

        [HttpPost("ApplicationReview/{applicationId:Guid}/UpdateAssessorReview")]
        public async Task<ActionResult> UpdateAssessorReview([FromRoute] Guid applicationId, [FromBody] UpdateAssessorReviewModel model)
        {
            var request = new UpdateAssessorReviewRequest(applicationId, model.ReviewNo, model.AssessorReviewStatus);

            await _mediator.Send(request);

            return Ok();
        }

        [HttpPost("ApplicationReview/{applicationId:Guid}/UpdateAssessorComments")]
        public async Task<ActionResult> UpdateAssessorComments([FromRoute] Guid applicationId, [FromBody] UpdateAssessorReviewCommentsModel model)
        {
            var request = new UpdateAssessorCommentsRequest(applicationId, model.ReviewNo, model.SectionId, model.PageId, model.Comment);

            await _mediator.Send(request);

            return Ok();
        }

        [HttpPost("ApplicationReview/{applicationId:Guid}/UpdateAssessorModeration")]
        public async Task<ActionResult> UpdateAssessorModeration([FromRoute] Guid applicationId, [FromBody] UpdateAssessorModerationModel model)
        {
            var request = new UpdateAssessorModerationRequest(applicationId, model.AssessorModerationStatus);

            await _mediator.Send(request);

            return Ok();
        }
    }
}