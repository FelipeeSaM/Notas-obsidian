Status: ready-for-human
Blocked by: 03, 04, 05, 06, 07, 08

# 10: Validação ponta a ponta, cenário retroativo e TestRail (B5 + B8)

## O que construir

Validação manual da Parte B num ambiente com a feature flag ligada e o critério "projeto novo" satisfeito, com passo a passo e prints, e os casos cadastrados no TestRail.

## Critérios de aceite

- [ ] **Retroativo:** pessoa com UMB aplicada num projeto Mailbox/OneDrive é reconhecida como `Active` ao ser adicionada a um projeto PCH novo, sem nova licença
- [ ] **Fluxo do épico:** adicionar pelo endereço original → licença "Yes" → ajustar o source address para a UPN → binding mantido
- [ ] **Adicionado pela UPN:** pessoa adicionada direto pela UPN (diferente do primary email address) gera CEU novo sem UMB; documentar o procedimento correto
- [ ] **Troca de domínio e de prefixo:** binding mantido (tela e SDK)
- [ ] **Status:** aplicar, revogar e expirar a UMB refletem na conversation line item
- [ ] **Revogação:** falha após Verify Credentials, após Full e após 24 horas; funciona dentro de 24 horas sem job; pessoa só de Mailbox continua como hoje
- [ ] **Azure:** CEU e data do binding visíveis na tabela Azure do PCH (se o ticket 08 continuar no escopo)
- [ ] **Projeto antigo:** nenhuma linha de projeto PCH existente ganha binding
- [ ] Casos cadastrados no TestRail
- [ ] Nota de release registrada: ligar a flag da Parte B antes ou junto com a flag de bloqueio da Parte A

## Seams under test

- Nenhuma automatizada (validação manual; os testes automatizados estão nos tickets 01 a 08).

## Notas

- Os workers do PCH (Platform API) não estão no repositório; a migração real pode exigir ambiente da BitTitan.
- A etapa final em conjunto com o Leonardo (seção 6 do plano) não faz parte deste ticket.

## Comments
