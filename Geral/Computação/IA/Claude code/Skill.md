---
tags:
  - IA
  - ClaudeCode
  - computação
---

| Recurso     | O que faz                        | Quando usar                         |
| ----------- | -------------------------------- | ----------------------------------- |
| CLAUDE.md   | Contexto persistente do projeto  | Regras gerais, estilo, convenções   |
| Skills      | instruções sob demanda + scripts | Tarefas especializadas e repetiveis |
| MCP Servers | Conexão com agentes externos     | APIs, bancos de dados, ferramentas  |
| Sub-agents  | Agentes especializados delegados | Tarefas paralelas e isoladas        |

## Anatomia
### Fontmatter:
Exemplo:
```
name: alguma-skill
description: essa é a skill para alguma coisa específica
agent: algum-agent-ex-dotnet
disable-model-invocation: false

---

o prompt em si que faz
```

Comandos:

| Campo                    | O que faz                           | Comentario                                      |
| ------------------------ | ----------------------------------- | ----------------------------------------------- |
| name                     | Nome da skill e do /comando         |                                                 |
| description              | Quando usar. Max 250 chars visiveis |                                                 |
| disable-model-invocation | true = so vocé invoca manualmente   | Com true, o claude não invoca isso              |
| user-invocable           | false = so o Claude pode usar       | Com false, você não invoca, só o claude         |
| allowed-tools            | Ferramentas liberadas sem permissao |                                                 |
| context                  | fork = roda em sub-agente isolado   | Importante para referenciar dentro de uma skill |
| agent                    | Qual sub-agente (Explore, Plan...)  |                                                 |
| effort                   | Nivel: low, medium, high, max       |                                                 |
| paths                    | Globs para ativação condicional     |                                                 |
| hooks                    | Hooks do ciclo de vida da skill     |                                                 |


## Criando uma skill
- Criar o diretório. Ex: ~/.claude/skills/alguma-skill
- Escrever a skill.md. Ex: ~/.claude/skills/alguma-skill/skill.md

## Invocando a skill:
/alguma-skill URL aleatória

o que vem depois da skill vira um argumento, que é referenciado com $ARGUMENTS dentro da skill.md

## Agentes já disponíveis do claude:

| Explore         | read-only, só lê         |
| --------------- | ------------------------ |
| Plan            | planejament e análise    |
| general-purpose | Acesso completo (padrão) |
| custom          | chama algum agente meu   |
