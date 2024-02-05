using Business.Contracts;

namespace Business
{
    public class ServiceManager : IServiceManager
    {
        private readonly IAuthService _authService;
        private readonly IEmailSender _emailSender;

        public ServiceManager(IAuthService authService, IEmailSender emailSender)
        {
            _authService = authService;
            _emailSender = emailSender;
        }

        public IAuthService AuthService => _authService;

        public IEmailSender EmailSender => _emailSender;
    }
}
