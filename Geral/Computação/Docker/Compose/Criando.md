O nome da pasta onde o docker-compose.yml está, será o nome do projeto dele.
Dentro de um [[Docker-compose]].yaml, temos que definir os serviços que serão levantados. A organização dos comandos e sub-comandos é feita com identação (espaço). Abaixo terá um exemplo de um sistema que usa *wordpress* e *mysql*.

```
services:
  mysql:
    image: mysql:8.0
    command: '--default-authentication-plugin=mysql_native_password'
    container_name: mysql-exemplo
    environment:
      - MYSQL_ROOT_PASSWORD=ex123
      - MYSQL_DATABASE=wordpress
      - MYSQL_USER=wordpressUser
      - MYSQL_PASSWORD=exemplosenha123
    expose:
      - "3306"
      - "33060"
    restart: always

  wordpress:
    depends_on: 
      - mysql
    image: wordpress:6.1.1
    environment:
      - WORDPRESS_DB_HOST=mysql
      - WORDPRESS_DB_USER=wordpressUser
      - WORDPRESS_DB_PASSWORD=exemplosenha123
      - WORDPRESS_DB_NAME=wordpress
    ports:
      - "9000:80"
    restart: always
```


Services > mysql (aqui pode ser o nome que eu quiser);
Services > mysql > [...] representa todos os parâmetros da aplicação mysql;
Services > mysql > container_name: se não definirmos o nome para a imagem, ela será criada seguindo o caminho da pasta do docker-compose + o nome do service + um número
Services > mysql > environment: podemos pegar as variáveis de ambiente existentes pesquisando a imagem no dockerhub. A do exemplo acima: https://hub.docker.com/_/mysql (seção environment variables).
Services > mysql > expose: expomos a(s) porta(s) de acesso à nossa aplicação mysql, no caso a que a aplicação wordpress irá acessar.
Services > mysql > restart: sempre que a aplicação falhar, vai restartar.

Services > wordpress > depends_on: aquele container só vai ser iniciado depois que a aplicação do mysql for terminada.
Services > wordpress > ports: porta do host associada a do containert (host:container)

Ao rodar o [[Comandos]] 
```
docker compose up
```
Podemos entrar em localhost://9000 ou 127.0.0.1:9000 e a interface da aplicação já estará rodando.