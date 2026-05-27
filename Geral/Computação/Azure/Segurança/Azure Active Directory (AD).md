---
tags:
  - azure
  - computação
nivel: basico
---
---
# Azure Active Directory

Azure Active Directory (Azure AD) é o serviço de identidade do [[Azure]].

## 📌 O que é
Um sistema de gerenciamento de identidades e autenticação que controla quem pode acessar recursos no Azure e em aplicações.

Hoje ele também é conhecido como Microsoft Entra ID.

## 📌 Para que serve
- Autenticar usuários (login)
- Controlar acesso a aplicações
- Gerenciar identidades (usuários, grupos, apps)

## 📌 Conceitos principais

### 🔹 Identidade
Pode ser:
- usuário
- aplicação
- serviço

---

### 🔹 Autenticação
Verifica **quem você é**

Ex:
- login com email e senha
- login com Microsoft

---

### 🔹 Autorização
Define **o que você pode acessar**

---

## 📌 Exemplo
Uma API pode:
- exigir login via Azure AD
- validar o token do usuário
- permitir acesso apenas a usuários autorizados

## 📌 Como usar (Azure CLI)

Você pode gerenciar usuários, grupos e aplicações do Azure Active Directory usando o Azure CLI.

Antes de tudo, é necessário autenticar:

```
az login
```
Isso abrirá o navegador para login.

### 🔹 Listar usuários

```
az ad user list --output table
```

---
### 🔹 Criar usuário

```
az ad user create \  --display-name "Usuario Teste" \  --user-principal-name usuario@seu-dominio.com \  --password "SenhaForte123!"
```

---
### 🔹 Listar grupos

```
az ad group list --output table
```
### 🔹 Criar grupo

```
az ad group create \  --display-name "Dev Team" \  --mail-nickname "devteam"
```

---
### 🔹 Adicionar usuário ao grupo

```
az ad group member add \  --group "Dev Team" \  --member-id <USER_ID>
```

---
### 🔹 Criar aplicação (Service Principal)

```
az ad sp create-for-rbac --name "minha-app"
```

👉 Esse comando retorna:

- clientId
- clientSecret
- tenantId

Usado para autenticação de aplicações.
## 🔗 Relacionado
- [[Azure]]
- [[RBAC]]
- [[Managed Identities]]

## 📌 Próximo passo
- [[RBAC]]