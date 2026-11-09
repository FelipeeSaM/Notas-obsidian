Status: done
Blocked by: 03

# 05: Status da UMB acompanha aplicar, revogar e expirar (N8)

## O que construir

Quando a UMB de um CEU é aplicada (consumida), revogada ou expira, as conversation line items ligadas a esse CEU têm o status de UMB atualizado (`Active`, `None` ou `Expired`), ao lado da atualização que já acontece hoje para os Mailboxes. É o que faz a coluna "UMB ativa" (MW-12746) refletir a assinatura real.

## Critérios de aceite

- [ ] Método de sync das conversation line items no serviço de sync, espelhando o dos Mailboxes
- [ ] Chamado em todos os pontos onde o sync dos Mailboxes é chamado: consumo da assinatura, revogação e expiração
- [ ] A lógica de consumo não muda (continua "1 UMB por CEU"; filtros de assinaturas já processadas e inválidas intactos)
- [ ] Os requests de consumo e de expiração são arquivos da Parte B (acordado com o Leonardo)
- [ ] CEUs sem conversation line item: nada muda
- [ ] Testes unitários passam; testes de integração escritos e compilados, não executados

## Seams under test

- unit: `MigrationWizToMspcSyncingService.SyncConversationLineItems` atualiza o status das conversation line items dos CEUs informados
- integration: `ConsumePrepaidSubscriptionRequest` marca como `Active` as conversation line items do CEU
- integration: `RevokeSubscriptionRequest` marca como `None` as conversation line items do CEU revogado
- integration: `SyncExpiredSubscriptionsRequest` marca como `Expired` (ou `None`, se revogada) as conversation line items do CEU

## Notas

- A Parte A não depende deste ticket (não bloqueia por status, ADR-0001).
- O ticket 09 depende deste: a revogação ao excluir parte do status `Active`.

## Comments
