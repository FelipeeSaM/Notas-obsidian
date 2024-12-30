Serve para criar um diretório onde outros comandos ocorrerão, como se fosse um "mini-container". Os comandos que rodam dentro do workdir são o [[COPY]], [[RUN]], [[ADD]], [[CMD]] e o
[[ENTRYPOINT]]. Pode-se ter vários WORKDIR em um único [[Dockerfile]].
Se o WORKDIR mencionar um diretório que ainda não existe, ele vai criar um.
**O comando cria uma nova camada**

Exemplo de dockerfile com WORKDIR:
```
FROM ubuntu:18.10

WORKDIR /copy

COPY arquivo-do-host .
```

Acima ele copia um arquivo do HOST com o comando [[COPY]] para dentro da nova pasta criada /copy. O . siginifica que o arquivo está no diretório atual do dockerfile.

Importante mencionar que o último WORKDIR do dockerfile vai ser a pasta em que o container recém-buildado vai iniciar por default.