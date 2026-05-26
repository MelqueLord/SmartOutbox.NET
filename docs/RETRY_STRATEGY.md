# Estratégia de Retry

Descrição da estratégia de tentativas e dead-letter adotada pelo processador de outbox.

Configurações relevantes

- `OutboxProcessor:PollingIntervalSeconds` — intervalo de polling do worker.
- `OutboxProcessor:MaxRetryCount` — número máximo de tentativas antes de dead-letter.
- `OutboxProcessor:BatchSize` — número de mensagens processadas por lote.

Comportamento

1. O worker consulta mensagens com `ProcessedAt IS NULL` e `RetryCount < MaxRetryCount`.
2. Para cada mensagem:
   - Tenta publicar no RabbitMQ.
   - Se publicar com sucesso: define `ProcessedAt = now` e limpa `Error`.
   - Se falhar: incrementa `RetryCount` e grava `Error` com a mensagem da exceção.
   - Se `RetryCount >= MaxRetryCount`: define `ProcessedAt = now` para dead-letter e preserva `Error`.

Melhorias recomendadas

- Implementar backoff exponencial com jitter para reduzir pressão no broker em picos de falha.
- Mover mensagens falhas para uma tabela de `dead_letter` com metadados e endpoint administrativo para reprocessamento.
- Adicionar métricas para monitorar taxa de falha, latência e crescimento de dead letters.

Idempotência

- Consumidores devem usar IDs de evento para deduplicação e garantir operações idempotentes.
