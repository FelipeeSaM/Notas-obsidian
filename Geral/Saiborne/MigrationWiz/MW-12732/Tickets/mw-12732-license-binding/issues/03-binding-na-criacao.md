Status: done
Blocked by: 01

# 03: Binding na criação da conversation line item (B1)

## O que construir

Ao criar uma conversation line item por qualquer caminho (tela/REST `Create` e `createBulk`, SDK, descoberta), ela é ligada ao CEU dono da UMB, espelhando o event handler do Mailbox:

- sai cedo se a linha já tem data de binding;
- sai cedo se o connector não tem customer ou endpoints (fica para a varredura do ticket 04);
- sai cedo se a feature flag da Parte B estiver desligada ou se o projeto não for novo;
- resolve o CEU pelo mesmo sync do Mailbox, com o source address (`ExportUser`) e, se não bater, o destination address (`ImportUserPrincipalName`) contra o primary email address do CEU, passando o CEU já informado, se houver. Se nada bater, cria o CEU como o Mailbox faz;
- grava o CEU, a data do binding e o status atual da UMB desse CEU (cobre o cenário retroativo: quem já tem UMB pelo Mailbox nasce `Active`).

## Critérios de aceite

- [ ] Event handler registrado na entidade Core (dispara para REST e SDK)
- [ ] Feature flag nova da Parte B (nome em `FeatureToggleNames`, classe em `BusinessToggle`, checagem por `.FeatureEnabledWithCaching`)
- [ ] Critério "projeto novo" num único método, retornando "não" até a resposta da 0.2c
- [ ] CEU já informado na criação (ex.: Add from MSPComplete, MW-12745) é respeitado
- [ ] Log `BindingCreated` (Information, `{Reason}` = CEU encontrado ou criado) e `BindingUnresolved` (Warning, com `{SourceAddress}`), no formato da spec
- [ ] Linhas de projetos existentes nunca são tocadas
- [ ] Verificado se a criação em lote da descoberta dispara o handler; resultado registrado em Comments (se não disparar, o ticket 04 cobre)
- [ ] Testes de integração escritos e compilados, não executados

## Seams under test

- integration: `ConversationLineItemEventHandler` liga a linha ao CEU cujo primary email address é o source address
- integration: `ConversationLineItemEventHandler` usa o destination address quando o source address não bate
- integration: `ConversationLineItemEventHandler` cria CEU novo quando nenhum endereço bate (cenário "adicionado pela UPN")
- integration: `ConversationLineItemEventHandler` grava `SyncSubscriptionStatus` `Active` quando o CEU já tem UMB
- integration: `ConversationLineItemEventHandler` mantém o CEU já informado na criação
- integration: `ConversationLineItemEventHandler` não faz nada com a flag desligada ou com projeto não novo
- integration: `ConversationLineItemEventHandler` não faz nada quando o connector não tem customer/endpoints

## Pendências

- **0.2c (arquiteto):** conteúdo do critério "projeto novo". O método existe e retorna "não" até a resposta.

## Notas

- Nota de release: ligar a flag da Parte B antes ou junto com a flag de bloqueio da Parte A.
- ADR-0001 (restrição de não alterar linhas existentes), glossário: *binding*, *primary email address*.

## Comments

- 2026-11-05 (Claude): **Descoberta dispara o handler.** `DiscoveryTasksService.ImportEntities` → `DataAccessManager.Create` itera as entidades e chama `Objects.ConversationLineItem.Create(ticket)` → `Core.ConversationLineItem.Create()` → `PersistedObject.Create()`, que abre `EventHandlerInvocationContext.BuildContext(EventAction.Create, this)`. Não há bulk insert direto em SQL, então o `ConversationLineItemEventHandler` roda para cada linha descoberta. REST `Create`/`createBulk` (`ConversationLineItemsService`, `conversationLineItem.Create(ticket)`) e SDK seguem o mesmo caminho. O ticket 04 continua necessário só para os casos que saem cedo (connector sem customer/endpoints, falha técnica).
