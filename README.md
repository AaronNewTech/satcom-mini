# Satcom Mini API


A minimal SATCOM-style service: ingest telemetry from ground stations, compute a quick location estimate, and expose REST endpoints. Built to demonstrate OO design, SQL/Postgres, REST, security, and modernization seams.


## Quick Start


1. **Bring up Postgres**
```bash
docker compose up -d