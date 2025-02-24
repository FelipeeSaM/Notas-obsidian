É uma [[Exchanges]] que envia mensagens para uma ou mais fila onde o [[Bindings]] seja igual à [[Routing keys]], Ex:
1. Uma mensagem é enviada ao Broker com a Routing key "criação de pedido";
2. A exchange do tipo Direct "order-service" manda para a fila em que a Routing key seja igual a "criação de pedido";