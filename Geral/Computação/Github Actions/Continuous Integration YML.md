---
tags:
  - github
  - computação
nivel: basico
---
---
# Continuous Integration YML

Arquivo `.yml` usado pelo GitHub Actions para definir o que acontece automaticamente no seu projeto.

## 📌 O que é

É um arquivo de configuração que diz para o GitHub:

👉 "Quando algo acontecer, execute esses passos"

Ele fica em:

```
.github/workflows/
```

Exemplo:

```
.github/workflows/ci.yml
```

---

## 📌 Como funciona

O GitHub lê esse arquivo e executa tudo que está definido nele.

Fluxo:

1. Evento acontece (ex: push)
2. GitHub lê o `.yml`
3. Cria uma máquina virtual
4. Executa os passos definidos

---

## 📌 Estrutura do arquivo

```
name: Nome do pipeline

on:
  evento

jobs:
  nome-do-job:
    runs-on: sistema

    steps:
      - passos
```

---

## 📌 Explicando cada parte

### 🔹 name

Nome do pipeline

```
name: CI
```

---

### 🔹 on

Define quando o pipeline roda

```
on:
  push:
    branches:
      - main
```

👉 Roda quando fizer push na main

---

### 🔹 jobs

Define o que será executado

```
jobs:
  build:
```

---

### 🔹 runs-on

Define o sistema

```
runs-on:
 ubuntu-latest
```

---

### 🔹 steps

Passos que serão executados

---

## 📌 Exemplo completo

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

      - name: Build
        run: dotnet build

      - name: Testar
        run: dotnet test
```

---

## 📌 Tipos de passos

### 🔹 uses

Usa uma ação pronta

```
uses:
 actions/checkout@v3
```

---

### 🔹 run

Executa comando

```
run: dotnet build
```

---

## 📌 Como escrever um

Passo a passo:

1. Criar pasta `.github/workflows`
2. Criar arquivo `.yml`
3. Definir evento (`on`)
4. Criar job (`jobs`)
5. Definir passos (`steps`)
6. Fazer commit

---

## 📌 Boas práticas

- Nomear bem os passos
- Separar build e deploy
- Usar versões fixas de actions
- Evitar comandos desnecessários

---

## 📌 Erros comuns

- YAML mal indentado
- Nome errado de branch
- esquecer `uses: checkout`

---

## 📌 Dica importante

YAML é sensível a espaços:

❌ errado:

```
jobs:
build:
```

✔ correto:

```
jobs:
  build:
```

---

## 🔗 Relacionado

- [[Azure]]
- [[CI CD]]
- [[CD]]

## 📌 Próximo passo

- [[Deploy no Azure]]