---
tags:
  - IA
  - ClaudeCode
---
Consiste em 6 passos:
1. Requisito (entender o que quer implementar)
2. Pedir os testes para o claude
3. Rodar (ciclo RED) e fazer falhar
4. Pedir o código, pode ser algo mínimo e depois desenvolve a funcionalidade
5. Rodar e confirmar que passou (GREEN)
6. Refatorar

Exemplo de prompt da fase 2:
```
Escreva testes pytest para calculate_discount(price, discount_pct).
Casos: normal, 0%, 100%, negativo, acima de 100%, preco zero. Nao
impLemente ainda.
```

Exemplo de prompt da fase 3:
```
Os testes em test_discount.py estado falhando. Implemente
calculate_discount(price, discount_pct) em pricing.py. Fa¢a APENAS
os testes passarem — sem extras.
```

Tipos de testes:

Testes unitarios
Testa uma funcção isolada. Claude gera casos abrangentes quando você lista os cenários.
"Testes para parse_date(s): ISO 8601, DD/MM/YYYY, invdalido, None.”

Testes de integracao
Testa interações entre módulos. Especifique mocks e fixtures necessarios.
"Teste de integração para UserService.create_user() com mock do banco."

Testes de edge cases
Claude antecipa casos extremos quando solicitado explicitamente.
"Liste todos os edge cases para Validate_cpf() e escreva um teste para cada."

Testes de erro
Verifica que exceções sao lançadas corretamente nas condições certas.
"Confirme que AuthError é Lançado: token expirado, invdlido, ausente."

Testes parametrizados
Claude gera pytest.mark.parametrize automaticamente para casos similares.
"Use parametrize para format_currency() com BRL, USD, EUR, valores negativos."