import { useEffect, useState } from 'react';

export interface GroundStation {
  id: string;
  name: string;
  country: string;
  lat: number;
  lon: number;
  elevationM: number;
}

export function useGroundStations() {
  const [groundStations, setGroundStations] = useState<GroundStation[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchGroundStations() {
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
        const res = await fetch(`${apiBaseUrl}/v1/groundstations`, {
          headers: { 'x-api-key': apiKey },
        });
        if (!res.ok) throw new Error('Failed to fetch groundstations');
        const data = await res.json();
        setGroundStations(data);
      } catch (err: any) {
        setError(err.message || 'Unknown error');
      } finally {
        setLoading(false);
      }
    }
    fetchGroundStations();
  }, []);

  return { groundStations, loading, error };
}
