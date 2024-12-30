Comando:
```
docker image build -it <nome [[imagem]]>:<versão imagem> .
```

Faz um build da [[Imagem]] a partir do que está contido no docker file. Entre no diretório que contenha um dockerfile e rode o comando.

Parâmetro --it

Ou outro comando é simplesmente, caso a linha do terminal esteja já no diretório onde tem o dockerfile:
```
docker image build .
```
Se rodarmos o comando 'docker image ls', vai mostrar a imagem recém-criada com a tag e id como <>none

Para criar com contexto:
```
docker image build -t "nomedorepository:nomedatag" .
```
