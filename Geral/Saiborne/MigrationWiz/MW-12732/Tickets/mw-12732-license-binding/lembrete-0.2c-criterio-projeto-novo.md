Status: needs-info
Blocked by: None

# Lembrete: resolver a 0.2c (critério de "projeto novo")

## O que é

A 0.2c é a decisão pendente que define **o que torna um projeto de Teams Private Chat (PCH) "novo"** para a Parte B do MW-12732.

A restrição vem do ADR-0001: o binding nunca pode alterar line items de projetos que já existem. Por isso, todo binding passa por um único método, `ConversationLineItemEventHandler.IsNewProject(MailboxConnector)`, em `MigrationCommon\Sdk\Api\1.0\Events\ConversationLineItemEventHandler.cs`. Hoje ele retorna sempre `false`.

## Para que serve

Enquanto a 0.2c não for respondida, **a Parte B inteira fica inerte**, mesmo com a flag `pch-umb-binding` ligada:

- **Ticket 03** (binding na criação): o handler sai cedo e nenhuma conversation line item ganha CEU.
- **Ticket 04** (varredura do AEF): a cada 12 horas a varredura lê todas as linhas sem binding e não liga nenhuma. É custo sem efeito.
- **Ticket 10** (validação ponta a ponta): não dá para validar sem projeto que satisfaça o critério.
- **Parte A** (bloqueio de licença): ela lê `CustomerEndUserId`. Sem binding, toda linha PCH conta como sem licença. Isso precisa ser alinhado antes de ligar a flag de bloqueio.

O critério separa os projetos que nascem no modelo novo (UMB ligada ao CEU) dos legados, que mantêm o comportamento atual.

## Implementação sugerida

A troca é de uma linha de lógica no método único. Os dois chamadores (handler e varredura) já usam esse método.

**Opção A (recomendada): data de corte por criação do connector.**
Um projeto é novo se `mailboxConnector.CreateDate >= <data de corte>`. A data de corte deve ser a data do release ou a data em que a flag foi ligada.
- Simples, determinística e auditável.
- Pode entrar no filtro SQL da varredura (via join com `MailboxConnectors.CreateDate`) e eliminar a leitura em massa citada nos Comments do ticket 04.
- A data de corte fica numa configuração ou constante. Se for configuração, pergunte o nome da chave antes; não leia arquivos de config.

**Opção B: marca explícita no connector.**
Uma coluna ou flag (ex.: `IsUmbBindingEnabled`) gravada na criação do projeto, quando a flag está ligada.
- É mais explícita, mas exige uma coluna nova (checklist das 5 camadas e aprovação do `.sql`) e não cobre projetos criados entre o deploy e a ligação da flag.

**Opção C: critério por organização ou workgroup** (piloto gradual).
Útil para um rollout controlado, mas pode ser feito com a própria feature flag, se ela suportar escopo.

Seja qual for a opção, os testes de integração já existentes (casos `isNewProject: null`, que usam o critério real) precisam ser revistos: hoje eles esperam "não faz nada".

## O que perguntar ao PO

> Para a Parte B do MW-12732 (licença UMB em Teams Private Chat), precisamos definir o que é um "projeto novo", porque só os projetos novos terão as linhas ligadas ao CEU e à UMB. Os projetos existentes ficam como estão. A proposta é: **um projeto PCH é novo se foi criado a partir da data do release (ou da data em que a flag for ligada)**. Isso atende? Em particular: (1) projetos PCH criados antes dessa data continuam sem binding e sem a cobrança/bloqueio por UMB? (2) a data de corte é a do release ou a da ativação da flag? (3) existe algum cliente ou piloto que precise entrar antes, por organização?

## Outra pergunta para o PO: excluir uma linha de PCH devolve a licença? (0.2f)

### Contexto

- Uma UMB vale para o mesmo usuário em todos os workloads: Mailbox, OneDrive e Teams Private Chat.
- A licença só pode ser devolvida (revogada) **dentro de 24h** do consumo e **se nenhum job foi submetido**, inclusive Verify Credentials. Depois disso, só o suporte devolve.
- No Mailbox e no OneDrive, **excluir linhas em lote já dispara a revogação** da UMB dessas linhas. O épico e o PRD não falam de exclusão, mas pedem para reaproveitar o comportamento do Mailbox/OneDrive.

### O que foi implementado (ticket 09, assumido e pendente de confirmação)

Excluir uma conversation line item, individual ou em lote, com UMB `Active` inicia a revogação da licença do usuário. A revogação respeita as regras acima. Linhas em migração são ignoradas, e tudo fica atrás da flag `pch-umb-binding`.

### O efeito colateral a confirmar

Como a UMB é a mesma em todos os workloads, excluir a linha de PCH pode devolver a licença que um projeto Mailbox do mesmo usuário ainda ia usar ("revogação cruzada"). O risco é limitado:
- só acontece dentro das 24h;
- não acontece se já houve qualquer job no PCH;
- não acontece se já houve migração no projeto Mailbox.

Isso já acontece hoje entre projetos Mailbox e OneDrive.

### Pergunta

> Para o Teams Private Chat, implementamos o mesmo comportamento do Mailbox/OneDrive: excluir uma linha com UMB ativa devolve a licença, desde que esteja dentro de 24h do consumo e sem nenhum job. Como a UMB é compartilhada entre workloads, isso pode devolver a licença que um projeto Mailbox do mesmo usuário ainda não usou, como já ocorre entre Mailbox e OneDrive. (1) Esse comportamento está correto para o PCH? (2) A devolução deve valer também para a exclusão de uma linha só (no Mailbox, hoje, só a exclusão em lote devolve)? Se a resposta for "não devolver", basta remover esse trecho; a flag já permite desligar.

## Referências

- `spec.md` (tabela "Pendente: arquiteto / Will", linha 0.2c)
- `issues\03-binding-na-criacao.md`, `issues\04-varredura-assincrona-de-binding.md`, `issues\10-validacao-ponta-a-ponta.md`
- `docs\adr\0001-pch-binding-columns-owned-by-part-b.md` (no repo, fora do Git)
- Código: `ConversationLineItemEventHandler.IsNewProject` e `ConversationLineItemToCustomerEndUserActivity.IsNewProject`, que delega para o primeiro

## Comments
