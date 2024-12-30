docker run <nome imagem>: o docker busca localmente por uma imagem com determinado nome para rodar. Caso não encontre, ele irá buscar no dockerhub uma imagem com o mesmo nome e tag para baixar e rodar. Ex: docker run da documentação: **docker run -d -p 8080:80 docker/welcome-to-docker** 

Código 0: o comando foi executado corretamente.
Código 137: o comando foi encerrado abruptamente.

[[Parâmetro]]

Ao entrar no container e digitar [[ENV]], o terminal mostra as variáveis de ambiente que estão rodando