# Задача #2: Core интерфейсы

## Цель
Определить контракты для всех ключевых систем: ICommand, IRobotController, ICommandExecutor, CommandBase, ExecutionContext

## Контекст
Зависит от: Задача #1
Это фундамент для остальных компонентов. Все команды будут наследоваться от CommandBase.

## Ключевые шаги
1. Создать `Core/ICommand.cs`:
   - int Id { get; }
   - CommandType Type { get; }
   - ICommand Next { get; set; }
   - IPromise Execute(IRobotController robot, ExecutionContext context)
   - bool CanExecute(IRobotController robot)
   - string GetDisplayName()
   - Color GetBlockColor()

2. Создать `Core/IRobotController.cs`:
   - Vector3 Position { get; }
   - Quaternion Rotation { get; }
   - bool IsExecuting { get; }
   - IPromise MoveForward(float units)
   - IPromise MoveBackward(float units)
   - IPromise TurnLeft()
   - IPromise TurnRight()
   - void Reset()
   - void Stop()

3. Создать `Core/ICommandExecutor.cs`:
   - bool IsRunning { get; }
   - IPromise ExecuteProgram(ICommand startCommand, IRobotController robot)
   - void Stop(), Pause(), Resume()
   - Events: OnCommandStarted, OnCommandCompleted, OnProgramCompleted, OnProgramFailed

4. Создать `Core/CommandBase.cs` - абстрактный базовый класс:
   - Наследует ICommand
   - Свойства Id, Type, Next
   - Виртуальные методы Execute, CanExecute, GetDisplayName, GetBlockColor
   - Защищённое поле float[] parameters

5. Создать `Execution/ExecutionContext.cs`:
   - int CurrentCommandId { get; set; }
   - int CommandsExecuted { get; set; }
   - float StartTime { get; set; }
   - Dictionary<string, object> Variables для будущих фич

## Критерии завершения
- [ ] Все интерфейсы созданы в namespace RobotProgramming.Core
- [ ] CommandBase создан и правильно наследует ICommand
- [ ] ExecutionContext создан с поддержкой переменных
- [ ] Проект компилируется без ошибок
- [ ] Все методы имеют правильные сигнатуры с использованием IPromise

## Заметки
- IPromise из существующей библиотеки PU.Promises
- Next property в ICommand - это связный список для последовательности команд
- Variables в ExecutionContext будут использоваться для циклов/условий в будущем
