import React from 'react';
import { useTelemetry } from '../hooks/useTelemetry';

const TelemetrySection: React.FC = () => {
  const { telemetryData, loading, error } = useTelemetry();

  return (
    <section id="management" style={{ marginBottom: '2rem' }}>
      <h2>Telemetry Data</h2>
      <p>Manage and view details for all telemetry data.</p>
      {loading && <p>Loading telemetry data...</p>}
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
                Received At (UTC)
              </th>
              <th style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left' }}>
                ID
              </th>
              <th style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left' }}>
                Satellite Id
              </th>
              <th style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left' }}>
                Ground Station Id
              </th>
              <th style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left' }}>
                Rssi (dBm)
              </th>
              <th style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left' }}>
                Toad (ms)
              </th>
              <th style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left' }}>
                Bearing Deg
              </th>
              <th style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left' }}>
                SNR (dB)
              </th>
            </tr>
          </thead>
          <tbody>
            {telemetryData.map((telemetry) => (
              <tr key={telemetry.receivedAtUtc}>
                <td style={{ borderLeft: '1px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left'}}>{telemetry.receivedAtUtc}</td>
                <td style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left'}}>{telemetry.id}</td>
                <td style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left'}}>{telemetry.satelliteId}</td>
                <td style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left'}}>{telemetry.stationId}</td>
                <td style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left'}}>{telemetry.rssiDbm}</td>
                <td style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left'}}>{telemetry.toadms}</td>
                <td style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left'}}>{telemetry.bearingDeg}</td>
                <td style={{ borderLeft: '30px solid #ffffffff',borderBottom: '1px solid #ccc', textAlign: 'left'}}>{telemetry.snrDb}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
};

export default TelemetrySection;
