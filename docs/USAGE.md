# Uso

Exemplos e fluxos comuns para utilizar o SmartOutbox.NET.

Criar um pedido (exemplo):

```bash
curl -X POST http://localhost:5000/orders \
  -H "Content-Type: application/json" \
  -d '{"customerName":"Acme Corp","amount":1499.90}'
```

Fluxo interno

1. A API persiste a entidade `Order` dentro de uma transação de banco de dados.
2. O evento `OrderCreatedEvent` é serializado e gravado na tabela `outbox_messages` dentro da mesma transação.
3. O `worker` de outbox consulta mensagens não processadas, publica no RabbitMQ e marca como processadas.

Health checks

- `GET /healthz` retorna o estado de saúde da aplicação incluindo verificações de conexão com o banco e RabbitMQ.

Logs e correlação

- A aplicação usa logs estruturados (Serilog). Envie o cabeçalho `X-Correlation-ID` nas requisições para correlacionar logs.

Fluxo de desenvolvimento

- Inicie tudo com Docker: `docker compose up --build`.
- Para desenvolvimento da API local, use `dotnet watch` em `src/SmartOutbox.SampleApi`.

Observabilidade

- Acesse a UI do RabbitMQ para inspeção de exchanges/queues em `http://localhost:15672`.
- Verifique a tabela `outbox_messages` para mensagens com `Error` e contagem de tentativas (`RetryCount`).

Resolução de problemas

- Mensagens permanecem não processadas: verifique `outbox_messages.Error` e `RetryCount`.
- Falha de conexão com RabbitMQ: confirme configuração/credenciais e se o broker está no ar.
- Esquema de banco ausente: execute as migrações do EF Core.
