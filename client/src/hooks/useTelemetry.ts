import { useEffect, useState } from 'react';

export interface Telemetry {
  id: string;
  satelliteId: string;
  stationId: string;
  receivedAtUtc: string;
  rssiDbm: number;
  toadms: number;
  bearingDeg: number;
  snrDb: number;
}

export function useTelemetry() {
  const [telemetryData, setTelemetryData] = useState<Telemetry[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchTelemetryData() {
      setLoading(true);
      setError(null);
      try {
        const res = await fetch('http://localhost:5143/v1/telemetry', {
          headers: { 'x-api-key': 'dev-key-123' },
        });
        if (!res.ok) throw new Error('Failed to fetch telemetry data');
        const data = await res.json();
        setTelemetryData(data);
      } catch (err: any) {
        setError(err.message || 'Unknown error');
      } finally {
        setLoading(false);
      }
    }
    fetchTelemetryData();
  }, []);

  return { telemetryData, loading, error };
}
