import { useState, useEffect } from 'react';

export interface Location {
  latitude: number;
  longitude: number;
  altitude: number;
}

export interface Error {
  error: string | null;
}

export function useMyLocation() {
  const [location, setLocation] = useState<null | Location>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // Check if geolocation is supported by the browser

    if (!navigator.geolocation) throw new Error('Failed to fetch telemetry data');
    

    // Success callback function
    const successHandler = (position: { coords: { latitude: any; longitude: any; altitude: any; }; }) => {
      setLocation({
        latitude: position.coords.latitude,
        longitude: position.coords.longitude,
        altitude: position.coords.altitude, // Altitude might be null or less accurate
      });

      setLoading(false)
    };

    // Error callback function
    const errorHandler = (error: { message: any; }) => {
        setLoading(false);
        setError(error.message);
    };

    // Options for the location request (optional)
    const options = {
      enableHighAccuracy: true,
      timeout: 5000,
      maximumAge: 0
    };

    // Request the current position
    navigator.geolocation.getCurrentPosition(successHandler, errorHandler, options);
  }, []); // Empty dependency array ensures this runs once when the component mounts
  console.log('My location:', location, 'Loading:', loading, 'Error:', error);
  return { location, error, loading };

};

export default useMyLocation;
