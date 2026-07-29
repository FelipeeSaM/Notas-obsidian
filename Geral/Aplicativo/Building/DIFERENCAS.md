# AplicativoPet — Development-Ready vs Production-Ready

## Infraestrutura — containers viram serviços gerenciados

| Componente | Development | Production |
|---|---|---|
| PostgreSQL + PostGIS | Container local com volume Podman | Azure Database for PostgreSQL Flexible Server (extensão PostGIS nativa) |
| Redis | Container local | Azure Cache for Redis ou Upstash |
| RabbitMQ | Container local | Azure Service Bus (MassTransit tem transport nativo — só muda a string de conexão) |
| Elasticsearch / Kibana | Containers locais | Elastic Cloud ou Azure Monitor + Application Insights |
| YARP Gateway | localhost | Container no Azure Container Apps |
| API | localhost | Azure Container Apps (escala automática, inclusive a zero) |

Serviços gerenciados eliminam os problemas de persistência de volume, versão de imagem, reinicialização de container e capacidade — tudo isso vira SLA do provedor.

---

## Aspire — só roda em desenvolvimento

O `AppHost` não vai para produção. Em produção, ele é usado para **gerar manifests de deploy**:

```bash
azd init   # detecta o AppHost e cria azure.yaml
azd up     # provisiona infraestrutura + faz deploy das apps no Azure
```

O `azd` lê o `AppHost`, substitui containers locais pelos recursos Azure equivalentes e cria todo o ambiente. O `ServiceDefaults` (OpenTelemetry, health checks) continua funcionando — agora exportando para Azure Monitor em vez do dashboard local.

---

## Secrets — user secrets viram Key Vault

| Aspecto | Development | Production |
|---|---|---|
| Segredos da app | `dotnet user-secrets` + `.env` | Azure Key Vault |
| JWT key | User secrets do AppHost | Key Vault secret referenciado via Managed Identity |
| Senhas do banco | Parâmetros Aspire | Connection string injetada pelo Container Apps direto do Key Vault |
| Acesso aos segredos | Leitura local | Sem senha no código — identidade gerenciada do serviço |

---

## Migrations — nunca no startup em produção 

O `MigrateAsync()` no startup causa problemas em produção:
- **Race condition** se múltiplas instâncias sobem simultaneamente
- **Deploy travado** se a migration demora
- **Risco de corrupção** em rollback parcial

### Solução: migration job no pipeline CI/CD

```bash
# Antes do deploy da API, no pipeline:
dotnet ef migrations bundle --project AplicativoPet.Api -o migrate
./migrate --connection "$CONNECTION_STRING"
```

O Azure Container Apps suporta `init containers` para executar a migration antes de subir as instâncias da API.

---

## HTTPS — obrigatório em produção

| Aspecto | Development | Production |
|---|---|---|
| TLS | Desabilitado (comentado no Program.cs) | Terminado no load balancer do Azure Container Apps |
| Certificado | N/A | Gerenciado automaticamente pelo Azure |
| `UseHttpsRedirection` | Comentado | Descomentado |
| Comunicação interna Gateway → API | HTTP | HTTP dentro da rede privada do Container Apps |

---

## Arquivos que mudam de dev para prod

| Arquivo | Development | Production |
|---|---|---|
| `appsettings.json` | Connection strings em `appsettings.Development.json` | Variáveis de ambiente injetadas pelo Container Apps + Key Vault |
| `Program.cs` | `UseSerilog` comentado | Ativo, sink → Elastic Cloud ou Azure Monitor |
| `Program.cs` | `UseHttpsRedirection` comentado | Ativo |
| `MensageriaConfiguracao.cs` | URI do RabbitMQ local | URI do Azure Service Bus (MassTransit abstrai o transport) |
| `AppHost.cs` | Containers locais com volumes | Não existe em produção |

---

## CI/CD — pipeline obrigatório

```
Push para main
  → GitHub Actions
    → dotnet build
    → dotnet test
    → docker build API e Gateway
    → push para Azure Container Registry
    → rodar migration bundle contra o banco de produção
    → deploy das novas imagens no Container Apps
```

---

## O que já é production-ready hoje

Estas partes **não mudam** ao ir para produção:

- Arquitetura VSA + CQRS
- JWT + refresh token (só habilita HTTPS)
- YARP Gateway com rate limiting e autenticação
- MassTransit com Outbox Pattern (transport muda, código não)
- `Result<T>` e FluentValidation
- Serilog com ECS (já estruturado para qualquer sink)

---

## Sequência prática ao ir para produção

1. Descomentar `UseHttpsRedirection` e `UseSerilog` no `Program.cs`
2. Criar migration bundle no pipeline e remover `MigrateAsync()` do startup
3. Configurar CORS com a origem do app Flutter publicado
4. Trocar RabbitMQ por Azure Service Bus na string de conexão
5. Criar Key Vault e migrar os secrets
6. Rodar `azd init` + `azd up` a partir do AppHost para provisionar o Azure Container Apps
7. Configurar GitHub Actions com `azd deploy` no merge para `main`
