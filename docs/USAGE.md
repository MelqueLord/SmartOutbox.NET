# Uso

Exemplos e fluxos comuns para utilizar o SmartOutbox.NET.

Criar um pedido (exemplo):

```bash
curl -X POST http://localhost:5000/orders \
  -H "Content-Type: application/json" \
  -d '{"customerName":"Acme Corp","amount":1499.90}'
```

Fluxo interno

1. A API inicia uma transação de banco de dados e persiste a entidade `Order`.
2. `IEventPublisher.PublishAsync()` é chamado, que adiciona um `OutboxMessage` serializado na mesma transação **sem fazer commit**.
3. A camada de aplicação (OrderService) chama `dbContext.SaveChangesAsync()` para persistir atomicamente `Order` e `OutboxMessage`.
4. O `worker` de outbox consulta mensagens com `ProcessedAt IS NULL` e `NextAttemptAt <= now`, publica no RabbitMQ.
5. Sucesso: marca `ProcessedAt = now`; Falha: incrementa `RetryCount` e agenda próxima tentativa via `NextAttemptAt` usando backoff exponencial com jitter.

Health checks

- `GET /healthz` retorna o estado de saúde da aplicação incluindo verificações de conexão com o banco e RabbitMQ.

Logs e correlação

- A aplicação usa logs estruturados (Serilog). Envie o cabeçalho `X-Correlation-ID` nas requisições para correlacionar logs.

Fluxo de desenvolvimento

- Inicie tudo com Docker: `docker compose up --build`.
- Para desenvolvimento da API local, use `dotnet watch` em `src/SmartOutbox.SampleApi`.

Observabilidade

- Acesse a UI do RabbitMQ para inspeção de exchanges/queues em `http://localhost:15672` (usuário: `guest`, senha: `guest`).
- Verifique a tabela `outbox_messages` para:
  - `Error`: descrição do erro da última falha.
  - `RetryCount`: quantidade de tentativas realizadas.
  - `NextAttemptAt`: timestamp da próxima tentativa agendada (NULL = nunca tentou ou foi bem-sucedida).
  - `ProcessedAt`: timestamp da conclusão (NULL = pendente).
- Use logs estruturados (Serilog) para rastrear tentativas e falhas: procure por `NextAttemptAt` e `Retry` nos logs.

Resolução de problemas

- Mensagens permanecem não processadas: verifique `outbox_messages.Error` e `RetryCount`.
- Falha de conexão com RabbitMQ: confirme configuração/credenciais e se o broker está no ar.
- Esquema de banco ausente: execute as migrações do EF Core.
