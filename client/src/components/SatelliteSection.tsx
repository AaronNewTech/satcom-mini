import React from 'react';
import { useSatellites } from '../hooks/useSatellites';

const SatelliteSection: React.FC = () => {
  const { satellites, loading, error } = useSatellites();

  return (
    <section id="management" style={{ marginBottom: '2rem' }}>
      <h2>Satellites</h2>
      <p>Manage and view details for all satellites.</p>
      {loading && <p>Loading satellites...</p>}
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
                Callsign
              </th>
              <th style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left' }}>
                ID
              </th>
              <th style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left' }}>
                NORAD ID
              </th>
            </tr>
          </thead>
          <tbody>
            {satellites.map((sat) => (
              <tr key={sat.id}>
                <td style={{borderLeft: '1px solid #ffffffff'}}>{sat.callsign}</td>
                <td style={{borderLeft: '30px solid #ffffffff'}}>{sat.id}</td>
                <td style={{borderLeft: '30px solid #ffffffff'}}>{sat.noradId}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
};

export default SatelliteSection;
