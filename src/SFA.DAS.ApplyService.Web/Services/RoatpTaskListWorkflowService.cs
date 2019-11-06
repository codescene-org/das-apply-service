﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MoreLinq;
using NPOI.POIFS.Storage;
using SFA.DAS.ApplyService.Domain.Entities;
using SFA.DAS.ApplyService.Web.Configuration;

namespace SFA.DAS.ApplyService.Web.Services
{
    public class RoatpTaskListWorkflowService: IRoatpTaskListWorkflowService
    {
        public  string SectionStatus(IEnumerable<ApplicationSequence> applicationSequences, List<NotRequiredOverrideConfiguration> notRequiredOverrides, int sequenceId, int sectionId, string applicationRouteId, bool sequential = false)
        {
            var sequence = applicationSequences?.FirstOrDefault(x => (int)x.SequenceId == sequenceId);

            var section = sequence?.Sections?.FirstOrDefault(x => x.SectionId == sectionId);
            if (section == null)
            {
                return string.Empty;
            }

            if (notRequiredOverrides!=null && notRequiredOverrides.Any(condition => condition.ConditionalCheckField == "ProviderTypeId" &&
                                                          applicationRouteId == condition.MustEqual &&
                                                          sectionId == condition.SectionId &&
                                                          sequenceId == condition.SequenceId))
            {
                return "Not required";
            }


            if (!PreviousSectionCompleted(sequence, sectionId, sequential))
            {
                return string.Empty;
            }

            var questionsCompleted = SectionHasCompletedQuestions(section);


            // NOTES: MFC The SectionText() method can be removed once the QnA json and process conforms to writing details
            // into the QnA, and has been fixed.  Current areas affected is all of sequence 1, and sequence 4, section 1
            // In addition, there is probably a case for removing all calls and reads to the ApplyRepository 'Completed' calls
            // (MarkSectionAsCompleted, IsSectionCompleted,RemoveSectionCompleted), and the table 'ApplicationWorkflow' may be dropped
            // I will need to double check there are no other uses for this endpoint before doing that
            var sectionTextNewWay = GetSectionText(section.PagesComplete, section.PagesActive, sequential);
            var sectionTextOldWay = SectionText(questionsCompleted, section.SectionCompleted, sequential);


            var sectionText = sectionTextNewWay;

            if (sectionTextOldWay != sectionTextNewWay)
            {
                if (sequenceId == 1 || (sequenceId == 4 && sectionId <= 2))
                {
                    sectionText = sectionTextOldWay;
                }
            }

            
            return sectionText;
        }

        public  bool PreviousSectionCompleted(ApplicationSequence sequence, int sectionId, bool sequential)
        {
            if (sequential && sectionId > 1)
            {
                var previousSection = sequence.Sections.FirstOrDefault(x => x.SectionId == (sectionId - 1));
                if (previousSection == null)
                {
                    return false;
                }

                if (previousSection.SectionCompleted)
                {
                    return true;
                }

                var previousSectionQuestionsCompleted = SectionHasCompletedQuestions(previousSection);
                var previousSectionQuestionsCount = previousSection.QnAData.Pages.Where(p => p.NotRequired == false).SelectMany(x => x.Questions)
                    .DistinctBy(q => q.QuestionId).Count();
                if (previousSectionQuestionsCompleted < previousSectionQuestionsCount)
                {
                    return false;
                }
            }

            return true;
        }

        private  int SectionHasCompletedQuestions(ApplicationSection section)
        {
            int answeredQuestions = 0;
            
            var pages = section.QnAData.Pages.Where(p => p.NotRequired == false);
            foreach (var page in pages)
            {
                var questionIds = page.Questions.Select(x => x.QuestionId);
                foreach (var questionId in questionIds)
                {
                    foreach (var pageOfAnswers in page.PageOfAnswers)
                    {
                        var matchedAnswer = pageOfAnswers.Answers.FirstOrDefault(y => y.QuestionId == questionId);
                        if (matchedAnswer != null && !String.IsNullOrEmpty(matchedAnswer.Value))
                        {
                            answeredQuestions++;
                        }
                    }
                }
            }

            return answeredQuestions;
        }

        private string GetSectionText(int completed, int active, bool sequential)
        {
            if (completed == active)
                return "Completed";

            if (sequential && completed == 0)
            {
                return "Next";
            }

            if (completed > 0)
            {
                return "In Progress";
            }

            return string.Empty;

        }

        private  string SectionText(int completedCount, bool sectionCompleted, bool sequential)
        {
            if (sectionCompleted)
            {
                return "Completed";
            }

            if (sequential && completedCount == 0)
            {
                return "Next";
            }

            if (completedCount > 0)
            {
                return "In Progress";
            }

            return string.Empty;

        }
    }
}
