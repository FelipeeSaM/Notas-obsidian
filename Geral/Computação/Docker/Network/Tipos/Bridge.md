Ao [[Computação/Docker/Network/Criar|Criar]] uma network, ela será o ponto de comunicação entre dois contêineres diferentes que utilizem da mesma rede. Ex de fluxo:

Criação do [[Dockerfile]]:
```
FROM alpine:3.17

RUN apk add iputils
```

Criando uma [[Imagem]] baseada em Alpine chamada alpine:network:
```
docker image build -t alpine:network .
```
O '.' acima significa que a imagem usará o diretório atual como contexto.

Criação da [[Network]]:
```
docker network create minha-rede-bridge
```
 * Criará automaticamente no tipo bridge

Criação dos [[Container]]es que compartilharão a mesma network:
```
docker container run -it --rm --name container-net01 --network minha-rede-bridge
docker container run -it --rm --name container-net02 --network minha-rede-bridge
```

Com isso conseguimos criar dois contêineres distintos que conseguem se comunicar entre si. Podemos ver os ips com o comando **ifconfig**.  Se fizermos um ping do IP mostrado em *inet addr: XXX.XX.X.X*, do outro container, ele responderá.