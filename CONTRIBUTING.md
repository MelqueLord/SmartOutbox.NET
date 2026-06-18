# Contributing

Thanks for improving SmartOutbox.NET.

## Development Workflow

1. Create a focused branch.
2. Keep changes small and production-relevant.
3. Run build and tests before opening a pull request.
4. Update documentation when behavior or configuration changes.

```bash
dotnet build SmartOutbox.NET.sln
dotnet test SmartOutbox.NET.sln
```

## Coding Guidelines

- Prefer simple, explicit code over clever abstractions.
- Keep the Transactional Outbox Pattern easy to understand.
- Use structured logging for operationally relevant events.
- Validate configuration at startup.
- Add tests for retry, persistence, serialization, and messaging behavior.

## Pull Request Checklist

- The solution builds.
- Unit and integration tests pass.
- New configuration is documented.
- Failure behavior is described when reliability code changes.
- Public docs remain recruiter-friendly and technically accurate.
