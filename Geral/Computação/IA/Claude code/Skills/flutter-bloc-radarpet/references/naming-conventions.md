# Nomenclatura — Events, Blocs, States, DTOs

Referência rápida. Ver `../../../../CLAUDE.md` na raiz para o racional completo.

## Regra geral

Campos de DTO/domínio em **português**, espelhando os records do back-end **exatamente como o JSON chega** (o back-end serializa em `camelCase` por padrão do ASP.NET Core Minimal API — não precisa `@JsonKey` na maioria dos casos).

Exemplo real (`Features/Auth/Command/Login/LoginCommand.cs`):

```csharp
public record LoginResponse(string AccessToken, string RefreshToken, DateTime ExpiracaoAccessToken);
```

vira, no JSON e no DTO Dart:

```json
{ "accessToken": "...", "refreshToken": "...", "expiracaoAccessToken": "2026-08-01T12:00:00Z" }
```

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

## Events — `sealed class` por feature, 1 Event por ação do usuário

Padrão de nome: `<Feature><Ação><Gatilho>`

| Gatilho | Sufixo | Exemplo |
|---|---|---|
| Usuário submeteu um formulário | `Submitted` | `AuthLoginSubmitted`, `PetCriarSubmitted` |
| Usuário pediu uma busca/listagem | `Solicitada(o)` | `AlertaListaSolicitada`, `PetListaSolicitada` |
| Tela abriu/fechou (lifecycle) | `TelaAberta` / `TelaFechada` | `AlertaTelaAberta`, `AlertaTelaFechada` |
| Usuário confirmou uma ação de negócio | `Confirmado(a)` | `AlertaResolverConfirmado` |

**Exceção documentada — `tutor/` usa Substantivo+Verbo.** `auth/`, `pet/`, `alerta/` e `denuncia/` seguem Verbo+Substantivo (ordem da tabela acima, espelhando o nome do Command/Query). `tutor/` inverteu a ordem de forma consistente em toda mutação (`SenhaAtualizarSubmitted`, `UsuarioAtualizarSubmitted`, `UsuarioDesativarSubmitted`, `FcmTokenRegistrarSolicitado`, `LocalizacaoAtualizarSolicitado`) — convenção própria do domínio, não desvio. Ação nova em `tutor/` segue essa ordem invertida; nos demais domínios, segue a ordem normal.

```dart
sealed class AuthEvent extends Equatable {
  const AuthEvent();
  @override
  List<Object?> get props => [];
}

final class AuthLoginSubmitted extends AuthEvent {
  const AuthLoginSubmitted({required this.email, required this.senha});
  final String email;
  final String senha;

  @override
  List<Object?> get props => [email, senha];
}
```

## Blocs — `<Feature>Bloc`

Um Bloc por feature (`AuthBloc`, `TutorBloc`, `PetBloc`, `AlertaBloc`), nunca um Bloc por tela — a mesma feature pode alimentar mais de uma página.

## States — `sealed class <Feature>State`, variantes por resultado

Não é freezed (freezed fica reservado para DTOs/`Result<T>`, ver `../../../../CLAUDE.md` → Stack técnico → Modelos). States usam `sealed class` + `Equatable` puro — mais simples, sem codegen, suficiente pro que o MVP precisa.

```dart
sealed class AuthState extends Equatable {
  const AuthState();
  @override
  List<Object?> get props => [];
}

final class AuthInicial extends AuthState {}
final class AuthCarregando extends AuthState {}

final class AuthAutenticado extends AuthState {
  const AuthAutenticado({required this.accessToken});
  final String accessToken;
  @override
  List<Object?> get props => [accessToken];
}

final class AuthErro extends AuthState {
  const AuthErro(this.mensagem);
  final String mensagem;
  @override
  List<Object?> get props => [mensagem];
}
```

## Repository e DTOs

- Repository: `<Feature>Repository` (ex: `AuthRepository`, `PetRepository`) — único ponto que conhece `dio`.
- DTO de request/response: `<NomeDoSliceDoBackEnd>Dto` — nome espelha o record C# do slice (`LoginResponse` → `LoginResponseDto`), não o nome da tela.

## Enums — sempre `int` no JSON, sem tradução de nome

```dart
enum TipoPetEnum { cao, gato, passaro, cobra, tartaruga, hamster } // 0..5
enum GeneroEnum { macho, femea } // 0, 1
enum TipoUsuarioEnum { administrador, basico } // 0, 1
```

Nomes do enum em português, iguais ao back-end (`Blocos.Nucleo/Enums/`) — nunca traduzir para inglês, a ordem dos valores é o contrato (índice = valor enviado/recebido).
