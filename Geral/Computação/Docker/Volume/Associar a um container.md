Associar um volume já criado a um container:
Primeiro nós criamos um volume com o comando de [[Computação/Docker/Volume/Criar|Criar]], depois podemos definir o diretório dentro do container em que este volume estará, como no exemplo:
```
docker container run --it --rm --name exemplo -v meu-volume:/mnt/meu-volume alipne:3.17
```

O comando acima criará um container e o executará, a linha de comando ficará livre (por causa do parâmetro [[--it]]), o container será apagado depois que parar e o volume dentro do [[Container]] se chamará 'meu-volume', e estará dentro do caminho /mnt/meu-volume. A imagem será do alpine na versão 3.17.

Ainda sobre o comando acima, como o volume foi criado antes do comando, podemos usar vários contêineres que consumam o mesmo volume sem que ele seja destruído (e nem seus arquivos), somente o container será apagado por causa do [[--rm]].