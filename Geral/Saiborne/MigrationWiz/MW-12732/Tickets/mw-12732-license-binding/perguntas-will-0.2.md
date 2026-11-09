Status: needs-info
Blocked by: None

# Perguntas para o Will: licença UMB no Teams Private Chat (MW-12732)

Contexto: estamos ligando cada linha de Teams Private Chat (PCH) ao usuário do MSPComplete e à UMB dele, como já acontece no Mailbox. Para mexer só nos projetos novos, os existentes ficam como estão.

1. **O que conta como "projeto novo"?** (bloqueia o PR) Sem essa definição, nenhuma linha é ligada à UMB. A proposta do claude: o projeto PCH é novo se foi criado a partir de uma data de corte, isso atende? A data de corte é a do release ou a da ativação da feature flag? Algum cliente piloto precisa entrar antes?
   - *Fonte:* PRD 4.5 ("só projetos novos").
1. **Excluir uma linha de PCH devolve a UMB?** A implementação seguiu como no Mailbox: a exclusão devolve a UMB se ainda estiver dentro de 24h do consumo e sem nenhum job. Como a UMB vale para todos os workloads, isso pode devolver a licença que um projeto Mailbox do mesmo usuário ainda ia usar. Isso já acontece hoje entre Mailbox e OneDrive. Está ok assim? A devolução vale também para a exclusão de uma linha só? No Mailbox, só a exclusão em lote devolve.
   - *Fonte:* nem o PRD nem o Jira falam de exclusão. Seguimos os pedidos de reaproveitar o Mailbox e o OneDrive: MW-12732, "License management UI/SDK logic for PCH must mirror the current UMB handling model"; MW-12746, "reuse the Apply/Remove License logic from Mailbox and OneDrive".
3. **Azure Tables:** gravamos o vínculo (usuário e data da sincronização) na tabela Azure do PCH, ou não gravamos nada? O que é o "support infrastructure database" citado no épico? É aceitável que as linhas criadas pela discovery, pelo SDK e pela rotina assíncrona de vínculo não sejam gravadas no Azure? É que no item 5.6 do PRD tem escrito "All updates related to code changes, license bindings, and project state transitions must be logged in both Azure Storage Tables and the support infrastructure database, for backend integrity, auditability, and consistent system-wide visibility.". EStou perguntando porque o Mailbox e o OneDrive não gravam o vínculo em Azure Tables, só no SQL. Espelho como no mailbox, ou faço esse fluxo novo do PCH no azure?
   - *Fonte:* PRD 5.6. MW-12732: "Store license bindings via existing Azure Storage Tables used for PCH licensing and job orchestration" e "license state changes and bindings are written to Azure Storage Tables and the DB". A frase do "support infrastructure database" se repete no MW-12732, no MW-12745 e no MW-12746.
4. **Troca de endereço:** o épico diz que o vínculo se mantém "se o prefixo do UPN não mudar". No Mailbox, porém, o vínculo se mantém em qualquer troca, e o fluxo de ajuste de UPN do próprio épico muda o prefixo. Mantemos em qualquer troca, como no Mailbox?
   - *Fonte:* MW-12732, "Persistence through domain suffix changes, as long as the user identity (UPN prefix) remains unchanged" e "license binding depends on exact UPN prefix match", em contraste com o item "Address Adjustment for UPN Matching" do mesmo ticket. O MW-12747 repete a regra do prefixo.

## Aviso (seguimos assim, salvo objeção)

**Regra de devolução da UMB:** seguindo o MW-12732, a UMB só pode ser devolvida dentro de 24h do consumo e se nenhum job foi submetido, inclusive o Verify Credentials. A regra vale apenas para usuários com pelo menos uma linha PCH; os demais continuam como hoje (sem prazo). Efeito colateral: como a UMB é uma só por usuário, quem está num projeto Mailbox e num projeto PCH passa a ter essa regra também ao devolver a licença pelo Mailbox.
- *Fonte:* MW-12732, "They can only be revoked within a 24-hour grace period, and only if no job (e.g., Verify Credentials, Pre-Stage, or Full Migration) has been submitted". O MW-12747 diz "non-transferable after 24 hours".

## Lembretes para o Leo (enviar depois do PR para a branch de integração)

1. **Critério de "projeto novo" fica com ele, e é o único interruptor.** Não há feature flag: a `pch-umb-binding` foi removida, e nem ele precisa criar uma. Hoje `ConversationLineItemEventHandler.IsNewProject` (`MigrationCommon\Sdk\Api\1.0\Events\ConversationLineItemEventHandler.cs`) retorna `false`, e por isso a Parte B está inerte. Ele deve implementar a regra **nesse método** e a Parte A (passo A5) deve chamá-lo. Assim o vínculo e o bloqueio ligam juntos, pelo mesmo critério. Com dois critérios, uma linha pode ficar sem CEU e ser bloqueada como "sem licença".
   - Sugestão: o projeto é novo se a `CreateDate` do connector for igual ou posterior à data em que a flag for ligada em produção (PRD 4.5: "created after the release date"). A data deve ficar numa configuração, não numa constante.
   - A varredura de 12h (`ConversationLineItemToCustomerEndUserActivity.IsNewProject`) delega para o mesmo método. **Importante sem a flag:** a varredura passa a ler todas as linhas PCH sem vínculo a cada execução, inclusive as de projetos antigos, que ela descarta. Colocar o critério no filtro SQL da varredura, via `MailboxConnectors.CreateDate`, para evitar essa leitura em massa.
   - Os testes de integração com `isNewProject: null` esperam hoje "não faz nada" e precisam ser revistos.
   - Detalhes: `lembrete-0.2c-criterio-projeto-novo.md`, seção "Implementação sugerida".
2. **Contrato da Parte A:** ler só `ConversationLineItem.CustomerEndUserId`. `Guid.Empty` significa "sem vínculo", e a linha é tratada como sem licença.
3. **Regra de devolução atualizada:** para quem tem PCH, qualquer job, inclusive o Verify Credentials, impede a devolução da UMB, e só dentro de 24h do consumo. Se a UI da Parte A mostrar alguma mensagem sobre devolução, ela deve seguir essa regra.
4. **Validação ponta a ponta (ticket 10):** pode ser feita por qualquer um de nós depois que o critério de "projeto novo" entrar.
5. **Provisionamento retroativo (PRD 4.5 e 5.2.1):** usuários que já tinham UMB antes do release precisam passar a ter o PCH liberado. Não está em nenhum ticket da Parte B. É da Parte A ou do MW-12099?
6. **Atualizar a branch dele com a integração:** a Parte B já está em `feature/MW-12732-pch-umb-licensing`. Ele deve trazer essa branch para a dele e trocar o mock do `CustomerEndUserId` pelas colunas reais (B0).
7. **Não duplicar a regra de devolução:** a regra de 24h e de nenhum job já está no `RevokeSubscriptionRequest` compartilhado (ADR-0004). O "Remove License" do MW-12746 deve só chamar a revogação existente, sem validar de novo.
8. **Exclusão:** só a exclusão em lote devolve a UMB; a exclusão de uma linha só não devolve (igual ao Mailbox).
9. **Deploy:** o `ConversationLineItem.sql` **precisa** estar publicado em produção antes do código. Sem a flag, o código antes do SQL quebra também a revogação, o consumo e a expiração de UMB do Mailbox e do OneDrive. Citar no PR final para o `develop`.
10. **Revisão cruzada:** revisar o PR da Parte B, que eu mesmo aprovei. Eu reviso o PR dele.
