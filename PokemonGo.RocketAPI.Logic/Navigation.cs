using System;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo.RocketAPI.Logic.Utils;

namespace PokemonGo.RocketAPI.Logic
{
    public class Navigation
    {

        private readonly Client _client;

        public Navigation(Client client)
        {
            _client = client;
        }

        public async Task<PlayerUpdateResponse> HumanLikeWalking(Location targetLocation, double walkingSpeedInKilometersPerHour)
        {
            double speedInMetersPerSecond = walkingSpeedInKilometersPerHour / 3.6;

            Location sourceLocation = new Location(_client.CurrentLat, _client.CurrentLng);

            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Distance to target location: {LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation)} meters.");

            double nextWaypointBearing = LocationUtils.CalculateBearing(sourceLocation, targetLocation);
            double nextWaypointDistance = speedInMetersPerSecond;
            Location waypoint = LocationUtils.CreateWaypoint(sourceLocation, nextWaypointDistance, nextWaypointBearing);

            //Initial walking
            DateTime requestSendDateTime = DateTime.Now;
            var result = await _client.UpdatePlayerLocation(waypoint.Latitude, waypoint.Longitude);

            do
            {
                await Task.Delay(3000);
                double millisecondsUntilGetUpdatePlayerLocationResponse = (DateTime.Now - requestSendDateTime).TotalMilliseconds;

                nextWaypointDistance = millisecondsUntilGetUpdatePlayerLocationResponse / 1000 * speedInMetersPerSecond;
                sourceLocation = new Location(_client.CurrentLat, _client.CurrentLng);
                nextWaypointBearing = LocationUtils.CalculateBearing(sourceLocation, targetLocation);
                waypoint = LocationUtils.CreateWaypoint(sourceLocation, nextWaypointDistance, nextWaypointBearing);

                requestSendDateTime = DateTime.Now;
                result = await _client.UpdatePlayerLocation(waypoint.Latitude, waypoint.Longitude);

                Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Distance to target location: {LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation)} meters.");

            } while (LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation) >= 10);

            return result;
        }

        public class Location
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }

            public Location(double latitude, double longitude)
            {
                Latitude = latitude;
                Longitude = longitude;
            }
        }
    }
}
