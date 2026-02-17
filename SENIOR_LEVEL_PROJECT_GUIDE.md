# SENIOR-LEVEL PROJECT GUIDE — Satcom Mini

This document captures a senior-level architecture blueprint for Satcom Mini: a distributed telemetry ingestion & processing platform designed to signal production readiness.

> Goal: What would Satcom look like if a senior backend engineer designed it for production?

🚀 SATCOM MINI — SENIOR-LEVEL ARCHITECTURE

We’re designing this as a:

Distributed Telemetry Ingestion & Processing Platform (think: aerospace IoT SaaS system)

## 1️⃣ High-Level Architecture

```
		  ┌─────────────────────┐
		  │  Satellite Devices  │
		  └─────────┬───────────┘
				│
				▼
		  ┌─────────────────────┐
		  │  API Gateway (NGINX)│
		  └─────────┬───────────┘
				│
				▼
		┌──────────────────────────┐
		│  Satcom.API (Stateless)  │
		└─────────┬────────────────┘
			    │
	    Publish Event to Queue
			    │
			    ▼
	    ┌─────────────────────────┐
	    │ RabbitMQ / Azure Bus    │
	    └─────────┬───────────────┘
			  │
			  ▼
	    ┌─────────────────────────┐
	    │ Satcom.Worker Service   │
	    └─────────┬───────────────┘
			  │
	  ┌───────────┼─────────────┐
	  ▼           ▼             ▼
   PostgreSQL     Redis       Metrics/Logs
					    │
					    ▼
				    Prometheus/Grafana
```

This immediately signals: service separation, event-driven architecture, stateless API, background processing, observability, scalability.

## 2️⃣ Solution Structure (Clean Architecture)

Suggested repo layout:

```
src/
 ├── Satcom.Api
 ├── Satcom.Worker
 ├── Satcom.Application
 ├── Satcom.Domain
 ├── Satcom.Infrastructure
 └── Satcom.Contracts
```

Responsibilities:

- Domain: entities, value objects, domain events, business rules
- Application: CQRS handlers, commands, queries, DTO mapping
- Infrastructure: EF Core, Redis, RabbitMQ, external clients
- API: controllers/minimal APIs, auth, validation, middleware

This enforces architecture discipline and testability.

## 3️⃣ Authentication & Authorization

Use:

- JWT bearer tokens
- Role- and claims-based identity

Roles: Admin, FleetOperator, Viewer

Features: refresh tokens, token revocation, per-tenant scoping, policy-based endpoint protection, authorization handlers.

## 4️⃣ Event-Driven Telemetry Pipeline

Ingestion flow:

- Device posts telemetry → API validates & authenticates → API publishes TelemetryReceivedEvent → Worker consumes → Worker persists + aggregates → Worker publishes TelemetryProcessedEvent.

Benefits: decoupling, replay capability, retry logic, and dead-letter handling.

## 5️⃣ Resilience Layer

Use Polly for:

- Retry with exponential backoff
- Circuit breaker
- Timeout policy
- Bulkhead isolation

Add idempotency keys and request deduplication.

## 6️⃣ Caching Strategy

Use Redis for:

- Latest satellite position
- Telemetry aggregates
- Dashboard summaries

Add cache invalidation on write, sliding expiration, and stampede prevention.

## 7️⃣ Observability & Monitoring

Add:

- Structured logging (Serilog, JSON)
- Correlation IDs
- OpenTelemetry tracing
- Prometheus metrics
- Health endpoints: /health/live, /health/ready, /metrics

Provide Grafana dashboards for latency, queue depth, error rates, and worker throughput.

## 8️⃣ Database Strategy

Use PostgreSQL with:

- Time-based partitioning on Telemetry table
- Indexes on SatelliteId and ReceivedAtUtc (and necessary composites)
- Read replica pattern for heavy read workloads

Add a retention/archival worker to move older telemetry to cold storage.

## 9️⃣ Horizontal Scaling

Local / small-scale topology (for dev/test) using docker-compose:

- 3 API instances, 1 Worker instance, Redis, RabbitMQ, PostgreSQL, plus NGINX LB.

In production: Cloud Run / K8s / ECS with autoscaling rules and stateless JWT auth.

## 🔟 CQRS Pattern

Separate write model (commands) and read model (queries/projections). Example commands: IngestTelemetry, StartMission. Queries: GetSatelliteStatus, GetLiveDashboard.

## 11️⃣ Testing Strategy

Include:

- Unit tests for domain logic
- Integration tests for API + DB
- Contract tests for message bus events
- Load tests (k6)
- Chaos testing for worker failures

Document throughput targets (example: 5,000 telemetry events/minute) and validate with load tests.

## 12️⃣ Security Hardening

Add:

- HMAC request signing for device ingestion
- API key rotation plan
- Secrets in Secret Manager (no secrets in repo)
- Strict CORS rules and input validation
- Per-tenant rate limiting

## 13️⃣ Multi-Tenancy

Support tenant isolation with:

- Tenant ID in JWT claims
- Scoped DB queries
- Per-tenant rate limits and quotas

## Interview/Storytelling Checklist

Be prepared to explain:

- Why a queue instead of direct DB write
- Why stateless services and horizontal scaling
- How retry and circuit breaker differ
- Why partitioning telemetry data matters
- How idempotency prevents duplicate ingestion
- How correlation IDs trace failures across services

## Final notes

If implemented, Satcom becomes a distributed, event-driven telemetry platform with CQRS, multi-tenancy, resilience patterns, caching, observability, and horizontal scaling — a production-grade system suitable for interviews and real workload demonstrations.

---

Document created: Feb 13, 2026
