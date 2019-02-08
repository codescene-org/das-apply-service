using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.ApplyService.Application.Email.Consts;
using SFA.DAS.ApplyService.Application.Interfaces;

namespace SFA.DAS.ApplyService.Application.Users.CreateAccount
{
    public class CreateAccountHandler : IRequestHandler<CreateAccountRequest, bool>
    {
        private readonly IContactRepository _contactRepository;
        private readonly IDfeSignInService _dfeSignInService;
        private readonly IEmailService _emailServiceObject;

        public CreateAccountHandler(IContactRepository contactRepository, IDfeSignInService dfeSignInService,
            IEmailService emailServiceObject)
        {
            _contactRepository = contactRepository;
            _dfeSignInService = dfeSignInService;
            _emailServiceObject = emailServiceObject;
        }

        public async Task<bool> Handle(CreateAccountRequest request, CancellationToken cancellationToken)
        {
            var existingContact = await _contactRepository.GetContact(request.Email);
            if (existingContact == null)
            {
                var newContact = await _contactRepository.CreateContact(request.Email, request.GivenName, request.FamilyName, "DfESignIn");
                var invitationResult = await _dfeSignInService.InviteUser(request.Email, request.GivenName, request.FamilyName, newContact.Id);
                if (!invitationResult.IsSuccess)
                {
                    return false;
                }
            }
            else
            {
                if (existingContact.SigninId == null)
                {
                    var invitationResult = await _dfeSignInService.InviteUser(request.Email, request.GivenName, request.FamilyName, existingContact.Id);
                    if (!invitationResult.IsSuccess)
                    {
                        return false;
                    }
                }
                await _emailServiceObject.SendEmailToContact(EmailTemplateName.APPLY_SIGNUP_ERROR, existingContact, new { });
            }
            
            return true;
        }
    }
}