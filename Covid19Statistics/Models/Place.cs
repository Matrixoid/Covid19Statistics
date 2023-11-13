namespace Covid19Statistics.Models
{
    internal class Place
    {
        public string Province { get; set; }
        public string Country { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public int[] Counts { get; set; }

        public Place(string province, string country, double latitude, double longitude, int[] counts)
        {
            Province = province;
            Country = country;
            Latitude = latitude;
            Longitude = longitude;
            Counts = counts;
        }

        public override string ToString()
        {
            string res = "";
            if (Province != "")
                res = $"{Province}, {Country}";
            else
                res = $"{Country}";
            return res;
        }
    }
}
