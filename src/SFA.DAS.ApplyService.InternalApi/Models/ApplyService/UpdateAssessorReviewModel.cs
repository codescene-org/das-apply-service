using SFA.DAS.ApplyService.Domain.Entities;

namespace SFA.DAS.ApplyService.InternalApi.Models.ApplyService
{
    public class UpdateAssessorReviewModel
    {
        public AssessorReviewNo ReviewNo { get; set; }
        public string AssessorReviewStatus { get; set; }
    }
}
