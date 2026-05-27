---
tags:
  - azure
  - computação
nivel: basico
---
---
# Managed Identities

Managed Identities são identidades gerenciadas pelo [[Azure]] para autenticação entre serviços.

## 📌 O que é
Uma identidade automática criada pelo Azure para um recurso (ex: VM, App Service), permitindo que ele se autentique sem usar credenciais explícitas.

## 📌 Para que serve
- Evitar uso de senha ou connection string
- Permitir acesso seguro entre serviços
- Simplificar autenticação

## 📌 Como funciona
1. Um recurso recebe uma identidade
2. Essa identidade é registrada no Azure AD
3. Você dá permissão via [[RBAC]]
4. O recurso acessa outro serviço sem senha

---

## 📌 Exemplo
Um App Service precisa acessar o Key Vault:

❌ Sem Managed Identity:
- usar senha ou segredo

✔ Com Managed Identity:
- autentica automaticamente
- sem expor credenciais

---

## 📌 Vantagens
- Mais seguro
- Sem gerenciamento de credenciais
- Integração nativa com Azure

## 🔗 Relacionado
- [[Azure]]
- [[Azure Active Directory]]
- [[RBAC]]
- [[Key Vault]]

## 📌 Próximo passo
- [[Key Vault]]