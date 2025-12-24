# Задача #4: Реализация команд

## Цель
Создать 4 базовых типа команд (MoveForward, MoveBackward, TurnLeft, TurnRight), наследуя CommandBase.

## Контекст
Зависит от: Задача #2, #3
Это конкретные реализации паттерна Команда.

## Ключевые шаги
1. Создать `Commands/MoveForwardCommand.cs`:
   - Override Type => CommandType.MoveForward
   - Constructor с параметром distance
   - Execute: return robot.MoveForward(distance)
   - GetDisplayName(), GetBlockColor()

2. Создать `Commands/MoveBackwardCommand.cs` - аналогично

3. Создать `Commands/TurnLeftCommand.cs`:
   - Override Type => CommandType.TurnLeft
   - Execute: return robot.TurnLeft()

4. Создать `Commands/TurnRightCommand.cs` - аналогично

5. Протестировать создание каждой команды и вызов Execute()

## Критерии завершения
- [ ] Все 4 команды созданы в namespace RobotProgramming.Commands
- [ ] Каждая наследует CommandBase
- [ ] Execute возвращает IPromise от robot методов
- [ ] GetDisplayName возвращает понятное название
- [ ] GetBlockColor возвращает разные цвета для каждой команды
- [ ] Каждая команда протестирована отдельно

## Шаблон
```csharp
public class MoveForwardCommand : CommandBase
{
    public override CommandType Type => CommandType.MoveForward;
    private float distance;

    public MoveForwardCommand(int id, float distance = 1f)
        : base(id, new float[] { distance })
    {
        this.distance = distance;
    }

    public override IPromise Execute(IRobotController robot, ExecutionContext context)
    {
        return robot.MoveForward(distance);
    }

    public override string GetDisplayName() => $"Вперёд {distance}";
    public override Color GetBlockColor() => new Color(0.3f, 0.6f, 1f);
}
```
