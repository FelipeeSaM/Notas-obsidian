# Glossário MW-12732: licenciamento UMB no Teams Private Chat

> Fontes: PRD *Teams Private Chat Licensing Enablement* 1.1, tickets do épico MW-12724 (MW-12732, MW-12745, MW-12746, MW-12747), *MW-12732 Plano de Divisão* (Parte A Leonardo / Parte B Felipe), código de `C:\$\Development` (branch `feature/felipe/MW-12732-license-binding`) e `C:\$\Front\FrontEnd\migrationwiz-app`.
> Leitura feita do zero, sem usar ADRs nem o `GLOSSARY.md` do repositório.
> Caminhos de backend são relativos a `C:\$\Development`; os de frontend, a `C:\$\Front\FrontEnd\migrationwiz-app\app`.

---

## 1. Glossário

> Esta seção explica **o que cada termo é** e **qual papel ele tem no processo**. Onde cada coisa aparece no código está nas seções 2 a 5.

### 1.0 O processo inteiro em poucas linhas

1. Uma **revenda (MSP)** atende **clientes** que vão trocar de tenant Microsoft 365. Ela compra na **BitTitan Store** licenças **UMB**, uma por **pessoa** a migrar.
2. As licenças ficam no **MSPComplete**, dentro do cadastro do cliente. Cada pessoa do cliente é um **CEU**. A UMB é **aplicada a um CEU**, não a um projeto.
3. No **MigrationWiz**, a revenda cria **projetos**, um por **workload**: um de Mailbox (e-mails), um de OneDrive (arquivos), um de **PCH** (chats privados do Teams). Cada projeto tem **line items**, que são as linhas "migre o usuário X da origem para o destino".
4. Cada line item precisa saber **de qual pessoa ele é**. Esse elo line item → CEU é o **vínculo (binding)**. Ele é resolvido pelo **endereço de origem**.
5. Na hora de migrar, o MigrationWiz pergunta: "a pessoa dona desta linha tem uma UMB ativa que **cobre este workload**?" Se sim, migra sem limite. Se não, **bloqueia**.
6. Como a UMB é da pessoa, **uma única UMB cobre** as linhas dessa pessoa no projeto de Mailbox, no de OneDrive e no de PCH. Repetir a migração (passes, retries) não gasta outra licença.
7. **O que o épico muda:** até hoje o PCH era gratuito, então ninguém perguntava pela UMB. Agora, nos **projetos PCH novos**, o PCH passa a exigir UMB como Mailbox e OneDrive já exigem.

**"PCH precisa de UMB, e UMB precisa de PCH?"**
- **PCH → UMB:** depois do épico, sim, nos projetos novos. Uma linha de PCH só migra se a pessoa dona dela tiver UMB ativa que inclua o PCH. Para isso a linha precisa estar **vinculada** à pessoa; sem vínculo não há como saber se existe licença.
- **UMB → PCH:** não. A UMB existe independentemente do PCH: é uma licença por pessoa que cobre vários workloads. O épico só **acrescenta o PCH à lista do que a UMB cobre**. Quem já tinha UMB por causa de um projeto de Mailbox deve ganhar o PCH "de graça", sem comprar outra (o chamado provisionamento **retroativo**).

---

### 1.1 Pessoas, empresas e sistemas

**MigrationWiz (MW)**
- **O que é:** o produto da BitTitan que copia dados de usuários de um ambiente (tenant) para outro: e-mails, arquivos, chats do Teams.
- **Papel no processo:** é onde se criam os projetos, se adicionam as linhas, se aplicam licenças e se disparam as migrações.

**MSP / revenda / parceiro**
- **O que é:** a empresa de TI que faz a migração para o cliente final. É o usuário do MigrationWiz.
- **Papel no processo:** compra as licenças, monta os projetos e decide quem recebe licença.

**Cliente (Customer)**
- **O que é:** a empresa que está sendo migrada (por exemplo, a "Contoso" que vai para um tenant novo).
- **Papel no processo:** agrupa as pessoas (CEUs) e as licenças. Um projeto do MigrationWiz fica associado a um cliente, e o vínculo só é possível se essa associação existir.

**MSPComplete (MSPC)**
- **O que é:** o portal da BitTitan onde a revenda gerencia os clientes, as pessoas de cada cliente (tela "People") e as licenças (tela "Settings > Licenses").
- **Papel no processo:** é o "cartório" das licenças. O MigrationWiz consulta o MSPComplete para saber quem é a pessoa por trás de uma linha e se ela tem UMB. Aplicar ou remover UMB pode ser feito nos dois sistemas, mas o registro mora aqui.

**BitTitan Store**
- **O que é:** a loja onde se compram as licenças.
- **Papel no processo:** o MW-12747 exige que a loja e as páginas de preço digam que a UMB agora cobre PCH, para o cliente saber que não precisa de outro produto.

**DP (Development Partner)**
- **O que é:** o time externo que implementa o PRD (vocês).
- **Papel no processo:** além do código, entrega o HLD (desenho de alto nível), as páginas no Confluence, a demo e o plano de QA. Toda saída de IA precisa de revisão humana.

---

### 1.2 Workloads, projetos e linhas

**Workload**
- **O que é:** o "tipo de coisa" que se migra: caixa de correio, OneDrive, chats privados do Teams, SharePoint etc.
- **Papel no processo:** cada workload tem regras próprias de licença. O épico faz o workload PCH entrar na mesma regra de licença que Mailbox e OneDrive já seguem.

**Projeto**
- **O que é:** uma migração de **um** workload, com uma origem e um destino definidos (por exemplo, "chats do tenant A para o tenant B").
- **Papel no processo:** reúne as linhas, as configurações e o cliente do MSPComplete. É no projeto que se escolhem as linhas e se manda migrar.
- **Relação:** uma mesma pessoa pode estar em vários projetos (Mailbox, OneDrive, PCH). É por isso que a licença fica na pessoa, e não no projeto.

**Endpoint**
- **O que é:** a conexão com um dos lados da migração (o tenant de origem ou o de destino), com as credenciais de acesso.
- **Papel no processo:** um projeto precisa de endpoint de origem e de destino. Sem eles, o sistema não consegue associar as linhas às pessoas do cliente, e o vínculo fica para depois.

**Line item (linha do projeto)**
- **O que é:** uma linha do projeto: "migre **este** usuário de origem para **este** usuário de destino".
- **Papel no processo:** é a unidade que se seleciona, migra, reprocessa ou exclui.
- **Relação:** **a linha não é a pessoa.** A linha **aponta** para a pessoa (CEU). A mesma pessoa pode ter uma linha em cada projeto.

**PCH (Teams Private Chat)**
- **O que é:** o workload que migra **conversas privadas do Teams**: chats 1:1, chats de grupo e chats de reunião. No produto aparece como projeto "Collaboration (Private Chats)".
- **Papel no processo:** é o alvo do épico. Hoje migra sem licença; vai passar a exigir UMB nos projetos novos. Fora do escopo: usuários externos/convidados, tenants GCC High, origem e destino no mesmo tenant.

**Linha de PCH (ConversationLineItem)**
- **O que é:** a linha de um projeto PCH: o par "usuário de origem → usuário de destino" cujos chats serão migrados.
- **Papel no processo:** é a linha que precisa ganhar o vínculo com a pessoa, para que a licença dela seja reconhecida.
- **Relação:** é o equivalente, no PCH, da linha de Mailbox, mas é uma estrutura **separada**. Por isso o PCH não herda de graça o que o Mailbox já faz.

**Linha de Mailbox e de OneDrive**
- **O que é:** a linha dos projetos de e-mail e de OneDrive. Os dois workloads usam a **mesma estrutura de linha**.
- **Papel no processo:** é o modelo a copiar. Já tem vínculo, status "UMB Active", bloqueio sem licença, troca de domínio e Add from MSPComplete.

**Endereço de origem / endereço de destino**
- **O que é:** o e-mail ou login do usuário no tenant de origem e no de destino.
- **Papel no processo:** o **endereço de origem** é a "identidade" pela qual a licença é reconhecida. O épico diz que a UMB fica presa ao endereço de origem usado quando ela foi aplicada. Quando o vínculo é criado, o sistema procura a pessoa cujo e-mail principal é igual ao endereço de origem (e, se não achar, ao de destino).

**UPN, prefixo e sufixo**
- **O que é:** o UPN (*User Principal Name*) é o login Microsoft no formato `prefixo@sufixo`, por exemplo `joao.silva@contoso.com`. O **prefixo** identifica a pessoa (`joao.silva`); o **sufixo** é o domínio (`contoso.com`).
- **Papel no processo:** o épico quer que trocar só o domínio (`joao.silva@contoso.com` → `joao.silva@novo.com`) não faça a pessoa perder a licença. Trocar o prefixo seria, em tese, "outra pessoa". Na prática, Mailbox e OneDrive mantêm a licença em qualquer troca (seção 2.3).

**LineItemId (identidade da linha)**
- **O que é:** o identificador único da linha.
- **Papel no processo:** o PRD exige que, ao corrigir endereços, a linha seja **editada** e não apagada e recriada. Recriar perderia o histórico e o vínculo, e poderia gastar outra licença.

**Tipos de migração: Verification, Pre-Stage, Full, pass, retry**
- **O que é:**
  - **Verification** (Verify Credentials): só testa o acesso, sem copiar dados.
  - **Pre-Stage:** copia o grosso dos dados antes da virada.
  - **Full:** a migração completa.
  - **Pass:** cada execução sobre a mesma linha.
  - **Retry:** reexecução depois de um erro.
- **Papel no processo:** a licença é checada na submissão. No Mailbox, só o Full exige licença. Para o PCH, quais tipos vão exigir ainda está em aberto. Passes e retries **não** gastam licença nova. A revogação da UMB fica proibida depois que algum job roda; para PCH, conta até a Verification.

---

### 1.3 Licenciamento

**UMB (User Migration Bundle)**
- **O que é:** uma licença **por pessoa**, válida por um ano, que dá direito a migrar **sem limite de volume** todos os workloads incluídos no pacote dessa pessoa: e-mail, OneDrive e, com o épico, chats privados do Teams.
- **Papel no processo:**
  1. A revenda compra UMBs.
  2. Ela **aplica** uma UMB a uma pessoa (CEU), pelo MSPComplete ou pelo botão "Apply User Migration Bundle License" no projeto.
  3. A partir daí, **toda linha vinculada a essa pessoa**, em qualquer projeto cujo workload a UMB cubra, migra sem gastar mais nada.
  4. A UMB só pode ser retirada dentro de uma janela curta e antes de qualquer migração.
- **Relação:**
  - **UMB × CEU:** a UMB pertence ao CEU.
  - **UMB × linha:** a linha só "enxerga" a UMB através do vínculo.
  - **UMB × PCH:** o PCH precisa passar a ser um workload coberto pela UMB, o que é uma configuração de licenciamento (MW-12099).
  - **UMB × projeto:** um projeto nunca "tem" UMB; quem tem são as pessoas das linhas dele.
- **Exemplo:** Maria tem uma UMB e já migrou o e-mail no projeto de Mailbox. Ao entrar no projeto de PCH, a linha dela se vincula à mesma Maria (mesmo CEU), aparece como "UMB Active = Yes" e migra sem consumir outra licença.

**CEU (Customer End User)**
- **O que é:** o cadastro de **uma pessoa** do cliente no MSPComplete, identificada principalmente pelo **e-mail principal**.
- **Papel no processo:** é o "dono" da licença e o ponto de encontro entre projetos. As linhas da Maria em Mailbox, OneDrive e PCH apontam todas para o mesmo CEU "Maria".
- **Relação:** se o sistema não achar o CEU de uma linha, ele cria um CEU novo (sem licença). Por isso, achar o CEU **certo** é crucial: um CEU errado ou duplicado faz a pessoa parecer sem licença e leva a revenda a gastar outra.

**Assinatura (Subscription)**
- **O que é:** o registro de que um CEU tem uma UMB aplicada, com data de início e de fim.
- **Papel no processo:** é a **fonte da verdade** sobre a licença. Tudo o mais (o "Yes/No" na tela, por exemplo) é cópia dela.

**Vínculo (binding)**
- **O que é:** a ligação **linha → pessoa (CEU)**.
- **Papel no processo:** é o **pré-requisito** de todo o licenciamento. Sem vínculo, a linha não tem dono conhecido, então não há como verificar licença e ela é tratada como "sem licença". É isso que o PRD chama de "mapeamento antes da validação": primeiro saber de quem é a linha, depois perguntar se essa pessoa tem UMB.
- **Como nasce:** quando a linha é criada, o sistema procura a pessoa pelo endereço de origem, ou usa a pessoa escolhida (Add from MSPComplete no Mailbox).
- **Como se mantém:** depois de criado, o vínculo **fica congelado**. Corrigir o endereço (trocar domínio, ajustar para o UPN real) **não** muda a pessoa. É assim que a licença "persiste através de mudanças de domínio".
- **Divisão do trabalho:** a Parte B (Felipe) cria e preserva o vínculo; a Parte A (Leonardo) lê o vínculo para decidir se bloqueia.

**Data do vínculo (MspcSynchronizationDate)**
- **O que é:** o momento em que a linha foi vinculada à pessoa.
- **Papel no processo:** funciona como uma "trava". Se a data existe, a linha já tem dono e o sistema não tenta descobrir de novo. É essa trava que faz o vínculo sobreviver a mudanças de endereço.

**"UMB Active" (SyncSubscriptionStatus)**
- **O que é:** uma cópia, na própria linha, do status da licença da pessoa: Ativa ou Nenhuma.
- **Papel no processo:** alimenta a coluna "User Migration Bundle Active" (Yes/No) da tela do projeto (MW-12746). É **só para exibição**: é atualizada quando a licença é aplicada, revogada ou expira, e pode atrasar. A decisão de bloquear consulta a assinatura de verdade, não esta cópia.

**Resolução da pessoa (sincronização MW ↔ MSPC)**
- **O que é:** o procedimento que, dada uma linha, descobre seu CEU. Primeiro tenta o e-mail principal igual ao endereço de origem; se não achar, tenta o de destino; se ainda não achar, **cria** um CEU novo.
- **Papel no processo:** é o que transforma "um endereço de e-mail" em "uma pessoa licenciável". Se a pessoa certa já vier indicada (como no Add from MSPComplete do Mailbox), ele a usa direto.

**Varredura periódica de vínculos**
- **O que é:** uma tarefa automática que roda a cada 12 horas e tenta vincular as linhas que ficaram sem dono.
- **Papel no processo:** é a rede de segurança. Uma linha criada quando o projeto ainda não tinha cliente ou endpoints é vinculada mais tarde, sem ação do usuário.

**Feature e SKU**
- **O que é:** uma **SKU** é um produto vendável (por exemplo, a "UMB anual"). Uma **feature** é um direito que a SKU concede (por exemplo, "pode migrar Mailbox", "pode migrar PCH"). Cada workload declara quais features exige.
- **Papel no processo:** "a UMB cobre PCH?" é literalmente "a SKU da UMB contém a feature que o PCH exige?". Hoje **não contém**, e o MW-12099 resolve isso, cadastrando a relação no arquivo de dados de licenciamento de cada ambiente (Produção e Alemanha). Sem isso, mesmo com tudo implementado, nenhuma UMB liberaria PCH.

**Elegibilidade / isenção de licença**
- **O que é:** o resultado da pergunta "esta linha está coberta?". Uma linha é **isenta** (de gastar licença avulsa) quando sua pessoa tem uma UMB ativa cuja SKU inclui a feature do workload.
- **Papel no processo:** é a checagem que roda na hora de migrar e na estimativa mostrada antes ("quantas licenças preciso?"). Para PCH, ainda precisa ser criada (Parte A).

**Consumo (aplicar a UMB)**
- **O que é:** o ato de "gastar" uma UMB do estoque da revenda numa pessoa.
- **Papel no processo:** acontece **uma vez**, quando a licença é aplicada, e não a cada migração. Por isso passes e retries não gastam de novo. Ao consumir, todas as linhas da pessoa, em todos os projetos, passam a mostrar "Yes".

**Licença por migração (modelo antigo)**
- **O que é:** o modelo anterior à UMB: cada migração completa gasta uma licença de um pacote, com limite de volume.
- **Papel no processo:** no Mailbox, se a pessoa não tem UMB, o sistema tenta esse modelo; se não houver pacote, recusa com "licença insuficiente". O épico só habilita o caminho UMB para o PCH.

**Revogação (remover a UMB)**
- **O que é:** desfazer a aplicação da UMB a uma pessoa, devolvendo a licença ao estoque.
- **Papel no processo:** serve para corrigir engano (UMB aplicada à pessoa errada). Regras do épico para PCH: só dentro de **24 horas** e só se **nenhum job** tiver sido submetido para a pessoa, nem mesmo Verify Credentials. Depois disso, só o Suporte da BitTitan revoga, após auditoria. Excluir linhas com UMB ativa em lote também dispara a revogação.
- **Relação:** como a UMB é da pessoa, revogar afeta as linhas dela em **todos** os projetos.

**Expiração**
- **O que é:** o fim do prazo (um ano) da UMB.
- **Papel no processo:** as linhas da pessoa voltam a mostrar "No", e novas migrações passam a ser bloqueadas.

**Janela de 24 horas**
- **O que é:** o prazo, contado a partir da aplicação da UMB, em que ela ainda pode ser revogada.
- **Papel no processo:** limita a "devolução" de licença para evitar abuso (migrar e depois devolver).

**Projeto novo**
- **O que é:** um projeto PCH criado **depois** do lançamento da funcionalidade.
- **Papel no processo:** só projetos novos passam a exigir UMB. Os existentes continuam como estão, para não quebrar migrações em andamento. O critério exato ("criado depois de quando?") ainda está pendente, e enquanto não for definido nenhuma linha de PCH é vinculada.

**Provisionamento retroativo**
- **O que é:** fazer as UMBs aplicadas **antes** do lançamento também valerem para PCH.
- **Papel no processo:** quem comprou UMB para Mailbox no ano passado não deve precisar de outra para migrar chats. Isso sai naturalmente se a cobertura for decidida pela SKU da UMB no momento da checagem, e não gravada na hora da compra.

---

### 1.4 Mapeamento e correção de endereços (Feature 2)

**User mapping (mapeamento de usuários)**
- **O que é:** a correspondência entre o usuário de origem e o usuário de destino, com um estado visível por linha: **mapped** (ok), **unmapped** (sem par) ou **conflicted** (conflito, com motivo e caminho de solução).
- **Papel no processo:** é a etapa que vem **antes** da validação de licença no fluxo do PRD. Com os pares confirmados, roda a checagem de licença.
- **Relação:** **não é a mesma coisa que vínculo.** O mapeamento liga **origem ↔ destino**; o vínculo liga **linha ↔ pessoa licenciada**.

**Change Domains (troca de domínio)**
- **O que é:** uma ação em lote que troca o sufixo de domínio dos endereços de várias linhas (`@antigo.com` → `@novo.com`).
- **Papel no processo:** é a ferramenta para o fluxo "adicione o endereço licenciado, confirme o Yes e depois ajuste para o UPN real", sem perder a licença. Já existe para Mailbox e OneDrive; para PCH ainda não (MW-12737).

**Alteração de endereços em lote via SDK**
- **O que é:** exportar os endereços com o SDK/PowerShell da BitTitan, editar fora (por exemplo, numa planilha) e reimportar, atualizando as linhas existentes.
- **Papel no processo:** é a correção em grande escala. Precisa manter a identidade da linha e o vínculo, sem gastar licença nova (MW-12738).

**Add from MSPComplete**
- **O que é:** adicionar linhas ao projeto escolhendo pessoas já cadastradas no MSPComplete, em vez de digitar endereços.
- **Papel no processo:** reduz erro de digitação e, no Mailbox, já leva junto a **pessoa escolhida**, então o vínculo nasce certo. Para PCH está em desenvolvimento (MW-12745, branch da Mel). Ver a seção 5 sobre o vínculo nesse fluxo.

**Discovery (descoberta automática)**
- **O que é:** o MigrationWiz lê o tenant de origem e propõe as linhas automaticamente.
- **Papel no processo:** é outra porta de entrada de linhas. Toda linha criada por ela também precisa ganhar vínculo.

---

### 1.5 Bastidores: auditoria e execução

**Processamento assíncrono (AEF)**
- **O que é:** o mecanismo que roda tarefas em segundo plano e tarefas periódicas.
- **Papel no processo:** roda a varredura de vínculos a cada 12 horas e as revogações disparadas pela exclusão de linhas, sem travar a tela do usuário.

**Tabelas do Azure do PCH**
- **O que é:** o armazenamento em nuvem usado pelos serviços que executam a migração de chats.
- **Papel no processo:** o épico pede que o vínculo de licença também seja gravado ali, para que a orquestração dos jobs de PCH e a auditoria enxerguem a mesma informação.

**Action Log (log de ações)**
- **O que é:** o histórico de "quem fez o quê" nas linhas (adicionou, editou, excluiu, aplicou licença).
- **Papel no processo:** é a auditoria visível para o Suporte e para os relatórios ("Project Action Log", MW-13865).

**Logs estruturados / Kibana**
- **O que é:** registros técnicos pesquisáveis num painel (Kibana).
- **Papel no processo:** o PRD exige que todo bloqueio por falta de licença registre o **motivo** e que o consumo de UMB gere **métricas**, para o Suporte responder sem abrir chamado de engenharia.

---

### 1.6 Tickets do épico (referência rápida)

| Ticket | Assunto |
|---|---|
| MW-12724 | Épico: licenciamento UMB no PCH |
| MW-12732 | Habilitar UMB no PCH: vínculo (Parte B) e bloqueio (Parte A) |
| MW-12745 | Add from MSPComplete no PCH |
| MW-12746 | Apply/Remove License e coluna "UMB Active" no PCH |
| MW-12747 | Loja e páginas de preço |
| MW-12099 | SKU/feature: fazer a UMB cobrir PCH |
| MW-11330 / 12737 / 12738 | Mapeamento, Change Domains, alteração em lote via SDK |
| MW-12739 / 13865 | Toolbar do PCH igual à dos outros projetos; relatórios |

---

## 2. Como Mailbox e OneDrive funcionam (o modelo a copiar)

### 2.1 Ciclo de vida da UMB no Mailbox/OneDrive

1. **Criação da linha** (UI, CSV, SDK, Discovery, Add from MSPComplete).
   - O REST `ProjectItemDTO.UpdateMailbox` grava `CustomerEndUserId = dto.CustomerEndUserId.GetValueOrDefault()` (`ProjectItemDTO.cs:425`), ou seja, **aceita o CEU vindo do cliente**.
   - O SDK `Mailbox.Validate` (Create), se vier um CEU, chama `CheckCustomerEndUserExists`, que verifica se o CEU é visível para o master system user do dono (`Sdk\...\Objects\Mailbox.cs:661-760`). Não verifica a organização do projeto, só o escopo da conta.
2. **Vínculo**: `MailboxEventHandler` (Create **e** Update) chama `RunSync`. Se o CEU já veio preenchido, ele é mantido; se não, é resolvido por e-mail (origem → destino) ou criado. Grava `CustomerEndUserId`, `MspcSynchronizationDate` e `SyncSubscriptionStatus`.
3. **Varredura**: `MailboxToCustomerEndUserActivity` a cada 12h, para as linhas que ficaram sem vínculo.
4. **Aplicar licença** (toolbar "Apply User Migration Bundle License", `components/project/attach-umb.js`): consome a UMB no CEU (`ConsumePrepaidSubscriptionRequest` + `LicenseLog`) e sincroniza o status de todas as linhas do CEU para Active.
5. **Grid**: coluna "User Migration Bundle Active" (sort por `syncSubscriptionStatus`; a lista inclui `customerEndUser.activeSubscription`).
6. **Submissão**: no `Full`, se a linha estiver isenta (CEU com UMB que cobre o projeto), não consome nada e o limite de dados vira ilimitado; se não estiver, tenta license pack e, sem pack, lança `LicenseInsufficient`. Trial/Verification não exigem licença.
7. **Passes/retries**: não consomem de novo, porque a isenção é recalculada a cada submissão a partir do CEU.
8. **Mudança de endereço** (edição, Change Domains via `updateDomainBulk`, SDK): como `MspcSynchronizationDate` já está preenchido, o handler retorna cedo e o vínculo fica. O SDK ainda restaura CEU/data quando um cliente desatualizado manda vazio.
9. **Revogação**: dentro da "janela" (na prática, sem janela de tempo aplicada) e sem histórico de migração de dados.
10. **Exclusão**: `ProjectItemsService` (bulk) dispara o workflow `MspcSubscriptionsRevoke` para os CEUs com status Active (`ProjectItemsService.cs:547-567`). A exclusão unitária não revoga.

### 2.2 Mailbox × OneDrive

- **Mesma entidade e mesma tabela** (`Mailbox`/`Mailboxes`), mesmos serviços REST, mesmo handler, mesmo sweep. Muda o tipo de projeto (`isStorage` / Document) e os endpoints.
- **Licenciamento**: OneDrive declara a feature UMB direto nos descritores (`DocumentDescriptors-OneDrive.xml`), então só a UMB é elegível. Mailbox declara features de mailbox, que tanto a UMB quanto as licenças avulsas de mailbox satisfazem.
- **Fluxo do épico "Address Adjustment for UPN Matching"**: adicionar o endereço licenciado, esperar "Yes" e depois trocar para o UPN real. O próprio épico diz que isso "espelha o OneDrive". Funciona porque o vínculo congelado sobrevive à troca.

### 2.3 Observação sobre o "prefixo do UPN"

O épico repete que o vínculo persiste "desde que o prefixo do UPN não mude". O código do Mailbox/OneDrive **não tem regra de prefixo**: depois de vinculado, qualquer mudança de endereço mantém o CEU. E o fluxo de "Address Adjustment" acima normalmente muda o prefixo. A branch seguiu o comportamento do Mailbox (mantém sempre e só registra o evento `AddressChangedBindingKept`). Vale confirmar com o arquiteto que a frase do épico é descritiva, não uma regra nova.

---

## 3. PCH × Mailbox/OneDrive

### 3.1 Onde convergem (já igual ou alinhado pela Parte B)

| Aspecto | Mailbox/OneDrive | PCH (branch atual) |
|---|---|---|
| Colunas de vínculo | `CustomerEndUserId`, `MspcSynchronizationDate`, `SyncSubscriptionStatus` | As mesmas três, mesmos defaults e índices (`ConversationLineItem.sql`), nas 5 camadas |
| Resolução do CEU | `RunSync` por e-mail origem→destino, cria CEU se preciso | Mesmo `RunSync` (com overload que informa se criou CEU, para o log) |
| Congelamento do vínculo | `MspcSynchronizationDate` preenchido faz o handler retornar cedo | Igual |
| Sweep AEF 12h | `MailboxToCustomerEndUserActivity` | `ConversationLineItemToCustomerEndUserActivity` (cópia, com filtros onmschina/onmicrosoft.de/endereços vazios) |
| Status por linha | Consume/Revoke/Expire → `SyncMailboxes` | Os mesmos requests também chamam `SyncConversationLineItems` (falha isolada, não derruba o Mailbox) |
| SDK update preserva vínculo | `Sdk\Objects\Mailbox.cs` restaura CEU/data vazios | `Sdk\Objects\ConversationLineItem.Validate(Update)` igual |
| Revogação na exclusão | Bulk revoga, unitária não | Igual (`DeleteBulk` → `StartRevocationForDeletedItems`; `Delete` sem revogação) |
| Revogação | `RevokeSubscriptionRequest` único | Mesmo request (a UMB é do CEU, então todo caminho passa por ele) |
| UMB por pessoa | Um CEU cobre todas as linhas | Um CEU com Mailbox/OneDrive já licenciado será reconhecido no PCH **assim que** a Parte A consultar o CEU e o MW-12099 incluir a feature (cenário retroativo B5) |

### 3.2 Onde divergem

| Aspecto | Mailbox/OneDrive | PCH | Motivo / impacto |
|---|---|---|---|
| Entidade/tabela | `Mailbox` / `Mailboxes` | `ConversationLineItem` / `ConversationLineItem` | Código paralelo: não dá para reaproveitar `ProjectItemsService`, `MailboxSummary` nem `GetMailboxesExemptFromLicensing` sem adaptar. |
| Nomes dos endereços | `ExportEmailAddress`/`ImportEmailAddress` | `ExportUser`/`ImportUserPrincipalName` | Só de nomenclatura, mas aparece em todos os filtros. |
| Gatilho do handler | `Create | Update` | Só `Create` | No Mailbox, um update de linha **ainda não vinculada** tenta vincular na hora. No PCH, ela espera o sweep (até 12h). |
| CEU enviado pelo cliente na criação (REST) | Aceito | **Descartado** (`GenerateConversationLineItem` zera) | Ver seção 5. |
| CEU enviado pelo cliente na criação (SDK) | Aceito após `CheckCustomerEndUserExists` | Aceito **sem** checagem (vai direto ao `RunSync` como "existente") | Assimetria; ver seção 5. |
| DTO no update | `UpdateMailbox` sobrescreve com `GetValueOrDefault()`, então um update sem CEU **zera** o vínculo | `UpdateConversationLineItem` só sobrescreve se vier valor | Divergência proposital: o PCH corrige um defeito do irmão. O Mailbox fica como está (fora de escopo). |
| `SyncSubscriptionStatus` pelo DTO | — | Só leitura; no SDK, `[CannotUpdateProperty(PrivilegedExempt = true)]` | Status só muda por sync. |
| Feature exigida | UMB (OneDrive) / features de mailbox | `CollaborationChat` (`80941263…`), ausente do `LicensingData.xml` | Sem MW-12099 (ou troca do `RequiredFeatures` para a feature UMB, como no OneDrive), nenhuma UMB cobre PCH. |
| Bloqueio na submissão | `MailboxMigration` Full → `LicenseInsufficient` | Nenhum. `ConversationItemMigrationsService.GetEstimate` devolve licenças 0 e todas as linhas como válidas, com o código real comentado | Parte A (A2/A3). |
| Regra de projeto novo | Não existe (vale para todos) | Obrigatória (PRD 4.5). `IsNewProject` ainda é `false` fixo | Hoje o vínculo PCH está **dormente**: nada é vinculado até o critério ser definido. |
| Regras de revogação | Sem janela de tempo efetiva; Verification não conta como migração | 24h desde o consumo + qualquer job (Verification inclusa) bloqueia | Exigência do épico. Efeito colateral: um CEU que está em Mailbox **e** num PCH novo passa a seguir a regra mais rígida mesmo revogando pelo Mailbox. Confirmar com o arquiteto. |
| Azure Storage Tables | Não grava vínculo | Grava (merge) em create/update/discovery | Exigência do épico para o PCH ("existing Azure Storage Tables used for PCH licensing and job orchestration"). |
| Logs | Logs genéricos | Template estruturado `PCH UMB {UmbEvent}…` | Exigência de Kibana do PRD. |
| Workers | No repositório | Platform API, fora do repositório | Teste ponta a ponta exige ambiente BitTitan. |
| Front: modelo | `project-item` com `customerEndUser` (belongsTo) | `conversation-line-item` **sem** `customerEndUser` nem `syncSubscriptionStatus`, também na branch da Mel | O front de PCH não envia nem exibe vínculo. Os campos novos do DTO da Parte B ainda não são consumidos. |
| Front: Add from MSPComplete | Lista **todos** os CEUs do cliente e envia o `customerEndUser` escolhido | Branch da Mel: lista **só** CEUs com UMB ativa (`activeSubscription.subscriptionProcessStateList=[1]` + assinatura não vencida) e **não envia** o CEU; o vínculo sai do e-mail | Ver seção 5. |
| Front: toolbar | Apply/Remove UMB, Add from MSPComplete, Change Domains, Request Report, Reset Credentials, filtros Pre-Stage etc. | Todos escondidos por `isCollaborationChat`/`isTeamsPCH` (`templates/project/-project-items-old.hbs` L111, 226, 585, 687, 983…; `projects-properties.js` Change Domains). Exceção: Add from MSPComplete, liberado na branch da Mel | MW-12746, 12739, 12737, 13865. |

---

## 4. O que o PCH ainda precisa em relação aos outros projetos

**Backend: licenciamento (Parte A e dependências)**
1. **SKU/feature (MW-12099)**: incluir `CollaborationChat` na SKU UMB do `LicensingData.xml` (Prod e Germany) **ou** trocar o `RequiredFeatures` dos descritores do PCH para a feature UMB, como faz o OneDrive. Sem isso, nenhuma linha fica "coberta".
2. **Consulta de elegibilidade** para `ConversationLineItem.CustomerEndUserId` (equivalente a `GetMailboxesExemptFromLicensing`), A1.
3. **Estimativa real** em `ConversationItemMigrationsService.GetEstimate` (hoje fixa em 0), A2.
4. **Bloqueio na submissão** com mensagem acionável e log do motivo, nos tipos definidos (Full? Pre-Stage? Verification?), sem reconsumir em passes/retries, A3/A4/A6.
5. **Critério de "projeto novo"** implementado em `ConversationLineItemEventHandler.IsNewProject` (ponto único usado pelo handler e pelo sweep) e usado também pela Parte A no bloqueio. Hoje é o "interruptor" geral do vínculo.

**Backend: vínculo e endereços (Parte B / Feature 2)**
6. Decidir se o handler do PCH deve reagir também a `Update`, como o Mailbox, para não depender do sweep de 12h quando a linha é criada antes de o projeto ter cliente/endpoints.
7. **Change Domains** e mudança em lote para PCH: endpoint REST equivalente a `updateDomainBulk` para `ConversationLineItem`, preservando LineItemId e vínculo (MW-12737/12738).
8. **User mapping** com estados mapped/unmapped/conflicted e motivo do conflito (MW-11330). Não existe nada hoje.
9. Aceitar CEU vindo do cliente na criação, **com validação** (seção 5), pré-requisito do MW-12745.

**Frontend**
10. Modelo `conversation-line-item`: adicionar `customerEndUserId` e `syncSubscriptionStatus`. Nem a branch da Mel tem isso ainda.
11. Coluna "User Migration Bundle Active" mostrando "No" explicitamente, e ações Apply/Remove UMB (`attach-umb`) habilitadas para PCH (MW-12746).
12. "Add from MSPComplete" para PCH: em andamento na branch da Mel. Falta decidir se ela envia o CEU (seção 5) e resolver o destino, que hoje fica igual à origem.
13. Change Domains, Request Report (Project Statistics / Action Log, MW-13865) e demais ações da toolbar (MW-12739), cada ausência justificada.

**Produto/processo**
14. Loja e marketing (MW-12747), HLD, Confluence, plano de QA/TestRail, demo gravada, validação em Germany.
15. Respostas pendentes da Etapa 0 do plano: tipos de job que exigem licença, critério de projeto novo, janela 24h vs 48h, efeito da regra PCH em CEUs compartilhados com Mailbox, exclusão revogando ou não.

---

## 5. Comentário: `ConversationLineItemDTO.GenerateConversationLineItem` zerando o CEU (commit `85a0a2a92d`)

> **Atualização (08/11/2026), depois de ler a branch da Mel** `origin/feature/mel/MW-12745-add-from-mspcomplete-pch` (commit `210887ded6`, 06/11; único commit dela, sem branch correspondente no backend). Esta subseção substitui as partes da análise original abaixo que falavam de "id do front descartado".

### 5.1 O que a branch da Mel faz

- `models/project.js`: remove `!isTeamsPCH` de `supportsAddFromMspc`, o que libera "Add from MSPComplete" para PCH.
- `add-items-mspc.js` / `.hbs`:
  - Para PCH, a lista mostra **só CEUs com UMB ativa** (`activeSubscription.subscriptionProcessStateList = [1]` e `subscription.subscriptionEndDateGreaterThan = agora`), com o aviso `addUsersMspc.licensedUsersOnly`. O Mailbox lista todos.
  - Cria `conversation-line-item` com `project`, `projectId`, `exportUser: email` e `importUserPrincipalName: email`, onde `email = customerEndUser.displayEmailAddress`. **Não passa o `customerEndUser`** (o modelo nem tem esse campo).
  - Envia `POST api/conversationLineItems/createBulk` com `{ conversationItems: [...] }` (sem `status`) e dispara `didCreateRecord.conversation-line-item`, mesmo evento que o Discovery do PCH já usa.

### 5.2 O que combina com a minha branch (Parte B)

- **Contrato REST ok**: a rota e a chave raiz `conversationItems` batem com `ConversationItemsController.CreateBulk` (`ConversationLineItemListMessage.ConversationItems`). Os campos enviados (`exportUser`, `importUserPrincipalName`, `projectId`) são os do DTO.
- **O zeramento não atrapalha o fluxo da Mel**: como ela não manda CEU, a linha `CustomerEndUserId = Guid.Empty` do `GenerateConversationLineItem` não tem efeito nesse caminho. **Correção da minha conclusão anterior:** o id do front não chega para ser descartado. Ele nunca é enviado. O comentário do commit e a branch da Mel descrevem a mesma decisão vista dos dois lados: o back não aceita e o front não manda.
- **O vínculo sai pelo caminho que já implementei**: `createBulk` → entidade Core `Create` → `ConversationLineItemEventHandler` → `RunSync` por e-mail; se falhar, a varredura `ConversationLineItemToCustomerEndUserActivity`. Depois, `WriteSavedBindingsToTableStorage` grava o vínculo na tabela Azure. O `RunSync` filtra pelo `OrganizationId` do projeto, e a lista da Mel também (`organizationIdList = project.organizationId`), então os dois lados olham a mesma organização.
- **Ainda dormente**: com `IsNewProject = false`, as linhas criadas pela Mel **não são vinculadas** hoje. Para testar ponta a ponta localmente, ela precisa do critério de projeto novo, ou de forçá-lo localmente sem commitar.

### 5.3 Onde as duas branches não fecham (riscos reais)

1. **`displayEmailAddress` nem sempre é o e-mail primário.** No modelo `mspc/customer-end-user` (`bittitan-ember-lib/addon/models/mspc/customer-end-user.js:96`), ele vale `primaryEmailAddress`, **ou** `userPrincipalName`, **ou** `generatedIdentity`. O `RunSync` só procura CEU por **`PrimaryEmailAddress`**. Quando o CEU licenciado não tem e-mail primário (ou ele difere do UPN), o `RunSync` não acha o CEU escolhido e **cria um CEU novo, sem UMB**. Resultado: um usuário selecionado de uma lista "só licenciados" vira uma linha **"No"**. Para corrigir, o usuário reaplica a licença, e aí aparece o risco de consumo duplicado citado no épico (MW-12052/11977/11711). O filtro `primaryEmailAddressContains = _domains` só reduz esse risco quando o projeto tem domínios configurados; sem domínios, a lista não filtra nada.
2. **Destino igual à origem.** `importUserPrincipalName = email de origem`. O PCH não suporta mesmo tenant (PRD 5.2.2), então toda linha adicionada pelo MSPC vai precisar trocar o destino, e o **Change Domains continua escondido para PCH** (`projects-properties.js`). Na prática, cada linha teria que ser editada à mão. O Mailbox faz o mesmo (export = import = email), mas lá existe o Change Domains. Isso amarra o MW-12745 ao MW-12737. Pelo lado do vínculo não há problema: a edição de endereço mantém o CEU na Parte B.
3. **"Traceable" pela metade.** O MW-12745 pede que a associação de licença, mesmo não aplicada na importação, "must remain traceable for future license binding". Sem enviar o CEU, a única rastreabilidade é o e-mail, com a ambiguidade do item 1.
4. **Filtro "só licenciados" × reconhecimento real.** A UMB ativa no CEU não garante que ela cubra PCH enquanto o MW-12099 não incluir a feature `CollaborationChat` na SKU. A lista vai mostrar usuários que a Parte A ainda pode bloquear. É aceitável durante o desenvolvimento, mas vale alinhar a mensagem do aviso.
5. **Assimetria REST × SDK (continua valendo).** O SDK `ConversationLineItem` aceita CEU na criação sem nenhuma validação. A proteção do REST não cobre esse caminho.

### 5.4 Recomendação revista

Para alinhar com o irmão Mailbox e eliminar o item 1, vejo duas saídas:

- **(A) Recomendada: enviar e validar o CEU.**
  - Front (Mel): adicionar `customerEndUserId: DS.attr('string')` ao `conversation-line-item` e preencher com `customerEndUser.id` no Add from MSPComplete.
  - Back (eu): trocar o zeramento por validação. O CEU precisa existir **e** ter `OrganizationId` igual ao do projeto; se não, `PropertyReferenceInvalid`, como o `CheckCustomerEndUserExists` do Mailbox, só que mais forte. Aplicar a mesma validação no `Validate(Create)` do SDK, para cobrir o item 5.
  - O `RunSync` já aceita o CEU existente e cria os `GroupMembership`, então o resto do fluxo não muda.
  - Isso toca arquivos dos dois lados: combinar com a Mel e com o Leonardo (o DTO é contrato com a Parte A).
- **(B) Mínima, se (A) não couber agora:** manter o e-mail, mas a Mel usa `primaryEmailAddress` (não `displayEmailAddress`) e esconde ou desabilita CEUs sem e-mail primário, com explicação. O zeramento no back continua como está.

Nos dois casos: registrar no MW-12745 que o destino precisa de Change Domains para PCH (MW-12737), e testar o cenário "CEU licenciado sem e-mail primário / com UPN diferente".

---

*Análise original (antes de ler a branch da Mel):*

```csharp
// A client-supplied customer end user is not accepted on creation until it can be validated against the project's organization (MW-12745)
conversationLineItem.CustomerEndUserId = Guid.Empty;
```

**O que acontece hoje**
- No `POST api/conversationLineItems` / `createBulk`, `GenerateConversationLineItem` chama `UpdateConversationLineItem` (que copiaria o `CustomerEndUserId` do DTO, se viesse) e logo depois zera o campo.
- Com o CEU vazio, o vínculo é resolvido **por e-mail**: primeiro, de forma síncrona, pelo `ConversationLineItemEventHandler` no `Create`; se ele não conseguir (projeto sem cliente/endpoints, erro), pela varredura `ConversationLineItemToCustomerEndUserActivity` a cada 12h. Os dois chamam `RunSync`, que procura por `PrimaryEmailAddress` = `ExportUser` e depois = `ImportUserPrincipalName`, e **cria um CEU** se não achar.
- Detalhe importante: no estado atual da branch, `IsNewProject` retorna `false`, então **nem o handler nem o sweep vinculam nada**. O descarte só passa a ter efeito quando o critério de projeto novo existir.
- No front atual o modelo `conversation-line-item` não tem `customerEndUser`, então **hoje nenhum id chega**. O descarte vai morder quando o MW-12745 (Add from MSPComplete para PCH) for feito: o fluxo do Mailbox (`add-items-mspc.js`) manda exatamente o `customerEndUser` escolhido na lista do MSPC.

**Por que a proteção faz sentido**
- `RunSync` trata `existingCustomerEndUserId` como confiável: não verifica se o CEU existe, se é do cliente do projeto nem se o e-mail bate. Aceitar o id cru do cliente permitiria vincular uma linha ao CEU (e à UMB) de outra pessoa ou de outra organização, e com isso migrar sem licença, ou "gastar" a licença alheia. O épico pede explicitamente que o backend "prevent bypassing license checks".
- Zerar na criação é a postura mais segura enquanto não há validação, e mantém a regra "a licença acompanha o endereço de origem".

**Riscos e custos dessa escolha**
1. **Divergência do irmão.** O Mailbox aceita o CEU do cliente (REST sem checagem; SDK com `CheckCustomerEndUserExists`). Para o PCH, o PRD pede "reuse existing code paths" e comportamento idêntico no Add from MSPComplete.
2. **Resolução por e-mail pode errar onde o id acertaria.** Se o usuário escolhe no MSPC um CEU cujo `PrimaryEmailAddress` difere do endereço posto na linha (alias, UPN ≠ e-mail primário, o caso "UPN and licensed address differ" do épico), o `RunSync` não acha o CEU licenciado e **cria um CEU novo sem UMB**. A linha aparece como "No", o usuário reaplica a licença e há risco de **consumo duplicado**, justamente o problema histórico citado no épico (MW-12052, MW-11977, MW-11711).
3. **CEUs órfãos.** Cada linha sem correspondência gera um CEU de origem MigrationWiz no cliente.
4. **Assimetria REST × SDK.** O SDK `ConversationLineItem` não zera nem valida o CEU na criação: um id enviado por PowerShell/SDK vai direto ao `RunSync` como "existente". A proteção do REST pode ser contornada pelo SDK. **Vale verificar** se a criação de `ConversationLineItem` está exposta no SDK público. Se estiver, o buraco que o REST fecha continua aberto ali.
5. **Contrato implícito com o front.** O DTO expõe `CustomerEndUserId` (lido no GET e aceito no update), mas o ignora silenciosamente no create, sem erro. Um dev de front que implementar o MW-12745 vai achar que funcionou.

**Sugestão**
- Manter o zeramento **só até** existir validação, e registrar isso explicitamente no MW-12745 como pré-requisito (não deixar só no comentário do código).
- A validação deve ser **mais forte que a do Mailbox**: o CEU precisa existir **e** pertencer ao `OrganizationId` do projeto (o comentário do commit já aponta isso). Idealmente o mesmo método protege REST e SDK (por exemplo, validação no `Validate(Create)` do SDK `ConversationLineItem`, usada pelos dois caminhos). Se inválido, rejeitar com `PropertyReferenceInvalid`, como o Mailbox, em vez de descartar em silêncio.
- Enquanto isso, testar o cenário "CEU licenciado com e-mail primário ≠ endereço da linha" para medir o risco 2, e logar `BindingCreated` com motivo "Customer end user created" (já existe), para que o Suporte consiga achar CEUs criados indevidamente no Kibana.
- Ao implementar: revisar o `IsNewProject` junto, porque o comportamento real de todo esse fluxo só aparece quando ele deixar de ser `false`.
