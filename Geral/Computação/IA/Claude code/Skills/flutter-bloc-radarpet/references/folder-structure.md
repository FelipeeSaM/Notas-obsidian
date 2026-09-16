# Estrutura de pastas — feature-first

Referência rápida. Espelha `Features/<Dominio>/` do back-end (VSA). Ver `../../../../CLAUDE.md` para o racional.

## Árvore geral

```
lib/
├── core/
│   ├── network/      dio_client.dart, auth_interceptor.dart, retry_interceptor.dart
│   ├── storage/       secure_storage_service.dart
│   ├── router/         app_router.dart
│   ├── di/               injection.dart
│   ├── theme/             app_theme.dart, app_colors.dart
│   └── result/              result.dart
├── features/
│   ├── auth/
│   ├── tutor/
│   ├── pet/
│   └── alerta/
└── shared/
    └── widgets/          app_button.dart, app_text_field.dart, loading_view.dart, empty_state.dart
```

## Exemplo completo — feature `auth`

```
lib/features/auth/
├── data/
│   ├── dto/
│   │   ├── login_request_dto.dart
│   │   ├── login_response_dto.dart
│   │   └── renovar_token_response_dto.dart
│   └── auth_repository.dart
├── bloc/
│   ├── auth_bloc.dart
│   ├── auth_event.dart
│   └── auth_state.dart
└── presentation/
    ├── login_page.dart
    └── registro_page.dart
```

### `data/` — único lugar que conhece `dio`

```dart
// auth_repository.dart
class AuthRepository {
  AuthRepository(this._dio);
  final Dio _dio;

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
}
```

### `bloc/` — nunca chama `dio` diretamente, só o Repository

```dart
class AuthBloc extends Bloc<AuthEvent, AuthState> {
  AuthBloc(this._repository) : super(AuthInicial()) {
    on<AuthLoginSubmitted>(_onLoginSubmitted);
  }

  final AuthRepository _repository;

  Future<void> _onLoginSubmitted(
    AuthLoginSubmitted event,
    Emitter<AuthState> emit,
  ) async {
    emit(AuthCarregando());
    final resultado = await _repository.login(event.email, event.senha);
    switch (resultado) {
      case Ok(:final valor):
        emit(AuthAutenticado(accessToken: valor.accessToken));
      case Falha(:final mensagem):
        emit(AuthErro(mensagem));
    }
  }
}
```

### `presentation/` — só dispara Events e reage a States

```dart
// login_page.dart (trecho)
onPressed: () => context.read<AuthBloc>().add(
  AuthLoginSubmitted(email: emailController.text, senha: senhaController.text),
),
```

## As outras 3 features seguem o mesmo molde

```
lib/features/tutor/{data,bloc,presentation}/
lib/features/pet/{data,bloc,presentation}/
lib/features/alerta/{data,bloc,presentation}/
```

Nenhuma delas tem código real ainda (só `.gitkeep`) — a ordem de implementação confirmada é **Auth → Tutor → Pet → Alerta** (ver `../../../../CLAUDE.md` → Roadmap).
