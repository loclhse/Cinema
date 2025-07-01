using System;
using System.Collections.Generic;
using System.Linq;
//using System.Net.Mail;
using System.Net;
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
using Microsoft.Extensions.Logging;

namespace Infrastructure.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, IUnitOfWork uow, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _uow = uow;
            _logger = logger;
        }

        /// <summary>
        /// Sends an email with the specified parameters
        /// </summary>
        //public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        //{
        //    _logger.LogInformation("Sending email to {Email} with subject {Subject}", toEmail, subject);

        //    // 1. Đọc config trực tiếp
        //    var smtpServer = _configuration["EmailSettings:SmtpServer"];
        //    var portRaw = _configuration["EmailSettings:SmtpPort"];
        //    var senderEmail = _configuration["EmailSettings:SenderEmail"];
        //    var senderName = _configuration["EmailSettings:SenderName"];
        //    var username = _configuration["EmailSettings:Username"];
        //    var password = _configuration["EmailSettings:Password"];

        //    // 2. Validate config
        //    if (string.IsNullOrWhiteSpace(smtpServer) ||
        //        !int.TryParse(portRaw, out var smtpPort) ||
        //        string.IsNullOrWhiteSpace(senderEmail) ||
        //        string.IsNullOrWhiteSpace(username) ||
        //        string.IsNullOrWhiteSpace(password))
        //    {
        //        var msg = "Incomplete EmailSettings (SmtpServer, SmtpPort, SenderEmail, Username or Password missing)";
        //        _logger.LogError(msg);
        //        throw new InvalidOperationException(msg);
        //    }

        //    // 3. Tạo và cấu hình SmtpClient
        //    using var smtpClient = new System.Net.Mail.SmtpClient(smtpServer, smtpPort)
        //    {
        //        EnableSsl = true,
        //        Credentials = new NetworkCredential(username, password)
        //    };

        //    // 4. Tạo MailMessage
        //    var mail = new MailMessage
        //    {
        //        From = new MailAddress(senderEmail, senderName),
        //        Subject = subject,
        //        Body = htmlBody,
        //        IsBodyHtml = true
        //    };
        //    mail.To.Add(toEmail);

        //    // 5. Gửi và log
        //    try
        //    {
        //        await smtpClient.SendMailAsync(mail);
        //        _logger.LogInformation("Email successfully sent to {Email}", toEmail);
        //    }
        //    catch (SmtpException ex)
        //    {
        //        _logger.LogError(ex, "SMTP error sending email to {Email}", toEmail);
        //        throw; // bubble lên để controller hoặc middleware xử lý
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Unexpected error sending email to {Email}", toEmail);
        //        throw;
        //    }
        //}

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
                var server = _configuration["EmailSettings:SmtpServer"] ?? throw new ArgumentNullException("Server configuration is missing");
                var portString = _configuration["EmailSettings:SmtpPort"] ?? throw new ArgumentNullException("Port configuration is missing");
                if (!int.TryParse(portString, out var port))
                {
                    throw new FormatException("Port configuration is not a valid integer");
                }

                await smtp.ConnectAsync(server, port, SecureSocketOptions.StartTls);

                // Authenticate with email and password
                var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? throw new ArgumentNullException("SenderEmail configuration is missing");
                var password = _configuration["EmailSettings:Password"] ?? throw new ArgumentNullException("Password configuration is missing");

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
            // 1. Lấy user
            var user = await _uow.UserRepo.GetUserByEmailAsync(toEmail);
            if (user == null)
                throw new Exception("Email does not exist.");

            // 2. Chuẩn bị tên hiển thị
            var displayName =
                string.IsNullOrWhiteSpace(user.FullName) || user.FullName.Contains("No name")
                ? "user"
                : user.FullName;

            // 3. Tạo PIN và lưu DB
            var pin = new Random().Next(100000, 999999).ToString();
            var expiry = DateTime.UtcNow.AddMinutes(15);
            var expiryLocal = DateTime.Now.AddMinutes(15); // convert UTC to local

            await _uow.OtpValidRepo.RemoveAllByUserIdAsync(user.Id);
            await _uow.OtpValidRepo.AddAsync(new OtpValid
            {
                AppUserId = user.Id,
                ResetPin = pin,
                ExpiryTime = expiry
            });
            await _uow.SaveChangesAsync();

            // 4. Tạo nội dung email (HTML rất đơn giản, inline CSS, không import ngoài)
            var subject = "FCinema: Password Reset OTP";
            var htmlBody = $@"
<div style=""font-family:Arial,sans-serif;line-height:1.5;color:#333;padding:20px;"">
  <p>Hello <strong>{displayName}</strong>,</p>
  <p>Your password reset code is:</p>
  <p style=""font-size:24px;font-weight:bold;color:#059669;"">{pin}</p>
  <p>This code will expire at {expiryLocal:HH:mm} UTC.</p>
  <p>If you did not request this, please ignore this email.</p>
  <hr />
  <p style=""font-size:12px;color:#888;"">&copy; {DateTime.UtcNow:yyyy} FCinema. All rights reserved.</p>
</div>";

            // 5. Gửi mail qua SmtpClient bằng hàm bạn đã tự viết
            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        /// <summary>
        /// Sends an e-ticket email after a successful booking.
        /// One seat = one ticket, but all tickets are grouped in a single message.
        /// </summary>
        /// <param name="toEmail">Customer’s email address</param>
        /// <param name="customerName">Name to greet the customer with</param>
        /// <param name="movieTitle">Movie title</param>
        /// <param name="showtimeUtc">Screening start time stored in UTC</param>
        /// <param name="cinemaRoom">Auditorium / room name</param>
        /// <param name="seatCodes">Seat identifiers (e.g., A1, B3)</param>
        public async Task SendETicketAsync(
            string toEmail,
            string customerName,
            string movieTitle,
            DateTime showtimeUtc,
            string cinemaRoom,
            IEnumerable<string> seatCodes)
        {
            // 1. Convert UTC → local time (server time zone or a specific one you choose)
            var showtimeLocal = showtimeUtc.ToLocalTime();

            // 2. Build the HTML rows for each seat
            var seatRows = new StringBuilder();
            int index = 1;
            foreach (var seat in seatCodes)
            {
                seatRows.AppendLine($@"
            <tr>
                <td style=""border:1px solid #ddd;padding:8px;text-align:center;"">{index++}</td>
                <td style=""border:1px solid #ddd;padding:8px;text-align:center;font-weight:bold;"">{seat}</td>
            </tr>");
            }

            // 3. Assemble the full email body
            var htmlBody = $@"
<div style=""font-family:Arial,sans-serif;line-height:1.5;color:#333;padding:20px;"">
  <h2 style=""color:#059669;margin-top:0;"">FCinema – E-Ticket Confirmation</h2>

  <p>Hello <strong>{customerName}</strong>,</p>
  <p>You have successfully booked <strong>{seatCodes.Count()} ticket(s)</strong>. Here are the details:</p>

  <h3 style=""margin-bottom:4px;"">Screening Details</h3>
  <ul style=""margin-top:0;"">
    <li><strong>Movie:</strong> {movieTitle}</li>
    <li><strong>Auditorium:</strong> {cinemaRoom}</li>
    <li><strong>Date & Time:</strong> {showtimeLocal:dddd, dd/MM/yyyy – HH:mm}</li>
  </ul>

  <h3 style=""margin-bottom:4px;"">Ticket Information</h3>
  <table style=""border-collapse:collapse;width:100%;margin-top:0;"">
    <thead>
      <tr style=""background:#f3f4f6;"">
        <th style=""border:1px solid #ddd;padding:8px;"">#</th>
        <th style=""border:1px solid #ddd;padding:8px;"">Seat</th>
      </tr>
    </thead>
    <tbody>
      {seatRows}
    </tbody>
  </table>

  <p style=""margin-top:16px;"">Please present this email (or the QR code in the app, if available) at the counter before the showtime.</p>

  <hr />
  <p style=""font-size:12px;color:#888;"">&copy; {DateTime.UtcNow:yyyy} FCinema. See you at the movies!</p>
</div>";

            // 4. Fire off the email
            var subject = $"[FCinema] E-Ticket – {movieTitle} ({showtimeLocal:dd/MM/yyyy HH:mm})";
            await SendEmailAsync(toEmail, subject, htmlBody);

            _logger.LogInformation("E-ticket sent to {Email} for {Movie} – {SeatCount} seat(s)",
                toEmail, movieTitle, seatCodes.Count());
        }


    }
}
