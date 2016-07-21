using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PokemonGo.RocketAPI.Logic.Navigation;

namespace PokemonGo.RocketAPI.Logic.Utils
{
    public static class LocationUtils
    {
        public static Location CreateWaypoint(Location sourceLocation, double distanceInMeters, double bearingDegrees) //from http://stackoverflow.com/a/17545955
        {
            double distanceKm = distanceInMeters / 1000.0;
            double distanceRadians = distanceKm / 6371; //6371 = Earth's radius in km

            double bearingRadians = DegreeToRadian(bearingDegrees);
            double sourceLatitudeRadians = DegreeToRadian(sourceLocation.Latitude);
            double sourceLongitudeRadians = DegreeToRadian(sourceLocation.Longitude);

            double targetLatitudeRadians = Math.Asin(Math.Sin(sourceLatitudeRadians) * Math.Cos(distanceRadians)
                    + Math.Cos(sourceLatitudeRadians) * Math.Sin(distanceRadians) * Math.Cos(bearingRadians));

            double targetLongitudeRadians = sourceLongitudeRadians + Math.Atan2(Math.Sin(bearingRadians)
                    * Math.Sin(distanceRadians) * Math.Cos(sourceLatitudeRadians), Math.Cos(distanceRadians)
                    - Math.Sin(sourceLatitudeRadians) * Math.Sin(targetLatitudeRadians));

            // adjust toLonRadians to be in the range -180 to +180...
            targetLongitudeRadians = ((targetLongitudeRadians + 3 * Math.PI) % (2 * Math.PI)) - Math.PI;

            return new Location(RadianToDegree(targetLatitudeRadians), RadianToDegree(targetLongitudeRadians));
        }

        public static double CalculateBearing(Location sourceLocation, Location targetLocation) // from http://www.movable-type.co.uk/scripts/latlong.html
        {

            double dLon = (targetLocation.Longitude - sourceLocation.Longitude);

            double y = Math.Sin(dLon) * Math.Cos(targetLocation.Latitude);
            double x = Math.Cos(sourceLocation.Latitude) * Math.Sin(targetLocation.Latitude) - Math.Sin(sourceLocation.Latitude)
                    * Math.Cos(targetLocation.Latitude) * Math.Cos(dLon);

            double brng = Math.Atan2(y, x);

            brng = RadianToDegree(brng);
            brng = (brng + 360) % 360;

            if (sourceLocation.Latitude < 0)
            {
                brng = 360 - brng;
            }

            return brng;
        }

        public static double CalculateDistanceInMeters(Location sourceLocation, Location targetLocation) // from http://stackoverflow.com/questions/6366408/calculating-distance-between-two-latitude-and-longitude-geocoordinates
        {
            var baseRad = Math.PI * sourceLocation.Latitude / 180;
            var targetRad = Math.PI * targetLocation.Latitude / 180;
            var theta = sourceLocation.Longitude - targetLocation.Longitude;
            var thetaRad = Math.PI * theta / 180;

            double dist =
                Math.Sin(baseRad) * Math.Sin(targetRad) + Math.Cos(baseRad) *
                Math.Cos(targetRad) * Math.Cos(thetaRad);
            dist = Math.Acos(dist);

            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515 * 1.609344 * 1000;

            return dist;
        }

        public static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        public static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}
