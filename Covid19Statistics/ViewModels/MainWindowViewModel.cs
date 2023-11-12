using Covid19Statistics.Models;
using Covid19Statistics.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

        public MainWindowViewModel() {
            Places = GetData();
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
