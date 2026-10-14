---
tags:
  - github
  - computação
---
Vou te explicar isso com bastante calma, passo a passo, como se você nunca tivesse feito isso antes. Antes de mais nada, dei uma olhada no seu repositório (FelipeeSaM/SincronizadorPdf) para já te contextualizar com o que existe lá hoje.

O que é uma "release" (pensando bem simples)

Imagina que seu código é uma caixa de brinquedo que você vai montando, peça por peça, todos os dias (isso são os commits). Uma release é o momento em que você para, olha pra caixa, diz "pronto, está bom assim!", cola um adesivo com um número nela (ex: v1.1.0) e entrega essa caixa pra alguém usar. É um "retrato congelado" do projeto num ponto específico, junto com uma etiqueta explicando o que tem de novo dentro dela.

No GitHub, uma release sempre se apoia em uma tag do Git — a tag é o adesivo, a release é a caixa bonita com o adesivo colado e uma cartinha explicando o conteúdo.

O que eu encontrei no seu repositório agora

- Já existe uma tag e uma release publicada: v1.0.0.
- Também existe uma release rascunho (Draft) vazia, sem nome e sem tag, criada em 2026-10-13. Parece ter sido deixada pela metade (alguém clicou em "Draft a new release" no site e não terminou). Não toquei nela — se quiser, eu apago depois, mas só com seu ok.

Agora vamos ao passo a passo para criar a próxima release (por exemplo, v1.1.0).

---

Passo 1 — Decidir o número da nova versão

O padrão mais usado é o SemVer: MAJOR.MINOR.PATCH (ex: 1.2.3).

Pensa nos 3 números como 3 "rodinhas" de um contador:
- PATCH (o último número, ex: 1.0.1): você só arrumou um defeito (bug), sem mudar nada na forma de usar. Gira essa rodinha.
- MINOR (o do meio, ex: 1.1.0): você adicionou uma funcionalidade nova, mas tudo que já funcionava continua funcionando igual. Gira essa rodinha e zera a de PATCH.
- MAJOR (o primeiro, ex: 2.0.0): você mudou algo que quebra o jeito antigo de usar (ex: mudou um formato de arquivo, uma API). Gira essa rodinha e zera as outras duas.

Olhando seus commits recentes (mDNS, pareamento, empacotamento do backend), isso são funcionalidades novas sem quebrar nada antigo → provavelmente a próxima seria v1.1.0.

Passo 2 — Ter certeza que o código está pronto, na branch certa

A release normalmente é feita a partir da main (não da dev), depois que tudo que você quer incluir já foi mesclado (merge) nela.

git checkout main
git pull origin main

Confirma visualmente que os commits que você quer estão lá (git log --oneline -10).

Passo 3 — Criar a "etiqueta" (a tag)

A tag marca exatamente qual commit da main é o "v1.1.0".

git tag -a v1.1.0 -m "v1.1.0"

- -a = tag anotada (guarda data, autor, mensagem — é a forma recomendada, melhor que uma tag "simples").
- -m = a mensagem curta dessa etiqueta.

Passo 4 — Enviar a tag pro GitHub

Até aqui a tag só existe no seu computador. Precisa mandar ela pra nuvem:

git push origin v1.1.0

Agora o GitHub já sabe que existe esse "adesivo" v1.1.0 em cima de um commit específico.

Passo 5 — Criar a release de fato, usando a tag

Como seu projeto já usa a ferramenta gh (GitHub CLI) para issues, o mais natural é criar a release pelo mesmo caminho, direto do terminal:

gh release create v1.1.0 --title "v1.1.0" --generate-notes

O que cada pedaço faz:
- v1.1.0 → diz qual tag usar (a que você acabou de enviar).
- --title → o nome que aparece bem grande na página da release.
- --generate-notes → pede pro GitHub escrever automaticamente a lista de mudanças, olhando todos os Pull Requests mesclados desde a última release (v1.0.0). É tipo mágica: ele lê o histórico e monta a "cartinha" de conteúdo pra você.

Se preferir escrever você mesmo as notas em vez de gerar automático:

gh release create v1.1.0 --title "v1.1.0" --notes "- Descoberta mDNS automática\n- Empacotamento self-contained do backend"

E se tiver um arquivo pra anexar (ex: o .zip do backend, ou um .apk)?

Dá pra anexar arquivos direto na criação — eles ficam disponíveis pra download na página da release:

gh release create v1.1.0 --title "v1.1.0" --generate-notes ./build/backend-linux.zip

Passo 6 (alternativa) — Fazer isso pelo site, sem terminal

Se preferir no navegador, o caminho visual é:

1. Entra no repositório no site do GitHub.
2. No menu lateral direito, clica em "Releases".
3. Clica no botão verde "Draft a new release".
4. Em "Choose a tag", digita v1.1.0 e clica em "Create new tag: v1.1.0 on publish" (o GitHub cria a tag pra você automaticamente nesse fluxo, não precisa ter feito os passos 3 e 4 na mão).
5. Escolhe a branch main no campo "Target".
6. Preenche o título (ex: v1.1.0) e a descrição (pode clicar em "Generate release notes", que é o mesmo recurso do --generate-notes).
7. Se quiser anexar arquivos, arrasta eles pra caixa de "Attach binaries" lá embaixo.
8. Se ainda não está pronto pra divulgar, marca "Set as a pre-release" (versão de teste) ou salva como rascunho sem clicar em publicar.
9. Clica no botão verde "Publish release".

Passo 7 — Confirmar que deu certo

gh release view v1.1.0

Isso mostra o título, as notas e os arquivos anexados — é a forma de checar que ficou do jeito que vocém.