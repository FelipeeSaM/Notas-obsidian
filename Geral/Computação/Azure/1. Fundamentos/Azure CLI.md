---
tags: [azure]  
nivel: basico  
---
# Azure CLI

Azure CLI é a ferramenta de linha de comando do [[Azure]] para gerenciar recursos.

## 📌 O que é
Uma interface baseada em terminal que permite criar, configurar e gerenciar recursos no Azure através de comandos.

## 📌 Para que serve
- Criar recursos automaticamente
- Gerenciar infraestrutura
- Automatizar tarefas
- Integrar com scripts e pipelines

## 📌 Conceito chave
Tudo que você faz no Portal também pode ser feito via CLI (e geralmente de forma mais rápida e automatizável).

---

## 📌 Como usar

### 🔹 Login
```bash
az login
```
### 🔹 Ver assinaturas

```
az account list --output table
```

---
### 🔹 Selecionar assinatura

```
az account set --subscription "<nome-ou-id>"
```

---
### 🔹 Criar Resource Group

```
az group create \  --name meu-resource-group \  --location brazilsouth
```

---
### 🔹 Listar recursos

```
az resource list --output table
```

---

## 📌 Exemplo prático

Criar um Resource Group e depois um recurso:

```
az group create --name rg-api --location brazilsouth
```

👉 Depois você pode criar App Services, bancos, etc., dentro dele.

---
## 📌 Quando usar

- Automação
- Scripts
- CI/CD
- Gerenciamento rápido

---
## 📌 Quando NÃO usar

- Exploração inicial (melhor usar portal)
- Usuários iniciantes

---
## 📌 Vantagens

- Mais rápido que o portal
- Reproduzível (scripts)
- Essencial para DevOps

## 🔗 Relacionado

- [[Azure]]
- [[Portal Azure]]
- [[CI CD]]

## 📌 Próximo passo

- [[Resource Groups]]