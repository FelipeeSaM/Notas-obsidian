# RULES.md — RadarPet Flutter

Lista objetiva de decisões já tomadas neste projeto. Consolidado de `CLAUDE.md` (raiz) e `App-back/CLAUDE.md`. Se uma dessas regras parecer errada pra tarefa atual, é sinal de perguntar ao usuário antes de quebrá-la — não de ignorá-la silenciosamente.

---

## Arquitetura & State Management

✅ Fazer: 1 Event explícito por ação do usuário (`AuthLoginSubmitted`, `PetCriarSubmitted`, `AlertaListaSolicitada`)
❌ Não fazer: usar Cubit ou métodos diretos sem Event — decisão revertida explicitamente neste projeto, Bloc completo é o padrão final

✅ Fazer: Event/Bloc/State como `sealed class` por feature (`auth_event.dart`, `auth_bloc.dart`, `auth_state.dart`)
❌ Não fazer: um Bloc por tela — o Bloc é por feature, pode alimentar mais de uma página

✅ Fazer: Bloc chama só o Repository da própria feature
❌ Não fazer: Bloc chamar `dio` diretamente, ou chamar Repository de outra feature

---

## Comunicação com back-end

✅ Fazer: abrir `<Slice>Command.cs`/`<Slice>Query.cs` + `<Slice>Endpoint.cs` reais em `App-back/AplicativoPet.Api/Features/<Dominio>/` antes de codar qualquer chamada HTTP
❌ Não fazer: assumir formato de request/response de memória ou de conversa anterior — o back-end pode ter mudado

✅ Fazer: campos do DTO Dart em `camelCase`, espelhando o record C# (JSON já sai em `camelCase` por padrão do ASP.NET Core)
❌ Não fazer: traduzir nomes de campo pra inglês, ou usar `snake_case`

✅ Fazer: confiar que o back-end resolve `TutorId` a partir do claim `sub` do JWT
❌ Não fazer: enviar `TutorId` manualmente no body/query de qualquer requisição autenticada

✅ Fazer: todas as chamadas passam pelo **Gateway** (`localhost:6000`/`6060` em dev)
❌ Não fazer: apontar o Dio pra API diretamente, pulando o Gateway

---

## Resiliência de rede

✅ Fazer: retry automático (2–3 tentativas, backoff 500ms/1s/2s) só em métodos idempotentes (**GET**)
❌ Não fazer: retry automático em POST/PUT — regra fixa até a Idempotency-Key estar implementada no back-end

✅ Fazer: `RetryInterceptor` só reage a falha de **transporte** (`err.type` em `connectionTimeout`/`sendTimeout`/`receiveTimeout`/`connectionError`)
❌ Não fazer: retry em qualquer `onError` de GET — um `401`/`404`/`500` é resposta válida do servidor (`DioExceptionType.badResponse`), não falha de rede; repetir um `401` só atrasa o `AuthInterceptor` entrar em ação

✅ Fazer: registrar `authInterceptor` **antes** de `retryInterceptor` em `core/network/dio_client.dart`
❌ Não fazer: inverter essa ordem — o Dio roda `onError` em ordem **FIFO** (ordem de registro, confirmado no código-fonte e na doc oficial do pacote — não é LIFO); com `authInterceptor` primeiro, ele vê o erro antes, cuida do `401`, e repassa o resto (via `handler.next()`) pro `RetryInterceptor` decidir sobre retry de falha de transporte

✅ Fazer: gerar `Idempotency-Key` (uuid) e mandar no header em mutações, deixando pronta a integração futura
❌ Não fazer: assumir que o back-end já checa essa key — ele **não checa ainda** (pendência documentada nos dois `CLAUDE.md`)

✅ Fazer: `QueuedInterceptorsWrapper` pra enfileirar requisições durante o refresh de token em 401
❌ Não fazer: disparar múltiplas chamadas concorrentes de `/api/auth/renovar` sem fila

---

## Autenticação & armazenamento

✅ Fazer: `accessToken`/`refreshToken` em `flutter_secure_storage`
❌ Não fazer: `SharedPreferences` pra qualquer dado sensível (tokens, credenciais)

✅ Fazer: em `401`, tentar renovar com `refreshToken`; se a renovação também falhar com `401`, deslogar e ir pro login
❌ Não fazer: tentar renovar indefinidamente, ou manter o usuário numa tela protegida após falha dupla de renovação

---

## Localização

✅ Fazer: atualização de localização só sob ação explícita do usuário (abrir tela de alertas, botão "Atualizar localização")
❌ Não fazer: rastreamento contínuo em background no MVP — fora de escopo

✅ Fazer: `LocationSettings` + parâmetros nomeados (`geolocator ^14`)
❌ Não fazer: usar `desiredAccuracy`/`timeLimit` posicionais — deprecados nessa versão

---

## Mapas

✅ Fazer: tela de alertas como lista (foto, distância aproximada, bairro) + botão "Ver no mapa" com deep link (`geo:lat,lng`/URL do Maps) via `url_launcher`
❌ Não fazer: adicionar mapa embutido (`flutter_map`, `google_maps_flutter`) sem validação prévia de que os usuários realmente sentem falta — decisão explícita de adiar pro pós-MVP

---

## Polling de alertas (tela de alertas próximos)

✅ Fazer: fetch ao abrir a tela + pull-to-refresh manual + polling leve (30–60s) **só em foreground**, via `Timer.periodic` dentro do `AlertaBloc`, iniciado em `AlertaTelaAberta` e cancelado em `AlertaTelaFechada`/`close()`
❌ Não fazer: depender só do push (FCM) pra atualizar a lista — entrega de push não é 100% garantida (Doze mode, otimização de bateria Xiaomi/Samsung)
❌ Não fazer: rodar o polling em background — FCM já cobre notificação com app fechado, polling em background só gasta bateria/rede à toa
❌ Não fazer: o timer disparar uma lógica de busca própria — ele reusa o mesmo Event do fetch manual (`AlertaListaSolicitada`)

---

## Notificações push (FCM)

✅ Fazer: `onBackgroundMessage` como função **top-level ou `static`**
❌ Não fazer: closure ou método de instância como background handler — lança `ArgumentError` em runtime (`firebase_messaging ^16`)

✅ Fazer: declarar `POST_NOTIFICATIONS` manualmente no `AndroidManifest.xml` (Android 13+)
❌ Não fazer: assumir que o plugin declara essa permissão sozinho — `flutter_local_notifications ^22` não declara mais automaticamente

✅ Fazer: atualizar o FCM token via `PUT /api/usuario/atualizar-fcm-token` no login e no callback `onTokenRefresh`
❌ Não fazer: deixar o token desatualizado no back-end — sem isso o tutor não recebe push

---

## `Result<T>` e modelos

✅ Fazer: `@freezed sealed class Result<T> with _$Result<T>` com `const Result._();` (freezed 3.x exige `sealed`/`abstract`; o construtor privado não é estritamente obrigatório pra essa definição mínima — validado empiricamente — mas vira obrigatório ao adicionar getter/método próprio, então já entra por padrão) e desconstruir com `switch`
❌ Não fazer: usar `.map()`/`.when()` neste projeto — foram removidos no freezed 3.0.0 e adicionados de volta na 3.1.0 (versão que este projeto resolve, 3.2.5), então **existem** na versão instalada, mas `switch`/pattern matching nativo é o padrão adotado aqui (mais idiomático, sobrevive a um futuro upgrade que os remova de novo)

✅ Fazer: Repository devolve `Result<T>`, nunca lança exceção pra erro de negócio
❌ Não fazer: `try/catch` de erro de negócio na `presentation/` ou no Bloc — o Repository já converteu em `Result.falha`

---

## Nomenclatura

✅ Fazer: campos, DTOs e Events em português, espelhando os records do back-end (`nome`, `tutorId`, `fotoUrl`); Event = nome do slice do back-end + sufixo (`Submitted`, `Solicitada(o)`, `TelaAberta`/`TelaFechada`)
❌ Não fazer: nomear em inglês ou inventar nome de campo diferente do que o back-end usa

✅ Fazer: enums em português, `int` no JSON, ordem = valor (`TipoPetEnum.cao == 0`)
❌ Não fazer: reordenar ou traduzir valores de enum — quebra o contrato com o back-end

---

## BlocObserver / Logging

✅ Fazer: confiar no `BlocObserver` global (`main.dart`) pra log de toda transição Event→State
❌ Não fazer: `debugPrint`/`log` manual dentro de um Bloc individual só pra registrar evento/estado — duplica ruído já coberto globalmente

---

## Testes

✅ Fazer: `bloc_test` + `mocktail`, mockando o **Repository** da feature
❌ Não fazer: mockar `Dio` diretamente num teste de Bloc — isso é nível de teste do Repository/interceptor, não do Bloc

---

## Versões de libs — pontos de atenção (breaking changes já mapeados)

✅ Fazer: checar `flutter pub outdated` antes de assumir comportamento de uma lib de memória — várias tiveram saltos de major version neste projeto
❌ Não fazer: copiar exemplo de tutorial/Stack Overflow antigo sem checar a versão instalada no `pubspec.yaml`

Pontos já confirmados nesta versão do projeto:
- `freezed ^3.1.0` — `sealed`/`abstract` obrigatório; `.map()`/`.when()` removidos na 3.0.0, adicionados de volta na 3.1.0 (versão resolvida aqui, 3.2.5, tem os dois), mas o projeto usa `switch` por convenção; `const Result._();` recomendado (obrigatório só se houver getter/método custom na classe)
- `flutter_local_notifications ^22` — sem auto-declaração de permissão, parâmetros nomeados em `initialize()`/`show()`/`zonedSchedule()`
- `geolocator ^14` — parâmetros nomeados, `LocationSettings`
- `firebase_messaging ^16` — background handler top-level/`static`
- `get_it ^9`, `go_router ^17` — sem breaking change relevante pro uso planejado
- Cubit sem eventos é suportado pelo `flutter_bloc ^9`, mas **não é o padrão adotado aqui**

---

## Setup nativo Android (Firebase)

✅ Fazer: manter o plugin `com.google.gms.google-services` declarado (`apply false`) até o `google-services.json` existir
❌ Não fazer: descomentar `id("com.google.gms.google-services")` em `android/app/build.gradle.kts` sem o `google-services.json` em `android/app/` — quebra o build pra todo mundo (ver `ROADMAP.md`)

---

## Escopo e ordem do MVP

✅ Fazer: seguir a ordem confirmada — **Auth → Tutor → Pet → Alerta** (cada uma depende da anterior)
❌ Não fazer: começar Alerta (a mais complexa, depende de Firebase configurado) antes das outras três estarem funcionando ponta a ponta

✅ Fazer: priorizar as 4 features funcionando ponta a ponta pra validar com usuários reais em Recife
❌ Não fazer: investir em robustez além do necessário pra essa validação (é MVP, não produto maduro nem projeto de estudo)
