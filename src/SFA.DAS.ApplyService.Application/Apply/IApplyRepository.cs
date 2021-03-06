using SFA.DAS.ApplyService.Application.Apply.Submit;
using SFA.DAS.ApplyService.Domain.Apply;
using SFA.DAS.ApplyService.Domain.Entities;
using SFA.DAS.ApplyService.Domain.Roatp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.ApplyService.Application.Apply
{
    public interface IApplyRepository
    {
        Task<Guid> StartApplication(Guid applicationId, ApplyData applyData, Guid organisationId, Guid createdBy);

        Task<Domain.Entities.Apply> GetApplication(Guid applicationId);
        Task<List<Domain.Entities.Apply>> GetUserApplications(Guid userId);
        Task<List<Domain.Entities.Apply>> GetOrganisationApplications(Guid userId);

        Task<bool> CanSubmitApplication(Guid applicationId);
        Task SubmitApplication(Guid applicationId, ApplyData applyData, Guid submittedBy);

        Task<bool> ChangeProviderRoute(Guid applicationId, int providerRoute);
        
        Task UpdateApplicationStatus(Guid applicationId, string status);
        
        Task<string> CheckOrganisationStandardStatus(Guid applicationId, int standardId);

        Task<IEnumerable<RoatpApplicationStatus>> GetExistingApplicationStatusByUkprn(string ukprn);

        Task<string> GetNextRoatpApplicationReference();

        Task<bool> IsUkprnWhitelisted(int ukprn);
    }
}