using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectMew
{
    public class GeoLocation
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Altitude { get; set; }

        public GeoLocation() { }
        public GeoLocation(double lng, double lat, double alt)
        {
            Longitude = lng;
            Latitude = lat;
            Altitude = alt;
        }
    }
}
