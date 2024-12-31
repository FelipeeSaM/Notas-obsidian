O comando `EXPOSE` no Dockerfile é utilizado para informar quais portas o contêiner escutará em tempo de execução. Ele não publica ou abre as portas diretamente no host, mas serve como documentação para outros desenvolvedores e ferramentas, indicando que o contêiner está preparado para usar determinadas portas.

**O comando cria uma nova camada.**

Comando:
```
FROM nginx:alpine 
EXPOSE 80
```

No exemplo acima, o container usa (escutará) na porta 80. Para expor esta porta para o host, é necessário usar o parâmetro [[-P]], ex:
```
$ docker run -p 8080:80 nome-da-imagem
```

O comando acima vincula a porta 80 do container com a 8080 do host (localhost:8080).

#### Boas práticas

- O `EXPOSE` é opcional, mas ajuda na documentação e no entendimento da configuração do contêiner.
- combinar `EXPOSE` com `-p` ou `--publish` ao rodar o contêiner para disponibilizar as portas.