import React, { useState } from 'react';
import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import L from 'leaflet';
import redIconUrl from '../assets/marker-icon-2x-red.png';
import blueIconUrl from '../assets/marker-icon-2x-blue.png';
import shadowUrl from 'leaflet/dist/images/marker-shadow.png';
import { useGroundStations } from '../hooks/useGroundStations';

const redIcon = new L.Icon({
  iconUrl: redIconUrl,
  shadowUrl,
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41],
});

const blueIcon = new L.Icon({
  iconUrl: blueIconUrl,
  shadowUrl,
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41],
});

const MapView: React.FC = () => {
  const { groundStations, loading, error } = useGroundStations();
  const [selectedStation, setSelectedStation] = useState<string>('');
  type Satellite = { id: string; callsign: string; lat: number; lon: number };
  const [satellites, setSatellites] = useState<Satellite[]>([]);

  const fetchSatellitesForStation = async (stationId: string) => {
    try {
      const apiBaseUrl =
        import.meta.env.VITE_API_BASE_URL || 'http://localhost:5143';
      const apiKey = import.meta.env.VITE_API_KEY;
      if (!apiKey)
        throw new Error(
          'VITE_API_KEY is not set. Create client/.env.local with VITE_API_KEY',
        );
      const res = await fetch(
        `${apiBaseUrl}/v1/telemetry/satellites?stationId=${stationId}`,
        { headers: { 'x-api-key': apiKey } },
      );
      const data = await res.json();
      // For each satellite, fetch its estimated location
      const satellitesWithLocation = await Promise.all(
        data.map(async (sat: any) => {
          let lat, lon;
          try {
            const locRes = await fetch(
              `${apiBaseUrl}/v1/satellites/${sat.id}/location`,
              {
                headers: { 'x-api-key': apiKey },
              },
            );

            if (locRes.ok) {
              const loc = await locRes.json();
              if (loc && loc.lat && loc.lon) {
                lat = loc.lat;
                lon = loc.lon;
              }
            }
          } catch {}
          return {
            id: sat.id,
            callsign: sat.callsign,
            lat,
            lon,
          };
        }),
      );
      setSatellites(satellitesWithLocation);
    } catch (err) {
      console.error('Error fetching satellites:', err);
    }
  };

  React.useEffect(() => {
    if (groundStations.length > 0 && !selectedStation) {
      setSelectedStation(groundStations[0].id);
    }
  }, [groundStations, selectedStation]);

  // Debug: log selected station
  const selected = groundStations.find((gs) => gs.id === selectedStation);
  React.useEffect(() => {
    if (selected) {
      fetchSatellitesForStation(selected.id);
    }
  }, [selected]);

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error loading ground stations.</div>;

  return (
    <div>
      <div style={{ marginBottom: '1rem' }}>
        <label htmlFor="station-select">Select Ground Station: </label>
        <select
          id="station-select"
          value={selectedStation}
          onChange={(e) => setSelectedStation(e.target.value)}
        >
          {groundStations.map((gs) => (
            <option key={gs.id} value={gs.id}>
              {gs.name}
            </option>
          ))}
        </select>
      </div>
      <div style={{ height: '500px', width: '100%' }}>
        <MapContainer
          center={
            groundStations.find((gs) => gs.id === selectedStation)
              ? ([
                  groundStations.find((gs) => gs.id === selectedStation)!.lat,
                  groundStations.find((gs) => gs.id === selectedStation)!.lon,
                ] as [number, number])
              : [28.5383, -81.3792]
          }
          zoom={3}
          style={{ height: '100%', width: '100%' }}
        >
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          {groundStations
            .filter((gs) => gs.id === selectedStation)
            .map((gs) => (
              <Marker
                key={gs.id}
                position={[gs.lat, gs.lon] as [number, number]}
                icon={blueIcon}
              >
                <Popup>
                  <div>
                    <strong>{gs.name}</strong>
                    <br />
                    Lat: {gs.lat}, Lon: {gs.lon}
                  </div>
                </Popup>
              </Marker>
            ))}
          {/* Offset overlapping satellite markers */}
          {(() => {
            // Group satellites by lat/lon string
            const grouped: { [key: string]: typeof satellites } = {};
            satellites.forEach((sat) => {
              if (sat.lat && sat.lon) {
                const key = `${sat.lat.toFixed(6)},${sat.lon.toFixed(6)}`;
                if (!grouped[key]) grouped[key] = [];
                grouped[key].push(sat);
              }
            });
            // For each group, offset markers if more than one at same spot
            const markerElements: JSX.Element[] = [];
            Object.values(grouped).forEach((group) => {
              group.forEach((sat, idx) => {
                let lat = sat.lat;
                let lon = sat.lon;
                if (group.length > 1) {
                  // Offset by up to ±0.02 deg (about 2km) in a circle
                  const angle = (2 * Math.PI * idx) / group.length;
                  lat += 0.02 * Math.cos(angle);
                  lon += 0.02 * Math.sin(angle);
                }
                markerElements.push(
                  <Marker
                    key={sat.id}
                    position={[lat, lon] as [number, number]}
                    icon={redIcon}
                  >
                    <Popup>
                      <div>
                        <strong>{sat.callsign}</strong>
                        <br />
                        Lat: {sat.lat ? sat.lat.toFixed(2) : 'N/A'}, Lon:{' '}
                        {sat.lon ? sat.lon.toFixed(2) : 'N/A'}
                      </div>
                    </Popup>
                  </Marker>,
                );
              });
            });
            return markerElements;
          })()}
        </MapContainer>
      </div>
    </div>
  );
};

export default MapView;
