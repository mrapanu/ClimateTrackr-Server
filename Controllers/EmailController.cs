using ClimateTrackr_Server.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace ClimateTrackr_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpPost("SendTestEmail")]
        public async Task<ActionResult<ServiceResponse<bool>>> SendTestEmail(TestEmailDto emailRequest)
        {
            var response = new ServiceResponse<bool>();
            string smtpServer = emailRequest.SmtpServer;
            int smtpPort = emailRequest.SmtpPort;
            string senderEmail = emailRequest.Username;
            string senderPassword = emailRequest.Password;

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(senderEmail);
            mail.To.Add(emailRequest.SmtpEmail);
            mail.IsBodyHtml = true;
            mail.Subject = "ClimateTrackr SMTP Test";
            mail.Body = "This is a test email sent from the ClimateTrackr server";


            SmtpClient smtpClient = new SmtpClient(smtpServer);
            smtpClient.Port = smtpPort;
            if (emailRequest.AuthOption == AuthenticationOption.UserAndPassword)
            {
                smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
            }
            if (emailRequest.ConnSecurity == ConnectionSecurity.STARTTLS || emailRequest.ConnSecurity == ConnectionSecurity.SSLTLS)
            {
                smtpClient.EnableSsl = true;
            }
            else
            {
                smtpClient.EnableSsl = false;
            }
            try
            {
                await smtpClient.SendMailAsync(mail);
                response.Data = true;
                response.Success = true;
                response.Message = $"Email sent successfully to {emailRequest.SmtpEmail}!";
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Data = false;
                response.Success = false;
                response.Message = $"Error sending email to {emailRequest.SmtpEmail} : {ex.Message} ";
                return BadRequest(response);
            }
            finally
            {
                smtpClient.Dispose();
            }
        }
    }
}