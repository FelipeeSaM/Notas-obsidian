---
tags:
  - IA
  - ClaudeCode
---
Prompts para avaliar o débito técnico de funções, classes ou projetos: 

### Arquivo unico:
cat src/auth/validators.py | claude
“Analise este arquivo: code smells, complexidade, duplicação e oportunidades de refatoração. Priorize por impacto (alto/medio/baixo)"

### Nivel 2 Modulo / pasta
find src/auth -name "*.py" | xargs cat | claude
"Mapeie o debito tecnico do módulo de autenticação. Identifique acoplamentos, responsabilidades e problemas arquiteturais."

### Projeto inteiro
git log --name-only --format=""_| sort |
uniq -c | sort -rn | head -3@ | claude
“Quais áreas do projeto acumulam mais mudanças? Isso indica débito técnico? Priorize onde refatorar."