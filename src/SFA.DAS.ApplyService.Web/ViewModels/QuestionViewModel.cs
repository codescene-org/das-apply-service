using System;
using System.Collections.Generic;
using ValidationErrorDetail = SFA.DAS.ApplyService.Domain.Apply.ValidationErrorDetail;
using SFA.DAS.QnA.Api.Types.Page;

namespace SFA.DAS.ApplyService.Web.ViewModels
{
    public class QuestionViewModel
    {
        public Guid Id => ApplicationId;
        public int SequenceNo => SequenceId;


        public string QuestionId { get; set; }
        public string Type { get; set; }
        public string Label { get; set; }
        public string ShortLabel { get; set; }
        public string QuestionBodyText { get; set; }
        public string Hint { get; set; }
        public string InputClasses { get; set; }
        public string Value { get; set; }
        public dynamic JsonValue { get; set; }
        public List<Option> Options { get; set; }
        public List<ValidationDefinition> Validations { get; set; }
        public List<ValidationErrorDetail> ErrorMessages { get; set; }

        public Guid ApplicationId { get; set; }
        public int SequenceId { get; set; }
        public int SectionId { get; set; }
        public string PageId { get; set; }
        public string RedirectAction { get; set; }

        public string DisplayAnswerValue(Answer answer)
        {
            if (Type == "Date" || Type == "MonthAndYear")
            {
                var dateparts = answer.Value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                if (Type == "Date")
                {
                    var datetime = DateTime.Parse($"{dateparts[0]}/{dateparts[1]}/{dateparts[2]}");
                    return datetime.ToString("dd/MM/yyyy");
                }
                else if (Type == "MonthAndYear")
                {
                    DateTime datetime;
                    DateTime.TryParse($"{dateparts[0]}/{dateparts[1]}", out datetime);
                    return datetime.ToString("MM/yyyy");
                }
            }

            return answer.Value;
        }
    }
}