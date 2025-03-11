Para conectar uma [[Computação/Docker/Network/Network]] de um container, rodamos o comando:
```
docker network connect <nome/tipo-da-network> <nome-container>
```

Ex:
```
docker network connect minha-network-bridge container01
```
