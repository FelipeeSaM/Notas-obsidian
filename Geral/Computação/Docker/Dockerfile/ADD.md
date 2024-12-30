Assim como o [[COPY]], o ADD também serve para copiar um arquivo do HOST para o [[WORKDIR]], mas também serve para copiar um arquivo de uma URL para dentro da [[Imagem]]. Ele também copia e descompacta arquivos da extensão .tar.
**O comando cria uma nova camada**

Comando:
```
FROM ubuntu:18.10

WORKDIR /adicionar

ADD nome-arquivo-ou-url .
ADD https://wow.zamimg.com/uploads/blog/images/38783-reclamation-of-gilneas-questline-in-patch-10-2-5-story-spoilers.jpg .
```

O . aqui referencia que o arquivo 'nome-arquivo-ou-url' está no mesmo diretório do dockerfile.