---
tags: [azure]
nivel: basico
---
---
# Blob Storage

Blob Storage é o serviço do [[Azure]] para armazenar arquivos e dados não estruturados.

## 📌 O que é
Um tipo de armazenamento dentro de uma [[Storage Account]] usado para guardar grandes quantidades de dados, como arquivos, imagens e vídeos.

"Blob" significa Binary Large Object.

## 📌 Para que serve
- Armazenar imagens e vídeos
- Armazenar arquivos de backup
- Servir arquivos estáticos
- Armazenar logs e dados brutos

## 📌 Estrutura
Blob Storage é organizado em:
- Contêineres → agrupam arquivos
- Blobs → arquivos em si

Exemplo:
contêiner: imagens  
└── foto1.jpg  
└── foto2.png

## 📌 Características
- Acesso via URL (HTTP/HTTPS)
- Alta escalabilidade
- Controle de acesso (público ou privado)
- Integração com CDNs

## 📌 Exemplo
Uma API pode:
- receber upload de imagem
- salvar no Blob Storage
- retornar a URL para acesso

## 🔗 Relacionado
- [[Azure]]
- [[Storage Account]]

## 📌 Próximo passo
- [[Azure SQL Database]]