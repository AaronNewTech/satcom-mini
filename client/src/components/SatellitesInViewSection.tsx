import React, { useEffect} from 'react';
import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import L from 'leaflet';
import redIconUrl from '../assets/marker-icon-2x-red.png';
import blueIconUrl from '../assets/marker-icon-2x-blue.png';
import shadowUrl from 'leaflet/dist/images/marker-shadow.png';
import { useMyLocation } from '../hooks/useMyLocation';
import useSatellitesInView from '../hooks/useSatellitesInView';

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

export const SatellitesInViewSection: React.FC = () => {
  const { location } = useMyLocation();
  const { satellites = [] } = useSatellitesInView(location);
  
  useEffect(() => {
  console.log('Satellites in view on section:', satellites);
}, [satellites]);

  return (
    <div style={{ padding: '1rem' }}>
      <div style={{ marginBottom: '1rem' }}>
        <h2>Satellites Currently Overhead</h2>
        <p>
          The map below shows satellites that are currently above your location.
          Red markers indicate satellites, while the blue marker shows your
          location (if available). Click on any marker for more details.
        </p>
      </div>
      <div style={{ height: '500px', width: '80%' }}>
        <MapContainer
          center={
            location && location.latitude != null && location.longitude != null
              ? [location.latitude, location.longitude]
              : [28.5383, -81.3792]
          }
          zoom={3}
          style={{ height: '100%', width: '100%' }}
        >
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          {location && (
            <Marker
              position={
                [location.latitude!, location.longitude!] as [number, number]
              }
              icon={redIcon}
            >
              <Popup>
                <div>
                  <strong>Your Location</strong>
                  <br />
                  Lat: {location.latitude}, Lon: {location.longitude}
                </div>
              </Popup>
            </Marker>
          )}
            {satellites.map((sat) => (
              <Marker
                key={sat.satid}
                position={[sat?.satlat, sat?.satlng] as [number, number]}
                icon={blueIcon}
              >
                <Popup>
                  <div>
                    <strong>{sat.satname}</strong>
                    <br />
                    Lat: {sat.satlat}, Lon: {sat.satlng}
                  </div>
                </Popup>
              </Marker>
            ))}
            
        </MapContainer>
      </div>
    </div>
  );
};

// try this instead of the above .map() to avoid the error about missing lat/lng values. The .filter() will ensure that only satellites with valid coordinates are passed to the .map() function, which should eliminate the TypeScript error about possibly undefined values.
// {satellites
//   // 1. Filter out any satellite that is missing latitude or longitude
//   .filter((sat) => sat.satlat !== undefined && sat.satlng !== undefined)
//   // 2. Now .map() only sees satellites that DEFINITELY have coordinates
//   .map((sat) => (
//     <Marker
//       key={sat.satid}
//       /* No ! or ? needed here because the filter already guaranteed these exist */
//       position={[sat.satlat, sat.satlng]} 
//       icon={blueIcon}
//     >
//       <Popup>
//         <strong>{sat.satname}</strong>
//         <br />
//         Lat: {sat.satlat}, Lon: {sat.satlng}
//       </Popup>
//     </Marker>
//   ))}
