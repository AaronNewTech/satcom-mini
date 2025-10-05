using Satcom.Api.Domain;
using System;
using System.Collections.Generic;

namespace Satcom.Api
{
    public static class Seed
    {
        public static void SeedAll(AppDbContext db)
        {
            // Ground stations
            var stations = new List<GroundStation>
            {
                new GroundStation { Name = "Goldstone Deep Space Communications Complex", Country = "USA (California)", Lat = 35.4259, Lon = -116.8889, ElevationM = 1002 },
                new GroundStation { Name = "Madrid Deep Space Communications Complex (Robledo)", Country = "Spain", Lat = 40.4272, Lon = -4.2492, ElevationM = 761 },
                new GroundStation { Name = "Canberra Deep Space Communications Complex (Tidbinbilla)", Country = "Australia", Lat = -35.3980, Lon = 148.9810, ElevationM = 550 },
                new GroundStation { Name = "Svalbard Satellite Station (SvalSat)", Country = "Norway (Svalbard)", Lat = 78.2290, Lon = 15.4078, ElevationM = 450 },
                new GroundStation { Name = "Luigi Broglio Space Centre (Malindi Ground Station)", Country = "Kenya", Lat = -2.9970, Lon = 40.1930, ElevationM = 10 },
                new GroundStation { Name = "Guam Remote Ground Terminal (GRGT)", Country = "Guam (US Territory)", Lat = 13.5878, Lon = 144.8411, ElevationM = 128 },
                new GroundStation { Name = "Awarua Satellite Ground Station", Country = "New Zealand", Lat = -46.5293, Lon = 168.3811, ElevationM = 8 },
                new GroundStation { Name = "McMurdo Ground Station", Country = "Antarctica", Lat = -77.8460, Lon = 166.6760, ElevationM = 10 },
                new GroundStation { Name = "Kourou Ground Station (ESA/ESTRACK)", Country = "French Guiana", Lat = 5.2514, Lon = -52.8047, ElevationM = 10 },
                new GroundStation { Name = "Wallops Ground Station", Country = "USA (Virginia)", Lat = 37.9400, Lon = -75.4600, ElevationM = 13 }
            };

            db.GroundStations.AddRange(stations);
            db.SaveChanges();

            // Satellites and Telemetry
            var satellites = new List<Satellite>();
            var telemetries = new List<Telemetry>();
            int satNum = 1;
            int numStations = stations.Count;
            for (int i = 0; i < numStations; i++)
            {
                var station = stations[i];
                for (int j = 0; j < 3; j++)
                {
                    var sat = new Satellite
                    {
                        Callsign = $"SAT-{station.Name.Substring(0, Math.Min(3, station.Name.Length)).ToUpper()}-{j+1}",
                        NoradId = $"{10000 + satNum}"
                    };
                    satellites.Add(sat);
                    // Add telemetry from this station
                    telemetries.Add(new Telemetry
                    {
                        Satellite = sat,
                        Station = station,
                        ReceivedAtUtc = DateTime.UtcNow.AddMinutes(-j*10),
                        RssiDbm = -100 + j*2,
                        ToaMs = 100 + j*5,
                        BearingDeg = 180 + j*10,
                        SnrDb = 20 - j
                    });
                    // Add telemetry from a different station (wrap around)
                    var otherStation = stations[(i+1)%numStations];
                    telemetries.Add(new Telemetry
                    {
                        Satellite = sat,
                        Station = otherStation,
                        ReceivedAtUtc = DateTime.UtcNow.AddMinutes(-j*10-5),
                        RssiDbm = -105 + j*2,
                        ToaMs = 110 + j*5,
                        BearingDeg = 200 + j*10,
                        SnrDb = 18 - j
                    });
                    satNum++;
                }
            }
            db.Satellites.AddRange(satellites);
            db.Telemetries.AddRange(telemetries);
            db.SaveChanges();
        }
    }
}
