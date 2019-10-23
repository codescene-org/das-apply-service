using MediatR;
using SFA.DAS.ApplyService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.ApplyService.Application.Apply.ApplicationReview
{
    public class UpdateAssessorCommentsHandler : IRequestHandler<UpdateAssessorCommentsRequest>
    {
        private readonly IApplyRepository _applyRepository;

        public UpdateAssessorCommentsHandler(IApplyRepository applyRepository)
        {
            _applyRepository = applyRepository;
        }

        public async Task<Unit> Handle(UpdateAssessorCommentsRequest request, CancellationToken cancellationToken)
        {
            var applicationReview = await _applyRepository.GetApplicationReview(request.ApplicationId);

            PageComments pageComments;

            switch(request.ReviewNo)
            {
                case AssessorReviewNo.Review1:
                    pageComments = applicationReview.AssessorReview1Comments;
                    break;
                case AssessorReviewNo.Review2:
                    pageComments = applicationReview.AssessorReview2Comments;
                    break;
                default:
                    throw new NotImplementedException($"Cannot handle retreving comments of reviewNo:{request.ReviewNo}");
            }

            if(pageComments == null)
            {
                pageComments = new PageComments
                {
                    Comments = new List<PageComment>()
                };
            }

            var pageComment = pageComments?.Comments.SingleOrDefault(c => 
                c.SectionId == request.SectionId && 
                c.PageId == request.PageId);

            if(pageComment == null)
            {
                pageComment = new PageComment
                {
                    SectionId = request.SectionId,
                    PageId = request.PageId
                };
                pageComments.Comments.Add(pageComment);
            }

            pageComment.Comment = request.Comment;

            switch(request.ReviewNo)
            {
                case AssessorReviewNo.Review1:
                    await _applyRepository.UpdateApplicationReviewAssessor1Comments(request.ApplicationId, pageComments);
                    break;
                case AssessorReviewNo.Review2:
                    await _applyRepository.UpdateApplicationReviewAssessor2Comments(request.ApplicationId, pageComments);
                    break;
                default:
                    throw new NotImplementedException($"Cannot handle updated of reviewNo:{request.ReviewNo}");
            }

            return Unit.Value;
        }
    }
}
