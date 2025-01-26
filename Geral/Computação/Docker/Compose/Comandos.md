Comandos para rodar um [[Docker-compose]]:

#### **Subir um docker-compose**
Precisamos entrar no diretório que contenha o arquivo *docker-compose.yaml* e digitar o comando:
```
docker compose up -d
```

A tag *-d* significa que o terminal ficará livre depois da subida.

O comando acima criará uma [[Computação/Docker/Network/Network]] específica para os contêineres, bem como os contêineres presentes dentro do .yaml.

#### **Parar/remover um docker-compose**
```
docker compose down
```
