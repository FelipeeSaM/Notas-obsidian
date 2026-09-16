# Integração com a API — Command/Query → Repository

Referência rápida. **Nunca assumir o formato de um endpoint de memória** — sempre abrir os dois arquivos do slice primeiro:

```
App-back/AplicativoPet.Api/Features/<Dominio>/<Command|Query>/<Slice>/
├── <Slice>Command.cs   ou   <Slice>Query.cs   ← campos exatos do request/response
└── <Slice>Endpoint.cs                          ← rota HTTP, método, status codes
```

## Passo a passo (exemplo real: Login)

**1. Abrir o Command** (`Features/Auth/Command/Login/LoginCommand.cs`):

```csharp
public record LoginRequest(string Email, string Senha);
public record LoginResponse(string AccessToken, string RefreshToken, DateTime ExpiracaoAccessToken);
```

**2. Abrir o Endpoint** (`LoginEndpoint.cs`) — rota, método, status codes:

```csharp
app.MapPost("/api/auth/login", async (LoginRequest request, ISender sender) => { ... })
    .Produces<LoginResponse>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized)
    .ProducesValidationProblem();
```

**3. Escrever o DTO Dart** (campos = passo 1, `camelCase`):

```dart
@freezed
abstract class LoginResponseDto with _$LoginResponseDto {
  const factory LoginResponseDto({
    required String accessToken,
    required String refreshToken,
    required DateTime expiracaoAccessToken,
  }) = _LoginResponseDto;

  factory LoginResponseDto.fromJson(Map<String, dynamic> json) =>
      _$LoginResponseDtoFromJson(json);
}
```

**4. Escrever o método no Repository** (rota + método = passo 2):

```dart
Future<Result<LoginResponseDto>> login(String email, String senha) async {
  try {
    final response = await _dio.post('/api/auth/login', data: {
      'email': email,
      'senha': senha,
    });
    return Result.ok(LoginResponseDto.fromJson(response.data));
  } on DioException catch (e) {
    return Result.falha(_extrairMensagemErro(e));
  }
}
```

## `TutorId` nunca é enviado manualmente

O back-end resolve o tutor autenticado a partir do claim `sub` do JWT — o Dio interceptor só precisa anexar o `Authorization: Bearer`, nada mais:

```csharp
// BuscarAlertasProximosEndpoint.cs — TutorId vem do token, não do body/query
var tutorId = Guid.Parse(user.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
```

Então no Flutter, o `AlertaRepository` só manda `latitude`/`longitude` como query params — nunca um `tutorId`:

```dart
Future<Result<List<AlertaProximoDto>>> buscarProximos(double lat, double lng) async {
  final response = await _dio.get('/api/alerta/proximos', queryParameters: {
    'latitude': lat,
    'longitude': lng,
  });
  final lista = (response.data as List)
      .map((json) => AlertaProximoDto.fromJson(json))
      .toList();
  return Result.ok(lista);
}
```

## `Result<T>` — desconstruir com `switch` (freezed 3.x)

```dart
@freezed
sealed class Result<T> with _$Result<T> {
  const Result._();
  const factory Result.ok(T valor) = Ok<T>;
  const factory Result.falha(String mensagem) = Falha<T>;
}
```

`const Result._();` **validado empiricamente** (`dart run build_runner build`, freezed ^3.1.0): sem essa linha o codegen também compila e `flutter analyze` passa limpo — não é estritamente obrigatório pra essa definição mínima (só factories, sem membro custom). Mesmo assim, mantemos como padrão do projeto porque (a) é o que o exemplo canônico da documentação oficial do freezed usa pra union genérica, e (b) vira **obrigatório** no momento em que qualquer getter/método for adicionado direto no corpo da `sealed class` — incluir desde já evita ter que lembrar disso depois.

```dart
final resultado = await _repository.login(email, senha);
switch (resultado) {
  case Ok(:final valor):
    emit(AuthAutenticado(accessToken: valor.accessToken));
  case Falha(:final mensagem):
    emit(AuthErro(mensagem));
}
```

`.map()`/`.when()` têm um histórico: **removidos** no freezed 3.0.0 (rewrite pra sealed classes/Dart 3) e **adicionados de volta** na 3.1.0 (2025-07-02 — changelog oficial: "Added when/map back"). Este projeto fixa `freezed: ^3.1.0`, que hoje resolve pra **3.2.5** — testado num cache limpo (`.dart_tool/build` apagado antes do `dart run build_runner build --delete-conflicting-outputs`) e confirmado que `.map()`/`.maybeMap()`/`.mapOrNull()`/`.when()`/`.maybeWhen()`/`.whenOrNull()` são gerados normalmente nessa versão. Mesmo assim, o padrão deste projeto continua sendo `switch`/pattern matching nativo do Dart 3 — mais idiomático, não depende da extensão gerada, e não quebra se um upgrade futuro do freezed voltar a removê-los.

## Mutação com body (exemplo: `PUT /api/usuario/atualizar-localizacao`)

```csharp
public record AtualizarLocalizacaoRequest(double Lat, double Lng);
```

```dart
Future<Result<void>> atualizarLocalizacao(double lat, double lng) async {
  try {
    await _dio.put('/api/usuario/atualizar-localizacao', data: {'lat': lat, 'lng': lng});
    return const Result.ok(null);
  } on DioException catch (e) {
    return Result.falha(_extrairMensagemErro(e));
  }
}
```

Mensagens de erro do body (ver `error-handling.md` para o shape completo) — nunca hardcodar texto de erro de negócio no Flutter quando o back-end já manda a mensagem em pt-BR.
