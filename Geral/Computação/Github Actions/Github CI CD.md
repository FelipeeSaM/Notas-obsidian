---
tags:
  - github
nivel: basico
---
---
# CI CD

CI/CD é um processo automático que pega seu código e faz tudo sozinho: compila, testa e publica.

## 📌 O que é

CI/CD é como um robô que trabalha para você toda vez que você faz um commit no código.

Imagine assim:

👉 Você escreve código  
👉 Dá um push no Git  
👉 Um robô pega esse código e:

- monta o projeto (build)
- verifica se funciona
- prepara para deploy

Esse robô é o **GitHub Actions**.

---

## 📌 O que é CI (Continuous Integration)

CI é a parte onde o código é verificado automaticamente.

É como se fosse:

👉 "Vamos ver se o código não está quebrado"

---

## 📌 COMO FAZER (PASSO A PASSO SIMPLES)

### 🔹 1. Ter um repositório no GitHub

Seu projeto precisa estar no github

---

### 🔹 2. Criar pasta de automação

Dentro do seu projeto:

```
mkdir -p .github/workflows
```

### 🔹 3. Criar arquivo de CI

```
nano .github/workflows/ci.yml
```

---
### 🔹 4. Escrever o CI (o robô)

```
name: CI

on:
  push:
    branches:
      - main

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
      - name: Baixar código
        uses: actions/checkout@v3

      - name: Instalar .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Compilar
        run: dotnet build

      - name: Testar
        run: dotnet test
```

---

### 🔹 5. Fazer push

```
git add .
git commit -m "adicionando CI"
git push
```

---

## 📌 O que acontece agora?

Quando você fizer push:

👉 GitHub automaticamente:

- pega seu código
- roda build
- roda testes

Se algo quebrar → você já sabe na hora

---

## 📌 Exemplo simples

Você erra um código:

❌ build falha  
✔ você corrige antes de ir pra produção

---

## 📌 Quando usar

👉 Sempre  
Todo projeto deve ter CI

---

## 🔗 Relacionado

- [[Azure]]
- [[Computação]]
- [[Deploy no Azure]]

## 📌 Próximo passo

- [[CD]]