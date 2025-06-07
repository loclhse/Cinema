using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IEmailService
    {
        Task SendPINForResetPassword(string toEmail);

        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
