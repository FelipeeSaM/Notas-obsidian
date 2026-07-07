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
20. init container que roda apenas migrations e encerra o container
21. Factory Method define interface para criar objetos, deixando subclasses decidirem qual classe instanciar — uma factory por produto. Abstract Factory cria famílias de objetos relacionados sem especificar classes concretas — múltiplas factories para produtos relacionados. Abstract Factory é uma coleção coordenada de Factory Methods.
22. Utilizar o padrão/regra "leftmost prefix" para organizar as propriedades dos objetos e as colunas no banco
23. Ver a necessidade de implementar o rate limiting do redis com o padrão tocken bucket. Configurar no YARP E no redis (defesa em profundidade)
24. Não rodar migrateAsync()
25. Evitar falhas básicas de segurança: autenticação, autorização e controle de acesso
26. FALTA><<<><><><><><><><> **`AtualizarFcmToken`** — Command pequeno mas crítico. Toda vez que o Flutter inicializa, ele chama esse endpoint para garantir que o token do dispositivo está atualizado no banco. Sem isso o push notification não funciona.

Contexto e dica de entrevista

Factory Method: INotificationFactory com EmailNotificationFactory e SmsNotificationFactory — cada uma cria um tipo de notificação. Abstract Factory: IUIComponentFactory com WindowsFactory (cria Button, Checkbox, ScrollBar Windows-style) e MacFactory (cria os mesmos mas Mac-style) — família coesa de objetos. Em .NET, IServiceProvider é uma Abstract Factory gerenciada pelo runtime.