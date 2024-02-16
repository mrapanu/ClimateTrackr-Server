using System.Text;
using iText.Kernel.Pdf;
using iText.Html2pdf;
using PuppeteerSharp;

namespace ClimateTrackr_Server.Services
{
    public class ReportService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ReportService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        private async Task GenerateHTMLReport(DateTime date, string roomName, int roomId, int days, ReportType reportType)
        {
            DateTime startDate = date.Date.AddDays(-days + 1);
            DateTime endDate = date.Date;
            List<TempAndHum> data = new List<TempAndHum>();
            int stepTime = (days == 1) ? 10 : (days == 7) ? 60 : 120;

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                data = dbContext.TempAndHums
                    .Where(x => x.Date.Date >= startDate && x.Date.Date <= endDate && x.Room == roomName)
                    .GroupBy(x => new DateTime(x.Date.Year, x.Date.Month, x.Date.Day, x.Date.Hour, x.Date.Minute / stepTime * stepTime, 0))
                    .Select(g => new TempAndHum
                    {
                        Date = g.Key,
                        Temperature = g.Average(entry => entry.Temperature),
                        Humidity = g.Average(entry => entry.Humidity)
                    }).ToList();

                string html = data.Any() ? GenerateHTML(data, roomName, startDate, endDate) : string.Empty;
                if (html != string.Empty)
                {
                    string evalHtml = await PreEvaluateHTMLAsync(html);
                    byte[] pdfReport = ConvertHtmlToPdf(evalHtml);

                    var report = new Report
                    {
                        RoomId = roomId,
                        RoomName = roomName,
                        StartDate = startDate,
                        EndDate = endDate,
                        PdfContent = pdfReport,
                        Type = reportType,
                    };

                    dbContext.Reports.Add(report);
                    await dbContext.SaveChangesAsync();
                }

            }
        }
        private string GenerateHTML(List<TempAndHum> data, string roomName, DateTime startDate, DateTime endDate)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            htmlBuilder.Append(@"
                <!DOCTYPE html>
                <html>
                <head>
                    <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
                    <script src='https://cdnjs.cloudflare.com/ajax/libs/html2canvas/0.4.1/html2canvas.min.js'></script>
                    <style>
                        .card {
                            margin-bottom: 10px;
                            display: flex;
                            flex-wrap: wrap;
                            align-items: center;
                            text-align: center;
                            justify-content: center;
                        }
                        .summary-section {
                            flex: 100%;
                            flex-direction: column;
                            display: flex;
                            flex-wrap: wrap;
                        }
                        .graph-section {
                            flex: 100%;
                            padding: 5px;
                            margin-bottom: 150px;
                        }
                        .card-title {
                            border: 2px solid #ccc;
                            border-radius: 10px;
                            font-size: 22px;
                            padding: 10px;
                            text-align: center;
                            margin-bottom: 10px;
                        }
                        .summary-item {
                            text-align: center;
                            margin: 10px;
                            padding: 10px;
                        }
                        .card-subtitle {
                            font-size: 18px;
                            margin-bottom: 5px;
                        }
                        .card-section {
                            font-size: 16px;
                            justify-content: center;
                            background-color: #f39c56;
                            color: white;
                            padding: 10px;
                            border-radius: 10px;
                        }
                    </style>
                </head>
                <body>");

            htmlBuilder.Append($@"
                <div class='card-title'>Temperature report from {startDate:MM/dd/yyyy} to {endDate:MM/dd/yyyy} in {roomName}</div>
                <div class='card'>
                    <div class='summary-section'>
                        <div class='summary-item'>
                            <div class='card-subtitle'>Average Temperature during the period</div>
                            <div class='card-section'>Average Temperature: {data.Average(entry => entry.Temperature).ToString("0.0")} °C</div>
                        </div>
                        <div class='summary-item'>
                            <div class='card-subtitle'>Lowest temperature during the period</div>
                            <div class='card-section'>");

            var lowestTemperatures = data.OrderBy(entry => entry.Temperature).Take(3);
            foreach (var entry in lowestTemperatures)
                htmlBuilder.Append($"Timestamp: {entry.Date:MM-dd HH:mm}, Temperature: {entry.Temperature.ToString("0.0")} °C<br>");

            htmlBuilder.Append(@"</div>
                </div>
                <div class='summary-item'>
                    <div class='card-subtitle'>Highest Temperature during the period</div>
                    <div class='card-section'>");

            var highestTemperatures = data.OrderByDescending(entry => entry.Temperature).Take(3);
            foreach (var entry in highestTemperatures)
                htmlBuilder.Append($"Timestamp: {entry.Date:MM-dd HH:mm}, Temperature: {entry.Temperature.ToString("0.0")} °C<br>");

            htmlBuilder.Append(@"</div>
                    </div>
                </div>
            </div>
            <p style='text-align: justify;'>The World Health Organization in 1987 found that comfortable indoor temperatures of between 18 and 24 °C (64 and 75 °F) were not associated with health risks for healthy adults with appropriate clothing, humidity, and other factors. For infants, elderly, and those with significant health problems, a minimum 20 °C (68 °F) was recommended. Temperatures lower than 16 °C (61 °F) with humidity above 65% were associated with respiratory hazards including allergies.</p>
            <div class='card'>
                <div class='graph-section'>
                    <canvas id='temperatureChart' width='600' height='280'></canvas>
                </div>
            </div>");

            htmlBuilder.Append($@"
                <div class='card-title'>Humidity report from {startDate:MM/dd/yyyy} to {endDate:MM/dd/yyyy} in {roomName}</div>
                <div class='card'>
                    <div class='summary-section'>
                        <div class='summary-item'>
                            <div class='card-subtitle'>Average Humidity for the period</div>
                            <div class='card-section' style='background-color: #4ad4f7;'>Average Humidity: {data.Average(entry => entry.Humidity).ToString("0.0")} %</div>
                        </div>
                        <div class='summary-item'>
                            <div class='card-subtitle'>Lowest Humidity during the period</div>
                            <div class='card-section' style='background-color: #4ad4f7;'>");

            var lowestHumidities = data.OrderBy(entry => entry.Humidity).Take(3);
            foreach (var entry in lowestHumidities)
                htmlBuilder.Append($"Timestamp: {entry.Date:MM-dd HH:mm}, Humidity: {entry.Humidity.ToString("0.0")} %<br>");

            htmlBuilder.Append(@"</div>
                </div>
                <div class='summary-item'>
                    <div class='card-subtitle'>Highest Humidity during the period</div>
                    <div class='card-section' style='background-color: #4ad4f7;'>");

            var highestHumidities = data.OrderByDescending(entry => entry.Humidity).Take(3);
            foreach (var entry in highestHumidities)
                htmlBuilder.Append($"Timestamp: {entry.Date:MM-dd HH:mm}, Humidity: {entry.Humidity.ToString("0.0")} %<br>");

            htmlBuilder.Append(@"</div>
                    </div>
                </div>
            </div>
            <p style='text-align: justify;'>The recommended indoor humidity level for most indoor environments is typically between 30% to 50%. This range is considered optimal for comfort, health, and preventing issues such as mold growth and damage to wooden furniture. However, preferences may vary slightly depending on personal comfort and specific needs. It's essential to monitor humidity levels, especially in areas prone to moisture buildup, and use humidifiers or dehumidifiers as needed to maintain a healthy indoor environment.</p>          
            <div class='card'>
                <div class='graph-section'>
                    <canvas id='humidityChart' width='600' height='280'></canvas>
                </div>
            </div>");

            htmlBuilder.Append(@"
                <script>
                    var temperatureData = {
                        labels: [");

            foreach (var entry in data)
                htmlBuilder.Append($"'{entry.Date.ToString("dd MMM")} {entry.Date.ToString("HH:mm")}',");

            htmlBuilder.Append(@"],
                datasets: [{
                    label: 'Temperature',
                    data: [");

            foreach (var entry in data)
                htmlBuilder.Append($"{Math.Round(entry.Temperature, 1)},");

            htmlBuilder.Append(@"],
                    borderColor: '#f39c56',
                    backgroundColor: 'rgba(243, 156, 86, 0.4)',
                    borderWidth: 1,
                    fill: 'start',
                    pointRadius: 1,
                }]
            };

            var humidityData = {
                labels: [");

            foreach (var entry in data)
                htmlBuilder.Append($"'{entry.Date.ToString("dd MMM")} {entry.Date.ToString("HH:mm")}',");

            htmlBuilder.Append(@"],
                datasets: [{
                    label: 'Humidity',
                    data: [");

            foreach (var entry in data)
                htmlBuilder.Append($"{Math.Round(entry.Humidity, 1)},");

            htmlBuilder.Append(@"],
                        borderColor: '#4ad4f7',
                        backgroundColor: 'rgba(74, 212, 247, 0.4)',
                        borderWidth: 1,
                        fill: 'start',
                        pointRadius: 1,
                    }]
                };
                var temperatureCtx = document.getElementById('temperatureChart').getContext('2d');
                var temperatureChart = new Chart(temperatureCtx, {
                    type: 'line',
                    data: temperatureData,
                    options: {
                        scales: {
                            y: {
                                min: 0,
                                max: 35
                            }
                        },
                        responsive: true, 
                        maintainAspectRatio: false
                    }
                });
                var humidityCtx = document.getElementById('humidityChart').getContext('2d');
                var humidityChart = new Chart(humidityCtx, {
                    type: 'line',
                    data: humidityData,
                    options: {
                        scales: {
                            y: {
                                min: 0,
                                max: 100
                            }
                        },
                        responsive: true, 
                        maintainAspectRatio: false
                    }
                });
            </script>");

            htmlBuilder.Append(@"
                <script>
                    setTimeout(function() {
                        html2canvas(document.getElementById('temperatureChart'), {
                            onrendered: function(canvas) {
                                var imgData = canvas.toDataURL('image/png');
                                var img = document.createElement('img');
                                img.src = imgData;
                                var chartCanvas = document.getElementById('temperatureChart');
                                chartCanvas.parentNode.replaceChild(img, chartCanvas);
                            }
                        });
                        html2canvas(document.getElementById('humidityChart'), {
                            onrendered: function(canvas) {
                                var imgData = canvas.toDataURL('image/png');
                                var img = document.createElement('img');
                                img.src = imgData;
                                var chartCanvas = document.getElementById('humidityChart');
                                chartCanvas.parentNode.replaceChild(img, chartCanvas);
                            }
                        });
                    }, 500);
                </script>
                </body>
                </html>");

            return htmlBuilder.ToString();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                    var currentTime = DateTime.Now;

                    var roomConfigs = dbContext.RoomConfigs.ToList();

                    if (roomConfigs.Any())
                    {
                        foreach (var roomConfig in roomConfigs)
                        {
                            if (ShouldGenerateLastMonthReport(currentTime))
                            {
                                await GenerateHTMLReportLastMonth(currentTime, roomConfig.RoomName, roomConfig.Id);
                            }

                            if (ShouldGenerateLastWeekReport(currentTime))
                            {
                                await GenerateHTMLReportLastWeek(currentTime, roomConfig.RoomName, roomConfig.Id);
                            }

                            if (ShouldGenerateCurrentDayReport(currentTime))
                            {
                                await GenerateHTMLReportCurrentDay(currentTime, roomConfig.RoomName, roomConfig.Id);
                            }
                        }
                    }
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        public async Task GenerateHTMLReportCurrentDay(DateTime date, string roomName, int roomId)
        {
            await GenerateHTMLReport(date, roomName, roomId, 1, ReportType.Daily);

        }

        public async Task GenerateHTMLReportLastWeek(DateTime date, string roomName, int roomId)
        {
            await GenerateHTMLReport(date, roomName, roomId, 7, ReportType.Weekly);
        }

        public async Task GenerateHTMLReportLastMonth(DateTime date, string roomName, int roomId)
        {
            await GenerateHTMLReport(date, roomName, roomId, 30, ReportType.Monthly);
        }

        private bool ShouldGenerateLastMonthReport(DateTime currentTime)
        {
            return currentTime.Day == DateTime.DaysInMonth(currentTime.Year, currentTime.Month) && currentTime.Hour == 23 && currentTime.Minute == 30;
        }

        private bool ShouldGenerateLastWeekReport(DateTime currentTime)
        {
            return currentTime.DayOfWeek == DayOfWeek.Sunday && currentTime.Hour == 23 && currentTime.Minute == 40;
        }

        private bool ShouldGenerateCurrentDayReport(DateTime currentTime)
        {
            return currentTime.Hour == 10 && currentTime.Minute == 39;
        }

        static byte[] ConvertHtmlToPdf(string htmlContent)
        {
            using (MemoryStream stream = new MemoryStream())
            {

                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdf = new PdfDocument(writer);
                ConverterProperties properties = new ConverterProperties();
                HtmlConverter.ConvertToPdf(htmlContent, pdf, properties);
                pdf.Close();
                return stream.ToArray();
            }
        }

        private async Task<string> PreEvaluateHTMLAsync(string htmlContent)
        {
            await new BrowserFetcher().DownloadAsync();
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
            });
            var page = await browser.NewPageAsync();
            await page.SetContentAsync(htmlContent);
            await page.WaitForTimeoutAsync(1000);
            string evaluatedHtml = await page.GetContentAsync();
            await browser.CloseAsync();
            return evaluatedHtml;
        }

    }
}
