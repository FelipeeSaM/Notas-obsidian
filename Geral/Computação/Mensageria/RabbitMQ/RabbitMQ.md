[[Computação]]

Site para simular RabbitMQ: https://tryrabbitmq.com/

Outras ferramentas para mensageria são: Apache Kafka, ActiveMQ, Amazon SQS e azure service bus.

#### Instalação: 
Manual: baixando direto do site: https://www.rabbitmq.com/
Pacotes: pelo chocolatey: choco install rabbitmq (choco uninstall rabbitmq)
Docker: 
```
docker run -d --name rabbitMq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

#### Usuário e senha padrões:
guest
guest
