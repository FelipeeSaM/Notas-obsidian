---
tags: [azure]
nivel: basico
---
---
# RBAC

RBAC (Role-Based Access Control) controla permissões no [[Azure]].

## 📌 O que é
Um sistema que define quem pode acessar quais recursos e o que pode fazer com eles.

## 📌 Para que serve
- Controlar acesso a recursos
- Evitar permissões excessivas
- Organizar segurança por papéis

## 📌 Como funciona
Você atribui:
- um usuário ou identidade
- a um papel (role)
- em um escopo

---

### 🔹 Escopos
- Subscription
- Resource Group
- Recurso específico

---

### 🔹 Papéis comuns
- Reader → só leitura
- Contributor → cria e edita
- Owner → controle total

---

## 📌 Exemplo
Um desenvolvedor pode:
- ter acesso Contributor em um Resource Group
- mas não ter acesso à produção

## 📌 Conceito importante
Sempre usar o princípio do **menor privilégio**.

## 🔗 Relacionado
- [[Azure]]
- [[Azure Active Directory (AD)]]
- [[Managed Identities]]

## 📌 Próximo passo
- [[Managed Identities]]