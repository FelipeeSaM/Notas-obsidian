Serve para executar alguns comandos dentro do [[WORKDIR]]. Podem ser comandos de instalação, configuração ou remoção de algo.
**O comando cria uma nova camada**

Comando:
```
FROM ubuntu:18:10

WORKDIR /executar

RUN apt-get update && apt-get install -y <nome-pacotes> <pode-mais-de-um>
```

Outro exemplo de parâmetros é o: "RUN apk add ansible", por exemplo.

O -y é para aceitar globalmente (y/n)

Podemos quebrar o comando RUN com \ , de modo:

```
RUN apt-get update && apt-get install -y \ 
	net-tools \
	curl \
	nginx
```


Segundo a documentação do [[Docker]], é recomendado que, quando usamos o apt-get, rodemos também no final o seguinte comando, ex:
```
RUN apt-get update && apt-get install -y \ 
	net-tools \
	curl \
	nginx \
	&& rm -rf /var/lib/apt/lists/*
```
De modo a deletar todo conteúdo desta pasta, que não servirá de nada para o build.