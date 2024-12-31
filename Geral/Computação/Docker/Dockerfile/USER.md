O comando `USER` no Dockerfile define qual usuário será usado para executar os comandos no contêiner, em vez de utilizar o usuário root (padrão). Isso aumenta a segurança, especialmente em ambientes de produção, ao limitar privilégios desnecessários.

**O comando cria uma nova camada.**

Exemplo de uso:
```
FROM ubuntu:20.04

RUN useradd -m meuusuario

USER meuusuario
```

Para verificar:
```
$ docker run nome-da-imagem whoami
meuusuario
```

#### Boas práticas

- Evitar rodar contêineres como root, sempre que possível.
- Configurar um usuário específico com permissões restritas para maior segurança.
- Combinar `USER` com boas práticas de permissões para diretórios e arquivos.

GPT:
```# Base da imagem
FROM ubuntu:20.04

# Evita interação durante a instalação de pacotes
ARG DEBIAN_FRONTEND=noninteractive

# Atualiza e instala pacotes necessários
RUN apt-get update && apt-get install -y \
    sudo \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Cria um novo usuário e grupo
RUN useradd -m -s /bin/bash restricteduser

# Cria dois diretórios com permissões específicas
RUN mkdir -p /app/read-only /app/write-only \
    && chown restricteduser:restricteduser /app/read-only \
    && chown restricteduser:restricteduser /app/write-only \
    && chmod 444 /app/read-only \  # Somente leitura
    && chmod 744 /app/write-only   # Leitura, escrita e execução pelo usuário

# Configura o contêiner para usar o novo usuário
USER restricteduser

# Define o diretório inicial do usuário
WORKDIR /app

# Comando padrão para execução (exemplo)
CMD ["bash"]

```

### **O que acontece aqui?**

1. **Novo usuário e grupo:**
    
    - Foi criado um usuário chamado `restricteduser` com o comando `useradd`.
    - Ele terá acesso apenas ao que for explicitamente permitido.
2. **Diretórios restritos:**
    
    - `/app/read-only`: Permissões de somente leitura (444).
    - `/app/write-only`: Permissões de leitura, escrita e execução para o usuário (744).
3. **Permissões específicas:**
    
    - O usuário `restricteduser` é dono dos diretórios criados, mas só pode modificar o conteúdo do diretório `/app/write-only`.
4. **Definição de segurança:**
    
    - Ao alternar para o usuário `restricteduser` com o comando `USER`, o contêiner executará tudo com privilégios reduzidos.

### **Executando o contêiner**

1. Construa a imagem: ``docker build -t restricted-container .``
2.  Execute o contêiner: ``docker run -it restricted-container`` 
3. Teste o acesso:
		Tente criar um arquivo em `/app/read-only` (deve falhar):
			``echo "Teste" > /app/read-only/test.txt``
		
		Crie um arquivo em `/app/write-only` (deve funcionar):
			`echo "Teste" > /app/write-only/test.txt`
