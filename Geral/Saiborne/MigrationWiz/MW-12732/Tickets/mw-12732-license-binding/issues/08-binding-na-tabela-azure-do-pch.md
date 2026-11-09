Status: done
Blocked by: 02, 03

# 08: Binding gravado na tabela Azure do PCH (B6)

## O que construir

Para cumprir o PRD 5.6 (bindings registrados em Azure Storage Tables), criar, criar em lote, editar ou descobrir uma conversation line item pela API REST grava o CEU e a data do binding na tabela Azure que o PCH já usa para as linhas, com a mesma PartitionKey (connector) e RowKey (line item), por merge. O status de UMB **não** é gravado no Azure, porque muda fora da camada REST e ficaria desatualizado; a fonte do status continua sendo o SQL.

Uma falha no Azure não derruba a operação: é registrada com `exception.LogError` e a criação ou edição segue. Linhas criadas pelo SDK não chegam ao Azure (lacuna levada ao arquiteto).

## Critérios de aceite

- [ ] Gravação feita pela dependência isolada no ticket 02
- [ ] CEU e data lidos da entidade já salva (mesmo valor do SQL)
- [ ] Sem gravação de `SyncSubscriptionStatus` no Azure
- [ ] Falha no Azure logada e não propagada
- [ ] Nenhuma tabela nova
- [ ] Testes unitários passam

## Seams under test

- unit: `ConversationLineItemsService.CreateBulk` grava `CustomerEndUserId` e `MspcSynchronizationDate` na tabela Azure (mock)
- unit: `ConversationLineItemsService.Update` grava `CustomerEndUserId` e `MspcSynchronizationDate` na tabela Azure (mock)
- unit: `ConversationLineItemsService.CreateBulk` conclui a criação e loga o erro quando a tabela Azure falha
- unit: `ConversationLineItemsService` nunca grava `SyncSubscriptionStatus` na tabela Azure

## Pendências

- **0.2e (arquiteto/Will):** recomendação (b), esta; alternativa (a) é não gravar nada no Azure (só canais do Mailbox). Também: o que é o "support infrastructure database" e se a lacuna do SDK é aceitável. Se a resposta for (a), este ticket vira `wontfix`.

## Comments

- 2026-11-05: Implementado no working tree (sem commit). `ConversationLineItemsService.WriteBindingsToTableStorage` (público) faz merge de `CustomerEndUserId` e `MspcSynchronizationDate` (UTC) via `IConversationLineItemTableStorage`, com PartitionKey = connector e RowKey = line item. Ignora linhas sem binding e resolve o storage uma vez por connector. Falhas de leitura ou do Azure são logadas e não propagadas. `Create`, `CreateBulk` e `Update` releem a entidade salva e chamam o método.
  - Seams ajustadas com o Felipe: os testes unitários ficam em `WriteBindingsToTableStorage`, porque `CreateBulk`/`Update` passam pelo ORM, e em teste o handler nunca grava a data enquanto `IsNewProject` = false (0.2c). Há também 1 teste de integração em `Update`, escrito e compilado, mas não executado.
  - Lacuna (decisão do Felipe, fora deste ticket): a discovery do PCH (`DiscoveryTasksController` → `ImportEntities`) não passa pelo serviço e não grava no Azure. Também não gravam no Azure as linhas criadas pelo SDK nem as vinculadas pela varredura assíncrona (N4). Levar ao arquiteto junto com a 0.2e.
- 2026-11-05: PRD 5.6 torna a gravação obrigatória (opção b), e a lacuna deixa de ser aceitável. A discovery do PCH (`DiscoveryTasksController`, caso `TeamsPrivateChat`) passou a chamar `ConversationLineItemsService.WriteSavedBindingsToTableStorage` (agora público) com as linhas criadas. Pendentes, aguardando decisão: o SDK e a varredura assíncrona (o resolvedor de credenciais Azure do PCH só existe no projeto REST e lê chaves de configuração desse host).
- 2026-11-05: decisão do Felipe: o SDK e a varredura ficam fora deste PR, para não refatorar código de produção. As chaves `PrivateChatStorage*_<ZONA>` existem só no `MigrationWiz\Web.config`; nos configs do `AEFExecutionService` elas não aparecem. Se o DevOps confirmar que o AEF de produção recebe essas chaves por variável de ambiente, a varredura pode chamar `WriteBindingsToTableStorage` depois do `Bind` (mudança só de acréscimo). Limitação conhecida a citar no PR: linhas criadas pelo SDK ou vinculadas pela varredura não são registradas no Azure; o vínculo continua no SQL.
