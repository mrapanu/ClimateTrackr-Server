
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace ClimateTrackr_Server.Services
{
    public class NotificationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public NotificationService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        private async Task<Smtp> GetSmtpSettings()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                var smtp = await dbContext.SmtpSettings.SingleOrDefaultAsync();
                return smtp!;
            }
        }

        private async Task SendEmail(byte[] report, string recipient, ReportType reportType, string roomName)
        {

            var smtpSettings = await GetSmtpSettings();

            if (smtpSettings != null)
            {

                MailMessage mail = new MailMessage();
                try
                {
                    mail.From = new MailAddress(smtpSettings.SmtpEmail);
                }

                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

                mail.To.Add(recipient);
                mail.IsBodyHtml = true;
                mail.Subject = $"[ClimateTrackr] - {reportType.ToString()} Report";
                mail.Body = $"Your {reportType.ToString()} Report for {roomName} is available in the attachment!";
                MemoryStream pdfStream = new MemoryStream(report);
                Attachment attachment = new Attachment(pdfStream, $"{reportType.ToString()}-{roomName}.pdf", MediaTypeNames.Application.Pdf);
                mail.Attachments.Add(attachment);

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
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);

                }
                finally
                {
                    smtpClient.Dispose();
                }
            }
        }

        private async Task SendDailyEmail()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                DateTime currentDate = DateTime.Today;
                var dailyReports = await dbContext.Reports.Where(r => r.StartDate.Date == currentDate && r.Type == ReportType.Daily).ToListAsync();
                var enabledUsers = await dbContext.Users.Where(u => u.EnableNotifications == true).ToListAsync();
                var dailyRecipients = await dbContext.NotificationSettings
                    .Where(ns => enabledUsers.Select(u => u.Id).Contains(ns.UserId) &&
                                 (ns.Frequency == NotificationFrequency.Daily ||
                                  ns.Frequency == NotificationFrequency.DailyWeekly ||
                                  ns.Frequency == NotificationFrequency.DailyMonthly ||
                                  ns.Frequency == NotificationFrequency.All))
                    .Select(ns => new { ns.UserEmail, ns.SelectedRoomNames })
                    .ToListAsync();

                foreach (var report in dailyReports)
                {
                    foreach (var dailyRecipient in dailyRecipients)
                    {
                        foreach (var usrRooms in dailyRecipient.SelectedRoomNames)
                        {
                            if (report.RoomId == usrRooms.RoomConfigId)
                            {
                                //await SendEmail(report.PdfContent, dailyRecipient.UserEmail, report.Type, report.RoomName);
                            }
                        }
                    }
                }
            }
        }

        private async Task SendWeeklyEmail()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                DateTime currentDate = DateTime.Today;
                var weeklyReports = await dbContext.Reports.Where(r => r.EndDate.Date == currentDate && r.Type == ReportType.Weekly).ToListAsync();
                var enabledUsers = await dbContext.Users.Where(u => u.EnableNotifications == true).ToListAsync();
                var weeklyRecipients = await dbContext.NotificationSettings
                    .Where(ns => enabledUsers.Select(u => u.Id).Contains(ns.UserId) &&
                                 (ns.Frequency == NotificationFrequency.Weekly ||
                                  ns.Frequency == NotificationFrequency.DailyWeekly ||
                                  ns.Frequency == NotificationFrequency.WeeklyMonthly ||
                                  ns.Frequency == NotificationFrequency.All))
                    .Select(ns => new { ns.UserEmail, ns.SelectedRoomNames })
                    .ToListAsync();

                foreach (var report in weeklyReports)
                {
                    foreach (var weeklyRecipient in weeklyRecipients)
                    {
                        foreach (var usrRooms in weeklyRecipient.SelectedRoomNames)
                        {
                            if (report.RoomId == usrRooms.RoomConfigId)
                            {
                                await SendEmail(report.PdfContent, weeklyRecipient.UserEmail, report.Type, report.RoomName);
                            }
                        }
                    }
                }
            }
        }

        private async Task SendMonthlyEmail()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                DateTime currentDate = DateTime.Today;
                var monthlyReports = await dbContext.Reports.Where(r => r.EndDate.Date == currentDate && r.Type == ReportType.Monthly).ToListAsync();
                var enabledUsers = await dbContext.Users.Where(u => u.EnableNotifications == true).ToListAsync();
                var monthlyRecipients = await dbContext.NotificationSettings
                    .Where(ns => enabledUsers.Select(u => u.Id).Contains(ns.UserId) &&
                                 (ns.Frequency == NotificationFrequency.Monthly ||
                                  ns.Frequency == NotificationFrequency.DailyMonthly ||
                                  ns.Frequency == NotificationFrequency.WeeklyMonthly ||
                                  ns.Frequency == NotificationFrequency.All))
                    .Select(ns => new { ns.UserEmail, ns.SelectedRoomNames })
                    .ToListAsync();

                foreach (var report in monthlyReports)
                {
                    foreach (var monthlyRecipient in monthlyRecipients)
                    {
                        foreach (var usrRooms in monthlyRecipient.SelectedRoomNames)
                        {
                            if (report.RoomId == usrRooms.RoomConfigId)
                            {
                                await SendEmail(report.PdfContent, monthlyRecipient.UserEmail, report.Type, report.RoomName);
                            }
                        }
                    }
                }
            }
        }

        private bool ShouldSendEmailsLastMonthReport(DateTime currentTime)
        {
            //return currentTime.Day == DateTime.DaysInMonth(currentTime.Year, currentTime.Month) && currentTime.Hour == 23 && currentTime.Minute == 30;
            return currentTime.Hour == 11 && currentTime.Minute == 45;
        }

        private bool ShouldSendEmailsLastWeekReport(DateTime currentTime)
        {
            //return currentTime.DayOfWeek == DayOfWeek.Sunday && currentTime.Hour == 23 && currentTime.Minute == 40;
            return currentTime.Hour == 11 && currentTime.Minute == 50;
        }

        private bool ShouldSendEmailsCurrentDayReport(DateTime currentTime)
        {
            return currentTime.Hour == 11 && currentTime.Minute == 55;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var currentTime = DateTime.Now;
                if (ShouldSendEmailsCurrentDayReport(currentTime))
                {
                    await SendDailyEmail();
                }
                if (ShouldSendEmailsLastWeekReport(currentTime))
                {
                    await SendWeeklyEmail();
                }
                if (ShouldSendEmailsLastMonthReport(currentTime))
                {
                    await SendMonthlyEmail();
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}