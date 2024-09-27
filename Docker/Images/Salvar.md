Comando:
```
docker image save
``` 

salva a [[imagem]] do docker num arquivo compactado. Ex: docker image save ngnix:3.12.2 -o imagem.tar
	Se eu tenho 3 imagens com o mesmo nome, mas com flags diferentes e rodar o docker image save ngnix -o imagens.tar, ele vai salvar as 3 imagens com o mesmo nome em um unico arquivo
