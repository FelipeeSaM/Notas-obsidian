Para utilizar a network dentro de um [[Docker-compose]], basta criar uma sessão networks: no mesmo nível de services (ex de [[Computação/Docker/Compose/Volume|Volume]]), seguindo a mesma lógica da linha do nível logo depois do networks ser o nome a ser referenciado no services, ex:

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
	networks:
	  - frontend-exemplo

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
	networks:
	  - frontend-exemplo
	  - backend-exemplo

volumes:
	mysql_data_exemplo:
		name: meu_banco

networks:
	frontend-exemplo:
		name: frontend-exemplo-nome
		driver: bridge

	backend-exemplo:
		name: backend-exemplo-nome
		driver: bridge
```

Se fizermos um docker network ls, as networks criadas no docker-compose estarão listadas.