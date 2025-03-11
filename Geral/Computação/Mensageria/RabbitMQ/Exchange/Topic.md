É uma [[Exchanges]] similar ao Direct, porém oferece mais flexibilidade, pois permite utilizar padrões de correspondência com o ( * ) e ( # ), não necessitando que a binding key seja igual a routing key.
- O ( * ) representa uma palavra qualquer;
- O ( # ) representa qualquer número de palavras;

Exemplo: 
Routing keys: `log.payments.marketplace`, `log.request.support` e `my-log.fraud.marketplace` com a Exchange do tipo Topic para "log-service":

log. * .marketplace -> log-marketplace
log.# -> log-all

Ou seja, qualquer coisa que tenha LOG **ALGUMA COISA** MARKETPLACE irá para a fila log-marketplace, enquanto que tudo que tenha LOG **QUALQUER COISA** irá para a fila log-all.  