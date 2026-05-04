---
tags: [azure]
nivel: basico
---

# Key Vault

Key Vault é o serviço do [[Azure]] para armazenar segredos com segurança.

## 📌 O que é
Um cofre seguro para armazenar:
- senhas
- strings de conexão
- chaves criptográficas
- certificados

## 📌 Para que serve
- Proteger dados sensíveis
- Centralizar segredos
- Evitar hardcode em código

## 📌 Características
- Controle de acesso via [[RBAC]] ou políticas
- Integração com [[Managed Identities]]
- Auditoria de acesso

## 📌 Exemplo
Uma aplicação precisa de uma connection string:

❌ Errado:
- salvar no código

✔ Correto:
- armazenar no Key Vault
- acessar via Managed Identity

---

## 📌 Outro exemplo
- API pega segredo no Key Vault
- usa para acessar banco
- segredo nunca fica exposto

---

## 📌 Boas práticas
- Nunca salvar segredos no código
- Usar Managed Identity sempre que possível
- Centralizar todos os segredos no Key Vault

## 🔗 Relacionado
- [[Azure]]
- [[Managed Identities]]
- [[RBAC]]

## 📌 Próximo passo
- [[CI CD]]