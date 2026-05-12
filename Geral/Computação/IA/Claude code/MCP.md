---
tags:
  - IA
  - ClaudeCode
---

# MCP (Model Context Protocol) — Cheat Sheet  
  
> Referência oficial:  
> https://code.claude.com/docs/en/mcp  
  
---  
  
# O que é MCP  
  
MCP (Model Context Protocol) é um protocolo aberto que permite conectar LLMs e agentes de IA a ferramentas externas.  
  
Com MCP, o Claude Code consegue acessar:  
  
- APIs  
- bancos de dados  
- serviços externos  
- arquivos  
- automações  
- ferramentas locais  
- sistemas corporativos  
  
O MCP transforma o Claude em um agente operacional capaz de executar ações reais.  
  
---  
  
# Arquitetura MCP  
  
O ecossistema MCP é composto por:  
  
| Componente | Função                 |
| ---------- | ---------------------- |
| MCP Client | consome ferramentas    |
| MCP Server | expõe ferramentas      |
| Resource   | fornece contexto/dados |
| Prompt     | comando reutilizável   |
  
Fluxo simplificado:    
Claude Code  
↓  
MCP Client  
↓  
MCP Server  
↓  
API / Banco / Serviço

# Tipos de Transporte

## HTTP (recomendado)

Ideal para serviços remotos/cloud.

```
claude mcp add --transport http notion https://mcp.notion.com/mcp
```

Com autenticação:

```
claude mcp add --transport http secure-api https://api.example.com/mcp \  --header "Authorization: Bearer TOKEN"
```

---

## STDIO

Executa processos locais.

```
claude mcp add --transport stdio myserver -- npx server
```

Com variáveis de ambiente:

```
claude mcp add --transport stdio \  --env KEY=value \  myserver -- python server.py
```

---

# Escopos MCP

|Scope|Disponibilidade|
|---|---|
|local|apenas no projeto atual|
|project|compartilhado via Git|
|user|disponível globalmente|

---
## local

Configuração privada.

```
claude mcp add --scope local ...
```

Arquivo:

```
~/.claude.json
```

---
## project

Compartilhado com o time.

```
claude mcp add --scope project ...
```

Arquivo:

```
.mcp.json
```

---
## user

Disponível em todos os projetos.

```
claude mcp add --scope user ...
```

---
# Principais Comandos MCP

## Adicionar servidor

### HTTP

```
claude mcp add --transport http NAME URL
```

### STDIO

```
claude mcp add --transport stdio NAME -- npx server
```

---
## Listar servidores

```
claude mcp list
```

---
## Ver detalhes

```
claude mcp get github
```

---
## Remover servidor

```
claude mcp remove github
```

---
## Painel MCP

Dentro do Claude Code:

```
/mcp
```

Usado para:

- autenticação OAuth
- status dos servidores
- diagnosticar problemas
- visualizar tools disponíveis

---

# Arquivos de Configuração

## ~/.claude.json

Configuração global/local do usuário.

---
## .mcp.json

Configuração compartilhada do projeto.

Exemplo:

```
{  "mcpServers": {    "shared-server": {      "command": "/path/server",      "args": [],      "env": {}    }  }}
```

---
# OAuth e Autenticação

Fluxo padrão:

1. adicionar servidor
2. abrir `/mcp`
3. autenticar no browser

Exemplo:

```
claude mcp add --transport http sentry https://mcp.sentry.dev/mcp
```

---
# Claude Code como MCP Server

Claude também pode atuar como servidor MCP.

## Inicializar

```
claude mcp serve
```

---
## Exemplo Claude Desktop

```
{  "mcpServers": {    "claude-code": {      "type": "stdio",      "command": "claude",      "args": ["mcp", "serve"],      "env": {}    }  }}
```

---
# MCP Resources

Resources permitem acessar contexto estruturado.

Exemplos:

- arquivos
- JSON
- schemas
- documentação
- datasets

---
## Referência via @

```
@github:issue://123
```

Exemplo:

```
Analyze @github:issue://123
```

Benefícios:

- menos tokens
- contexto sob demanda
- melhor performance

---
# MCP Prompts

Prompts MCP viram comandos slash reutilizáveis.

Formato:

```
/mcp__server__prompt
```

Exemplos:

```
/mcp__github__list_prs
```

Com argumentos:

```
/mcp__jira__create_issue "Bug login" high
```

---
# Tool Search

Tool Search reduz consumo de contexto.

Em vez de carregar todas as tools no início, Claude:

1. carrega apenas nomes
2. busca schemas sob demanda
3. injeta somente o necessário

---
## Ativar

```
ENABLE_TOOL_SEARCH=auto
```

Benefícios:

- menos tokens
- menos contexto
- melhor escalabilidade

---
# Limite de Saída MCP

Padrão:

```
10000 tokens
```

Aumentar:

```
MAX_MCP_OUTPUT_TOKENS=50000
```

---
# Variáveis de Ambiente Importantes

|Variável|Função|
|---|---|
|ENABLE_TOOL_SEARCH|ativa tool search|
|MAX_MCP_OUTPUT_TOKENS|aumenta saída|
|ANTHROPIC_BASE_URL|endpoint custom|
|CLAUDE_PLUGIN_ROOT|root de plugins|

---
# Exemplo PostgreSQL

```
claude mcp add --transport stdio db -- \  npx -y @bytebase/dbhub \  --dsn "postgresql://readonly:pass@prod.db.com:5432/analytics"
```

Exemplos de perguntas:

```
What's our total revenue this month?
```

```
Show me the schema for orders
```

---
# Estrutura Recomendada de Projeto

```
project/
├── .claude/
│   ├── commands/
│   ├── skills/
│   ├── agents/
│   └── settings.json
├── .mcp.json
├── CLAUDE.md
└── src/
```

---
# Troubleshooting

## Reautenticar MCP

```
/mcp
```

---
## Verificar PATH

```
which claude
```

---
## Reset approvals

```
claude mcp reset-project-choices
```

---
# Boas Práticas

## Prefira HTTP

Mais compatível e moderno.

---
## Use `project` para times

Permite versionamento do `.mcp.json`.

---
## Use `user` para ferramentas pessoais

Exemplos:

- GitHub
- Slack
- Notion

---
## Ative Tool Search

Evita excesso de contexto.

```
ENABLE_TOOL_SEARCH=auto
```

---
## Prefira Resources ao copiar/colar

Mais eficiente em tokens.

---
# Resumo Rápido

## Adicionar servidor

```
claude mcp add --transport http NAME URL
```

---
## Listar servidores

```
claude mcp list
```

---
## Abrir painel MCP

```
/mcp
```

---
## Executar Claude como MCP Server

```
claude mcp serve
```