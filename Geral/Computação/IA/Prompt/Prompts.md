---
tags:
  - IA
---
Explorar, planejar, implementar, commitar.
Mapear impactos e riscos.

## como pedir muda tudo

Anatomia, técnicas, prompts prontos e os erros que separam quem pede de quem obtém. Especificidade vence verbosidade.

---
# 01 Anatomia de um prompt forte

## Seis blocos. Sempre os mesmos.

Prompt fraco = resposta genérica. Prompt forte tem papel, contexto, tarefa, formato, restrições e exemplo. Você não precisa dos seis sempre — mas saber quais omitir é o que separa o iniciante do operador.

### `prompt-template.md`

```md
# Papel → quem o Claude deve ser
# Contexto → o que ele precisa saber antes
# Tarefa → o que exatamente você quer
# Formato → como a resposta deve vir
# Restrições → o que ele NÃO deve fazer
# Exemplo → 1 exemplo vale mais que 5 parágrafos
```

# 02 5 técnicas que mudam o jogo

## O arsenal básico

---

## Chain-of-Thought — force o raciocínio antes da resposta

- "Antes de responder, pense passo a passo."
- "Liste suas premissas antes de concluir."
- "Raciocine em voz alta e dê a resposta no final."

→ Use em:

- decisões complexas
- debugging
- análises com trade-offs

---

## Few-Shot — mostre, não descreva

### Exemplo

|Input|Classificação|
|---|---|
|"O relatório não carrega ao filtrar por data"|Bug|
|"Como exporto para Excel?"|Dúvida|
|"Poderia ter modo escuro"|Feature Request|

Agora classifique: `[seus casos]`

→ 1 a 3 exemplos transformam a saída.

---

## Prompts negativos — tão úteis quanto os positivos

- Sem: "revolucionário", "inovador", "robusto"
- Sem: resumir o que eu já disse antes de responder
- Sem: disclaimer no início — vá direto ao ponto
- Sem: inventar fontes — se não souber, diga

---

## XML pra contexto complexo — separe dado de instrução

```
<contexto>quem você é e o que está acontecendo</contexto>  
<dados>números, textos, inputs</dados>  
<tarefa>o que você quer que ele faça</tarefa>  
<formato>como quer a resposta</formato>
```

---

## Iteração — o melhor prompt é o refinado, não o primeiro

- "Encurte para metade."
- "Mais formal, menos coloquial."
- "Foque só no item 2, desenvolva mais."
- "Agora faça uma versão pra público não-técnico."

→ Refine na mesma conversa. Não recomece.

---

# 03 Prompts prontos pra colar

## Quatro templates que cabem em qualquer fluxo

---

### `análise-de-dados.md`

```
Analise em três camadas:1. O que os números dizem (fatos)2. O que sugerem (interpretação)3. O que eu deveria fazer (ação)[cole os dados]
```

---

### `revisão-de-código.md`

```
Revise com foco em:- bugs- performance- legibilidade- segurança
Pra cada problema:- explique o risco- mostre a correção- classifique como crítico, médio ou cosmético[cole o código]
```

---

### `tomada-de-decisão.md`

```
Antes de recomendar:
1. Que informações faltam?
2. Quais critérios mais importam?
3. Qual sua recomendação e por quê?4. Qual o principal risco da opção recomendada?
```

---

### `resumo-crítico.md`

```
Entregue:
1. TL;DR — 3 linhas, o que importa de verdade
2. Decisões que preciso tomar
3. Perguntas que o doc levanta mas não responde
   NÃO faça resumo linear. Quero leitura crítica.[cole o documento]
```

---

# 04 Erros que todo mundo comete

## Pare de fazer isso

|Erro|Problema|Correção|
|---|---|---|
|Tudo de uma vez|Pedir resposta gigante num prompt só.|Quebre em etapas e valide cada uma antes da próxima.|
|Contexto implícito|Achar que ele "já sabe" do que você fala.|Cole o que um colega novo precisaria saber pra responder.|
|Opinião sem critério|"Qual é melhor?" sem definir os eixos.|Liste critérios de avaliação antes de pedir o veredito.|
|Não pedir a fonte|Aceitar dados sem rastreabilidade.|Adicione: "indique a fonte ou diga que não tem certeza".|
|Sessão eterna|Continuar conversa de 80 mensagens trocando de assunto.|Use /compact ou abra nova conversa quando virar de tema.|

---

# 05 Técnica certa pra cada situação

## Quando usar o quê

|Situação|Técnica|
|---|---|
|Problema complexo|Chain-of-Thought. Force ele a pensar passo a passo antes de concluir.|
|Tom ou formato específico|Few-Shot com exemplos. Mostre 1-3 amostras do que você quer.|
|Muitos inputs diferentes|XML pra estruturar. Tags separam dado de instrução com clareza.|
|Resposta genérica demais|Negativos + persona específica. Diga o que NÃO fazer e quem ele deve ser.|
|Resultado bom mas não perfeito|Iteração na mesma conversa. Refine, não recomece. Conversa > prompt único.|
|Tarefa repetitiva|Salve como template em Project. Skill ou CLAUDE.md, dependendo da ferramenta.|