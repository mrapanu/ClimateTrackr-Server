using System.Text;

namespace ClimateTrackr_Server.Services
{
    public class ReportService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ReportService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        private async Task SaveReportToDatabaseAsync(string roomName, DateTime startDate, DateTime endDate, string htmlContent, ReportType reportType)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                var report = new Report
                {
                    RoomName = roomName,
                    StartDate = startDate,
                    EndDate = endDate,
                    HtmlContent = Encoding.UTF8.GetBytes(htmlContent),
                    Type = reportType,
                };

                dbContext.Reports.Add(report);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task GenerateHTMLReportCurrentDay(DateTime date, string roomName)
        {
            await GenerateHTMLReport(date, roomName, 1, ReportType.Daily);
        }

        public async Task GenerateHTMLReportLastWeek(DateTime date, string roomName)
        {
            await GenerateHTMLReport(date, roomName, 7, ReportType.Weekly);
        }

        public async Task GenerateHTMLReportLastMonth(DateTime date, string roomName)
        {
            await GenerateHTMLReport(date, roomName, 30, ReportType.Monthly);
        }

        private async Task GenerateHTMLReport(DateTime date, string roomName, int days, ReportType reportType)
        {
            DateTime startDate = date.Date.AddDays(-days + 1);
            DateTime endDate = date.Date;
            List<TempAndHum> data = new List<TempAndHum>();
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                if (days == 1)
                {
                    data = dbContext.TempAndHums
                        .Where(x => x.Date.Date >= startDate && x.Date.Date <= endDate && x.Room == roomName)
                        .GroupBy(x => new DateTime(x.Date.Year, x.Date.Month, x.Date.Day, x.Date.Hour, x.Date.Minute / 10 * 10, 0))
                        .Select(g => new TempAndHum
                        {
                            Date = g.Key,
                            Temperature = g.Average(entry => entry.Temperature),
                            Humidity = g.Average(entry => entry.Humidity)
                        })
                        .ToList();
                }

                else if (days == 7)
                {
                    data = dbContext.TempAndHums
                        .Where(x => x.Date >= startDate && x.Date <= endDate && x.Room == roomName)
                        .GroupBy(x => new DateTime(x.Date.Year, x.Date.Month, x.Date.Day, x.Date.Hour, x.Date.Minute / 60 * 60, 0))
                        .Select(g => new TempAndHum
                        {
                            Date = g.Key,
                            Temperature = g.Average(entry => entry.Temperature),
                            Humidity = g.Average(entry => entry.Humidity)
                        })
                        .ToList();
                }
                else
                {
                    data = dbContext.TempAndHums
                        .Where(x => x.Date >= startDate && x.Date <= endDate && x.Room == roomName)
                        .GroupBy(x => new DateTime(x.Date.Year, x.Date.Month, x.Date.Day, x.Date.Hour, x.Date.Minute / 120 * 120, 0))
                        .Select(g => new TempAndHum
                        {
                            Date = g.Key,
                            Temperature = g.Average(entry => entry.Temperature),
                            Humidity = g.Average(entry => entry.Humidity)
                        })
                        .ToList();
                }

            }
            string html;

            if (data.Any())
            {
                html = GenerateHTML(data, roomName, startDate, endDate);
            }
            else
            {
                html = GenerateNoDataHTML(roomName, startDate, endDate);
            }

            await SaveReportToDatabaseAsync(roomName, startDate, endDate, html, reportType);
        }

        private string GenerateNoDataHTML(string roomName, DateTime startDate, DateTime endDate)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            htmlBuilder.Append($@"<!DOCTYPE html>
            <html>
            <head>
                <title>No Data Available</title>
                <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
                <style>
                    .card {{
                        border: 1px solid #ccc;
                        border-radius: 5px;
                        padding: 10px;
                        margin-bottom: 20px;
                        background-color: #f9f9f9;
                        display: flex;
                        flex-wrap: wrap;
                    }}
                    .left-section {{
                        flex: 25%;
                        padding-right: 5%;
                    }}
                    .right-section {{
                        flex: 70%;
                    }}
                    .card-title {{
                        font-size: 20px;
                        margin-bottom: 10px;
                    }}
                    .card-subtitle {{
                        font-size: 16px;
                        margin-bottom: 5px;
                    }}
                    .card-section {{
                        margin-bottom: 15px;
                    }}
                </style>
            </head>
            <body>");

            htmlBuilder.Append($@"
            <div class='card'>
                <div class='left-section'>
                    <div class='card-title'>Temperature and Humidity report from {startDate:MM/dd/yyyy} to {endDate:MM/dd/yyyy} in {roomName}</div>
                    <div class='card-section' style='background-color: #f9f9f9; padding: 10px; border-radius: 5px;'>
                        <h3>No data available for {roomName} from {startDate:MM/dd/yyyy} to {endDate:MM/dd/yyyy}.</h3>
                    </div>
                </div>
                <div class='right-section'>
                    <canvas id='temperatureChart' width='500' height='100'></canvas>
                    <canvas id='humidityChart' width='500' height='100'></canvas>
                </div>
            </div>");

            htmlBuilder.Append(@"
            <script>
                // Display warning message
                var temperatureCtx = document.getElementById('temperatureChart').getContext('2d');
                var temperatureChart = new Chart(temperatureCtx, {
                    type: 'bar',
                    data: {
                        labels: ['No Data'],
                        datasets: [{
                            label: 'No Data',
                            data: [0],
                            backgroundColor: '#f39c56',
                            borderWidth: 1
                        }]
                    },
                    options: {
                        scales: {
                            y: {
                                beginAtZero: true
                            }
                        }
                    }
                });

                var humidityCtx = document.getElementById('humidityChart').getContext('2d');
                var humidityChart = new Chart(humidityCtx, {
                    type: 'bar',
                    data: {
                        labels: ['No Data'],
                        datasets: [{
                            label: 'No Data',
                            data: [0],
                            backgroundColor: '#4ad4f7',
                            borderWidth: 1
                        }]
                    },
                    options: {
                        scales: {
                            y: {
                                beginAtZero: true
                            }
                        }
                    }
                });
            </script>
        ");

            htmlBuilder.Append(@"
            </body>
            </html>");

            return htmlBuilder.ToString();
        }

        private string GenerateHTML(List<TempAndHum> data, string roomName, DateTime startDate, DateTime endDate)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            htmlBuilder.Append($@"<!DOCTYPE html>
            <html>
            <head>
                <title>Graphs</title>
                <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
                <style>
                    .card {{
                        border: 1px solid #ccc;
                        border-radius: 5px;
                        padding: 10px;
                        margin-bottom: 20px;
                        background-color: #f9f9f9;
                        display: flex;
                        flex-wrap: wrap;
                    }}
                    .left-section {{
                        flex: 25%;
                        padding-right: 5%;
                    }}
                    .right-section {{
                        flex: 70%;
                    }}
                    .card-title {{
                        font-size: 20px;
                        margin-bottom: 10px;
                    }}
                    .card-subtitle {{
                        font-size: 16px;
                        margin-bottom: 5px;
                    }}
                    .card-section {{
                        margin-bottom: 15px;
                    }}
                </style>
            </head>
            <body>");

            htmlBuilder.Append($@"<div class='card'>
                <div class='left-section'>
                    <div class='card-title'>Temperature report from {startDate:MM/dd/yyyy} to {endDate:MM/dd/yyyy} in {roomName}</div>
                    <div class='card-subtitle'>Average Temperature for the period</div>
                    <div class='card-section' style='background-color: #f39c56; color: white; padding: 10px; border-radius: 5px;'>");

            double averageTemperature = data.Average(entry => entry.Temperature);
            htmlBuilder.Append("Average Temperature: " + averageTemperature.ToString("0.0") + " °C");

            htmlBuilder.Append(@"</div>
                    <div class='card-subtitle'>Lowest temperature during the period</div>
                    <div class='card-section' style='background-color: #f39c56; color: white; padding: 10px; border-radius: 5px;'>");
            var lowestTemperatures = data.OrderBy(entry => entry.Temperature).Take(3);
            foreach (var entry in lowestTemperatures)
            {
                htmlBuilder.Append($"Timestamp: {entry.Date:HH:mm}, Temperature: {entry.Temperature.ToString("0.0")} °C<br>");
            }

            htmlBuilder.Append(@"</div>
                    <div class='card-subtitle'>Highest Temperature during the period</div>
                    <div class='card-section' style='background-color: #f39c56; color: white; padding: 10px; border-radius: 5px;'>");

            var highestTemperatures = data.OrderByDescending(entry => entry.Temperature).Take(3);
            foreach (var entry in highestTemperatures)
            {
                htmlBuilder.Append($"Timestamp: {entry.Date:HH:mm}, Temperature: {entry.Temperature.ToString("0.0")} °C<br>");
            }

            htmlBuilder.Append(@"</div>
                </div>
                <div class='right-section'>
                    <canvas id='temperatureChart' width='500' height='100'></canvas>
                </div>
                <p style='color: #f39c56; font-size: 18px;'>The World Health Organization in 1987 found that comfortable indoor temperatures of between 18 and 24 °C (64 and 75 °F) were not associated with health risks for healthy adults with appropriate clothing, humidity, and other factors. For infants, elderly, and those with significant health problems, a minimum 20 °C (68 °F) was recommended. Temperatures lower than 16 °C (61 °F) with humidity above 65% were associated with respiratory hazards including allergies.</p>
            </div>");

            htmlBuilder.Append($@"<div class='card'>
                <div class='left-section'>
                    <div class='card-title'>Humidity report from {startDate:MM/dd/yyyy} to {endDate:MM/dd/yyyy} in {roomName}</div>
                    <div class='card-subtitle'>Average Humidity for the period</div>
                    <div class='card-section' style='background-color: #4ad4f7; color: white; padding: 10px;  border-radius: 5px;'>");
            double averageHumidity = data.Average(entry => entry.Humidity);
            htmlBuilder.Append("Average Humidity: " + averageHumidity.ToString("0.0") + " %");

            htmlBuilder.Append(@"</div>
                    <div class='card-subtitle'>Lowest Humidity during the period</div>
                    <div class='card-section' style='background-color: #4ad4f7; color: white; padding: 10px; border-radius: 5px;'>");

            var lowestHumidities = data.OrderBy(entry => entry.Humidity).Take(3);
            foreach (var entry in lowestHumidities)
            {
                htmlBuilder.Append($"Timestamp: {entry.Date:HH:mm}, Humidity: {entry.Humidity.ToString("0.0")} %<br>");
            }

            htmlBuilder.Append(@"</div>
                    <div class='card-subtitle'>Highest Humidity during the period</div>
                    <div class='card-section' style='background-color: #4ad4f7; color: white; padding: 10px; border-radius: 5px;'>");

            var highestHumidities = data.OrderByDescending(entry => entry.Humidity).Take(3);
            foreach (var entry in highestHumidities)
            {
                htmlBuilder.Append($"Timestamp: {entry.Date:HH:mm}, Humidity: {entry.Humidity.ToString("0.0")} %<br>");
            }

            htmlBuilder.Append(@"</div>
                </div>
                <div class='right-section'>
                    <canvas id='humidityChart' width='500' height='100'></canvas>
                </div>
                <p style='color: #4ad4f7; font-size: 18px;'> The recommended indoor humidity level for most indoor environments is typically between 30% to 50%. This range is considered optimal for comfort, health, and preventing issues such as mold growth and damage to wooden furniture. <br>However, preferences may vary slightly depending on personal comfort and specific needs. It's essential to monitor humidity levels, especially in areas prone to moisture buildup, and use humidifiers or dehumidifiers as needed to maintain a healthy indoor environment.</p>
            </div>
            <script>
                // Chart.js code for drawing temperature and humidity charts...
                var temperatureData = {
                    labels: [");

            foreach (var entry in data)
            {
                var dayMonth = entry.Date.ToString("dd MMM");
                var timeOfDay = entry.Date.ToString("HH:mm");
                var label = $"{dayMonth} {timeOfDay}";
                htmlBuilder.Append($"'{label}',");
            }

            htmlBuilder.Append(@"],
                    datasets: [{
                        label: 'Temperature',
                        data: [");

            foreach (var entry in data)
            {
                htmlBuilder.Append($"{Math.Round(entry.Temperature, 1)},");
            }

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
            {
                var dayMonth = entry.Date.ToString("dd MMM");
                var timeOfDay = entry.Date.ToString("HH:mm");
                var label = $"{dayMonth} {timeOfDay}";
                htmlBuilder.Append($"'{label}',");
            }

            htmlBuilder.Append(@"],
                    datasets: [{
                        label: 'Humidity',
                        data: [");

            foreach (var entry in data)
            {
                htmlBuilder.Append($"{Math.Round(entry.Humidity, 1)},");
            }

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
                                await GenerateHTMLReportLastMonth(currentTime, roomConfig.RoomName);
                            }

                            if (ShouldGenerateLastWeekReport(currentTime))
                            {
                                await GenerateHTMLReportLastWeek(currentTime, roomConfig.RoomName);
                            }

                            if (ShouldGenerateCurrentDayReport(currentTime))
                            {
                                await GenerateHTMLReportCurrentDay(currentTime, roomConfig.RoomName);
                            }
                        }
                    }
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
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
            return currentTime.Hour == 23 && currentTime.Minute == 30;
        }
    }
}
