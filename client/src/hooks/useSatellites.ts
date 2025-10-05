import { useEffect, useState } from 'react';

export interface Satellite {
  id: string;
  callsign: string;
  noradId?: string;
}

export function useSatellites() {
  const [satellites, setSatellites] = useState<Satellite[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchSatellites() {
      setLoading(true);
      setError(null);
      try {
        const res = await fetch('http://localhost:5143/v1/satellites', {
          headers: { 'x-api-key': 'dev-key-123' },
        });
        if (!res.ok) throw new Error('Failed to fetch satellites');
        const data = await res.json();
        setSatellites(data);
      } catch (err: any) {
        setError(err.message || 'Unknown error');
      } finally {
        setLoading(false);
      }
    }
    fetchSatellites();
  }, []);

  return { satellites, loading, error };
}
