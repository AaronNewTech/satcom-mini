import React from 'react';
import { useGroundStations } from '../hooks/useGroundStations';

const GroundStationsSection: React.FC = () => {
  const { groundStations, loading, error } = useGroundStations();

  return (
    <section id="management" style={{ marginBottom: '2rem' }}>
      <h2>Ground Stations</h2>
      <p>Manage and view details for all ground stations.</p>
      {loading && <p>Loading ground stations...</p>}
      {error && <p style={{ color: 'red' }}>{error}</p>}
      {!loading && !error && (
        <table
          style={{
            width: '100%',
            borderCollapse: 'collapse',
            marginTop: '1rem',
          }}
        >
          <thead>
            <tr>
              <th style={{ borderLeft: '1px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left' }}>
                Name
              </th>
              <th style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left' }}>
                Country
              </th>
              <th style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left' }}>
                ID
              </th>
              <th style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left' }}>
                Latitude
              </th>
              <th style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left' }}>
                Longitude
              </th>
              <th style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left' }}>
                Elevation (m)
              </th>
            </tr>
          </thead>
          <tbody>
            {groundStations.map((station) => (
              <tr key={station.id}>
                <td style={{ borderLeft: '1px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left'}}>{station.name}</td>
                <td style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left'}}>{station.country}</td>
                <td style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left'}}>{station.id}</td>
                <td style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left'}}>{station.lat}</td>
                <td style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left'}}>{station.lon}</td>
                <td style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left'}}>{station.elevationM}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
};

export default GroundStationsSection;
