# Estratégia de Retry

Descrição da estratégia de tentativas e dead-letter adotada pelo processador de outbox.

Configurações relevantes

- `OutboxProcessor:PollingIntervalSeconds` — intervalo de polling do worker (padrão: 5s).
- `OutboxProcessor:MaxRetryCount` — número máximo de tentativas antes de dead-letter (padrão: 5).
- `OutboxProcessor:BatchSize` — número de mensagens processadas por lote (padrão: 20).
- `OutboxProcessor:BackoffBaseSeconds` — base em segundos para cálculo de backoff exponencial (padrão: 5s).

Comportamento

1. O worker consulta mensagens com `ProcessedAt IS NULL`, `RetryCount < MaxRetryCount` e `NextAttemptAt <= now`.
2. Para cada mensagem:
   - Tenta publicar no RabbitMQ via `IRabbitMqClient`.
   - Se publicar com sucesso: define `ProcessedAt = now` e limpa `Error`.
   - Se falhar: incrementa `RetryCount`, grava `Error` com a mensagem da exceção, e calcula próxima tentativa.
   - Cálculo de backoff exponencial com jitter:
     ```
     delaySeconds = BackoffBaseSeconds * 2^(RetryCount - 1)
     delaySeconds *= (1 + jitter)  // jitter = ±20%
     NextAttemptAt = now + delaySeconds
     ```
   - Se `RetryCount >= MaxRetryCount`: define `ProcessedAt = now` (dead-letter) e preserva `Error`.

## Exemplos de timing

Com `BackoffBaseSeconds=5` e `MaxRetryCount=5`:

| Tentativa | Delay mínimo | Delay máximo | Delay esperado |
|-----------|--------------|--------------|----------------|
| 1ª falha  | 4s           | 6s           | 5s             |
| 2ª falha  | 8s           | 12s          | 10s            |
| 3ª falha  | 16s          | 24s          | 20s            |
| 4ª falha  | 32s          | 48s          | 40s            |
| 5ª falha  | 64s          | 96s          | 80s            |

Melhorias recomendadas

- Mover mensagens falhas para uma tabela de `dead_letter` com metadados e endpoint administrativo para reprocessamento.
- Adicionar métricas para monitorar taxa de falha, latência e crescimento de dead letters.
- Implementar alertas para quando `dead_letter` cresce acima de um threshold.

Idempotência

- Consumidores devem usar IDs de evento para deduplicação e garantir operações idempotentes.
