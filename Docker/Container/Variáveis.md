Comando:
```bash
docker container run -it --name <algum nome> -e <nomeVariavel>=<valor> [...] <imagem>:<versão>
```

O comando acima inicia um [[Container]] com a variável (comando env para checar) que foi definida. Ex:
docker container run -it --name container-teste -e VAR1=abc -e VAR2=def alpine:3.16.12