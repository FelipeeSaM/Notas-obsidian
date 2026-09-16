# Event → Bloc → State

Referência rápida. Bloc completo (não Cubit) — 1 Event por ação do usuário. Ver `../../../../CLAUDE.md` para o racional da decisão.

## Exemplo mínimo

```dart
// pet_event.dart
sealed class PetEvent extends Equatable {
  const PetEvent();
  @override
  List<Object?> get props => [];
}

final class PetListaSolicitada extends PetEvent {}

// pet_state.dart
sealed class PetState extends Equatable {
  const PetState();
  @override
  List<Object?> get props => [];
}

final class PetInicial extends PetState {}
final class PetCarregando extends PetState {}
final class PetListaCarregada extends PetState {
  const PetListaCarregada(this.pets);
  final List<PetDto> pets;
  @override
  List<Object?> get props => [pets];
}
final class PetErro extends PetState {
  const PetErro(this.mensagem);
  final String mensagem;
  @override
  List<Object?> get props => [mensagem];
}

// pet_bloc.dart
class PetBloc extends Bloc<PetEvent, PetState> {
  PetBloc(this._repository) : super(PetInicial()) {
    on<PetListaSolicitada>(_onListaSolicitada);
  }

  final PetRepository _repository;

  Future<void> _onListaSolicitada(
    PetListaSolicitada event,
    Emitter<PetState> emit,
  ) async {
    emit(PetCarregando());
    final resultado = await _repository.listar();
    switch (resultado) {
      case Ok(:final valor):
        emit(PetListaCarregada(valor));
      case Falha(:final mensagem):
        emit(PetErro(mensagem));
    }
  }
}
```

## Fluxo assíncrono completo — `AlertaBloc` com polling em foreground

Implementa a convenção do `../../../../CLAUDE.md`: fetch ao abrir a tela + pull-to-refresh + polling leve (30–60s) **só em foreground**. O timer dispara o **mesmo Event** do fetch manual — sem duplicar lógica de busca.

```dart
// alerta_event.dart
sealed class AlertaEvent extends Equatable {
  const AlertaEvent();
  @override
  List<Object?> get props => [];
}

final class AlertaTelaAberta extends AlertaEvent {}
final class AlertaTelaFechada extends AlertaEvent {}
final class AlertaListaSolicitada extends AlertaEvent {}

// alerta_state.dart
sealed class AlertaState extends Equatable {
  const AlertaState();
  @override
  List<Object?> get props => [];
}

final class AlertaInicial extends AlertaState {}
final class AlertaCarregando extends AlertaState {}
final class AlertaListaCarregada extends AlertaState {
  const AlertaListaCarregada(this.alertas);
  final List<AlertaProximoDto> alertas;
  @override
  List<Object?> get props => [alertas];
}
final class AlertaErro extends AlertaState {
  const AlertaErro(this.mensagem);
  final String mensagem;
  @override
  List<Object?> get props => [mensagem];
}

// alerta_bloc.dart
class AlertaBloc extends Bloc<AlertaEvent, AlertaState> {
  AlertaBloc(this._repository, this._localizacao) : super(AlertaInicial()) {
    on<AlertaTelaAberta>(_onTelaAberta);
    on<AlertaTelaFechada>(_onTelaFechada);
    on<AlertaListaSolicitada>(_onListaSolicitada);
  }

  final AlertaRepository _repository;
  final LocalizacaoAtual _localizacao;
  Timer? _pollingTimer;

  static const _intervaloPolling = Duration(seconds: 45);

  void _onTelaAberta(AlertaTelaAberta event, Emitter<AlertaState> emit) {
    add(AlertaListaSolicitada());
    _pollingTimer = Timer.periodic(_intervaloPolling, (_) => add(AlertaListaSolicitada()));
  }

  void _onTelaFechada(AlertaTelaFechada event, Emitter<AlertaState> emit) {
    _pollingTimer?.cancel();
  }

  Future<void> _onListaSolicitada(
    AlertaListaSolicitada event,
    Emitter<AlertaState> emit,
  ) async {
    if (state is! AlertaListaCarregada) emit(AlertaCarregando());
    final posicao = _localizacao.ultimaConhecida();
    final resultado = await _repository.buscarProximos(posicao.lat, posicao.lng);
    switch (resultado) {
      case Ok(:final valor):
        emit(AlertaListaCarregada(valor));
      case Falha(:final mensagem):
        emit(AlertaErro(mensagem));
    }
  }

  @override
  Future<void> close() {
    _pollingTimer?.cancel();
    return super.close();
  }
}
```

```dart
// alerta_page.dart (trecho)
@override
void initState() {
  super.initState();
  context.read<AlertaBloc>().add(AlertaTelaAberta());
}

@override
void dispose() {
  context.read<AlertaBloc>().add(AlertaTelaFechada());
  super.dispose();
}
```

Pontos que não podem ser esquecidos:
- `if (state is! AlertaListaCarregada) emit(AlertaCarregando())` evita "piscar" um loading spinner por cima de uma lista já carregada a cada polling — só mostra loading no fetch inicial.
- Timer cancelado em **dois** lugares (`AlertaTelaFechada` e `close()`) — cobre tanto o usuário saindo da tela normalmente quanto o Bloc sendo descartado pelo `get_it`/DI sem o `dispose()` rodar.
- `Timer` é nativo do `dart:async` — nenhuma dependência nova.

## BlocObserver global (`main.dart`)

```dart
class AppBlocObserver extends BlocObserver {
  @override
  void onTransition(Bloc bloc, Transition transition) {
    super.onTransition(bloc, transition);
    debugPrint('${bloc.runtimeType} ${transition.event} → ${transition.nextState}');
  }
}

void main() {
  Bloc.observer = AppBlocObserver();
  runApp(const RadarPetApp());
}
```
