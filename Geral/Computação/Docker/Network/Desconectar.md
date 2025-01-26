Para desconectar um container de uma [[Computação/Docker/Network/Network]], rodamos o comando:
```
docker network disconnect <nome/tipo-network> <nome-container>
```

Ex:
```
docker network disconnect minha-network-bridge container01
```
