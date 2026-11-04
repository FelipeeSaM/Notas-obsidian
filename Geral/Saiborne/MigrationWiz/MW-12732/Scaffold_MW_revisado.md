---
tags: [migrationwiz, impacto, PRD, MW-12732, MW-12745, PCH, UMB]
base: "[[Scaffold_MigrationWiz_Feature1]]"
gerado_em: 2026-11-03
branch: feature/MW-12732-pch-umb-licensing (a37f211b00)
fontes: ["Teams_PCH_Licensing_Enablement_PRD_1.1.pdf (4.5, 5.2, 5.3, 5.6, 7.1, 7.2)", "jira-tickets-mw.txt (épico + MW-12745, MW-12746, MW-12747)", "varredura C:\\$\\Development + C:\\$\\Front\\FrontEnd"]
---

# Scaffold MW revisado: impacto de código da Feature 1 (PCH + UMB)

> [!summary] Conclusão
> O fluxo UMB inteiro já existe para **Mailbox/OneDrive**, e o PCH ficou de fora em quatro pontos:
> 1. O **catálogo** (`LicensingData.xml`) não tem a feature CollaborationChat ligada ao UMB.
> 2. O line item PCH (**`ConversationLineItem`**) não tem binding com o CustomerEndUser em nenhuma camada (SQL, Core, SDK, DTO, model Ember).
> 3. A **submissão** PCH (Platform API) não passa por nenhuma checagem de licença.
> 4. A **UI** desliga as ações por `isTeamsPCH` / `isCollaborationChat`.
>
> A Feature 1 consiste em **estender esses caminhos ao `ConversationLineItem`**, sem criar arquitetura nova.

Tabela de projetos, padrões de codificação e lista de arquivos novos (N1–N13): [[Scaffold_MigrationWiz_Feature1]].

---

## 1. Módulos impactados (visão por projeto)

| # | Projeto / Módulo | .cs hoje | Arquivos a alterar | Arquivos novos | Impacto | Tickets |
|---|---|---|---|---|---|---|
| 1 | **Deployment** (LicensingData.xml Prod + Germany) | 2 XML | 2 (×2 branches: master e develop) | 0 | 🔴 Crítico (pré-requisito) | 12099, 12732 |
| 2 | **Databases\MigrationWiz** (SSDT) | — | `Tables\ConversationLineItem.sql` | script de upgrade | 🟠 Médio | 12732 |
| 3 | **MigrationCommon** | 1738 | ~6: Core e Sdk `ConversationLineItem`, `LicenseManager`, `ConsumePrepaidSubscriptionRequest`, `RevokeSubscriptionRequest`, `SyncExpiredSubscriptionsRequest` | `ConversationLineItemEventHandler` | 🔴 Alto | 12732, 12746 |
| 4 | **Common** | 1438 | ~4: `SubscriptionEntitlementManager`, `Access\…\ConversationLineItem`, `FeatureToggleNames`, MWActionLog | 0 | 🟠 Médio | 12732, 12099 |
| 5 | **EmailClientCommon** | 131 | `MigrationWizToMspcSyncingService` (+ `SyncConversationLineItems`) | 0 | 🟠 Médio | 12732, 12745 |
| 6 | **MigrationWiz** (REST) | 628 | ~11: `ConversationLineItemDTO`, `ConversationLineItemsService`, `ConversationItemsController`, `ConversationItemFindRequest`, `LicenseAttachService`/`Controller`, `ConversationItemMigrationsService`/`Controller`, `PchMigrationEstimatesController`, `ProjectDTO` | 5: `PchLicenseValidator` + interface, `ConversationItemSubscriptionRequest`, `PchLicenseBindingTableEntity`, `PchLicenseBindingStorageService` | 🔴 Alto | 12732, 12745, 12746 |
| 7 | **AEFWorkflows** | — | Subscription Processing, Revoke e Expired; workflow de Synchronization | `ConversationLineItemToCustomerEndUserActivity` | 🟠 Médio | 12732 |
| 8 | **AEFInternalAutomationWorkflows** | — | `AuditLicenseLogActivity`, `AuditSubscriptionAbuseActivity` | 0 | 🟡 Baixo | 12732 |
| 9 | **BitTitanWebAPI** | 377 | `RestApi\Services\SubscriptionService.cs` (validar; não tocar nos gerados) | 0 | 🟡 Baixo / validar | 12746 |
| 10 | **BitTitanInternalWebApp** | 251 | 0 (configuração via "View/Edit Product SKU") | 0 | ⚪ Configuração | 12099 |
| 11 | **migrationwiz-app** (Ember) | — | ~12: `project.js`, `add-items-mspc` js+hbs, `conversation-line-item` model+serializer, `controllers/project/project-items.js`, `-project-items-old.hbs`, `attach-umb`, `actions/project-items.js`, `project-item-billing`, `start-migration` js+hbs, `projects-properties` | validator PCH | 🔴 Alto | 12745, 12746, 12732 |
| 12 | **bittitan-i18n** | — | `migrationwiz-app\en-PT`, `store-app\en-PT` | 0 | 🟡 Baixo | 12745–12747 |
| 13 | **store-app** | — | `product-card-700002.hbs`, `user-migration-bundle.hbs` | 0 | 🟡 Baixo | 12747 |
| 14 | **Testes** (UnitTestMigrationWiz, UnitTestCommonComponents, IntegrationTests, ApiTests, AEF\*, TestCommon, testes FE) | 564 / 475 / 431 / 258 / 188 / 47 | ~12 suites existentes | ~7 suites novas | 🟠 Médio | todos |

**Sem alteração (PRD 5.2.2 e análise):**
- `TeamsChatMigrationService` e o engine/rehydration PCH.
- `PlatformApiClient`: só passa a ser chamado *depois* da validação.
- Os módulos Clean Architecture (Google\*/M365\*).
- MigrationWizWebApp (SOAP) e PortalWebApp.
- **mspcomplete-app:** a licença é por usuário (`activeSubscription`), sem visão por workload. Só precisa de verificação E2E de que o consumo PCH aparece em Settings > Licenses.

---

## 2. Rastreabilidade: requisito → módulo → arquivos

| Requisito (fonte) | Módulo | Arquivos-chave |
|---|---|---|
| PCH reconhecido como workload válido para UMB (épico, Tech Req 1; MW-12099) | Deployment, Common, MigrationCommon | `Deployment\{Prod,Germany}\LicensingData.xml` (Feature `80941263-…` + ProductSkuFeature UMB 700002); `SubscriptionEntitlementManager.cs:18-47`; `LicenseManager.GetEligibleProductSkuIds` (`:732`, alternativa no estilo T2T em `:739`) |
| Licença ligada ao **endereço de origem** na associação do usuário (épico, Tech Req 2) | Databases, MigrationCommon, Common, EmailClientCommon | `ConversationLineItem.sql`; Core, Sdk e Access `ConversationLineItem.cs`; **novo** `ConversationLineItemEventHandler`; `MigrationWizToMspcSyncingService.RunSync` (`:116`) |
| Binding em **Azure Storage Tables + DB** para auditoria (épico; PRD 5.6) | MigrationWiz, Common | **novos** `PchLicenseBindingTableEntity`/`StorageService`; `LicenseLog` via `LicenseManager.UseLicenses` (`:2424`); `MWActionLog` |
| **Bloquear** Verify, Pre-Stage e Full sem licença, com mensagem clara (épico; PRD 5.3 AC1) | MigrationWiz, migrationwiz-app | `ConversationItemMigrationsService.CreateBulk` (`:107`) + **novo** `PchLicenseValidator`; `ErrorCode.LicenseInsufficient`; `start-migration.js:900,961` + `.hbs:247-255` |
| Status e consumo visíveis no projeto durante todo o ciclo (PRD 5.3 AC4) | MigrationWiz, migrationwiz-app | `ConversationItemMigrationsService.GetEstimate` (`:296-337`, hoje zerado); `ProjectDTO.IsCustomerEndUserSubscriptionApplicable`; `-project-items-old.hbs:1359-1361,1441-1455` |
| Coluna **"User Migration Bundle Active"** com Yes/**No** explícito (MW-12746) | migrationwiz-app, MigrationWiz | `conversation-line-item.js` (+ `syncSubscriptionStatus`); `ConversationLineItemDTO`; template do grid |
| **Apply / Remove License** na toolbar (MW-12746) | migrationwiz-app, MigrationWiz, MigrationCommon, BitTitanWebAPI, AEF | `controllers\project\project-items.js:28`; `-project-items-old.hbs:226-248,711-746`; `attach-umb.js`; `actions\project-items.js:596-636`; `project-item-billing.js`; `LicenseAttachService.cs:71/118/207`; `ConsumePrepaidSubscriptionRequest`; `RevokeSubscriptionRequest` |
| Revogação só dentro de **24h** e sem job submetido (épico; MW-12746) | MigrationCommon, MigrationWiz | `RevokeSubscriptionRequest.cs:30` (48h, não usada) e `:270-305`; `LineItemQueueTableEntity` / `ConversationLineItem.Status` |
| **Add from MSPComplete** como 1º item do Add (MW-12745) | migrationwiz-app, MigrationWiz | ver **seção 3** |
| Retries e passes **não consomem** licença extra; sem over-consumption em vários projetos (épico; MW-12052/11977/11711) | MigrationCommon | `ConsumePrepaidSubscriptionRequest` (`FilterAndFixAlreadyProcessedSubscriptions`, `DeleteInvalidSubscriptions`); "1 UMB por CEU"; o PCH reutiliza o CEU existente |
| Persistência em **Change Domain** / SDK bulk (contrato com a Feature 2) | MigrationCommon, MigrationWiz, migrationwiz-app | Sdk `ConversationLineItem` preservando CEU/MspcSyncDate (como `Sdk\Objects\Mailbox.cs:696-701`); early return no handler (como `MailboxEventHandler.cs:58-69`); endpoint de domínio PCH (modelo `ProjectItemsService.UpdateDomainBulk:1051` + `DomainReplacement.cs:17`) |
| **Provisionamento retroativo** de UMBs já aplicadas (PRD 4.5) | AEFWorkflows | **novo** `ConversationLineItemToCustomerEndUserActivity` |
| Somente **projetos novos** (PRD 4.5) | Common, MigrationWiz | `FeatureToggleNames.cs` + data de criação do connector no `PchLicenseValidator` |
| Logs no **Kibana** com motivo de bloqueio e métricas de consumo (PRD 5.3, 7.2) | BTLog, Common, MigrationWiz | Serilog estruturado; `MWActionLogService.LogActionForItems` |
| Store e marketing com PCH sob UMB (MW-12747) | store-app, bittitan-i18n | `product-card-700002.hbs:16-40`, `user-migration-bundle.hbs:2-21`, `store-app\en-PT:209-226` |
| SKU validado em **Germany** e **Prod** (MW-12099) | Deployment, BitTitanInternalWebApp | os dois `LicensingData.xml`; telas `ViewProductSku`/`CreateEditProductSku` |

---

## 3. Recorte do MW-12745: Add from MSPComplete no PCH (insumo para o `/grill-with-docs`)

### 3.1 Como funciona hoje para Mailbox/OneDrive

1. **Toolbar.** O dropdown Add fica em `templates\project\-project-items-old.hbs:82-131`, nesta ordem: Manage PST → **Add from MSPComplete** (`:71-78`) → Quick Add → Bulk Add → Autodiscover. Também aparece no empty state (`templates\components\project-items\list-empty.hbs:45-54`).
   - ⚠️ O ticket pede "Add from MSPComplete" como **primeiro** item. Hoje Manage PST vem antes, mas não se aplica ao PCH, então na prática ele já seria o primeiro. Confirmar.
2. **Gate.** `models\project.js:904-923` (`supportsAddFromMspc`). A exclusão do PCH é **`!isTeamsPCH` na linha 917**. As demais condições são: não ser T2T, não ser GG→M365, ter `organizationId` e ter `emailAddressSupported` nos dois lados. Os descritores PCH têm `IdentifierRequirements=EmailAddress` (`CollaborationChatDescriptors.xml:30,62,95,127`).
3. **Rota e flyout.** A rota é `project.project-items.addItemsMspc` (`router.js:18`). O template `templates\project\project-items\add-items-mspc.hbs` usa um flyout à esquerda e renderiza `{{project/add-items-mspc}}`.
4. **Componente** (`components\project\add-items-mspc.js`, 341 linhas, + `.hbs`, 142 linhas):
   - **Lista:** `infinite-list` de `mspc/customer-end-user`, que chama `GET {bitTitan}/api/customerEndUsers` com `organizationIdList`, `primaryEmailAddressContains: _domains` e `orderBy`.
   - **Busca e filtro de grupo:** o filtro usa `mspc/customer-group` (`GET /api/customerGroups`).
   - **Colunas e ação:** checkbox, Email, First Name, Last Name; botão "Save Users and Close".
   - **Não há filtro de "somente licenciados"**, embora o model tenha `hasActiveSubscription`.
5. **Criação.** A task `_addItems` faz `store.queryAll` dos selecionados e então:
   1. `createRecord('project-item', {project, customerEndUser, exportEmailAddress, importEmailAddress})`;
   2. valida com o `ProjectItemValidator` local (`:21-51`);
   3. faz `POST {migrationWiz}/api/projectItems/createBulk {projectItems:[…]}` em lotes de 100;
   4. dispara o alerta de sucesso e o evento `didCreateRecord.project-item`.
6. **Backend.** No Mailbox, o `CustomerEndUserId` vai no DTO. A entidade `Mailbox` tem `[EventHandler]`, que sincroniza com o MSPC (`MailboxEventHandler` → `RunSync`).

### 3.2 O que muda para o PCH

| Camada | Arquivo | Mudança |
|---|---|---|
| FE: gate | `app\models\project.js:917` | Remover `!isTeamsPCH`, possivelmente atrás de feature flag. Atualizar `tests\unit\models\project-test.js:652-744`. |
| FE: componente | `app\components\project\add-items-mspc.js` | Ramo `project.isTeamsPCH`:<br>- `createRecord('conversation-line-item', { project, projectId, customerEndUser, exportUser: displayEmailAddress, importUserPrincipalName: displayEmailAddress })`<br>- validator PCH (N12)<br>- `POST /api/conversationLineItems/createBulk` com root **`conversationItems`** (como em `bulk-add-item.js:1867-1886`; remover o `status` nulo como em `:2124-2131`)<br>- `storeEventsService.trigger('didCreateRecord.conversation-line-item')` |
| FE: template | `app\templates\components\project\add-items-mspc.hbs` | Textos genéricos ("users" no lugar de "mailboxes") e, se aprovado, filtro/coluna de licença |
| FE: model/serializer | `app\models\conversation-line-item.js`, `app\serializers\conversation-line-item.js` | `customerEndUser: DS.belongsTo('mspc/customer-end-user')`, serializado como `customerEndUserId` |
| FE: i18n | `bittitan-i18n\translations\migrationwiz-app\en-PT\EmberAppTranslations.json` (`addUsersMspc.*` `:126-128`) | Novas strings, se houver |
| FE: testes | `tests\unit\components\project\add-items-mspc-test.js`, `tests\integration\components\project\add-items-mspc-test.js`, `tests\unit\serializers\conversation-line-item-test.js` | Casos PCH: payload, endpoint, validação, evento |
| BE: DTO | `MigrationWiz\Api\DTOs\ConversationLineItemDTO.cs` | `public Guid? CustomerEndUserId` + mapeamento nos dois sentidos |
| BE: controller e serviço | `ConversationItemsController.CreateBulk` (`:157`), `ConversationLineItemsService.CreateBulk` (`:152-179`) | Aceitar e persistir o CEU. Validar que o CEU pertence à organização do projeto (o ticket pede "tenant scope"). O MWActionLog `ItemAdded` **já existe** (`:166-175`). Somar a auditoria na tabela Azure (N5). |
| BE: entidade | Core e Access `ConversationLineItem` + SQL | Colunas `CustomerEndUserId`, `MspcSynchronizationDate`, `SyncSubscriptionStatus` (compartilhado com MW-12732) |
| BE: binding | `ConversationLineItemEventHandler` (N3) | Se o CEU veio do MSPC, gravar a binding direto. Se não veio (Quick/Bulk Add ou SDK), resolver por endereço via `RunSync`. |

### 3.3 Perguntas para o grill (decisões em aberto do MW-12745)

- [ ] **"Somente usuários licenciados".** O ticket diz que só usuários com UMB ativo podem ser importados, mas também diz que "license association is not applied at the point of import". Filtrar a lista por `hasActiveSubscription`, só sinalizar, ou não filtrar (igual ao Mailbox hoje)?
- [ ] **Qual endereço vira `exportUser` e qual vira `importUserPrincipalName`?** No Mailbox, os dois recebem `displayEmailAddress`. No PCH o destino é um **UPN**: usar `userPrincipalName` do CEU no lado import?
- [ ] **Filtro por domínio.** `_domains` vem de `exportSettings.domains`/`importSettings.domains`. Os settings PCH expõem `domains`? Se não, a lista vem vazia ou sem filtro.
- [ ] **Dependência do backend.** O MW-12745 depende das colunas CEU no `ConversationLineItem`, que pertencem ao MW-12732. Quem cria a migração SQL, e em que ordem?
- [ ] **Feature flag.** O "Add from MSPC" para PCH entra atrás do mesmo flag de enforcement (só projetos novos)?
- [ ] **Auditoria.** Basta o `MWActionLog(ItemAdded)` atual, ou é preciso também a tabela Azure de binding (PRD 5.6 pede as duas)?
- [ ] **Ordem do dropdown.** Garantir "Add from MSPComplete" como 1º item mesmo se Manage PST um dia valer para PCH?

---

## 4. Ordem de implementação sugerida

1. **Catálogo** (MW-12099): LicensingData.xml (Prod + Germany, master + develop) e `SubscriptionEntitlementManager`. *Sem isso, nada funciona.*
2. **Persistência:** colunas SQL e entidades Core, Sdk, Access e DTO do `ConversationLineItem`, mais model e serializer Ember.
3. **Binding:** `ConversationLineItemEventHandler`, `SyncConversationLineItems` e a Activity AEF de reparo/retroativo.
4. **MW-12745 (Add from MSPC):** desbloqueia o caminho "usuário com CEU" de ponta a ponta. Depende de 2 e 3.
5. **Apply/Revoke** (MW-12746): `LicenseAttachService` + request PCH, Consume/Revoke (janela + job no Azure), tabela de auditoria, coluna e filtro UMB.
6. **Enforcement** (MW-12732): `PchLicenseValidator` em `CreateBulk`, `GetEstimate` real, feature flag e Start Migration.
7. **Store** (MW-12747) e i18n.
8. **Testes:** unitários, API, integração, AEF e Ember, seguidos da regressão da toolbar (ponte com a Feature 3).

---

## 5. Pontos em aberto para o PM/Arquiteto (HLD)

- [ ] Janela de revogação: o requisito diz **24h**; o código declara 48h e não usa.
- [ ] Elegibilidade: `ProductSkuFeature` no LicensingData **ou** atalho no `GetEligibleProductSkuIds` (como T2T)? Quais SKUs UMB (padrão, Incentive, EDU) recebem a feature?
- [ ] Pre-Stage no PCH: como distinguir de Full no `ScheduleMigrationAsync` para aplicar o bloqueio?
- [ ] Onde fica o "audit log": `MWActionLog`, `LicenseLog`, tabela Azure nova, ou todos?
- [ ] "Só projetos novos": feature flag, data de criação do connector, ou os dois?
- [ ] Provisionamento retroativo: job único (script/Activity) ou reparo contínuo pela Activity de Synchronization?
