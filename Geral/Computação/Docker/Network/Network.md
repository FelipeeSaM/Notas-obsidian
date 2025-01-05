No [[Docker]], uma **network** é um recurso que permite que contêineres se comuniquem entre si ou com o ambiente externo. Ela cria um ambiente de rede virtual isolado e gerenciado, onde os contêineres podem trocar dados de forma segura e eficiente, sem interferir no sistema de rede do host.

As redes no Docker são fundamentais para arquiteturas baseadas em microsserviços, pois garantem conectividade e isolamento entre diferentes serviços ou aplicações em execução.
Por default, caso não especifiquemos um tipo de network no comando, ele criará com o tipo **bridge**.

Só podemos ter uma network do tipo **host**, e uma network do tipo **null**, pois só temos uma máquina host, e só temos um ambiente onde não tenha driver network (null).

GPT:
### **Principais Tipos de Redes no Docker**

1. **Bridge (Padrão para Contêineres Independentes)**
    
    - É a rede padrão para contêineres que não especificam outra configuração de rede.
    - Permite que os contêineres conectados à mesma rede se comuniquem entre si.
    - Ideal para cenários onde contêineres no mesmo host precisam conversar.
    
    Exemplo de criação de rede bridge:
		`docker network create my-bridge-network` 
		

2. **Host (Compartilha a Rede do Host)**
	- Neste modo, o contêiner usa diretamente a rede do host, sem isolamento.
	- Não há camada de abstração entre o contêiner e a rede do host.
	- Útil para casos de desempenho, como contêineres que precisam usar portas diretamente.
	
	Exemplo de uso:
		`docker run --network host nome-da-imagem`

3.  **Overlay (Para Comunicação Entre Hosts)**
	- Permite conectar contêineres que estão rodando em diferentes hosts Docker.
	- Ideal para clusters de Docker Swarm ou outras arquiteturas distribuídas.
	- Requer que o Docker esteja configurado em modo swarm.
	
	Exemplo de criação de rede overlay:
		`docker network create -d overlay my-overlay-network`
