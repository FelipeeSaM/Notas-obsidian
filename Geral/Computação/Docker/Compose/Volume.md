Um volume no [[Docker-compose]]  tem que estar no mesmo nível do Services (a exemplo do [[Criando]]), dou um nome para o meu volume (ex: db-data). O meu Services tem que referenciar o volume dentro do parâmetro backend:, e deve apontar o diretório aonde vai estar, ex:

```
services:
 backend:
  image: awesome/database
  volumes:
   - db-data:/etc/data

 backup:
  image: backup-service
  volumes:
   - db-data:/var/lib/backup/data

volumes:
 db-data:
```

Um exemplo, complementando o docker-compose do [[Criando]]:

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
    volumes:
	  - mysql_data_exemplo:/var/lib/mysql 

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

volumes:
	mysql_data_exemplo:
		name: meu_banco
```

E para apagar o volume quando parar o container:
```
docker compose down --volumes
```
