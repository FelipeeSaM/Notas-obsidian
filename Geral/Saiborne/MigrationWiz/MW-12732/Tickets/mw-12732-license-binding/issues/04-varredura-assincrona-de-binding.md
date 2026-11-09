Status: done
Blocked by: 03

# 04: Varredura assíncrona de binding (N4)

## O que construir

Conversation line items que ficaram sem binding são ligadas depois por um processo periódico do AEF, espelhando a varredura do Mailbox (`MailboxToCustomerEndUser`). Exemplos: connector sem customer/endpoints no momento da criação, falha passageira do sync, ou criação em lote pela descoberta sem disparar o handler.

O processo busca as linhas sem data de binding (só de projetos novos e com a flag ligada), liga cada uma pelo mesmo sync do ticket 03 e grava CEU, data e status, com a mesma proteção contra laço infinito do Mailbox.

## Critérios de aceite

- [ ] Activity com construtor sem parâmetros e workflow de sincronização próprio, espelhando o do Mailbox
- [ ] Respeita a feature flag da Parte B e o critério "projeto novo"
- [ ] Mesma proteção contra laço infinito (contador e filtro por data de criação)
- [ ] Agendamento igual ao do Mailbox; se ele morar em configuração ou deploy, registrar em Comments como item para o arquiteto, sem alterar configuração
- [ ] Linhas de projetos existentes nunca são tocadas
- [ ] Testes de integração escritos e compilados, não executados

## Seams under test

- integration: `ConversationLineItemToCustomerEndUserActivity` liga conversation line items sem data de binding ao CEU
- integration: `ConversationLineItemToCustomerEndUserActivity` ignora linhas de projetos não novos e com a flag desligada
- integration: `ConversationLineItemToCustomerEndUserActivity` termina quando não há linhas processáveis

## Pendências

- **0.2c (arquiteto):** critério "projeto novo" (mesmo método do ticket 03).

## Comments

- 2026-11-05 (Claude): **Agendamento fica no código, não em configuração.** O Mailbox agenda pelo atributo `[Recurrent("2016-03-06T00:00:00", RecurrenceFrequency.Hourly, 12, areConcurrentInstancesAllowed: false, EnvironmentType = EnvironmentType.All)]` em `MailboxToCustomerEndUserWorkflow`, e não há outro registro de `synchronization.mspc.mailboxes` no repositório. O `ConversationLineItemToCustomerEndUserWorkflow` (`synchronization.mspc.conversationlineitems`) usa o mesmo atributo. Nada precisa ser alterado em configuração ou deploy.
- 2026-11-05 (Claude): **Para o arquiteto (0.2c, custo da varredura).** O critério "projeto novo" é avaliado em memória, uma vez por connector, e não entra no filtro SQL. Enquanto `IsNewProject` retornar `false`, cada execução com a flag ligada lê todas as linhas sem binding e não liga nenhuma. Quando o critério for definido, se der para expressá-lo como coluna do connector ou data da linha, vale colocá-lo no filtro do `Retrieve`.
- 2026-11-05 (Claude): **Diferença em relação ao Mailbox.** A proteção contra laço infinito ganhou um caso extra: o contador também sobe quando o filtro por data de criação não avança. Isso acontece com um lote cheio de linhas não processáveis que têm o mesmo `CreateDate`, por exemplo as criadas em lote pela descoberta. No Mailbox esse caso gira para sempre.
