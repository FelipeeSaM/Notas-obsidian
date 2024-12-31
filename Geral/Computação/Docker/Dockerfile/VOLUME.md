O comando `VOLUME` no Dockerfile é utilizado para declarar diretórios que devem ser persistidos fora do ciclo de vida do contêiner. Ele cria um ponto de montagem para armazenar dados, garantindo que os arquivos persistam mesmo que o contêiner seja removido.

**O comando cria uma nova camada.**

Comando:
```
FROM mysql:8

VOLUME /var/lib/mysql
```

No comando acima, o diretório `/var/lib/mysql`, onde o MySQL armazena os dados, será persistido fora do contêiner. Isso garante que os dados do banco de dados não sejam perdidos se o contêiner for recriado.

Para mapear o volume para um diretório no host:
```
$ docker run -v /caminho/no/neu/pc:/var/lib/mysql nome-da-imagem
```

Se eu quiser um container temporário:
Se  não especificar um volume nomeado e usar a opção `--rm` ao executar o contêiner, qualquer dado armazenado no volume anônimo será excluído ao terminar o contêiner.
Ex:
```
docker run --rm -v /caminho/no/container:/dados nome-da-imagem
```

Quando o contêiner for encerrado, o diretório `/dados` (e o conteúdo armazenado nele) será excluído automaticamente.
#### Boas práticas

- Use `VOLUME` para dados que precisam ser persistentes, como bancos de dados ou logs.
- Defina os volumes necessários no Dockerfile ou diretamente ao rodar o contêiner ([[-v]]).