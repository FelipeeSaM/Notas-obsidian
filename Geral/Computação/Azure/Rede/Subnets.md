---
tags:
  - azure
  - computação
nivel: basico
---

# Subnets

Subnets são divisões da [[Virtual Network]] usadas para organizar e controlar recursos.

## 📌 O que é
Uma subnet é um segmento dentro de uma Virtual Network.

Ela permite separar recursos em grupos menores dentro da mesma rede.

## 📌 Para que serve
- Organizar recursos por tipo (API, banco, etc.)
- Aplicar regras de segurança específicas
- Controlar tráfego interno

## 📌 Características
- Cada subnet possui um range de IPs
- Recursos dentro da mesma subnet se comunicam facilmente
- Pode ter regras de segurança próprias

## 📌 Exemplo
Uma VNet pode ter:

- Subnet API → aplicações
- Subnet DB → banco de dados

👉 Isso permite separar responsabilidades e aplicar regras diferentes

## 🔗 Relacionado
- [[Azure]]
- [[Virtual Network]]
- [[Network Security Groups]]

## 📌 Próximo passo
- [[Network Security Groups]]