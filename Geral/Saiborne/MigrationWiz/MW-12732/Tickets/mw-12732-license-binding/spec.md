# MW-12732 Parte B: binding e ciclo de vida da UMB em conversation line items

Dono: Felipe. Parte A (validação e bloqueio): Leonardo.
Fonte das decisões: sessão `/grill-with-docs` de 04/11/2026. Vocabulário em `GLOSSARY.md`; decisões duráveis em `docs/adr/0001` a `0004` (repositório MigrationWiz, locais, fora do Git).

## Objetivo

Toda conversation line item de um projeto PCH **novo** fica ligada (binding) ao CEU dono da UMB, mantém essa ligação em qualquer edição de endereço, tem o status de UMB atualizado quando a UMB é aplicada, revogada ou expira, e segue as regras de revogação do MW-12732, espelhando Mailbox e OneDrive.

## Restrições

- Nenhuma linha existente, de nenhum workload, tem valores alterados. O B0 só acrescenta colunas com default na tabela do PCH; o binding nunca grava em linhas de projetos que já existem (ADR-0001).
- Tudo atrás da feature flag da Parte B. O critério "projeto novo" fica num único método e retorna "não" até a resposta da 0.2c.
- Nunca executar nada contra banco; a descrição do `.sql` precisa de aprovação antes de ser escrita.
- Nunca fazer commit; deixar no working tree para revisão.

## Contrato com a Parte A (acordado com o Leonardo)

- A Parte A lê só `CustomerEndUserId`; `Guid.Empty` = sem binding = não licenciado.
- A Parte A nunca bloqueia por `SyncSubscriptionStatus` (status de tela, pode atrasar).
- O A1 depende do ticket 01; até lá, usa mock da consulta.
- Os requests de consumo e de expiração de assinatura são arquivos da Parte B.
- Formato de log comum: `"PCH UMB {UmbEvent} for line item {ConversationLineItemId} in project {ConnectorId}: {Reason}"`, propriedades em PascalCase, sem interpolação; `{SourceAddress}` só em Warning.
- Nota de release: ligar a flag da Parte B antes ou junto com a flag de bloqueio da Parte A.

## Eventos de log da Parte B (`{UmbEvent}`)

`BindingCreated` (Information), `BindingUnresolved` (Warning), `AddressChangedBindingKept` (Information), `RevokeBlockedByHistory` (Warning), `RevokeBlockedByWindow` (Warning), `RevokeOnDelete` (Information). Falha técnica: `exception.LogError`.

## Pendente: arquiteto / Will

| Item | Recomendação | Afeta |
|---|---|---|
| 0.2c: critério de "projeto novo" | — | conteúdo do critério nos tickets 03 e 04 |
| 0.2d/ADR-0004: janela de 24h e "qualquer job" só para CEUs com PCH | aplicar (opção B) | confirmação; nada bloqueado |
| 0.2e: Azure Tables | (b) campos do binding na tabela Azure do PCH; alternativa (a) só canais do Mailbox; lacuna do SDK; o que é "support infrastructure database" | ticket 08 |
| 0.2f: exclusão revoga a UMB? | espelhar o Mailbox; aceitar revogação cruzada? | ticket 09 (needs-info) |
| Prefixo (ADR-0002) | confirmar que a frase do épico está imprecisa | nada bloqueado |

## Combinar com o Leonardo (aberto)

- Reenviar o item 4 atualizado: para quem tem PCH, Verify Credentials impede a revogação (ADR-0004).

## Observações para o Will (fora do escopo, não fazer)

- Mailbox: update REST sem CEU zera o binding.
- MSPComplete: mensagem de 48h na revogação não é aplicada por código.
- PCH: trava de edição de linha migrada está comentada.
- Jira: notas do MW-12732 citam MW-12745 como backend de licença (referência trocada).
- MW-12732 pede botão "Change Domain" no PCH: Feature 2.

## Tickets

01 colunas de binding → 02 isolar tabela Azure (prefactor) → 03 binding na criação → 04 varredura, 05 status, 06 edição, 07 revogação, 08 Azure → 09 exclusão (needs-info) → 10 validação ponta a ponta (ready-for-human).
