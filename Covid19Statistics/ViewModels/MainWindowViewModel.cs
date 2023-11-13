using Covid19Statistics.Infrastructure.Commands;
using Covid19Statistics.Models;
using Covid19Statistics.ViewModels.Base;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace Covid19Statistics.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        private const string _dataUrl = @"https://raw.githubusercontent.com/CSSEGISandData/COVID-19/master/csse_covid_19_data/csse_covid_19_time_series/time_series_covid19_confirmed_global.csv";

        private List<Place> _places;

        public List<Place> Places
        {
            get => _places;
            set => Set(ref _places, value);
        }

        private Dictionary<string, string> _wrongPlacesReplace = new Dictionary<string, string>() {
            {"Korea,", "Korea -"},
            {"Bonaire,", "Bonaire -"},
            {"Saint Helena,", "Saint Helena -"}
        };

        private PlotModel _plotModel;
        public PlotModel PlotModel
        {
            get => _plotModel;
            set => Set(ref _plotModel, value);
        }

        public ICommand DisplayGraphCommand { get; }

        private bool CanDisplayGraphCommandExecuted(object p) => SelectedPlace != null && SelectedDateSince <= SelectedDateUntil;

        private void OnDisplayGraphCommandExecute(object p)
        {
            PlotModel = new PlotModel { Title = "Заражения"};
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left});
            PlotModel.Axes.Add(new DateTimeAxis { Position = AxisPosition.Bottom, StringFormat = "dd.MM.yy"});
            var line1 = new OxyPlot.Series.LineSeries() {
                Title = "Заражение по стране",
                Color = OxyColors.Blue,
                StrokeThickness = 1,
            };
            var line2 = new OxyPlot.Series.LineSeries()
            {
                Title = "Максимальное заражение",
                Color = OxyColors.Red,
                StrokeThickness = 1
            };

            var dates = GetDates();

            for (int i = 0; i < dates.Count(); i++)
            {
                if (dates[i] < SelectedDateSince) continue;
                if (dates[i] > SelectedDateUntil) continue;
                line1.Points.Add(new DataPoint(DateTimeAxis.ToDouble(dates[i]), (double)SelectedPlace?.Counts[i]));
                line2.Points.Add(new DataPoint(DateTimeAxis.ToDouble(dates[i]), (double)MaxInfectionOnDay[i]));
            }

            PlotModel.Series.Add(line1);
            PlotModel.Series.Add(line2);

            PlotModel.Legends.Add(new Legend()
            {
                LegendTitle = "Legend",
                LegendPosition = LegendPosition.TopLeft
            });

            PlotModel.InvalidatePlot(true);
        }

        private Place _selectedPlace;

        public Place SelectedPlace
        {
            get => _selectedPlace;
            set => Set(ref _selectedPlace, value);
        }

        private DateTime _selectedDateSince = new DateTime(2020, 1, 22);

        public DateTime SelectedDateSince
        {
            get => _selectedDateSince;
            set => Set(ref _selectedDateSince, value);
        }

        private DateTime _selectedDateUntil = new DateTime(2023, 9, 1);

        public DateTime SelectedDateUntil
        {
            get => _selectedDateUntil;
            set => Set(ref _selectedDateUntil, value);
        }

        private int[] _maxInfectionOnDay;

        public int[] MaxInfectionOnDay
        {
            get => _maxInfectionOnDay;
            set => Set(ref _maxInfectionOnDay, value);
        }

        public MainWindowViewModel() {
            Places = GetData();

            DisplayGraphCommand = new LambdaCommand(OnDisplayGraphCommandExecute, CanDisplayGraphCommandExecuted);

            int[] maxInfectionOnDay = new int[GetDates().Count()];
            for (int i = 0; i < maxInfectionOnDay.Count(); i++)
            {
                for (int j = 0; j < Places.Count; j++) 
                {
                    maxInfectionOnDay[i] = Math.Max(maxInfectionOnDay[i], Places[j].Counts[i]);
                }
            }
            MaxInfectionOnDay = maxInfectionOnDay;
        }

        private IEnumerable<string> GetDataLines()
        {
            var data_stream = File.OpenRead("../../Data/time_series_covid19_confirmed_global.csv");
            var data_reader = new StreamReader(data_stream);

            while (!data_reader.EndOfStream)
            {
                var line = data_reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                foreach (var wpr in _wrongPlacesReplace)
                {
                    line = line.Replace(wpr.Key, wpr.Value);
                }
                yield return line;
            }
        }

        private DateTime[] GetDates() => GetDataLines()
            .First()
            .Split(',')
            .Skip(4)
            .Select(s => DateTime.Parse(s, CultureInfo.InvariantCulture))
            .ToArray();

        private List<Place> GetData()
        {
            var lines = GetDataLines().Skip(1).Select(s => s.Split(','));
            List<Place> places = new List<Place>();

            foreach (var line in lines)
            {
                var province = line[0].Trim(' ', '"');
                var country = line[1].Trim();
                var latitude = double.Parse(line[2] == "" ? "0" : line[2], CultureInfo.CreateSpecificCulture("en-GB"));
                var longitude = double.Parse(line[3] == "" ? "0" : line[3], CultureInfo.CreateSpecificCulture("en-GB"));
                var counts = line.Skip(4).Select(int.Parse).ToArray();

                Place place = new Place(province, country, latitude, longitude, counts);
                places.Add(place);

            }
            return places;
        }
    }
}
