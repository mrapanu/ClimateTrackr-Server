using ClimateTrackr_Server.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace ClimateTrackr_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly DataContext _context;
        public EmailController(DataContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("SaveSmtpSettings")]
        public async Task<ActionResult<ServiceResponse<GetSmtpSettingsDto>>> SaveSmtpSettings(SmtpSettingsDto request)
        {
            var response = new ServiceResponse<GetSmtpSettingsDto>();
            var smtp = await _context.SmtpSettings.SingleOrDefaultAsync();
            string emailPattern = @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            if (!Regex.IsMatch(request.SmtpEmail, emailPattern))
            {
                response.Message = $"{request.SmtpEmail} is not a valid email address!";
                response.Success = false;
                return Ok(response);
            }
            if (!Enum.IsDefined(typeof(AuthenticationOption), request.AuthOption))
            {
                response.Message = "Selected Authentication option doesn't exist.";
                response.Success = false;
                return BadRequest(response);
            }

            if (!Enum.IsDefined(typeof(ConnectionSecurity), request.ConnSecurity))
            {
                response.Message = "Selected Connection Security option doesn't exist.";
                response.Success = false;
                return BadRequest(response);
            }

            if (smtp == null)
            {
                _context.SmtpSettings.Add(new Smtp
                {
                    SmtpEmail = request.SmtpEmail,
                    SmtpHelo = request.SmtpHelo,
                    SmtpPort = request.SmtpPort,
                    SmtpServer = request.SmtpServer,
                    AuthOption = request.AuthOption,
                    ConnSecurity = request.ConnSecurity,
                    Username = request.Username,
                    Password = Convert.ToBase64String(Encoding.UTF8.GetBytes(request.Password))
                });
                await _context.SaveChangesAsync();
                response.Message = "Successfully created SMTP Settings!";
                response.Success = true;
                var data = await _context.SmtpSettings.SingleOrDefaultAsync();
                response.Data = new GetSmtpSettingsDto
                {
                    SmtpEmail = data!.SmtpEmail,
                    SmtpServer = data.SmtpServer,
                    SmtpPort = data.SmtpPort,
                    SmtpHelo = data.SmtpHelo,
                    AuthOption = data.AuthOption,
                    ConnSecurity = data.ConnSecurity,
                    Username = data.Username
                };
                History hist = new History
                {
                    DateTime = DateTime.Now,
                    User = User.FindFirst(ClaimTypes.Name)!.Value,
                    ActionMessage = "Created SMTP Settings successfully!",
                };
                _context.History.Add(hist);
                await _context.SaveChangesAsync();
                return Ok(response);
            }
            else
            {
                smtp.SmtpEmail = request.SmtpEmail;
                smtp.SmtpHelo = request.SmtpHelo;
                smtp.SmtpPort = request.SmtpPort;
                smtp.SmtpServer = request.SmtpServer;
                smtp.AuthOption = request.AuthOption;
                smtp.ConnSecurity = request.ConnSecurity;
                smtp.Username = request.Username;
                smtp.Password = Convert.ToBase64String(Encoding.UTF8.GetBytes(request.Password));
                await _context.SaveChangesAsync();
                response.Message = "Successfully updated SMTP Settings!";
                response.Success = true;
                var data = await _context.SmtpSettings.SingleOrDefaultAsync();
                response.Data = new GetSmtpSettingsDto
                {
                    SmtpEmail = data!.SmtpEmail,
                    SmtpServer = data.SmtpServer,
                    SmtpPort = data.SmtpPort,
                    SmtpHelo = data.SmtpHelo,
                    AuthOption = data.AuthOption,
                    ConnSecurity = data.ConnSecurity,
                    Username = data.Username
                };
                History hist = new History
                {
                    DateTime = DateTime.Now,
                    User = User.FindFirst(ClaimTypes.Name)!.Value,
                    ActionMessage = "Updated SMTP Settings successfully!",
                };
                _context.History.Add(hist);
                await _context.SaveChangesAsync();
                return Ok(response);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetSmtpSettings")]
        public async Task<ActionResult<ServiceResponse<GetSmtpSettingsDto>>> GetSmtpSettings()
        {
            var response = new ServiceResponse<GetSmtpSettingsDto>();
            var smtp = await _context.SmtpSettings.SingleOrDefaultAsync();
            if (smtp == null)
            {
                response.Data = new GetSmtpSettingsDto
                {
                    SmtpEmail = "noreply@example.com",
                    SmtpServer = "smtp.example.com",
                    SmtpPort = 25,
                    SmtpHelo = "example.com",
                    AuthOption = 0,
                    ConnSecurity = 0,
                    Username = ""
                };
                response.Success = false;
                response.Message = "SMTP Settings not set. Getting default values";
                return Ok(response);
            }
            else
            {
                response.Message = "Successfully get SMTP Settings!";
                response.Success = true;
                response.Data = new GetSmtpSettingsDto
                {
                    SmtpEmail = smtp.SmtpEmail,
                    SmtpServer = smtp.SmtpServer,
                    SmtpPort = smtp.SmtpPort,
                    SmtpHelo = smtp.SmtpHelo,
                    AuthOption = smtp.AuthOption,
                    ConnSecurity = smtp.ConnSecurity,
                    Username = smtp.Username
                };
                return Ok(response);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("SendTestEmail")]
        public async Task<ActionResult<ServiceResponse<bool>>> SendTestEmail(TestEmailDto emailRequest)
        {
            string emailPattern = @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            var smtpSettings = await _context.SmtpSettings.SingleOrDefaultAsync();
            var response = new ServiceResponse<bool>();
            if (!Regex.IsMatch(emailRequest.Recipient, emailPattern))
            {
                response.Message = $"{emailRequest.Recipient} is not a valid email address!";
                response.Success = false;
                return Ok(response);
            }
            if (smtpSettings != null)
            {

                MailMessage mail = new MailMessage();
                try
                {
                    mail.From = new MailAddress(smtpSettings.SmtpEmail);
                }

                catch (Exception ex)
                {
                    response.Data = false;
                    response.Success = false;
                    response.Message = $"Error sending email to {emailRequest.Recipient} : {ex.Message} ";
                    return BadRequest(response);
                }

                mail.To.Add(emailRequest.Recipient);
                mail.IsBodyHtml = true;
                mail.Subject = "ClimateTrackr SMTP Test";
                mail.Body = "This is a test email sent from the ClimateTrackr server";


                SmtpClient smtpClient = new SmtpClient(smtpSettings.SmtpServer);
                smtpClient.Port = smtpSettings.SmtpPort;
                if (smtpSettings.AuthOption == AuthenticationOption.UserAndPassword)
                {
                    smtpClient.Credentials = new NetworkCredential(smtpSettings.Username, System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(smtpSettings.Password)));
                }
                if (smtpSettings.ConnSecurity == ConnectionSecurity.STARTTLS || smtpSettings.ConnSecurity == ConnectionSecurity.SSLTLS)
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
                    response.Message = $"Email sent successfully to {emailRequest.Recipient}!";
                    History hist = new History
                    {
                        DateTime = DateTime.Now,
                        User = User.FindFirst(ClaimTypes.Name)!.Value,
                        ActionMessage = "Sent test email successfully!",
                    };
                    _context.History.Add(hist);
                    await _context.SaveChangesAsync();
                    return Ok(response);
                }
                catch (Exception ex)
                {
                    response.Data = false;
                    response.Success = false;
                    response.Message = $"Error sending email to {emailRequest.Recipient} : {ex.Message} ";
                    return BadRequest(response);
                }
                finally
                {
                    smtpClient.Dispose();
                }
            }
            else
            {
                response.Data = false;
                response.Success = false;
                response.Message = "Don't have any settings for SMTP. Set SMTP Settings first!";
                return BadRequest(response);
            }
        }
    }
}