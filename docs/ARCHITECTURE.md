# Arquitetura

Visão geral da arquitetura implementada pelo SmartOutbox.NET, baseada no padrão Transactional Outbox.

Componentes principais

- `SmartOutbox.SampleApi` (Camada de API): recebimento de requisições HTTP, regras de aplicação e escrita de entidades e eventos em transação.
- `SmartOutbox.EntityFramework` (Infraestrutura de persistência): `ApplicationDbContext`, mapeamentos EF Core e `EfEventPublisher` que grava mensagens na tabela de outbox **sem chamar `SaveChanges`** (responsabilidade da camada de aplicação).
- `SmartOutbox.RabbitMQ` (Transporte): `IRabbitMqClient` (abstração) e `DefaultRabbitMqClient` (implementação) para publicar mensagens no RabbitMQ de forma testável; `RabbitMqPublisher` implementa `IEventPublisher` de forma simples.
- `SmartOutbox.Worker` (Processador de Outbox): serviço em background que lê `outbox_messages`, publica eventos e faz retries/dead-letter.
- `SmartOutbox.Core` (Domínio): entidades, contratos de eventos, serviço de serialização JSON e interfaces compartilhadas.

Fluxo de dados

1. Requisição para criar domínio (ex.: criar pedido).
2. Serviço cria a entidade e chama `IEventPublisher.PublishAsync(integrationEvent)` dentro de uma transação.
3. `EfEventPublisher` serializa o evento e grava um `OutboxMessage` na mesma transação do banco **sem chamar `SaveChanges`**.
4. A camada de aplicação (ex.: `OrderService`) chama `dbContext.SaveChangesAsync()` para persistir atomicamente entidade e outbox.
5. `Worker` lê mensagens pendentes com `ProcessedAt IS NULL` e `NextAttemptAt <= now`, publica no RabbitMQ.
6. Em caso de sucesso: atualiza `ProcessedAt = now` e limpa `Error`.
7. Em caso de falha: incrementa `RetryCount`, grava `Error`, calcula `NextAttemptAt` usando backoff exponencial com jitter.
8. Após `MaxRetryCount` tentativas: marca como dead-letter preservando o erro.

Princípios de projeto

- Separação de responsabilidades e inversão de dependência: publicação é abstrata via `IEventPublisher` e cliente RabbitMQ via `IRabbitMqClient`.
- Atomicidade transacional: eventos são persistidos na mesma transação dos dados de negócio para eliminar o problema de dupla gravação; `EfEventPublisher` não commita, deixando o controle para a aplicação.
- Isolamento de falhas: publicação externa é feita de forma assíncrona por workers independentes, desacoplando a API do broker.
- Testabilidade: abstrações `IRabbitMqClient` e `IRabbitMqPublisher` permitem mocks e testes sem dependência de RabbitMQ real.
- Backoff adaptativo: `NextAttemptAt` evita polling constante e permite estratégias de retry mais eficientes.

Topologia de exchange

- Exchange durável do tipo `topic` chamada `outbox.events`.
- Chave de roteamento: nome do tipo do evento (`EventType`), permitindo assinaturas específicas por tipo.

Manuseio de falhas

- Retries controlados pelo `OutboxProcessorOptions.MaxRetryCount`.
- Mensagens que atingem o limite de tentativas são marcadas como processadas (dead-lettered) preservando o erro.
- Consumidores devem ser idempotentes para tolerar re-entregas.
