using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using SFA.DAS.ApplyService.Application.Apply;
using SFA.DAS.ApplyService.Application.Organisations;
using SFA.DAS.ApplyService.Domain.Apply;
using SFA.DAS.ApplyService.Domain.Entities;

namespace SFA.DAS.ApplyService.Application.UnitTests.Handlers.StartApplicationHandlerTests
{
    [TestFixture]
    public class StartApplicationHandlerTestsBase
    {
        protected static Guid UserId;
        protected Mock<IApplyRepository> ApplyRepository;
        protected Mock<IOrganisationRepository> OrganisationRepository;
        protected StartApplicationHandler Handler;
        protected Guid ApplyingOrganisationId;
        
        [SetUp]
        public void Setup()
        {
            UserId = Guid.NewGuid();
            var latestWorkflowId = Guid.NewGuid();
            ApplyingOrganisationId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();

            ApplyRepository = new Mock<IApplyRepository>();

            ApplyRepository.Setup(r => r.GetAssets()).ReturnsAsync(new List<Asset>());
            ApplyRepository.Setup(r => r.GetLatestWorkflow("EPAO")).ReturnsAsync(latestWorkflowId);
            ApplyRepository.Setup(r => r.CreateApplication("EPAO", ApplyingOrganisationId, UserId, latestWorkflowId)).ReturnsAsync(applicationId);
            ApplyRepository.Setup(r => r.CopyWorkflowToApplication(applicationId, latestWorkflowId, It.IsAny<string>())).ReturnsAsync(new List<ApplicationSection>
            {
                new ApplicationSection {SectionId = 1, QnAData = new QnAData {Pages = new List<Page>{new Page(){Title = "REPLACEME"}}}},
                new ApplicationSection {SectionId = 2, QnAData = new QnAData {Pages = new List<Page>()}},
                new ApplicationSection {SectionId = 3, QnAData = new QnAData {Pages = new List<Page>()}},
                new ApplicationSection {SectionId = 4, QnAData = new QnAData {Pages = new List<Page>()}}
            });
            ApplyRepository.Setup(r => r.GetSequences(applicationId)).ReturnsAsync(new List<ApplicationSequence>
            {
                new ApplicationSequence {SequenceId = SequenceId.Stage1},
                new ApplicationSequence {SequenceId = SequenceId.Stage2}
            });

            OrganisationRepository = new Mock<IOrganisationRepository>();
            
            
            Handler = new StartApplicationHandler(ApplyRepository.Object, OrganisationRepository.Object);
        }
    }
}