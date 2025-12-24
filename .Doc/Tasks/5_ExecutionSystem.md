# Задача #5: Система выполнения

## Цель
Реализовать CommandExecutor с рекурсивной цепочкой Promise-ов для выполнения последовательности команд.

## Контекст
Зависит от: Задача #2, #4
Это оркестратор, который превращает связный список команд в выполнение через Promise.Then().

## Ключевые шаги
1. Создать `Execution/ProgramSequence.cs`:
   - Dictionary<int, ICommand> commandsById
   - Методы: AddCommand, LinkCommands, SetStartCommand, GetCommandById, Clear
   - Свойства: StartCommand, CommandCount

2. Создать `Execution/CommandExecutor.cs` наследуя ICommandExecutor:
   - Поле: bool IsRunning
   - ExecuteProgram() вызывает ExecuteCommandChain с начальной командой
   - Private ExecuteCommandChain(command) - рекурсивный метод:
     * Если command == null → OnProgramCompleted и Deferred.Resolved()
     * Иначе: OnCommandStarted → command.Execute() → OnCommandCompleted → ExecuteCommandChain(command.Next)
   - Events: OnCommandStarted, OnCommandCompleted, OnProgramCompleted, OnProgramFailed
   - Pause/Resume: используя промежуточный Deferred

3. Добавить Timers.Instance проверку в RobotController

4. Протестировать цепочку из 3-4 команд: Move → Turn → Move → Turn

## Критерии завершения
- [ ] ProgramSequence создана и управляет связным списком
- [ ] CommandExecutor реализует ICommandExecutor
- [ ] ExecuteCommandChain рекурсивно выполняет все команды
- [ ] Events срабатывают в правильном порядке
- [ ] Promise.Then() правильно цепляет команды
- [ ] Pause/Resume работают
- [ ] Цепочка из 4+ команд выполняется полностью
- [ ] IsRunning правильно отражает состояние

## Ключевой код
```csharp
private IPromise ExecuteCommandChain(ICommand command, IRobotController robot, ExecutionContext context)
{
    if (command == null)
    {
        IsRunning = false;
        OnProgramCompleted?.Invoke();
        return Deferred.Resolved();
    }

    OnCommandStarted?.Invoke(command);

    return command.Execute(robot, context)
        .Done(() => OnCommandCompleted?.Invoke(command))
        .Then(() => ExecuteCommandChain(command.Next, robot, context));
}
```
