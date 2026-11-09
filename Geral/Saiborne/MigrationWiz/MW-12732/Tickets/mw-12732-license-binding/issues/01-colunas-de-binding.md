Status: done
Blocked by: None

# 01: Colunas de binding na conversation line item (B0)

## O que construir

Toda conversation line item passa a ter onde guardar o binding: o CEU (`CustomerEndUserId`, NOT NULL, default `Guid.Empty`), a data do binding (`MspcSynchronizationDate`, nullable) e o status de UMB (`SyncSubscriptionStatus`, NOT NULL, default `None`), nas cinco camadas e com os mesmos tipos, defaults e índices do Mailbox. É só schema: nada preenche os campos ainda.

A API REST passa a devolver o CEU e o status de cada conversation line item. O update pela API nunca apaga o binding: o CEU só é sobrescrito quando vem com valor, e o status nunca é escrito pelo DTO (ADR-0003). Linhas existentes mantêm todos os valores atuais; as colunas novas entram com default.

## Critérios de aceite

- [ ] Descrição da mudança no `.sql` apresentada ao Felipe e aprovada **antes** de escrever o arquivo; nada é executado contra banco
- [ ] Checklist das 5 camadas marcado: tabela, entidade Core, objeto SDK, wrapper de acesso (só faltam data e status; o CEU já existe), DTO
- [ ] Tipos, defaults e índices iguais aos do Mailbox
- [ ] DTO expõe `CustomerEndUserId` (`Guid?`) e `SyncSubscriptionStatus` na leitura
- [ ] Update sem CEU mantém o CEU atual; `SyncSubscriptionStatus` nunca é alterado pelo DTO
- [ ] Nenhuma lógica de preenchimento (é do ticket 03)
- [ ] Projetos alterados buildam; testes unitários do DTO passam; teste de integração escrito e compilado, não executado

## Seams under test

- unit: `ConversationLineItemDTO(ConversationLineItem)` expõe `CustomerEndUserId` e `SyncSubscriptionStatus` da entidade
- unit: `ConversationLineItemDTO.UpdateConversationLineItem` mantém o `CustomerEndUserId` atual quando o DTO vem sem CEU
- unit: `ConversationLineItemDTO.UpdateConversationLineItem` sobrescreve o `CustomerEndUserId` quando o DTO traz um valor
- unit: `ConversationLineItemDTO.UpdateConversationLineItem` nunca altera `SyncSubscriptionStatus`
- integration: `ConversationLineItem.Create` persiste os defaults (`Guid.Empty`, `null`, `None`)

## Referências

- ADR-0001 (colunas criadas pela Parte B no B0), ADR-0003 (DTO nunca apaga o binding)

## Notas

- Desbloqueia o A1 do Leonardo (acordado).
- Aceitar o CEU enviado pelo cliente no `createBulk` e validar a organização é do MW-12745, que também depende deste ticket.

## Comments
