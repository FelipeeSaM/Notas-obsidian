O comando `CMD` no Docker é usado para definir o comando padrão que será executado quando o contêiner for iniciado. Diferentemente do `ENTRYPOINT`, o `CMD` pode ser sobrescrito ao executar o contêiner, bastando passar outro comando.

Se tanto `CMD` quanto `ENTRYPOINT` forem definidos no Dockerfile, o `CMD` será tratado como os argumentos padrão do `ENTRYPOINT`.

Comando:
```
FROM ubuntu:18.04

CMD ["echo", "koe a boa?"]
```

Se rodar o container sem argumentos adicionais, vai ser usado o comando definido no parâmetro:
`$ docker run nome-da-imagem
``koe a boa?``

Porém se passar outro comando no parâmetro na hora de rodar o container, ele vai substituir o cmd do dockerfile, ex:
```
$ docker run nome-da-imagem "qualquer coisa"
qualquer coisa
```

#### **Boas práticas entre CMD e ENTRYPOINT**

- Utilize [[ENTRYPOINT]] para definir o comportamento principal do contêiner.
- Use `CMD` para definir argumentos padrão ou configurar valores que podem ser facilmente sobrescritos.
- Combine `ENTRYPOINT` e `CMD` quando quiser um comando fixo com argumentos configuráveis.