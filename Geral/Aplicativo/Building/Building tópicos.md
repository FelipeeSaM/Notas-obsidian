---
tags:
  - "#Aplicativo"
  - Features
---
1. YARP para OAUTH além do balanceador de carga (tokens JWT) 
2. Checar DDOs
3. Balanço de carga
4. Google analytcs
5. Trust boudaries
6. Perigos (verificar o STRIDE (security) no wikipedia)
7. Vulnerabilidades: checar no OWASP
8. Tópicos a serem considerados na segurança: Encriptação, autenticação, controle de acesso, audição de logs, rate limiting
9. The 80-20 rule 
10. Adicionar health-check das APIs (watchdogs?)
11. Diferentes jeitos de identificar diferentes causas de problemas (erros, alertas etc.) usando elastic e o serilog sink
12. Fazer um schedule/loop para saber quando os frameworks e bibliotecas foram atualizadas em comparação as que são usadas no projeto e me avisar
13. Código simples, abstração
14. general properties like security, reliability, compliance, scalability, compatibility and
maintainability)
15. Auditoria
16. Sugestão: API gateway + vertical slice + cache-aside + polly + circuit breaker + elastic stack
17. logar tentativas de acesso
18. Usar open telemetry com zipkin
19. Rate limiting por ip com resposta 429