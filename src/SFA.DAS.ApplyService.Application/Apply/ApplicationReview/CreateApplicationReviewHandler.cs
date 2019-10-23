using MediatR;
using SFA.DAS.ApplyService.Domain.Entities;
using SFA.DAS.ApplyService.Domain.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.ApplyService.Application.Apply.ApplicationReview
{
    class CreateApplicationReviewHandler : IRequestHandler<CreateApplicationReviewRequest, Guid>
    {
        private readonly IApplyRepository _applyRepository;

        public CreateApplicationReviewHandler(IApplyRepository applyRepository)
        {
            _applyRepository = applyRepository;
        }

        public async Task<Guid> Handle(CreateApplicationReviewRequest request, CancellationToken cancellationToken)
        {
            var newApplicationReviewId = Guid.NewGuid();

            var application = await _applyRepository.GetApplication(request.ApplicationId);

            if(!application.CanCreateApplicationReview())
            {
                throw new Exception($"Cannot submit application '{application.Id}' as status is: {application.ApplicationStatus}");
            }

            //Set application status to be UnderReview
            await _applyRepository.UpdateApplicationStatus(application.Id, ApplicationStatus.UnderReview);

            //Create a new ApplicationReview
            await _applyRepository.CreateApplicationReview(newApplicationReviewId, application.Id);
            
            return newApplicationReviewId;
        }
    }
}
