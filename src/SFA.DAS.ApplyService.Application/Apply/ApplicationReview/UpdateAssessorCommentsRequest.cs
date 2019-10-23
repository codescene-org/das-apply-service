using MediatR;
using SFA.DAS.ApplyService.Domain.Entities;
using System;

namespace SFA.DAS.ApplyService.Application.Apply.ApplicationReview
{
    public class UpdateAssessorCommentsRequest : IRequest
    {
        public Guid ApplicationId { get; }
        public AssessorReviewNo ReviewNo { get; }
        public Guid SectionId { get; }
        public string PageId { get; }
        public string Comment { get; }

        public UpdateAssessorCommentsRequest(Guid applicationId, AssessorReviewNo reviewNo, Guid sectionId, string pageId, string comment)
        {
            ApplicationId = applicationId;
            ReviewNo = reviewNo;
            SectionId = sectionId;
            PageId = pageId;
            Comment = comment;
        }
    }
}
