using SFA.DAS.ApplyService.Domain.Entities;
using System;

namespace SFA.DAS.ApplyService.InternalApi.Models.ApplyService
{
    public class UpdateAssessorReviewCommentsModel
    {
        public AssessorReviewNo ReviewNo { get; set; }
        public Guid SectionId { get; set; }
        public string PageId { get; set; }
        public string Comment { get; set; }
    }
}
