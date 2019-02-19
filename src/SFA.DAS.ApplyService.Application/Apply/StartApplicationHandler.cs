using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using MediatR;
using Newtonsoft.Json;
using SFA.DAS.ApplyService.Application.Organisations;
using SFA.DAS.ApplyService.Domain.Apply;
using SFA.DAS.ApplyService.Domain.Entities;

namespace SFA.DAS.ApplyService.Application.Apply
{
    public class StartApplicationHandler : IRequestHandler<StartApplicationRequest>
    {
        private readonly IApplyRepository _applyRepository;
        private readonly IOrganisationRepository _organisationRepository;

        public StartApplicationHandler(IApplyRepository applyRepository, IOrganisationRepository organisationRepository)
        {
            _applyRepository = applyRepository;
            _organisationRepository = organisationRepository;
        }

        public async Task<Unit> Handle(StartApplicationRequest request, CancellationToken cancellationToken)
        {
            var assets = await _applyRepository.GetAssets();

            var org = await _organisationRepository.GetUserOrganisation(request.UserId);         

            var workflowId = await _applyRepository.GetLatestWorkflow("EPAO");
            var applicationId =
                await _applyRepository.CreateApplication("EPAO", org.Id, request.UserId, workflowId);

            var sections =
                await _applyRepository.CopyWorkflowToApplication(applicationId, workflowId, org.OrganisationType);

            foreach (var applicationSection in sections)
            {
                string QnADataJson = JsonConvert.SerializeObject(applicationSection.QnAData);
                foreach (var asset in assets)
                {
                    QnADataJson = QnADataJson.Replace(asset.Reference, HttpUtility.JavaScriptStringEncode(asset.Text));
                }

                applicationSection.QnAData = JsonConvert.DeserializeObject<QnAData>(QnADataJson);
            }

            var sequences = await _applyRepository.GetSequences(applicationId);
            
            DisableSequencesAndSectionsAsAppropriate(org, sequences, sections);

            await _applyRepository.UpdateSections(sections);
            await _applyRepository.UpdateSequences(sequences);
            
            return Unit.Value;
        }

        private void DisableSequencesAndSectionsAsAppropriate(Organisation org, List<ApplicationSequence> sequences, List<ApplicationSection> sections)
        {
            bool isEpao = IsOrganisationOnEPAORegister(org);
            if (isEpao)
            {
                RemoveSectionsOneAndTwo(sections);
            }

            bool isFinancialExempt = IsFinancialExempt(org.OrganisationDetails?.FHADetails);
            if (isFinancialExempt)
            {
                RemoveSectionThree(sections);
            }

            if (isEpao && isFinancialExempt)
            {
                RemoveSequenceOne(sequences);
            }
        }

        private static bool IsOrganisationOnEPAORegister(Organisation org)
        {
            if (org is null) return false;

            return org.RoEPAOApproved;
        }

        private static bool IsFinancialExempt(FHADetails financials)
        {
            if (financials is null) return false;

            bool financialExempt = financials.FinancialExempt ?? false;
            bool financialIsNotDue = (financials.FinancialDueDate?.Date ?? DateTime.MinValue) > DateTime.Today;

            return financialExempt || financialIsNotDue;
        }

        private void RemoveSequenceOne(List<ApplicationSequence> sequences)
        {
            foreach (var seq1 in sequences.Where(seq => seq.SequenceId == SequenceId.Stage1))
            {
                seq1.IsActive = false;
                seq1.NotRequired = true;
                seq1.Status = ApplicationSequenceStatus.Approved;

                SetSubmissionData(seq1.ApplicationId, seq1.SequenceId).GetAwaiter().GetResult();
            }

            foreach (var seq2 in sequences.Where(seq => seq.SequenceId == SequenceId.Stage2))
            {
                seq2.IsActive = true;
            }
        }

        private async Task SetSubmissionData(Guid applicationId, SequenceId sequenceId)
        {
            var application = await _applyRepository.GetApplication(applicationId);

            if (application != null)
            {
                if(application.ApplicationData == null)
                {
                    application.ApplicationData = new ApplicationData();
                }

                if (sequenceId == SequenceId.Stage1)
                {
                    application.ApplicationData.LatestInitSubmissionDate = DateTime.UtcNow;
                    application.ApplicationData.InitSubmissionClosedDate = DateTime.UtcNow;
                }
                else if (sequenceId == SequenceId.Stage2)
                {
                    application.ApplicationData.LatestStandardSubmissionDate = DateTime.UtcNow;
                    application.ApplicationData.StandardSubmissionClosedDate = DateTime.UtcNow;
                }

                await _applyRepository.UpdateApplicationData(application.Id, application.ApplicationData);
            }
        }

        private void RemoveSectionThree(List<ApplicationSection> sections)
        {
            foreach(var sec in sections.Where(s => s.SectionId == 3))
            {
                sec.NotRequired = true;
                sec.Status = ApplicationSectionStatus.Evaluated;

                var fhaGrade = sec.QnAData.FinancialApplicationGrade;
                if (fhaGrade is null)
                {
                    fhaGrade = new FinancialApplicationGrade();
                }

                fhaGrade.SelectedGrade = FinancialApplicationSelectedGrade.Exempt;
                fhaGrade.GradedDateTime = DateTime.UtcNow;
            }
        }

        private void RemoveSectionsOneAndTwo(List<ApplicationSection> sections)
        {
            foreach (var sec in sections.Where(s => s.SectionId == 1 || s.SectionId == 2))
            {
                sec.NotRequired = true;
                sec.Status = ApplicationSectionStatus.Evaluated;
            }
        }
    }
}