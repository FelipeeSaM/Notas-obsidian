Comando:
```
docker rmi <nome da imagem> ou <id imagem>
```

 remove uma [[Imagem]]. Se rodar docker image rm <tag:versão>, removerá especificamente esta. 

Se rodar docker image rm <id> e tiver mais de um id, tem que rm especificamente uma das imagens com o mesmo id e depois funcionará.
Explicação: tags diferentes no docker hub podem apontar para a mesma imagem, então o hash/id de uma imagem local pode ser igual a outra imagem, cuja outra tag aponte para a mesma imagem no repositório remoto.

Ex: docker hub tem a imagem sqlserver:1.32.2 (imagem sql server e tag 1.32.2) que apontam para uma imagem de id '22222'. Outra imagem sqlserver:1.25.1 pode apontar para a mesma imagem de id '22222', então teríamos a estrutura:
 TAG             IMAGE ID   
 2.11.0-full   14c4fc2f44e
 2.21.0          14c4fc2f44e

Para resolver, terá de deletar a imagem pelo nome ou pela tag.