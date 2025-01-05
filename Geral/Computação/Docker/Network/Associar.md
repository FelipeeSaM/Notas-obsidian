Para associar um [[Container]] a uma [[Network]], usamos o comando:
```
docker container run --dt --name <nome-container> --entrypoint /bin/sh --network <nome-da-network> <imagem>
```

* O parâmetro do [[--entrypoint]] é opcional.
