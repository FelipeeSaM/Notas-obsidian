O comando `ENTRYPOINT` no Docker é utilizado para configurar um contêiner como executável. Ele especifica o programa ou script que será executado quando o contêiner iniciar. Ao contrário do `CMD`, o `ENTRYPOINT` é imutável e sempre será executado, independentemente dos argumentos passados para o contêiner.

Isso é útil quando você deseja que o contêiner sempre execute um comando específico, mas também queira permitir a passagem de argumentos adicionais durante a execução.

**O comando cria uma nova camada.**

Comando:
```
FROM ubuntu:18.04 
ENTRYPOINT ["echo", "ola, mundo."]
```

Rodando o container sem argumentos extras, a saída seria:
`$ docker run nome-da-imagem 
`Hello, Docker!` 

E eu posso passar quantos argumentos quiser, depois do primeiro parâmetro:
```
FROM ubuntu:18.04 
ENTRYPOINT ["echo", "ola, mundo.", "como vai?", "eae"]
```

E posso, também, passar mais argumentos na hora de rodar o container, como no exemplo:
```
$ docker run nome-da-imagem "dale"
ola, mundo. como vai? eae dale
```

E caso precise sobrescrever um `ENTRYPOINT` durante a execução, pode usar a opção `--entrypoint` no comando `docker run`.