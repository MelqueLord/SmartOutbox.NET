# Roteiro (Roadmap)

Planejamento das próximas melhorias e evolução do SmartOutbox.NET.

Curto prazo

- Adicionar backoff exponencial e jitter às tentativas do worker.
- Fornecer UI ou endpoint administrativo para visualizar e reprocessar `dead_letter`.
- Escrever testes unitários e de integração para publisher e worker.

Médio prazo

- Biblioteca de consumidores para facilitar padrões de idempotência e deduplicação.
- Exportação de métricas (Prometheus) e dashboards (Grafana).
- Suporte a múltiplos transportes e topologias por tenant.

Longo prazo

- Estratégias de particionamento e sharding para outbox de grande volume.
- Mecanismos plugáveis de armazenamento e transporte (ex.: Kafka, serviço de eventos gerenciado).

Contribuição

- Abra issues e PRs. Siga as diretrizes de contribuição do repositório.
