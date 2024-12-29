Comando:
```bash
docker container exec <tag> <nome container> <shell>
```

Para acessar um container *em execução*, digitamos o comando acima passando os parâmetros e em qual ambiente (cmd ou powershell) utilizar. Acessar um container permite inserir comandos linux. Comando 'exit' para sair do [[Container]].

ou
Comando:
```bash
docker container exec <nome container> <comando>
```
Roda um comando diretamente no CMD sem precisar entrar no container.