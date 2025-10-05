import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import SatellitesPage from './pages/SatellitesPage';
import GroundStationsPage from './pages/GroundStationsPage';
import TelemetryPage from './pages/TelemetryPage';
import MapView from './pages/MapView';

function App() {
  return (
    <Router>
      <div
        className="app-container"
        style={{ fontFamily: 'sans-serif', padding: '2rem' }}
      >
        <header style={{ marginBottom: '2rem' }}>
          <h1>Satcom Mini Dashboard</h1>
          <p>Monitor satellite telemetry and visualize geospatial data.</p>
        </header>
        <nav style={{ marginBottom: '2rem' }}>
          <ul
            style={{
              listStyle: 'none',
              padding: 0,
              display: 'flex',
              gap: '1.5rem',
            }}
          >
            <li>
              <Link to="/map">Map View</Link>
            </li>
            <li>
              <Link to="/telemetry">Telemetry</Link>
            </li>
            <li>
              <Link to="/satellites">Satellites</Link>
            </li>
            <li>
              <Link to="/ground-stations">Ground Stations</Link>
            </li>
          </ul>
        </nav>
        <Routes>
          <Route
            path="/"
            element={
              <main>
                <section
                  id="telemetry"
                  style={{ marginBottom: '2rem' }}
                ></section>
                <section id="map" style={{ marginBottom: '2rem' }}>
                  <h2>Map View</h2>
                  <p>
                    Visualize satellite and ground station positions on an
                    interactive map.
                  </p>
                </section>
              </main>
            }
          />
          <Route path="/telemetry" element={<TelemetryPage />} />
          <Route path="/map" element={<MapView />} />
          <Route path="/satellites" element={<SatellitesPage />} />
          <Route path="/ground-stations" element={<GroundStationsPage />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;
