---
tags:
  - azure
  - computação
nivel: basico
---
---
# Azure Functions

Serviço serverless do [[Azure]] para executar código sob demanda, sem necessidade de gerenciar servidores.

## 📌 O que é
Azure Functions é um serviço que permite executar pequenas unidades de código (funções) em resposta a eventos, sem precisar provisionar ou manter infraestrutura.

No modelo tradicional, você precisa subir um servidor ou aplicação (como em um [[App Services]]). Já com Azure Functions, você escreve apenas a lógica necessária e o Azure se encarrega de executar, escalar e gerenciar os recursos automaticamente.

Esse modelo é conhecido como **serverless**, onde o foco está no código e não na infraestrutura.

## 📌 Como funciona
Uma função é executada a partir de um **gatilho (trigger)**.

Alguns exemplos de gatilhos:
- Requisição HTTP
- Mensagens em filas
- Execução agendada (timer)
- Eventos em storage (ex: upload de arquivo)

Fluxo básico:
1. Um evento acontece
2. A função é acionada
3. O código é executado
4. Um resultado é retornado ou outra ação é disparada

## 📌 Para que serve
- Criar APIs leves e endpoints rápidos
- Processar tarefas assíncronas
- Automatizar rotinas
- Integrar diferentes sistemas
- Executar tarefas em background

## 📌 Características
- Escala automática conforme a demanda
- Pode iniciar sem instâncias (cold start)
- Cobrança baseada em execução
- Suporte a várias linguagens (como .NET, Node.js, Python)
- Integração nativa com serviços do Azure

## 📌 Exemplos

### 🔹 HTTP Trigger (API simples)
Uma função que responde a uma requisição HTTP:

```
csharp
public static IActionResult Run(HttpRequest req)
{
    return new OkObjectResult("Hello from Azure Functions!");
}
```

## 🔗 Relacionado

- [[Azure]]
- [[App Services]]
- [[Containers]]
- [[Event-driven architecture]]

## 📌 Próximo passo

- [[Containers]]