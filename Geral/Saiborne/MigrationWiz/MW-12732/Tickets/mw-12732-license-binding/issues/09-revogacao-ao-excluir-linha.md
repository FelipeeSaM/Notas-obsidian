Status: done
Blocked by: 05, 07

# 09: Revogação ao excluir conversation line item (B4)

## O que construir

Espelhando a exclusão de Mailbox: excluir conversation line items (uma ou em lote) com status de UMB `Active`, exceto as que estão em migração, dispara o workflow de revogação para os CEUs dessas linhas. A revogação passa pelas regras do ticket 07, então só acontece dentro de 24 horas e sem job submetido.

Se a resposta da 0.2f for "fora do escopo", o ticket passa a ser só um teste garantindo que excluir uma conversation line item não revoga nada.

## Critérios de aceite

- [ ] `Delete` e `DeleteBulk` disparam a revogação só para CEUs de linhas com status `Active`
- [ ] Linhas em migração são ignoradas, como no Mailbox
- [ ] Atrás da feature flag da Parte B
- [ ] Log `RevokeOnDelete` (Information) no formato da spec
- [ ] Testes de integração escritos e compilados, não executados

## Seams under test

- integration: `ConversationLineItemsService.DeleteBulk` inicia a revogação para os CEUs das linhas com status `Active`
- integration: `ConversationLineItemsService.Delete` inicia a revogação para o CEU de uma linha com status `Active`
- integration: `ConversationLineItemsService.DeleteBulk` não inicia revogação para linhas sem binding ou com status `None`

## Pendências

- **0.2f (Will/arquiteto):** a exclusão entra no escopo? Aceitam a revogação cruzada (excluir a linha de PCH revoga a UMB que um projeto Mailbox ainda não migrado ia usar), igual ao que já ocorre entre projetos Mailbox?

## Comments

### 2026-11-05: decisão assumida na implementação (0.2f pendente)

- **Opção A assumida:** exclusão em escopo, espelhando o Mailbox, **incluindo o `Delete` individual**. O Mailbox hoje só revoga no `DeleteBulk`; aqui o texto do ticket prevaleceu.
- **Linhas em migração ignoradas:** o Mailbox inclui essas linhas na revogação, mas a regra de histórico do ticket 07 já bloquearia a revogação delas. O efeito é o mesmo, sem disparar um workflow à toa.
- **Base:** o épico e o PRD não falam de exclusão, mas pedem para reaproveitar os caminhos do Mailbox/OneDrive. A revogação cruzada fica limitada pelas regras do ticket 07 (24h e sem job).
- **Falha ao iniciar a revogação:** é logada e não bloqueia a exclusão.
- Pergunta ao PO registrada em `lembrete-0.2c-criterio-projeto-novo.md`. Se a resposta for "fora do escopo", remover `StartRevocationForDeletedItems` e manter só um teste de "não revoga".

### 2026-11-05: pendência resolvida pelo PRD e pelo código do Mailbox

- PRD 5.6 ("reuse existing code paths from Mailbox and OneDrive"). No Mailbox, `ProjectItemsService.DeleteBulk` revoga e `Delete(Guid)` não revoga.
- Ajuste: `ConversationLineItemsService.Delete` deixou de revogar; só o `DeleteBulk` revoga. O teste de integração `Delete_GivenLineItemWithActiveUmb_StartsRevocationForItsCustomerEndUser` virou `Delete_GivenLineItemWithActiveUmb_DoesNotStartRevocation` (escrito, compilado, não executado).
- A seam "`Delete` inicia a revogação" deixa de valer e passa a ser "`Delete` não inicia revogação".
