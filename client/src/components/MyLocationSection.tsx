import React from 'react';
import { useMyLocation } from '../hooks/useMyLocation';

const MyLocationSection: React.FC = () => {
  const { location, loading, error } = useMyLocation();

  return (
    <section id="management" style={{ marginBottom: '2rem' }}>
      <h2>My Location</h2>
      {loading && <p>Loading location...</p>}
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
              <tr >
                <td style={{ borderLeft: '1px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left'}}>{location.latitude ?? 'N/A'}</td>
                <td style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left'}}>{location.longitude ?? 'N/A'}</td>
                <td style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left'}}>{location.altitude ?? 'N/A'}</td>
              </tr>
          </tbody>
        </table>
      )}
    </section>
  );
};

export default MyLocationSection;
