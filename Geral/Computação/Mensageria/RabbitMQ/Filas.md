- A fila é um armazenamento temporário para mensagens que ainda não foram processadas;
- As mensagens são publicadas para os [[Exchanges]], que então os direciona para as filas correspondentes;
- Consumidores (ou [[Consumers]]) recebem as [[Mensagens]] das filas e processam suas informações;

Existem as seguintes propriedades na criação de uma fila:
- Durabilidade: (bool) se for true, elas permanecerão mesmo após o reinício do servidor do [[RabbitMQ]], e as mensagens não confirmadas serão reenviadas;
- Exclusividade: (bool) se for true, significa que esta fila será exclusiva, o que quer dizer que ela será utilizada somente por uma conexão, e então será excluída quando essa conexão for fechada;
- Auto-delete: (bool) se for true, ela será automaticamente apagada quando o último consumidor se desconectar;
- Message TTL: se refere ao período em que uma mensagem não entregue poderá permanecer na fila antes de ser descartada ou movida para outra fila. O valor é em milissegundos.