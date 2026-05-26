# Instalação

Este documento descreve como instalar e executar o SmartOutbox.NET em ambiente local ou semelhante a produção.

Requisitos

- .NET 8 SDK
- Docker e Docker Compose (recomendado para execução em contêineres)
- PostgreSQL (pode ser executado via Docker)
- RabbitMQ (pode ser executado via Docker)

Execução rápida (Docker Compose)

1. Construa e inicie os serviços:

```bash
docker compose up --build -d
```

2. Aplique as migrações do EF Core (se necessário):

```bash
dotnet tool install --global dotnet-ef --version 8.0.0
cd src/SmartOutbox.EntityFramework
dotnet ef database update --project SmartOutbox.EntityFramework.csproj --startup-project ../SmartOutbox.SampleApi/SmartOutbox.SampleApi.csproj
```

3. Verifique os serviços:

- API: http://localhost:5000
- RabbitMQ (UI): http://localhost:15672 (usuário: `guest`, senha: `guest`)

Configuração

- As configurações principais ficam em `appsettings.json` de cada serviço.
- Substitua valores sensíveis (senhas, usuários) por variáveis de ambiente ou gerenciador de segredos em produção.

Credenciais usadas no `docker-compose.yml` (padrões):

- Host: `postgres`
- Porta: `5432`
- Banco: `smartoutbox`
- Usuário: `smartoutbox`
- Senha: `smartoutbox`

Boas práticas para produção

- Proteja credenciais usando mecanismos de segredo (Vault, AWS Secrets Manager, Azure Key Vault, etc.).
- Habilite TLS para RabbitMQ e configure conexões seguras.
- Execute migrações como parte do pipeline CI/CD antes de iniciar serviços.
- Monitore métricas e logs (ex.: Prometheus + Grafana, ELK/EFK).
