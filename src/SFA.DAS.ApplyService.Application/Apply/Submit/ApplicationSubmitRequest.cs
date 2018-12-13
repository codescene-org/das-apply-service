using System;
using MediatR;

namespace SFA.DAS.ApplyService.Application.Apply.Submit
{
    public class ApplicationSubmitRequest : IRequest
    {
        public int SequenceId { get; set; }
        public Guid ApplicationId { get; set; }
        public Guid UserId { get; set; }
    }
}