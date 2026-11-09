Status: done
Blocked by: 03

# 06: Binding mantido na edição de endereço (B2)

## O que construir

Editar o source address ou o destination address de uma conversation line item, seja só o domínio, o endereço inteiro ou o prefixo, pela API REST ou pelo SDK, mantém o CEU e a linha. É o que permite o fluxo do épico: adicionar pelo endereço original, ver a licença "Yes" e só então ajustar para a UPN (ADR-0002).

## Critérios de aceite

- [ ] Event handler sai cedo quando a linha já tem data de binding (ticket 03), sem olhar endereço
- [ ] Update REST não apaga o CEU (ticket 01, ADR-0003)
- [ ] Objeto SDK restaura o CEU e a data do binding quando o update chega com eles vazios, espelhando o objeto SDK do Mailbox
- [ ] Log `AddressChangedBindingKept` (Information) no formato da spec
- [ ] A trava de edição de linha migrada, hoje comentada, fica como está (observação para o Will)
- [ ] Testes de integração escritos e compilados, não executados

## Seams under test

- integration: `ConversationLineItemsService.Update` mantém o `CustomerEndUserId` ao trocar só o domínio do source address
- integration: `ConversationLineItemsService.Update` mantém o `CustomerEndUserId` ao trocar o prefixo do source address
- integration: `ConversationLineItem` (SDK) `Update` restaura `CustomerEndUserId` e `MspcSynchronizationDate` quando chegam vazios

## Referências

- ADR-0002 (binding sobrevive a qualquer troca de endereço; arquiteto só confirma a frase do épico), ADR-0003

## Comments
