O bind-mount é uma ferramenta para [[Associar a um container]] a um local da máquina host. É uma pasta que eu consigo criar arquivos dentro dela no windows/macOS e o container será alimentado com ele, e vice-versa.
O comando:
```
docker container run --it --rm --name <nome-container> -v <caminho-pasta-do-pc>:<caminho-dentro-do-docker> <imagem:versao-imagem>
```

Exemplo:
```
docker container run --it --rm --name Exemplo -v /Users/eu/desktop/pasta-compartilhada:/mnt/bind-mount alpine:3.17
```

Qualquer arquivo que eu criar dentro da pasta no windows, o container também terá. E se eu criar um arquivo dentro do caminho no container, eu consigo vê-lo na pasta windows.