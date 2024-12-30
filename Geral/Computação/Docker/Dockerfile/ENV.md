Vem de environment e, diferente do [[ARG]], ela é definida e fixa antes do build, não pode ser alterada.
Serve para criar uma variável de ambiente.

Mas pode ser "burlada" em tempo de build da seguinte maneira:

[[ARG]] TAG=latest

[[FROM]] alipne:$TAG

[[ARG]] CONTAINER_NAME_ARG  ((note que não tem nada))

[[ENV]] CONTAINER_NAME_ENV=${{CONTAINER_NAME_ARG:-"Container 1 exemplo"}}

Agora o ENV, que é fixo na hora do build, vai receber o valor que podemos passar dinamicamente no ARG.