﻿namespace SFA.DAS.ApplyService.Web.ViewModels
{
    public class SubmittedViewModel
    {
        public string ReferenceNumber { get; set; }
        public string FeedbackUrl { get; set; }
        public string StandardName { get; set; }
        public string StandardReference { get; internal set; }
        public int? StandardLevel { get; internal set; }
    }
}
