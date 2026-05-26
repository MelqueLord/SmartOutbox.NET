# Roteiro (Roadmap)

Planejamento das próximas melhorias e evolução do SmartOutbox.NET.

Implementado

- ✅ Backoff exponencial com jitter e `NextAttemptAt` para scheduling eficiente.
- ✅ Abstração `IRabbitMqClient` para maior testabilidade e desacoplamento.
- ✅ Controle transacional melhorado: `EfEventPublisher` não commita, responsabilidade da aplicação.

Curto prazo

- Escrever testes unitários e de integração para publisher, worker e processador de outbox.
- Fornecer UI ou endpoint administrativo para visualizar e reprocessar `dead_letter`.
- CI/CD pipeline com build automatizado, testes e push para repositório remoto.

Médio prazo

- Biblioteca de consumidores para facilitar padrões de idempotência e deduplicação.
- Exportação de métricas (Prometheus) e dashboards (Grafana).
- Suporte a múltiplos transportes e topologias por tenant.

Longo prazo

- Estratégias de particionamento e sharding para outbox de grande volume.
- Mecanismos plugáveis de armazenamento e transporte (ex.: Kafka, serviço de eventos gerenciado).

Contribuição

- Abra issues e PRs. Siga as diretrizes de contribuição do repositório.
