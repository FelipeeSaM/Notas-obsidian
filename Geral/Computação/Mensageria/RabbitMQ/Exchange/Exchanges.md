É um componente essencial na arquitetura do [[RabbitMQ]], pois elas recebem mensagens dos produtores e as redirecionam para as filas. Este direcionamento é baseado em regras denominadas de [[Bindings]], que são estabelecidas entre as conexões e as [[Filas]]. As [[Bindings]] podem avaliar a [[Routing keys]], que seria um atributo adicionado ao cabeçalho da mensagem, que serve como um "endereço" que o [[Exchanges]] poderá decidir como rotear a mensagem na definição do [[Bindings]].
Existe quatro tipos de exchanges:
- [[Direct]];
- [[Default]];
- [[Topic]];
- [[Fanout]]