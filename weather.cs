// weather.cs - Погодное приложение на C# Windows Forms
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace WeatherApp
{
    public partial class MainForm : Form
    {
        private TextBox cityBox;
        private ComboBox unitsBox;
        private Label tempLabel, feelsLabel, humidityLabel, windLabel, pressureLabel, descLabel;
        private ListBox forecastList;
        private string apiKey;

        public MainForm()
        {
            apiKey = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = Microsoft.VisualBasic.Interaction.InputBox("Введите OpenWeatherMap API ключ:", "API Key");
            }
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Погодное приложение C#";
            this.Size = new Size(600, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            var topPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Padding = new Padding(5) };
            topPanel.Controls.Add(new Label { Text = "Город:", AutoSize = true });
            cityBox = new TextBox { Width = 200 };
            topPanel.Controls.Add(cityBox);
            var searchBtn = new Button { Text = "Поиск" };
            searchBtn.Click += async (s, e) => await FetchWeather();
            topPanel.Controls.Add(searchBtn);
            unitsBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Items = { "metric (°C)", "imperial (°F)" }, SelectedIndex = 0 };
            topPanel.Controls.Add(unitsBox);
            this.Controls.Add(topPanel);
            
            var infoPanel = new TableLayoutPanel { RowCount = 6, ColumnCount = 2, Dock = DockStyle.Top, Height = 200, Padding = new Padding(10) };
            infoPanel.Controls.Add(new Label { Text = "Температура:" }, 0,0);
            tempLabel = new Label(); infoPanel.Controls.Add(tempLabel, 1,0);
            infoPanel.Controls.Add(new Label { Text = "Ощущается:" }, 0,1);
            feelsLabel = new Label(); infoPanel.Controls.Add(feelsLabel, 1,1);
            infoPanel.Controls.Add(new Label { Text = "Влажность:" }, 0,2);
            humidityLabel = new Label(); infoPanel.Controls.Add(humidityLabel, 1,2);
            infoPanel.Controls.Add(new Label { Text = "Ветер:" }, 0,3);
            windLabel = new Label(); infoPanel.Controls.Add(windLabel, 1,3);
            infoPanel.Controls.Add(new Label { Text = "Давление:" }, 0,4);
            pressureLabel = new Label(); infoPanel.Controls.Add(pressureLabel, 1,4);
            infoPanel.Controls.Add(new Label { Text = "Описание:" }, 0,5);
            descLabel = new Label(); infoPanel.Controls.Add(descLabel, 1,5);
            this.Controls.Add(infoPanel);
            
            forecastList = new ListBox { Dock = DockStyle.Fill };
            this.Controls.Add(forecastList);
        }

        private async Task FetchWeather()
        {
            string city = cityBox.Text.Trim();
            if (string.IsNullOrEmpty(city)) return;
            string units = unitsBox.SelectedIndex == 0 ? "metric" : "imperial";
            using var client = new HttpClient();
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units={units}&lang=ru";
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Город не найден или ошибка API");
                return;
            }
            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            DisplayCurrent(json, units);
            
            string forecastUrl = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&appid={apiKey}&units={units}&lang=ru";
            var forecastResp = await client.GetAsync(forecastUrl);
            if (forecastResp.IsSuccessStatusCode)
            {
                var forecastJson = JObject.Parse(await forecastResp.Content.ReadAsStringAsync());
                DisplayForecast(forecastJson, units);
            }
        }

        private void DisplayCurrent(JObject json, string units)
        {
            var main = json["main"];
            double temp = (double)main["temp"];
            double feels = (double)main["feels_like"];
            int humidity = (int)main["humidity"];
            int pressure = (int)main["pressure"];
            double windSpeed = (double)json["wind"]["speed"];
            string desc = (string)json["weather"][0]["description"];
            string tempUnit = units == "metric" ? "°C" : "°F";
            string windUnit = units == "metric" ? "м/с" : "mph";
            tempLabel.Text = $"{temp:F1} {tempUnit}";
            feelsLabel.Text = $"{feels:F1} {tempUnit}";
            humidityLabel.Text = $"{humidity}%";
            windLabel.Text = $"{windSpeed:F1} {windUnit}";
            pressureLabel.Text = $"{pressure} гПа";
            descLabel.Text = desc;
        }

        private void DisplayForecast(JObject json, string units)
        {
            var list = json["list"];
            var days = new Dictionary<string, List<double>>();
            foreach (var item in list)
            {
                long dt = (long)item["dt"];
                var date = DateTimeOffset.FromUnixTimeSeconds(dt).DateTime;
                string day = date.ToString("yyyy-MM-dd");
                double temp = (double)item["main"]["temp"];
                if (!days.ContainsKey(day)) days[day] = new List<double>();
                days[day].Add(temp);
            }
            forecastList.Items.Clear();
            string tempUnit = units == "metric" ? "°C" : "°F";
            int count = 0;
            foreach (var day in days.Keys)
            {
                if (count++ >= 5) break;
                var temps = days[day];
                double min = temps.Min();
                double max = temps.Max();
                forecastList.Items.Add($"{day}: {min:F1} .. {max:F1} {tempUnit}");
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }
}
