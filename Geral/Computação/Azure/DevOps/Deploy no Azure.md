---
tags: [azure]
nivel: intermediario
---
---
# Deploy no Azure

Deploy no [[Azure]] é o processo de publicar sua aplicação na nuvem.

## 📌 O que é

Enviar sua aplicação (API, site, etc.) para rodar em um serviço do Azure, como [[App Services]].

---

## 📌 Para que serve

- Tornar aplicação acessível online
- Executar código em ambiente real
- Disponibilizar APIs

---

## 📌 Formas de deploy

- Portal Azure (manual)
- Azure CLI
- GitHub Actions (CI/CD)
- FTP (menos comum)

---

# 📌 COMO FAZER (PASSO A PASSO COMPLETO)

## 🔹 1. Criar Resource Group

```
az group create \
  --name rg-api \
  --location brazilsouth
```

---
## 🔹 2. Criar App Service Plan

```
az appservice plan create \  --name meu-plano \  --resource-group rg-api \  --sku B1 \  --is-linux
```

---
## 🔹 3. Criar App Service

```
az webapp create \  --resource-group rg-api \  --plan meu-plano \  --name nome-unico-app \  --runtime "DOTNET|8.0"
```

---
## 🔹 4. Publicar aplicação local

No seu projeto .NET:

```
dotnet publish -c Release -o publish
```

---
## 🔹 5. Fazer deploy via ZIP

```
cd publishzip -r app.zip .
```

```
az webapp deployment source config-zip \  --resource-group rg-api \  --name nome-unico-app \  --src app.zip
```

---
## 🔹 6. Acessar aplicação

```
https://nome-unico-app.azurewebsites.net
```

---
# 📌 ALTERNATIVA: Deploy via GitHub Actions

(automatizado)

👉 já explicado em [[CI CD]]

---
## 📌 Boas práticas

- Nunca fazer deploy manual em produção
- Usar CI/CD
- Separar ambientes (dev, staging, prod)
- Usar variáveis seguras (Key Vault)

---
## 📌 Problemas comuns

- Nome do app não é único → erro
- Runtime errado (.NET versão)
- Falta de permissões

---
## 📌 Exemplo real

API .NET:

- sobe via pipeline
- conecta com Azure SQL
- usa Key Vault para segredos

## 🔗 Relacionado

- [[Azure]]
- [[App Services]]
- [[CI CD]]
- [[Azure CLI]]
