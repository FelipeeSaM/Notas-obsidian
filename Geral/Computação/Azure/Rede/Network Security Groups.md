---
tags: [azure]
nivel: basico
---
---
# Network Security Groups

Network Security Groups (NSG) controlam o tráfego de rede no [[Azure]].

## 📌 O que é
Um conjunto de regras que permite ou bloqueia o tráfego de entrada e saída de uma rede ou subnet.

Funciona como um firewall básico.

## 📌 Para que serve
- Proteger recursos
- Controlar quem pode acessar o quê
- Definir regras de comunicação

## 📌 Características
- Regras de entrada (inbound)
- Regras de saída (outbound)
- Baseado em IP, porta e protocolo

## 📌 Exemplo
Permitir apenas acesso HTTP:

- Porta 80 → permitido
- Outras portas → bloqueadas

👉 Ou:

- API acessível pela internet
- Banco acessível apenas internamente

## 🔗 Relacionado
- [[Azure]]
- [[Virtual Network]]
- [[Subnets]]

## 📌 Próximo passo
- [[Load Balancer]]