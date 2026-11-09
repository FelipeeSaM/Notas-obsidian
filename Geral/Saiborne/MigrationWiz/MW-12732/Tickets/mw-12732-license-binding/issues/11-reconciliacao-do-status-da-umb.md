Status: needs-triage
Blocked by: 05

# 11: Reconciliação periódica do status da UMB das conversation line items

## O que construir

Um processo periódico do AEF que recalcula o `SyncSubscriptionStatus` das conversation line items já ligadas a um CEU, a partir da assinatura real desse CEU, e corrige as que estiverem divergentes.

O ticket 05 atualiza o status nos pontos de consumo, revogação e expiração, mas nenhum desses fluxos tenta de novo quando o sync das conversation line items falha. A falha fica só no log, ou no alerta por e-mail, e a linha permanece com o status errado (por exemplo, `Active` com a UMB já expirada) até alguém agir. Esta reconciliação é a garantia de que o status converge sozinho.

O cálculo do status deve reaproveitar a regra que já existe, em vez de criar uma nova: `MigrationWizToMspcSyncingService.RetrieveSubscriptionStatusByCustomerEndUserId`, a mesma usada pelo binding na criação (ticket 03).

## Por que é necessário (achados do ticket 05)

- **Expiração:** o `SubscriptionsExpiredProcessingActivity` processa janelas de 30 minutos. Quando uma janela falha, a próxima que dá certo grava o ponto de controle depois dela, e a janela que falhou nunca é reprocessada.
- **Consumo:** o retry automático do `SafeNetworkExecute` repete o request. A segunda execução encontra as assinaturas já processadas, sai cedo e retorna sucesso, sem alerta. A falha do PCH fica só no log.
- **Revogação:** a falha é só logada, como já acontece com os Mailboxes.

## Critérios de aceite

- [ ] Activity e workflow recorrente próprios no AEF, espelhando a varredura do ticket 04 (`ConversationLineItemToCustomerEndUserActivity`)
- [ ] Considera só conversation line items ligadas (`MspcSynchronizationDate` preenchida e `CustomerEndUserId` diferente de vazio)
- [ ] Status calculado por `RetrieveSubscriptionStatusByCustomerEndUserId`, uma vez por CEU, sem regra nova
- [ ] Atualiza só as linhas cujo status gravado difere do calculado
- [ ] Processa em lotes, com proteção contra laço infinito no mesmo padrão do ticket 04
- [ ] Respeita a feature flag da Parte B (`pch-umb-binding`)
- [ ] Nunca altera Mailbox nem OneDrive (ver Pendências)
- [ ] Testes de integração escritos e compilados, não executados

## Seams under test

- integration: `<ReconciliationActivity>.Execute` corrige para o status calculado as conversation line items ligadas cujo status diverge da assinatura do CEU
- integration: `<ReconciliationActivity>.Execute` não altera conversation line items cujo status já está correto
- integration: `<ReconciliationActivity>.Execute` ignora conversation line items sem binding e não faz nada com a flag desligada
- integration: `<ReconciliationActivity>.Execute` termina quando não há linhas processáveis

O nome da classe será definido na implementação; o teste fica em `AsyncExecutionFramework\AEFIntegrationTests\Synchronization\`, ao lado do teste do ticket 04.

## Pendências

- **Escopo (decisão humana):** o Mailbox tem a mesma lacuna hoje, sem reconciliação nenhuma. Decidir se este ticket cobre só o PCH, como está escrito, ou se vira um ticket separado para o Mailbox. Mexer no Mailbox altera fluxo em produção e precisa de aprovação.
- **Frequência:** a varredura do ticket 04 roda a cada 12 horas. Definir se esse intervalo é aceitável para o status da UMB, ou se precisa ser menor.
- **Custo:** a reconciliação lê todas as linhas ligadas a cada execução. Avaliar o volume esperado e, se preciso, limitar a busca por data (por exemplo, linhas de CEUs com assinatura que terminou ou mudou desde a última execução).
- **Relação com o ticket 09:** a revogação ao excluir uma linha parte do status `Active`. Com a reconciliação, esse status fica confiável mesmo depois de uma falha de sync.

## Comments

- 2026-11-05 (Claude): Ticket criado a partir da revisão do ticket 05. Lá, o sync das conversation line items foi isolado com `try/catch` para nunca interromper o Mailbox. No consumo e na expiração a falha é relançada no fim, para disparar o alerta; na revogação ela só é logada. O usuário considerou não aceitável que o status do PCH fique desatualizado depois de uma falha, e por isso a reconciliação.
