# Arquitetura

Visão geral da arquitetura implementada pelo SmartOutbox.NET, baseada no padrão Transactional Outbox.

Componentes principais

- `SmartOutbox.SampleApi` (Camada de API): recebimento de requisições HTTP, regras de aplicação e escrita de entidades e eventos em transação.
- `SmartOutbox.EntityFramework` (Infraestrutura de persistência): `ApplicationDbContext`, mapeamentos EF Core e `EfEventPublisher` que grava mensagens na tabela de outbox.
- `SmartOutbox.RabbitMQ` (Transporte): responsável por publicar mensagens no RabbitMQ e prover health checks.
- `SmartOutbox.Worker` (Processador de Outbox): serviço em background que lê `outbox_messages`, publica eventos e faz retries/dead-letter.
- `SmartOutbox.Core` (Domínio): entidades, contratos de eventos, serviço de serialização JSON e interfaces compartilhadas.

Fluxo de dados

1. Requisição para criar domínio (ex.: criar pedido).
2. Serviço cria a entidade e chama `IEventPublisher.PublishAsync(integrationEvent)`.
3. `EfEventPublisher` serializa o evento e grava um `OutboxMessage` na mesma transação do banco.
4. `Worker` lê mensagens pendentes e publica no RabbitMQ; em caso de sucesso atualiza `ProcessedAt`, em caso de falha incrementa `RetryCount`.

Princípios de projeto

- Separação de responsabilidades e inversão de dependência: publicação é abstrata via `IEventPublisher`.
- Atomicidade transacional: eventos são persistidos na mesma transação dos dados de negócio para eliminar o problema de dupla gravação.
- Isolamento de falhas: publicação externa é feita de forma assíncrona por workers independentes, desacoplando a API do broker.

Topologia de exchange

- Exchange durável do tipo `topic` chamada `outbox.events`.
- Chave de roteamento: nome do tipo do evento (`EventType`), permitindo assinaturas específicas por tipo.

Manuseio de falhas

- Retries controlados pelo `OutboxProcessorOptions.MaxRetryCount`.
- Mensagens que atingem o limite de tentativas são marcadas como processadas (dead-lettered) preservando o erro.
- Consumidores devem ser idempotentes para tolerar re-entregas.
