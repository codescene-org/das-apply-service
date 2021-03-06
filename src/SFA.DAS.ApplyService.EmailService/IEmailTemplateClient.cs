﻿using SFA.DAS.ApplyService.Domain.Entities;
using System.Threading.Tasks;

namespace SFA.DAS.ApplyService.EmailService
{
    public interface IEmailTemplateClient
    {
       Task<EmailTemplate> GetEmailTemplate(string templateName);
    }
}
