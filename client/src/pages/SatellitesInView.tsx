import React from 'react';
import MyLocationSection from '../components/MyLocationSection';
import { SatellitesInViewSection } from '../components/SatellitesInViewSection';


const SatellitesInView: React.FC = () => {
  

  return (
    <div>
        <MyLocationSection />
        <SatellitesInViewSection />
    </div>
  );
};

export default SatellitesInView;