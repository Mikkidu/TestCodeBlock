# Задача #3: RobotController и конфигурация

## Цель
Реализовать управление роботом с плавной lerp-анимацией через Promises. Робот должен двигаться и поворачиваться используя Timers.Wait().

## Контекст
Зависит от: Задача #2
Это ядро физического управления роботом. Использует существующую библиотеку Promises и Timers.

## Ключевые шаги
1. Создать `Robot/RobotConfig.cs` ScriptableObject:
   - moveDistance = 1f
   - turnAngle = 90f
   - moveSpeed = 2f
   - turnSpeed = 180f
   - movementCurve (AnimationCurve)
   - robotColor, directionColor

2. Создать `Robot/RobotController.cs` наследуя IRobotController:
   - Поля: RobotConfig config, Transform directionIndicator
   - Свойства: Position, Rotation, IsExecuting
   - MoveForward: вычислить targetPosition, длительность, вызвать Timers.Wait() с lerp callback
   - MoveBackward, TurnLeft, TurnRight: аналогично для rotation
   - Private методы AnimateMovement, AnimateRotation
   - Reset(), Stop()

3. Создать Robot prefab в Assets/Prefabs/:
   - Main Game Object "Robot"
   - Cube как тело робота
   - Дочерний Cube как индикатор направления (смещение по Z)

4. Создать RobotConfig.asset в Assets/ScriptableObjects/

5. Назначить RobotConfig в Robot prefab и настроить значения

6. Протестировать в Editor (Play mode), вызывая методы движения вручную

## Критерии завершения
- [ ] RobotConfig создан и можно редактировать в Inspector
- [ ] RobotController реализует IRobotController полностью
- [ ] MoveForward работает с плавной анимацией
- [ ] MoveBackward, TurnLeft, TurnRight реализованы
- [ ] Robot prefab создан с индикатором направления
- [ ] В Play mode робот движется плавно при вызове методов
- [ ] IsExecuting корректно устанавливается во время анимации

## Ключевой код
```csharp
private IPromise AnimateMovement(Vector3 targetPosition, float duration)
{
    Vector3 startPos = Position;

    return Timers.Instance.Wait(duration, progress =>
    {
        float curved = config.movementCurve.Evaluate(progress);
        transform.position = Vector3.Lerp(startPos, targetPosition, curved);
    })
    .Done(() => IsExecuting = false);
}
```

## Заметки
- Timers.Instance - это singleton из существующей библиотеки
- AnimationCurve позволяет настраивать ease-in/ease-out анимацию
- IsExecuting предотвращает перекрытие команд
- Reset() должна восстанавливать начальную позицию
