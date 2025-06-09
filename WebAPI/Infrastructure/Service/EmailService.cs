using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application;
using Application.IServices;
using Domain.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Infrastructure.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _uow;

        public EmailService(IConfiguration configuration, IUnitOfWork uow)
        {
            _configuration = configuration;
            _uow = uow;
        }

        /// <summary>
        /// Sends an email with the specified parameters
        /// </summary>
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var email = new MimeMessage();

                // Set sender information
                email.From.Add(new MailboxAddress(
                    _configuration["EmailSettings:SenderName"] ?? throw new ArgumentNullException("SenderName configuration is missing"),
                    _configuration["EmailSettings:SenderEmail"] ?? throw new ArgumentNullException("SenderEmail configuration is missing")
                ));

                // Set recipient information
                email.To.Add(new MailboxAddress("", toEmail));

                // Set email subject
                email.Subject = subject;

                // Create HTML content
                email.Body = new TextPart("html") { Text = body };

                // Connect and send email via SMTP server
                using var smtp = new SmtpClient();

                // Connect to SMTP server (Using TLS)
                var server = _configuration["EmailSettings:Server"] ?? throw new ArgumentNullException("Server configuration is missing");
                var portString = _configuration["EmailSettings:Port"] ?? throw new ArgumentNullException("Port configuration is missing");
                if (!int.TryParse(portString, out var port))
                {
                    throw new FormatException("Port configuration is not a valid integer");
                }

                await smtp.ConnectAsync(server, port, SecureSocketOptions.StartTls);

                // Authenticate with email and password
                var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? throw new ArgumentNullException("SenderEmail configuration is missing");
                var password = _configuration["EmailSettings:SenderPassword"] ?? throw new ArgumentNullException("Password configuration is missing");

                await smtp.AuthenticateAsync(senderEmail, password);

                // Send email
                await smtp.SendAsync(email);

                // Disconnect after sending
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception($"Failed to send email: {ex.Message}", ex);
            }
        }

        public async Task SendPINForResetPassword(string toEmail)
        {
            var user = await _uow.UserRepo.GetUserByEmailAsync(toEmail);
            if (user == null)
                throw new Exception("Email does not exist.");

            if (string.IsNullOrEmpty(user.FullName) || user.FullName.Contains("No name"))
            {
                user.FullName = "user";
            }

            // 1. Tạo PIN mới
            var pin = new Random().Next(100000, 999999).ToString();
            var expiryTime = DateTime.UtcNow.AddMinutes(15);

            // 2. Xoá các OTP cũ (nếu có), để tránh tình trạng nhiều bản ghi
            await _uow.OtpValidRepo.RemoveAllByUserIdAsync(user.Id);
            // (Nếu RemoveAllByUserIdAsync chưa gọi SaveChangesAsync, thì gọi _uow.SaveChangesAsync() để commit)

            // 3. Tạo entity mới và lưu vào database
            var otpEntity = new OtpValid
            {
                AppUserId = user.Id,
                ResetPin = pin,
                ExpiryTime = expiryTime
            };
            await _uow.OtpValidRepo.AddAsync(otpEntity);
            await _uow.SaveChangesAsync();


            string subject = "FCinema - Password Reset Confirmation";
            string body = $@"
            <html>
            <head>
                <style>
                    @import url('https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;600;700&display=swap');
            
                    body {{ 
                        font-family: 'Poppins', Arial, sans-serif; 
                        line-height: 1.6; 
                        color: #4b5563; 
                        background-color: #f3f4f6; 
                        margin: 0; 
                        padding: 0; 
                    }}
            
                    .container {{ 
                        max-width: 600px; 
                        margin: 0 auto; 
                        padding: 20px; 
                    }}
            
                    .email-wrapper {{ 
                        background-color: #ffffff; 
                        border-radius: 16px; 
                        overflow: hidden; 
                        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08); 
                    }}
            
                    .header {{ 
                        background: linear-gradient(135deg, #059669 0%, #047857 100%);
                        color: #fff; 
                        padding: 30px; 
                        text-align: center; 
                        position: relative; 
                        overflow: hidden; 
                    }}
            
                    .header-pattern {{ 
                        position: absolute; 
                        inset: 0; 
                        background-image: url('https://via.placeholder.com/600x200?text='); 
                        opacity: 0.1; 
                        background-size: cover;
                    }}
            
                    .header h1 {{ 
                        position: relative; 
                        z-index: 10; 
                        margin: 0; 
                        font-size: 28px; 
                        font-weight: 600;
                        letter-spacing: 0.5px;
                    }}
            
                    .content {{ 
                        padding: 40px 30px; 
                        background-color: #fff; 
                    }}
            
                    .greeting {{
                        font-size: 18px;
                        margin-bottom: 20px;
                    }}
            
                    .action-button {{ 
                        display: inline-block; 
                        background: linear-gradient(to right, #059669, #047857); 
                        color: #fff !important; 
                        padding: 14px 32px; 
                        text-decoration: none; 
                        border-radius: 30px; 
                        font-weight: 600; 
                        margin-top: 20px; 
                        box-shadow: 0 4px 12px rgba(5, 150, 105, 0.2);
                        transition: all 0.3s ease;
                        font-size: 16px;
                        text-align: center;
                    }}
            
                    .action-button:hover {{ 
                        transform: translateY(-2px);
                        box-shadow: 0 6px 15px rgba(5, 150, 105, 0.25);
                    }}
            
                    .footer {{ 
                        text-align: center; 
                        padding: 25px 20px; 
                        font-size: 13px; 
                        color: #9ca3af; 
                        background-color: #f9fafb;
                        border-top: 1px solid #f3f4f6;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='email-wrapper'>
                        <div class='header'>
                            <div class='header-pattern'></div>
                            <h1>Password Reset</h1>
                        </div>
                
                        <div class='content'>
                            <p class='greeting'>Hello <strong>{user.FullName}</strong>,</p>
                    
                            <p>We have received your request to reset your password. Please use the PIN code below to complete the password reset process:</p>
                    
                            <div style='text-align: center; margin: 30px 0;'>
                                <h2 style='font-size: 24px; color: #059669; font-weight: bold;'>PIN Code: {pin}</h2>
                                <p style='font-size: 16px; color: #4b5563;'>This PIN code will expire in 15 minutes.</p>
                                <p style='font-size: 16px; color: #4b5563;'>For security reasons, please do not share this PIN code with anyone.</p>
                            </div>

                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='http://FCinema.com/contact' class='action-button'>Reset Password</a>
                            </div>
                        </div>
                
                        <div class='footer'>
                            <p>&copy; {DateTime.Now.Year} FCinema. All rights reserved.</p>
                            <p>This email was sent automatically, please do not reply.</p>
                            <div style='margin-top: 15px;'>
                                <p>Contact to us: <a href='http://FCinema.com/contact' style='color: #059669; font-weight: 600; text-decoration: underline;'>click here</a></p>
                            </div>
                        </div>
                    </div>
                </div>
            </body>
            </html>";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
