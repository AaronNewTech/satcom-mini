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
        const apiBaseUrl =
          import.meta.env.VITE_API_BASE_URL || 'http://localhost:5143';
        const apiKey = import.meta.env.VITE_API_KEY;
        if (!apiKey)
          throw new Error(
            'VITE_API_KEY is not set. Create client/.env.local with VITE_API_KEY',
          );
        const res = await fetch(`${apiBaseUrl}/v1/satellites`, {
          headers: { 'x-api-key': apiKey },
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
