using Business.Contracts;

namespace Business.Contracts
{
    public interface IServiceManager
    {
        IAuthService AuthService
        {
         get; 
        }
        IEmailSender EmailSender
        {
        get; }
    }
}