---
name: flutter-bloc-radarpet
description: Convenções exatas de código Flutter do projeto RadarPet — não Bloc genérico, mas a variante específica usada aqui (Bloc completo com Events, 1 Event por ação do usuário, nomenclatura espelhando Command/Query do back-end .NET, Result<T> via freezed 3.x, Repository isolando Dio). Use sempre que for criar ou revisar um arquivo *_event.dart/*_bloc.dart/*_state.dart, um Repository, um DTO, escrever um teste com bloc_test, ou implementar qualquer slice das features auth/tutor/pet/alerta — mesmo que o usuário só diga "cria o bloc de X" ou "implementa a tela de Y" sem mencionar Bloc explicitamente.
---

# Flutter Bloc — padrões do RadarPet

Este projeto usa **Bloc completo com Events** (não Cubit, não Bloc "genérico" de tutorial). Esta skill ensina a variante exata usada aqui. Fonte da verdade: `CLAUDE.md` (raiz deste repo) e `App-back/CLAUDE.md`, deste projeto.

Referências detalhadas (ler quando a tarefa tocar o tema específico):
- `references/naming-conventions.md` — nomenclatura completa de Events/Blocs/States/DTOs
- `references/folder-structure.md` — árvore feature-first completa
- `references/api-integration.md` — como consultar o back-end e mapear pra Repository
- `references/state-management.md` — Event→Bloc→State, incluindo o fluxo assíncrono completo do `AlertaBloc` com polling
- `references/error-handling.md` — RetryInterceptor, Idempotency-Key, 401/renovação de token

---

## 1. Antes de escrever qualquer código: consultar o back-end

**Nunca assumir o formato de um endpoint de memória.** O back-end (`App-back/`) é a fonte da verdade. Antes de criar um DTO ou método de Repository, abrir os dois arquivos do slice correspondente:

```
App-back/AplicativoPet.Api/Features/<Dominio>/<Command|Query>/<Slice>/
├── <Slice>Command.cs   ou   <Slice>Query.cs   ← campos exatos do request/response
└── <Slice>Endpoint.cs                          ← rota HTTP, método, status codes, RequireAuthorization
```

Domínios existentes: `Auth/`, `Usuarios/`, `Pet/`, `Alerta/`. Se o slice que você precisa não existir ainda no back-end, isso é um bloqueio real — avise o usuário, não invente o contrato.

O back-end serializa JSON em `camelCase` por padrão (ASP.NET Core Minimal API) — os campos do DTO Dart batem com os campos do record C# sem precisar `@JsonKey` na maioria dos casos.

## 2. Estrutura de arquivo por feature

Cada feature (`auth`, `tutor`, `pet`, `alerta`) segue exatamente:

```
lib/features/<feature>/
├── data/
│   ├── dto/                  ← um arquivo por request/response do back-end
│   └── <feature>_repository.dart   ← único lugar que conhece Dio
├── bloc/
│   ├── <feature>_event.dart  ← sealed class, 1 Event por ação do usuário
│   ├── <feature>_bloc.dart   ← on<Event> mapeando pra chamada do Repository
│   └── <feature>_state.dart  ← sealed class, variantes por resultado
└── presentation/
    └── <tela>_page.dart      ← só dispara Events e reage a States
```

Detalhes e exemplo completo da árvore: `references/folder-structure.md`.

## 3. Nomenclatura de Events — espelha o Command/Query do back-end

Regra: pegar o nome do slice do back-end (sem o sufixo `Command`/`Query`) e aplicar o sufixo que corresponde ao tipo de ação.

| Back-end (`*Command.cs`/`*Query.cs`) | Event no Flutter | Sufixo |
|---|---|---|
| `RegistrarAlertaCommand` | `RegistrarAlertaSubmitted` | `Submitted` — mutação disparada por ação explícita (botão) |
| `LoginCommand` | `AuthLoginSubmitted` | `Submitted` |
| `AtualizarFcmTokenCommand` | `AtualizarFcmTokenSubmitted` | `Submitted` |
| `BuscarAlertasProximosQuery` | `AlertaListaSolicitada` | `Solicitada(o)` — busca/listagem |
| *(lifecycle de tela, sem Command/Query correspondente)* | `AlertaTelaAberta` / `AlertaTelaFechada` | `TelaAberta`/`TelaFechada` |

Tabela completa de casos (incluindo confirmação de ação de negócio) em `references/naming-conventions.md`.

**Exceção documentada — domínio `tutor/` inverte a ordem (Substantivo+Verbo).** A regra acima (Verbo+Substantivo, espelhando a ordem do nome do Command/Query) é o padrão pros domínios `auth/`, `pet/`, `alerta/` e `denuncia/`. O domínio `tutor/` estabeleceu, de forma consistente em todas as suas ações de mutação, a ordem invertida: `AtualizarSenhaCommand` → `SenhaAtualizarSubmitted` (não `AtualizarSenhaSubmitted`), `AtualizarFcmTokenCommand` → `FcmTokenRegistrarSolicitado`, `DesativarUsuarioCommand` → `UsuarioDesativarSubmitted`, etc. Não é inconsistência — é a convenção própria já fixada nesse domínio. Escrevendo uma ação nova em `tutor/`, siga a ordem invertida pra bater com as vizinhas; nos outros domínios, siga a ordem normal da tabela acima.

**Um Event por ação do usuário** — nunca um método genérico `carregar()` ou `submit()` reaproveitado para ações diferentes. Se o usuário pode disparar duas ações distintas na mesma tela, são dois Events distintos.

## 4. `Result<T>` — freezed 3.x, padrão é `switch` (não `.map()`/`.when()`)

```dart
@freezed
sealed class Result<T> with _$Result<T> {
  const Result._();
  const factory Result.ok(T valor) = Ok<T>;
  const factory Result.falha(String mensagem) = Falha<T>;
}
```

`const Result._();` foi validado empiricamente (`dart run build_runner build`, freezed ^3.1.0): compila com e sem essa linha pra essa definição mínima — não é estritamente obrigatório aqui, mas vira obrigatório assim que a `sealed class` ganhar algum getter/método próprio. Mantemos como padrão do projeto por já vir assim no exemplo canônico do freezed e por evitar retrabalho depois. Detalhes do teste: `references/api-integration.md`.

Desconstruir sempre com `switch` (pattern matching nativo do Dart 3):

```dart
switch (resultado) {
  case Ok(:final valor):
    emit(FooSucesso(valor));
  case Falha(:final mensagem):
    emit(FooErro(mensagem));
}
```

`.map()`/`.when()` foram **removidos no freezed 3.0.0** e **adicionados de volta na 3.1.0** (changelog oficial: "Added when/map back"). Este projeto fixa `freezed: ^3.1.0` (resolve hoje pra 3.2.5) — testado em cache limpo e confirmado que ambos existem nessa versão. Mesmo assim não são o padrão adotado aqui: `switch` é mais idiomático em Dart 3, não depende da extensão gerada, e não quebra se um upgrade futuro remover `.map()`/`.when()` de novo. Detalhes do histórico e do teste: `references/api-integration.md`.

Classes com factory constructor no freezed 3.x exigem `sealed` ou `abstract` — sem isso o codegen falha.

## 5. Repository — único dono do Dio

O Bloc **nunca** chama `dio` diretamente. Toda chamada HTTP passa pelo Repository da feature, que devolve `Result<T>` (nunca lança exceção pra erro de negócio):

```dart
class TutorRepository {
  TutorRepository(this._dio);
  final Dio _dio;

  Future<Result<void>> atualizarFcmToken(String fcmToken) async {
    try {
      await _dio.put('/api/usuario/atualizar-fcm-token', data: {'fcmToken': fcmToken});
      return const Result.ok(null);
    } on DioException catch (e) {
      return Result.falha(_extrairMensagemErro(e));
    }
  }
}
```

`TutorId` nunca é passado manualmente — o back-end resolve via claim `sub` do JWT (o Dio interceptor só anexa `Authorization: Bearer`). Detalhes: `references/api-integration.md`.

## 6. BlocObserver global — não duplicar logging

`main.dart` já registra um `BlocObserver` que loga toda transição Event→State desde o início do projeto:

```dart
Bloc.observer = AppBlocObserver();
```

**Nunca** adicionar `debugPrint`/`log` manual dentro de um Bloc individual pra registrar eventos ou mudanças de estado — isso já é coberto globalmente e duplicaria ruído no console. Logging específico de negócio (não de transição) é a única exceção.

## 7. Retry automático — só em GET, só em falha de transporte; ordem dos interceptors importa

- `RetryInterceptor` (custom, em `core/network/`) faz retry automático (2–3 tentativas, backoff 500ms/1s/2s) **apenas em GET** e **apenas quando `err.type` é falha de transporte** (`connectionTimeout`, `sendTimeout`, `receiveTimeout`, `connectionError`) — nunca numa resposta válida do servidor (`401`, `404`, `500`, que são `DioExceptionType.badResponse`). Repetir um `401` não vira `200` na próxima tentativa, só atrasa o `AuthInterceptor`.
- **Ordem de registro em `dio_client.dart` importa**: o Dio roda `onRequest`/`onResponse`/`onError` sempre em ordem FIFO (ordem de registro — confirmado no código-fonte e na doc oficial do pacote, não há inversão de ordem no `onError`). Registrar `authInterceptor` **antes** de `retryInterceptor` na lista — assim o `AuthInterceptor` vê o erro primeiro, cuida do `401`, e repassa qualquer outro erro (via `handler.next()`) pro `RetryInterceptor` decidir se é falha de transporte. Detalhes e o porquê: `references/error-handling.md`.
- POST/PUT (`RegistrarAlertaSubmitted`, `PetCriarSubmitted` etc.) **não têm retry automático** — dependem de um header `Idempotency-Key` que o back-end **ainda não checa** (confirmado em `App-back/CLAUDE.md`, seção "Pendência acordada com o front-end").
- **Não assumir que a Idempotency-Key já funciona.** Não ativar retry em mutações antes dessa checagem existir no Handler do back-end — duplicaria registros (ex: dois alertas pro mesmo evento).

Shape dos erros e o interceptor de 401/renovação de token: `references/error-handling.md`.

## 8. Testes — `bloc_test` + `mocktail`

```dart
class MockTutorRepository extends Mock implements TutorRepository {}

void main() {
  late MockTutorRepository repository;

  setUp(() => repository = MockTutorRepository());

  blocTest<TutorBloc, TutorState>(
    'emite [TutorCarregando, TutorFcmTokenAtualizado] quando dá certo',
    build: () {
      when(() => repository.atualizarFcmToken(any()))
          .thenAnswer((_) async => const Result.ok(null));
      return TutorBloc(repository);
    },
    act: (bloc) => bloc.add(const AtualizarFcmTokenSubmitted(fcmToken: 'token-123')),
    expect: () => [TutorCarregando(), TutorFcmTokenAtualizado()],
  );

  blocTest<TutorBloc, TutorState>(
    'emite [TutorCarregando, TutorErro] quando o Repository falha',
    build: () {
      when(() => repository.atualizarFcmToken(any()))
          .thenAnswer((_) async => const Result.falha('Não foi possível completar a ação.'));
      return TutorBloc(repository);
    },
    act: (bloc) => bloc.add(const AtualizarFcmTokenSubmitted(fcmToken: 'token-123')),
    expect: () => [TutorCarregando(), const TutorErro('Não foi possível completar a ação.')],
  );
}
```

Não mockar `Dio` diretamente nos testes de Bloc — mockar o `Repository`. Testar o `Dio`/interceptors é responsabilidade de outro nível de teste (não coberto por `bloc_test`).

---

## Exemplo completo — slice `AtualizarFcmToken`

Slice real e simples do back-end (`Features/Usuarios/Command/AtualizarFcmToken/`), bom molde por ter só 1 campo. Sempre que precisar implementar algo parecido (uma mutação simples, sem lista, sem parâmetros de busca), siga este exato molde.

**Back-end** (`AtualizarFcmTokenCommand.cs` + `AtualizarFcmTokenEndpoint.cs`, já implementados):

```csharp
public record AtualizarFcmTokenRequest(string FcmToken);
public record AtualizarFcmTokenCommand(Guid TutorId, string FcmToken) : ICommand<Result<AtualizarFcmTokenResponse>>;
public record AtualizarFcmTokenResponse(Guid TutorId);

// PUT /api/usuario/atualizar-fcm-token — RequireAuthorization("UsuarioAutenticado")
// 200 OK | 404 NotFound | 401 Unauthorized | 400 ValidationProblem
```

**`tutor_event.dart`**

```dart
sealed class TutorEvent extends Equatable {
  const TutorEvent();
  @override
  List<Object?> get props => [];
}

final class AtualizarFcmTokenSubmitted extends TutorEvent {
  const AtualizarFcmTokenSubmitted({required this.fcmToken});
  final String fcmToken;
  @override
  List<Object?> get props => [fcmToken];
}
```

**`tutor_state.dart`**

```dart
sealed class TutorState extends Equatable {
  const TutorState();
  @override
  List<Object?> get props => [];
}

final class TutorInicial extends TutorState {}
final class TutorCarregando extends TutorState {}
final class TutorFcmTokenAtualizado extends TutorState {}
final class TutorErro extends TutorState {
  const TutorErro(this.mensagem);
  final String mensagem;
  @override
  List<Object?> get props => [mensagem];
}
```

**`tutor_bloc.dart`**

```dart
class TutorBloc extends Bloc<TutorEvent, TutorState> {
  TutorBloc(this._repository) : super(TutorInicial()) {
    on<AtualizarFcmTokenSubmitted>(_onAtualizarFcmTokenSubmitted);
  }

  final TutorRepository _repository;

  Future<void> _onAtualizarFcmTokenSubmitted(
    AtualizarFcmTokenSubmitted event,
    Emitter<TutorState> emit,
  ) async {
    emit(TutorCarregando());
    final resultado = await _repository.atualizarFcmToken(event.fcmToken);
    switch (resultado) {
      case Ok():
        emit(TutorFcmTokenAtualizado());
      case Falha(:final mensagem):
        emit(TutorErro(mensagem));
    }
  }
}
```

**`data/tutor_repository.dart`**

```dart
class TutorRepository {
  TutorRepository(this._dio);
  final Dio _dio;

  Future<Result<void>> atualizarFcmToken(String fcmToken) async {
    try {
      await _dio.put('/api/usuario/atualizar-fcm-token', data: {'fcmToken': fcmToken});
      return const Result.ok(null);
    } on DioException catch (e) {
      return Result.falha(_extrairMensagemErro(e));
    }
  }
}
```

**Chamado no callback do Firebase** (`onTokenRefresh`, e também logo após o login):

```dart
FirebaseMessaging.instance.onTokenRefresh.listen((novoToken) {
  context.read<TutorBloc>().add(AtualizarFcmTokenSubmitted(fcmToken: novoToken));
});
```

Um segundo exemplo — mais complexo, com fluxo assíncrono completo (loading/success/error) e polling em foreground — está em `references/state-management.md`, usando o `AlertaBloc`.
