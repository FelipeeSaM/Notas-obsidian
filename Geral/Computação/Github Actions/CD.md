---
tags: [github]
nivel: basico
---

# CD

CD (Continuous Delivery) é o processo de preparar automaticamente seu código para ser entregue após o push no repositório.

## 📌 O que é

Se o CI verifica se o código está funcionando, o CD pega esse código e o deixa pronto para ser usado.

No contexto do GitHub:

👉 CD é o passo onde o código já testado é:
- empacotado
- preparado
- enviado para algum destino (ou salvo como artefato)

---

## 📌 Como funciona

Fluxo simples:

1. Você faz push
2. O CI roda (build + testes)
3. O CD roda depois do CI
4. O código é preparado para uso

---

## 📌 COMO FAZER (PASSO A PASSO)

## 🔹 1. Usar o mesmo arquivo do CI

No GitHub Actions, CI e CD geralmente ficam no mesmo `.yml`

---

## 🔹 2. Adicionar etapa de publicação

Exemplo completo:

```
name: CI/CD

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

      - name: Testes
        run: dotnet test

      - name: Publicar aplicação
        run: dotnet publish -c Release -o publish
```

## 🔹 3. Salvar o resultado (artefato)

Adicionar:

```
- name: Salvar artefato  
uses: actions/upload-artifact@v3  
with:  
name: app-publicada  
path: publish
```

---

## 📌 O que é um artefato?

É o resultado final do seu código pronto para uso.

Exemplo:

- arquivos compilados
- build pronto
- pacote da aplicação

---

## 📌 O que acontece agora?

Toda vez que você faz push:

👉 GitHub:

- compila o código
- roda testes
- gera versão pronta
- salva como artefato

---

## 📌 Exemplo simples

Você altera um código:

```
return "Hello";
```

👉 Push

O GitHub:

- compila
- testa
- gera versão final
- guarda para uso futuro

---

## 📌 Quando usar

- Preparar código para deploy
- Separar build de deploy
- Garantir consistência

---

## 📌 Quando NÃO usar

- Projetos extremamente simples (sem pipeline)

---

## 🔗 Relacionado

- [[Azure]]
- [[Computação]]
- [[CI CD]]

## 📌 Próximo passo

- [[Continuous Integration YML]]