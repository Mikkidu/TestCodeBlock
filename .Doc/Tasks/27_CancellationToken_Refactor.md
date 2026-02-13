# #27 Рефактор механизма остановки — CancellationToken через ExecutionContext

## Goal
Заменить статический глобальный флаг `CommandBase.ShouldStopExecution` и прямые сигналы `GameManager → Command` на токен отмены, контролируемый `CommandExecutor`. Сброс робота и обновление флагов происходят только после фактического завершения текущей команды.

## Context
До рефактора:
- `CommandBase.ShouldStopExecution` — статическая переменная на всё приложение. `GameManager` устанавливал её напрямую.
- `Stop()` в Executor немедленно сбрасывал `IsRunning`, не ожидая завершения текущей команды (анимация робота продолжалась).
- `LoopCommand` имел собственную дублирующую логику цепочки и также читал статический флаг.
- `GameManager.OnRobotReachedFinish()` перебирал все блоки и вызывал `RequestStop()` на каждом `LoopCommand` — прямое связывание Manager → конкретный тип команды.

После рефактора:
- `ExecutionContext.IsCancelled` — единственный источник сигнала отмены. Контекст создаётся и хранится в `CommandExecutor`.
- `Stop()` вызывает `context.Cancel()` и резюмирует паузу. `IsRunning` сбрасывается только когда цепочка промисов дойдёт до проверки и завершится.
- Новый event `OnProgramStopped` — единая точка для всех операций после фактической остановки.
- `GameManager` просто подписывается на `OnProgramStopped` и там делает сброс робота + обновление UI.

## Key Steps

1. **ExecutionContext** — добавлен `IsCancelled` (read-only property), метод `Cancel()`, сброс в `Clear()`.

2. **CommandBase / ICommand** — удалён `static ShouldStopExecution`, удалён `RequestStop()` из интерфейса и реализации.

3. **CommandExecutor** — добавлено поле `currentContext` (сохраняется при старте программы), event `OnProgramStopped`. `Stop()`: если не running — return; иначе `context.Cancel()` + resolve pauseDeferred (чтобы цепочка продолжилась до проверки). Оба метода цепочки (`ExecuteBlockChain`, `ExecuteCommandChain`) проверяют `context.IsCancelled` в двух точках — перед старtem следующей команды и в Done-handler после завершения текущей.

4. **Все команды** (MoveForward, MoveBackward, TurnLeft, TurnRight, Wait) — замена `if (ShouldStopExecution)` на `if (context.IsCancelled)`.

5. **LoopCommand** — аналогичная замена во всех трёх точках проверки (`ExecuteIteration`, `ExecuteInnerChain`), удалён `ShouldStopExecution = false` в `Execute()`.

6. **GameManager** — добавлен `levelCompleted` флаг и `HandleLevelCompletion()`. `OnStopButtonClicked` теперь только вызывает `Stop()`. `OnProgramStoppedHandler`: если `levelCompleted` — победа; иначе — сброс робота + UI. `OnRobotReachedFinish` упрощён: ставит флаг и вызывает Stop. `OnResetButtonClicked` упрощён: если running — stop (который сбросит робот), иначе reset напрямую.

7. **Cleanup** — убраны неиспользуемые `using System.Collections.Generic` и `using PU.Promises` из GameManager.

## Flow (после рефактора)

```
Юзер жмёт Stop
  → GameManager.OnStopButtonClicked()
    → commandExecutor.Stop()
      → context.Cancel()           // сигнал поставлен
      → pauseDeferred.Resolve()    // если была пауза — снимаем

  ... текущая команда доанимируется до конца ...

  → Done-handler в ExecuteBlockChain
    → context.IsCancelled == true
      → IsRunning = false
      → OnProgramStopped.Invoke()  // ЕДИНСТВЕННОЕ место

  → GameManager.OnProgramStoppedHandler()
    → isProgramRunning = false     // флаг ПОСЛЕ факт. остановки робота
    → robotController.Reset()      // робот на старт ПОСЛЕ факт. остановки
    → UpdateStatusDisplay("Stopped")
```

## Blockers & Risks

- Нет блокеров. Рефактор внутренний, внешний API не меняется.
- Рисок: если новая команда не проверяет `context.IsCancelled` в начале `Execute()` — она всё равно запустится. Но после её завершения цепочка останавливается в Done-handler Executor. Это по-дизайну: текущая команда доанимируется до конца.

## Acceptance Criteria

- [✓] Статический `ShouldStopExecution` удалён из кодовой базы полностью (grep = 0 matches).
- [✓] `RequestStop()` удалён из `ICommand` и `CommandBase`.
- [✓] `CommandExecutor.Stop()` не сбрасывает `IsRunning` синхронно — это делается в цепочке промисов.
- [✓] `OnProgramStopped` event добавлен и вызывается только после завершения текущей команды.
- [✓] Робот сбрасывается на старт только в `OnProgramStoppedHandler`, не в `OnStopButtonClicked`.
- [✓] Победа при финише обрабатывается через тот же `OnProgramStopped` путь (via `levelCompleted` flag).
- [✓] Компилируется без ошибок, коммит `cacc43c`.

## Notes

- `WaitCommand` и анимации робота не прерываются посередине — они доанимируются до конца, и только потом цепочка останавливается. Это осознанный выбор: робот визуально не "прыгает" обратно пока ещё движется.
- В будущем для мгновенного прерывания анимации можно передать `context` в `robot.MoveForward()` и прервать lerp по `IsCancelled`.
- Backlog пункты "Реализовать правильный Stop программы" и "Реализовать дополнительный вызов стоп при нажатии клавиш Reset и Clear" закрыты этой задачей.

## Changed Files

| Файл | Изменение |
|------|-----------|
| `ExecutionContext.cs` | `+IsCancelled`, `+Cancel()`, сброс в `Clear()` |
| `ICommand.cs` | Удалён `RequestStop()` |
| `CommandBase.cs` | Удалён `static ShouldStopExecution`, удалён `RequestStop()` |
| `CommandExecutor.cs` | `+currentContext`, `+OnProgramStopped`. `Stop()` — cancel + resume. Цепочки — IsCancelled checks |
| `MoveForwardCommand.cs` | `ShouldStopExecution` → `context.IsCancelled` |
| `MoveBackwardCommand.cs` | `ShouldStopExecution` → `context.IsCancelled` |
| `TurnLeftCommand.cs` | `ShouldStopExecution` → `context.IsCancelled` |
| `TurnRightCommand.cs` | `ShouldStopExecution` → `context.IsCancelled` |
| `WaitCommand.cs` | `ShouldStopExecution` → `context.IsCancelled` |
| `LoopCommand.cs` | `ShouldStopExecution` → `context.IsCancelled` (3 точки), удалён reset флага |
| `GameManager.cs` | `+levelCompleted`, `+OnProgramStoppedHandler`, `+HandleLevelCompletion()`. Убрана вся статика и RequestStop |
