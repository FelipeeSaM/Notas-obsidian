---
tags:
  - IA
  - computação
---
## Instalar:
```
/plugin install ralph-loop@claude-plugins-official
```

## Usar:
```
/ralph-loop:ralph-loop "Improve project, add user management, kanban boards..."
```

O **Loop Ralph Wiggum** ==é uma técnica autônoma de agente de IA (frequentemente implementada com Claude Code) que executa o mesmo comando ou tarefa em um loop contínuo até que um objetivo seja alcançado==. Nomeada em homenagem ao personagem de Os Simpsons, a técnica foca na "persistência alegre" de continuar tentando, falhando e aprendendo iterativamente.
Como Funciona

- **Reinício de Contexto:** Cada iteração começa com uma "janela de contexto" nova para evitar a degradação de desempenho (que ocorre quando a IA acumula muita informação antiga).
- **Progresso do Estado:** O estado do projeto é mantido por meio de arquivos de log e _commits_ de controle de versão (como o Git) em vez de na memória da IA.
- **Iteração:** A IA verifica as pendências, implementa código, roda testes ou linters e comita as alterações. O processo se repete ciclicamente até que a IA emita uma condição de conclusão.

Vantagens Principais
- **Evita desistência precoce:** Impede que o modelo pare de gerar código antes de terminar o projeto.
- **Autonomia:** Permite que desenvolvedores deixem o agente trabalhando em tarefas longas sem intervenção manual

Ferramentas e Configuração

A técnica pode ser executada com um simples script Bash `while true` ou usando o plugin oficial do Ralph no Anthropic Claude Code. Ele é ideal para tarefas bem definidas e guiadas por testes, embora exija supervisão para projetos complexos que exijam julgamento humano aprofundado.

Prompt? da vídeo aula:
```
Técnica Ralph Wiggum Loop: claude trabalha em tarefas repetidamente, revisando o trabalho anterior até sua conclusão, dividindo em etapas. O ciclo é fazer -> revisar -> corrigir -> repetir até o critério de conclusão ser atingido. Me sugira três aplicações para este projeto (ou módulo)
```