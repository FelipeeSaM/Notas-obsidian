Status: done
Blocked by: None

# 02: Dependência injetável para a tabela Azure das conversation line items (aditivo)

## O que construir

Mudança só aditiva: o código existente que fala com a tabela Azure do PCH (métodos estáticos do `ConversationLineItemsService`, `ConversationItemMigrationsService`) **não é alterado**, mesmo onde há código morto ou duplicado, para não arriscar o que já roda em produção.

Cria-se uma interface injetável de acesso à tabela Azure do PCH (resolução das credenciais por zona, leitura por query, gravação por merge) com a implementação real, e o `ConversationLineItemsService` passa a aceitá-la como dependência opcional no construtor, no padrão do projeto (`deps ?? new Dep()`, sem container de DI). Só código novo usa essa dependência.

Prepara o ticket 08 (gravar o binding na tabela Azure) para ser testável.

## Critérios de aceite

- [ ] Nenhuma linha existente de leitura/gravação no Azure alterada; diff em arquivo existente limitado à propriedade nova e ao parâmetro opcional do construtor
- [ ] O serviço aceita a dependência opcional; sem ela, usa a implementação real
- [ ] Nenhum projeto, camada ou biblioteca nova; nada de container, MediatR etc.
- [ ] `/// <summary>` em todo membro novo
- [ ] Testes existentes continuam passando; testes unitários novos passam

## Seams under test

- unit: `ConversationLineItemsService` usa a dependência de tabela Azure injetada (mock)
- unit: `ConversationLineItemsService` sem dependência injetada usa a implementação real (`ConversationLineItemTableStorage`)

> Seams novas (fora do mapa de `testing.md`), acordadas na criação deste ticket e revisadas em 2026-11-05 (ver Comments).

## Referências

- Pendência 0.2e (spec): a gravação do binding no Azure é do ticket 08, que usa `this.TableStorage` só no código novo.

## Comments

- 2026-11-05: Escopo revisado de "refatoração" para "aditivo". Motivos:
  - Decisão do Felipe: não refatorar código validado em produção, mesmo morto ou duplicado; só acrescentar o necessário.
  - As seams antigas não batiam com o código: `UpdateLineItemStatus` (estático) não tem chamador (o status é gravado pelo Engine em `PrivateChatHydrationProcessActionsUtilities`); o histórico e as estatísticas expostos pela API vêm das colunas SQL `ExecutionHistory` e `MigrationStatistics`, não do Azure; a leitura de histórico no Azure (`DownloadLineItemQueueTableEntitiesByPartitionKey`) é código morto.
  - Custo aceito: a criação do cliente Azure fica duplicada (classe nova e método antigo `GetAzureTableStorageClient`).
- 2026-11-05: Implementado no working tree (sem commit): `IConversationLineItemTableStorage` e `ConversationLineItemTableStorage` em `MigrationWiz\Api\Azure\Storage\`; propriedade `TableStorage` + parâmetro opcional no construtor do `ConversationLineItemsService`; `ConversationLineItemsServiceTests` com 2 testes passando.
