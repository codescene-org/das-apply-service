﻿using System;
using System.Collections.Generic;

namespace SFA.DAS.ApplyService.Domain.Entities
{
    public class Apply : EntityBase
    {
        public Guid ApplicationId { get; set; }
        public Guid OrganisationId { get; set; }
        public string ApplicationStatus { get; set; }
        public string AssessorReviewStatus { get; set; }
        public ApplyData ApplyData { get; set; }
    }

    public class ApplyData
    {
        public List<ApplySequence> Sequences { get; set; }
        public ApplyDetails ApplyDetails { get; set; }
    }

    public class ApplyDetails
    {
        // NOTE THIS IS A SIMILAR COPY OF RoatpApplicationData
        public string ReferenceNumber { get; set; }
        public string UKPRN { get; set; }
        public string OrganisationName { get; set; }
        public string TradingName { get; set; }
        public int ProviderRoute { get; set; } // was string - ApplicationRouteId
        public DateTime? ApplicationSubmittedOn { get; set; }
        public Guid? ApplicationSubmittedBy { get; set; }
    }

    public class ApplySequence
    {
        public Guid SequenceId { get; set; }
        public int SequenceNo { get; set; }
        public List<ApplySection> Sections { get; set; }
        //public string Status { get; set; }
        //public bool IsActive { get; set; }
        //public bool NotRequired { get; set; }
        //public bool Sequential { get; set; }
        //public string Description { get; set; }
        //public DateTime? ApprovedDate { get; set; }
        //public string ApprovedBy { get; set; }
    }

    public class ApplySection
    {
        public Guid SectionId { get; set; }
        public int SectionNo { get; set; }
        //public string Status { get; set; }
        //public DateTime? ReviewStartDate { get; set; }
        //public string ReviewedBy { get; set; }
        //public DateTime? EvaluatedDate { get; set; }
        //public string EvaluatedBy { get; set; }
        //public bool NotRequired { get; set; }
        //public bool? RequestedFeedbackAnswered { get; set; }
    }
}