import { useEffect, useState } from 'react';

type Satellite = {
  satid: string;
  satlat: number;
  satlng: number;
  satname: string;
};

type UseSatellitesInViewResult = {
  satellites: Satellite[];
  loading: boolean;
  error: string | null;
};

export default function useSatellitesInView(
  location: { latitude: number; longitude: number; altitude: number } | null,
): UseSatellitesInViewResult {
    
  const [satellites, setSatellites] = useState<Satellite[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  useEffect(() => {
    let mounted = true;

    async function fetchSatellites() {
      if (!location) {
        setSatellites([]);
        setLoading(false);
        setError(null);
        return;
      }

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

        const res = await fetch(
          `${apiBaseUrl}/v1/satellite/above/${location.latitude}/${location.longitude}/${location.altitude}/${70}/${18}`,
          { headers: { 'x-api-key': apiKey } },
        );
        if (!res.ok) throw new Error('Failed to fetch satellites list');
  const data = await res.json();
        // Normalize response to an array. Some APIs return { satellites: [...] } or an array directly.
        // const list = Array.isArray(data)
        //   ? data
        //   : Array.isArray(data?.satellites)
        //     ? data.satellites
        //     : [];

        // console.log('Normalized satellites list:', list);
        // if (!Array.isArray(list)) {
        //   throw new Error('Unexpected satellites response format');
        // }
  const satelliteList = Array.isArray(data?.above) ? data.above : [];
        // For each satellite, fetch its estimated location
        // const satellitesWithLocation: Satellite[] = await Promise.all(
        //   satelliteList.map(async (sat: any) => {
        //     let lat: number | undefined;
        //     let lon: number | undefined;
        //     try {
        //       const locRes = await fetch(
        //         `${apiBaseUrl}/v1/satellites/${sat.id}/location`,
        //         { headers: { 'x-api-key': apiKey } },
        //       );
        //       if (locRes.ok) {
        //         const loc = await locRes.json();
        //         if (
        //           loc &&
        //           typeof loc.lat === 'number' &&
        //           typeof loc.lon === 'number'
        //         ) {
        //           lat = loc.lat;
        //           lon = loc.lon;
        //         }
        //       }
        //     } catch (e) {
        //       // ignore individual location fetch errors
        //     }
        //     return {
        //       id: sat.id,
        //       callsign: sat.callsign,
        //       lat,
        //       lon,
        //     } as Satellite;
        //   }),
        // );

        if (!mounted) return;
        setSatellites(satelliteList);
        setLoading(false);
      } catch (err: any) {
        if (!mounted) return;
        setError(err instanceof Error ? err.message : 'Unknown error');
        setLoading(false);
      }
    }

    fetchSatellites();

    return () => {
      mounted = false;
    };
  }, [location]);

  return { satellites, loading, error };
}
