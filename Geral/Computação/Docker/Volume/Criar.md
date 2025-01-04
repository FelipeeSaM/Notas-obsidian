O comando abaixo criará um [[Computação/Docker/Volume/Volume|Volume]]:
```
docker volume create
```

O comando acima criará um volume solto e com um nome aleatório.

Podemos, também, criar um volume e nomeá-lo de duas formas diferentes:
```
docker volume create <nome-volume>
ou
docker volume create --name <nome-volume>
```