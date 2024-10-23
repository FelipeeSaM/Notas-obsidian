Ex: 
```
docker container run -dt --name <nome qualquer> -P mysql:8.0.30
```

Associa todas as portas do container a portas aleatórias do host. A imagem do sql acima possui duas portas associadas a ela. Rodando este comando, o docker associará estas portas com portas aleatórias da máquina host

OU

Ex2:
```
docker container run -d --name <nome qualquer> -p <porta do host> : <porta do container> <imagem do container>
```
Ex: docker container run -d --name meu container -p 8081:56 mysql:8.0.30