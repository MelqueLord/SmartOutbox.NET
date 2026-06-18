# Changelog

All notable changes to this project will be documented in this file.

## Unreleased

- Reworked README into a production-focused portfolio overview.
- Added focused architecture, outbox, retry/backoff, and RabbitMQ documentation.
- Added unit and integration test projects.
- Added runtime-type event serialization to preserve derived event payloads.
- Added correlation ID propagation into outbox rows and RabbitMQ messages.
- Added message ID tracking using outbox message IDs.
- Added RabbitMQ publisher confirms, durable queue topology, DLX/DLQ setup, and automatic recovery options.
- Added startup validation for database, RabbitMQ, and worker options.
- Added deterministic exponential backoff calculator.
- Added MIT license and contribution guide.
- Removed stale duplicate root-level project folders and solution.
