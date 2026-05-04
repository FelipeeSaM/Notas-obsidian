---
tags: [azure]
nivel: basico
---
---
# Virtual Network

Virtual Network (VNet) é a rede privada do [[Azure]] onde seus recursos se comunicam.

## 📌 O que é
Uma rede virtual isolada dentro do Azure que permite que seus recursos (como VMs, bancos e serviços) se comuniquem entre si de forma segura.

É equivalente a uma rede interna em um ambiente local (on-premise), mas na nuvem.

## 📌 Para que serve
- Isolar recursos do resto da internet
- Controlar comunicação entre serviços
- Definir regras de acesso
- Criar arquiteturas seguras

## 📌 Características
- Totalmente isolada por padrão
- Permite comunicação interna entre recursos
- Pode ser conectada à internet ou a redes locais (VPN)

## 📌 Exemplo
Uma aplicação pode ter:
- API dentro da VNet
- Banco de dados dentro da mesma VNet

👉 Comunicação ocorre de forma privada, sem exposição à internet

## 🔗 Relacionado
- [[Azure]]
- [[Subnets]]
- [[Network Security Groups]]

## 📌 Próximo passo
- [[Subnets]]