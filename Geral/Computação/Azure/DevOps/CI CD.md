---
tags:
  - azure
  - computação
nivel: intermediario
---
---
# CI CD

CI/CD (Continuous Integration / Continuous Delivery) é o processo de automatizar build, testes e deploy de aplicações no [[Azure]].

## 📌 O que é

CI/CD é uma prática de DevOps que automatiza o ciclo de vida da aplicação:

- CI (Integração Contínua): build + testes automáticos
- CD (Entrega/Deploy Contínuo): publicação automática

---

## 📌 Para que serve

- Automatizar deploy
- Evitar erros manuais
- Garantir qualidade
- Aumentar velocidade de entrega

---

## 📌 Como funciona

Fluxo básico:

1. Você faz commit no código
2. Pipeline é acionado
3. Build é executado
4. Testes rodam
5. Deploy acontece automaticamente

---

## 📌 Ferramentas no Azure

- Azure DevOps
- GitHub Actions (muito comum)
- Azure CLI (scripts)

---

## 📌 Exemplo prático (com GitHub Actions)

### 🔹 1. Criar arquivo de pipeline

Dentro do seu projeto:

```bash
mkdir -p .github/workflows
```

Criar arquivo:

```
nano .github/workflows/deploy.yml
```

---

### 🔹 2. Pipeline básico para .NET + Azure

```
name: Deploy .NET to Azure

on:
  push:
    branches:
      - main

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Build
        run: dotnet build --configuration Release

      - name: Publish
        run: dotnet publish -c Release -o publish

      - name: Deploy to Azure
        uses: azure/webapps-deploy@v2
        with:
          app-name: <NOME_DO_APP_SERVICE>
          publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
```

---

### 🔹 3. Configurar segredo no GitHub

No repositório:

Settings → Secrets → Actions → New secret

Nome:

```
AZURE_PUBLISH_PROFILE
```

Valor:  
👉 Copiar do Azure (App Service → Get publish profile)

---

### 🔹 4. Resultado

Ao fazer push na `main`:

✔ build automático  
✔ deploy automático  
✔ aplicação atualizada

---

## 📌 Quando usar

- Qualquer projeto profissional
- Deploy frequente
- Times com mais de uma pessoa

---

## 📌 Quando NÃO usar

- Projetos extremamente simples
- Testes locais

---

## 🔗 Relacionado

- [[Azure]]
- [[Deploy no Azure]]
- [[Azure CLI]]

## 📌 Próximo passo

- [[Deploy no Azure]]