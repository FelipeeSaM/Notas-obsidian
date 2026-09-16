# Tratamento de erros e resiliência de rede

Referência rápida. Ver `../../../../CLAUDE.md` → Convenções para o racional de cada decisão.

## Shape dos erros que vêm do back-end

**Erro de regra de negócio** (`Result<T>.Falha`, ex: `Features/Auth/Command/Login/LoginHandler.cs`) — string simples no body:

```json
"Credenciais inválidas."
```

**Erro de validação** (FluentValidation + `.ProducesValidationProblem()`, ex: `CriarPetValidator.cs`) — `ValidationProblemDetails` padrão do ASP.NET Core:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Nome": ["Nome não pode ser vazio"],
    "Descricao": ["Descrição não pode ultrapassar 500 caracteres"]
  }
}
```

Função única de extração no Repository (usada em todo `catch (DioException)`):

```dart
String _extrairMensagemErro(DioException e) {
  final data = e.response?.data;
  if (data is String && data.isNotEmpty) return data;
  if (data is Map && data['errors'] is Map) {
    final primeiro = (data['errors'] as Map).values.first;
    if (primeiro is List && primeiro.isNotEmpty) return primeiro.first.toString();
  }
  return 'Não foi possível completar a ação. Tente novamente.';
}
```

## `401` → renovar token → repetir a requisição original

`QueuedInterceptorsWrapper` enfileira requisições concorrentes enquanto o refresh está em andamento (evita disparar `/api/auth/renovar` várias vezes em paralelo):

```dart
dio.interceptors.add(
  QueuedInterceptorsWrapper(
    onError: (error, handler) async {
      if (error.response?.statusCode != 401) return handler.next(error);

      final refreshToken = await secureStorage.read('refreshToken');
      if (refreshToken == null) {
        await authRepository.logout();
        return handler.next(error);
      }

      try {
        final novoToken = await authRepository.renovarToken(refreshToken);
        error.requestOptions.headers['Authorization'] = 'Bearer ${novoToken.accessToken}';
        final retryResponse = await dio.fetch(error.requestOptions);
        return handler.resolve(retryResponse);
      } on DioException {
        // renovação também falhou com 401 → desloga e redireciona pro login
        await authRepository.logout();
        return handler.next(error);
      }
    },
  ),
);
```

Regra: se o **próprio refresh** retornar `401`, não insiste — desloga e manda pro login (ver `../../../../CLAUDE.md` → Comportamentos importantes).

## Ordem de registro dos interceptors importa (FIFO em todas as fases)

O Dio executa `onRequest`, `onResponse` e `onError` sempre na **mesma ordem de registro** (FIFO) — não há inversão entre fases. Confirmado no código-fonte do pacote (`dio_mixin.dart`): as três fases são construídas com `for (final interceptor in interceptors)` sobre a mesma lista, na mesma ordem, cada uma encadeada via `.then()`/`.catchError()`. A doc oficial do pacote (`Interceptors` class) também afirma isso explicitamente: "Interceptors will be executed with FIFO."

Isso importa porque `AuthInterceptor` (trata `401`/refresh) e `RetryInterceptor` (trata falha de transporte) reagem a `onError` e podem competir pelo mesmo erro. Registro correto em `core/network/dio_client.dart`:

```dart
dio.interceptors.addAll([
  authInterceptor,   // registrado 1º → roda 1º no onError (FIFO)
  retryInterceptor,  // registrado depois → só vê o que o Auth repassou
]);
```

Com essa ordem: `AuthInterceptor` vê o erro primeiro. Se for `401`, cuida do refresh. Pra qualquer outro erro (incluindo falha de transporte), chama `handler.next(err)` — passando adiante pro `RetryInterceptor`, que decide se vale repetir a requisição (ver seção abaixo). Se a ordem fosse invertida (`RetryInterceptor` antes de `AuthInterceptor`), o `RetryInterceptor` veria o erro primeiro — inofensivo hoje, já que ele só reage a falha de transporte e repassa o resto via `handler.next(err)`, mas inverte a intenção: o interceptor "dono" do erro de autenticação deveria ser o primeiro a examiná-lo.

## Retry automático — só em GET **e só em falha de transporte**

```dart
class RetryInterceptor extends Interceptor {
  static const _backoffs = [Duration(milliseconds: 500), Duration(seconds: 1), Duration(seconds: 2)];

  static const _tiposFalhaDeRede = {
    DioExceptionType.connectionTimeout,
    DioExceptionType.sendTimeout,
    DioExceptionType.receiveTimeout,
    DioExceptionType.connectionError,
  };

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) async {
    final isGet = err.requestOptions.method.toUpperCase() == 'GET';
    final isFalhaDeRede = _tiposFalhaDeRede.contains(err.type);
    final tentativa = err.requestOptions.extra['tentativa'] ?? 0;

    if (!isGet || !isFalhaDeRede || tentativa >= _backoffs.length) {
      return handler.next(err);
    }

    await Future.delayed(_backoffs[tentativa]);
    err.requestOptions.extra['tentativa'] = tentativa + 1;
    try {
      final response = await Dio().fetch(err.requestOptions);
      return handler.resolve(response);
    } on DioException catch (e) {
      return handler.next(e);
    }
  }
}
```

**Por que só GET**: repetir um GET nunca duplica dado. Repetir um POST/PUT (ex: `POST /api/pet/alerta/{id}`) sem proteção pode registrar o mesmo alerta duas vezes.

**Por que só falha de transporte**: `err.type` distingue "a requisição não chegou a ter resposta" (timeout, sem conexão — `connectionTimeout`/`sendTimeout`/`receiveTimeout`/`connectionError`) de "o servidor respondeu, só que com erro" (`DioExceptionType.badResponse` — `401`, `404`, `500` etc.). Só o primeiro caso justifica retry automático: um `401` repetido não vira `200` na segunda tentativa, só atrasa o `AuthInterceptor` entrar em ação; um `500` repetido pode sobrecarregar um servidor que já está com problema.

## Idempotency-Key — pendência (POST/PUT ainda sem retry automático)

Mutações **não têm retry automático hoje**. Quando o back-end implementar a checagem (ver `App-back/CLAUDE.md`), o client já deve mandar a key:

```dart
import 'package:uuid/uuid.dart';

final response = await _dio.post(
  '/api/pet/alerta/${pet.id}',
  options: Options(headers: {'Idempotency-Key': const Uuid().v4()}),
);
```

**Não ativar retry automático em POST/PUT antes dessa checagem existir no Handler do back-end** — duplicaria registros em caso de reenvio.

## Mensagens de erro na UI

Sempre priorizar a mensagem vinda do body (já em pt-BR). Mensagem genérica local só como fallback (rede indisponível, timeout, parsing falhou):

```dart
case Falha(:final mensagem):
  emit(AlertaErro(mensagem)); // mensagem já é pt-BR, vem pronta do Repository
```
