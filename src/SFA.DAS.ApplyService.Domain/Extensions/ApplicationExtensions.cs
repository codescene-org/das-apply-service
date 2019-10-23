using SFA.DAS.ApplyService.Domain.Entities;

namespace SFA.DAS.ApplyService.Domain.Extensions
{
    public static class ApplicationExtensions
    {
        public static bool CanCreateApplicationReview(this Application application)
        {
            return application.ApplicationStatus == ApplicationStatus.Submitted;
        }
    }
}
