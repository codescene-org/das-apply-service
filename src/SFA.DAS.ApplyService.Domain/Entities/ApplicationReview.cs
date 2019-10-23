using System;
using System.Collections.Generic;

namespace SFA.DAS.ApplyService.Domain.Entities
{
    public class ApplicationReview
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public string GatewayReviewStatus { get; set; }
        public string PmoReviewStatus { get; set; }
        public string AssessorReview1Status { get; set; }
        public  string AssessorReview2Status { get; set; }
        public string AssessorModerationStatus { get; set; }
        public PageComments AssessorReview1Comments { get; set; }
        public PageComments AssessorReview2Comments { get; set; }
    }

    public class ApplicationReviewStatus
    {
        public const string Pending = "Pending";
        public const string UnderReview = "UnderReview";
        public const string Feedback = "Feedback";
        public const string Passed = "Passed";
        public const string Failed = "Failed";
    }

    public class PageComment
    {
        public Guid SectionId { get; set; }
        public string PageId { get; set; }
        public string Comment { get; set; }
    }

    public class PageComments
    {
        public List<PageComment> Comments { get; set; }
    }

    public enum AssessorReviewNo
    {
        Review1 = 1,
        Review2 = 2
    }
}
