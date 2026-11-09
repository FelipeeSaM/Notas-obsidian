Status: done
Blocked by: 03

# 07: Regras de revogação para CEUs com PCH (B3)

## O que construir

Para um CEU que tem ao menos uma conversation line item ligada (inclusive excluída), a revogação da UMB falha quando:

- existe **qualquer** job PCH submetido no histórico da linha, inclusive Verify Credentials; ou
- já se passaram **24 horas** desde o consumo da UMB.

O CEU vai para a lista de falhas da revogação, como já acontece com o histórico de Mailbox. CEUs sem conversation line item ligada mantêm exatamente o comportamento atual: sem janela de tempo, com o filtro de jobs do Mailbox (ADR-0004).

## Critérios de aceite

- [ ] Histórico do PCH lido da própria conversation line item (SQL), sem Azure, por código que não dependa da camada REST
- [ ] Data do consumo obtida do registro de consumo da UMB que a revogação já lê
- [ ] Regras aplicadas só a CEUs com conversation line item ligada; regra do Mailbox intacta
- [ ] Atrás da feature flag da Parte B
- [ ] Logs `RevokeBlockedByHistory` e `RevokeBlockedByWindow` (Warning) no formato da spec
- [ ] Testes de integração escritos e compilados, não executados

## Seams under test

- integration: `RevokeSubscriptionRequest` falha para CEU cuja conversation line item tem job Verify Credentials submetido
- integration: `RevokeSubscriptionRequest` falha para CEU cuja conversation line item tem job Full submetido
- integration: `RevokeSubscriptionRequest` falha para CEU com PCH depois de 24 horas do consumo da UMB
- integration: `RevokeSubscriptionRequest` revoga CEU com PCH dentro de 24 horas e sem job
- integration: `RevokeSubscriptionRequest` considera conversation line items excluídas com histórico
- integration: `RevokeSubscriptionRequest` mantém o comportamento atual para CEU só de Mailbox (sem janela; Verify Credentials ignorado)

## Pendências

- **Arquiteto:** confirmar o efeito de ADR-0004 (quem está no Mailbox e no PCH ganha a janela de 24h em todos os workloads). Nada bloqueado.
- **Leonardo:** reenviar o item 4 atualizado (Verify Credentials impede a revogação para quem tem PCH).

## Referências

- ADR-0004

## Comments

### 2026-11-05: observação aceita na implementação

- **Limitação aceita (igual à do Mailbox):** line items de projetos PCH **excluídos** não entram nas regras de revogação. O `includeIsDeleted: true` só remove o filtro `IsDeleted` da própria line item. O `INNER JOIN` de segurança com `MailboxConnector` continua descartando projetos excluídos. Um CEU que só tem histórico PCH em projeto excluído é tratado como "sem PCH", sem janela de 24h e sem a regra de jobs. O Mailbox (`ValidateAssociatedEntities`) tem o mesmo comportamento hoje. Decisão: espelhar o Mailbox e não fazer consulta direta sem o join. Levar ao Will/arquiteto junto com a ADR-0004.
- **Nota de release:** ligar `pch-umb-binding` só depois do deploy das colunas do ticket 01. Com a flag ligada e sem a coluna `CustomerEndUserId`, a busca de line items lança exceção e a revogação falha também para CEUs só de Mailbox.
