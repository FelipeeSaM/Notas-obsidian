Comando:
```bash
docker container cp <origem> <destino>
```

Permite copiar um arquivo de dentro do container para fora (no host), ou do host para um [[Container]].

Exemplo de uso do host para o container:

Comando:
```bash
docker container cp nginx-logs:/container-file .
```

O comando acima copia os arquivos do root (rodando o comando pwd temos o /), na subpasta container-file, para o diretório da onde o comando esteja sendo rodado.

Trocando o . pelo caminho completo podemos salvar em um lugar específico, exemplo:
```bash
docker container cp nginx-logs:/container-file C:\Users\luiz_\AppData\Local\npm-cache\_logs
```

Se quiser salvar um arquivo do computador dentro do diretório do container, é só inverter a ordem.