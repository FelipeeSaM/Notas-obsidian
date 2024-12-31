O parâmetro `-v` no comando `docker run` é usado para **montar volumes no contêiner**, conectando um diretório ou volume do host a um diretório dentro do contêiner. Ele é essencial para compartilhar dados entre o host e o contêiner ou para persistir dados de forma que eles não sejam perdidos quando o contêiner for recriado.

Exemplo:
docker run -v /pasta/no/meu/pc:<caminho-no-container> nome-da-imagem
