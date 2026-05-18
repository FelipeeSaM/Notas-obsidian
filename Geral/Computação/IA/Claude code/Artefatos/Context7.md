---
tags:
  - IA
  - ClaudeCode
  - MCP
---
## O que é:
Context7 é um servidor MCP que entrega documentação atualizada e exemplos de código
especificos por versão diretamente nos seus prompts. Ele resolve um problema comum com
LLMs: dados de treinamento desatualizados que levam a APIs alucinadas e padrões de
códigos obsoletos. Basicamente obtém a documentação mais atualizada das linguagens e frameworks.

## Instalando:
Bash:
```
claude mcp add --transport http context7 https://mcp.context7.com/mcp
```

## Usando:
3. Exemplos de prompts para testar:
A forma mais simples de ativar o Context7 é adicionar "use context7" no final do seu
prompt:
	"Como configurar middleware no .net, use context7"
	"Use o context7 para implementar autenticação básica com FastAPI""

Se vocé ja sabe exatamente qual biblioteca quer usar, adicione o ID Context7 diretamente
no prompt. Assim o Context7 pula a etapa de resolução do nome e busca a documentação
diretamente. A sintaxe com barra ( /supabase/supabase ) diz ao Context7 exatamente
biblioteca carregar.

4. Leste de verificação rapida
Pega algo como "Mostre um exemplo de FastAPI com endpoints async". Se o Context7
estiver funcionando, a resposta vai incluir sintaxe atualizada e referências as versões
recentes.

