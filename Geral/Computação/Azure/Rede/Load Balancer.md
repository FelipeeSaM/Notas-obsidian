---
tags: [azure]
nivel: basico
---
---
# Load Balancer

Load Balancer distribui o tráfego entre múltiplos recursos no [[Azure]].

## 📌 O que é
Um serviço que recebe requisições e distribui entre várias instâncias de uma aplicação.

## 📌 Para que serve
- Evitar sobrecarga em um único servidor
- Melhorar disponibilidade
- Aumentar escalabilidade

## 📌 Características
- Distribuição automática de tráfego
- Alta disponibilidade
- Pode operar em nível de rede

## 📌 Exemplo
Uma API rodando em 3 instâncias:

- Usuário faz requisição
- Load Balancer distribui entre:
  - Instância 1
  - Instância 2
  - Instância 3

👉 Isso melhora performance e resiliência

## 🔗 Relacionado
- [[Azure]]
- [[Virtual Network]]

## 📌 Próximo passo
- [[Application Gateway]]