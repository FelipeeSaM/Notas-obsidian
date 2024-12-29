Comando:
```
docker container run -d –name <algum nome> <imagem>
```

-name <algum nome>: Dá um nome personalizado ao [[Container]], facilitando sua identificação (em vez de usar um nome gerado automaticamente).
-<imagem>: Refere-se à imagem Docker que será utilizada para criar o container (como `nginx`, `ubuntu`, etc.).

Parâmetro -d e -name


> [!Também podemos rodar no seguinte modo:]
> Rodar associando a porta do host à porta do container

Comando:
```
docker container run --name <algum nome> -p <porta do host>:<porta do container>
```
