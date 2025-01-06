Para buildar um projeto já criado para dentro de um [[Docker-compose]], nós temos que criar o arquivo *docker-compose.yaml* em um diretório acima do nosso projeto a ser buildado, como no exemplo:
```
Pasta-contendo-tudo
| - docker-compose.yaml
| - pasta-do-projeto
```

Então podemos aproveitar o docker-compose da seção [[Criando]] e fazer umas alterações:
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
    image: wordpress:<algum-nome>
    build: ./pasta-do-projeto
    environment:
      - WORDPRESS_DB_HOST=mysql
      - WORDPRESS_DB_USER=wordpressUser
      - WORDPRESS_DB_PASSWORD=exemplosenha123
      - WORDPRESS_DB_NAME=wordpress
    ports:
      - "9000:80"
    restart: always
```

E rodar o comando 
```
docker compose up -d
```