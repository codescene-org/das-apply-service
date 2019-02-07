using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using SFA.DAS.ApplyService.Application.Apply;
using SFA.DAS.ApplyService.Domain.Entities;

namespace SFA.DAS.ApplyService.Application.UnitTests.Handlers.StartApplicationHandlerTests
{
    public class When_epao_organisation_with_due_financials_starts_application : StartApplicationHandlerTestsBase
    {
        private void Init()
        {
            OrganisationRepository.Setup(r => r.GetUserOrganisation(UserId)).ReturnsAsync(new Organisation
            {
                Id = ApplyingOrganisationId,
                OrganisationType = "",
                RoEPAOApproved = true,
                OrganisationDetails = new OrganisationDetails(){FHADetails = new FHADetails
                {
                    FinancialDueDate = DateTime.Today.AddDays(-4)
                }}
            });
        }
        
        [Test]
        public void Then_only_sections_3_and_4_should_be_required()
        {
            Init();
            
            Handler.Handle(new StartApplicationRequest(UserId), new CancellationToken()).Wait();

            ApplyRepository.Verify(r => r.UpdateSections(It.Is<List<ApplicationSection>>(response => 
                response.Any(section => section.SectionId == 1 && section.NotRequired == true)
                && response.Any(section => section.SectionId == 2 && section.NotRequired == true)
                && response.Any(section => section.SectionId == 3 && section.NotRequired == false) 
                && response.Any(section => section.SectionId == 4 && section.NotRequired == false))));
        }
        
        [Test]
        public void Then_both_sequences_should_be_required()
        {
            Init();
            
            Handler.Handle(new StartApplicationRequest(UserId), new CancellationToken()).Wait();

            ApplyRepository.Verify(r => r.UpdateSequences(It.Is<List<ApplicationSequence>>(response => response.All(sequence => sequence.NotRequired == false))));
        }   
    }
}