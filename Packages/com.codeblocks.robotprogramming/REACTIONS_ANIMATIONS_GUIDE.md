# Реакции и анимации: устройство и расширение

Документ описывает текущую архитектуру реакций/анимаций в пакете и как безопасно добавлять новые реакции без правок кода.

## 1) Как устроено сейчас

### Поток реакции (runtime)
1. Команда движения (`MoveForward` / `MoveBackward`) получает `MoveDecision`.
2. По `obstacleTypeId` выбирается поведение движения (`ReactionProfileResolver` + `ReactionConfig`).
3. Для `StopProgramAtTarget` выполняется остановка с причиной через `ExecutionContext.Cancel("Reaction:<TypeId>")`.
4. Выбор trigger для визуальной реакции делает `ReactionAnimationResolver`:
   - сначала из `ReactionAnimationConfig` по `obstacleTypeId`,
   - затем fallback на `ReactionProfile.animationId` (обратная совместимость).
5. Trigger отправляется в `RobotController -> RobotAnimationDriver`.

### Тайминг запуска анимации
Поле `ReactionConfig.ReactionProfile.animationTriggerTiming` управляет моментом триггера:
- `Start`: trigger ставится сразу при начале обработки реакции (анимация может начаться до завершения перемещения).
- `End`: trigger ставится после завершения перемещения к целевой клетке.

По умолчанию используется `End`.

### Поведение остановки программы
- Для реакционных остановок используется `StopReason` в формате `Reaction:*`.
- `GameManager` не делает авто-перезапуск при `Reaction:*`.
- Игрок видит итог состояния (например, падение в яму) и вручную решает, когда перезапустить уровень.

### Ключевые классы
- `ReactionConfig` - логика движения (outcome/speed/distance/timing).
- `ReactionProfileResolver` - получение профиля реакции (включая alias/fallback).
- `ReactionAnimationConfig` - data-driven mapping `obstacleTypeId` -> `triggerId`.
- `ReactionAnimationResolver` - выбор trigger для запуска анимации.
- `RobotAnimationDriver` - изолированная работа с Animator робота.
- `ExecutionContext` / `CommandExecutor` - остановка выполнения с причиной (`StopReason`).

## 2) Как добавить новую реакцию (без изменения кода)

Пример: новый obstacle `Lava`.

1. Добавить объект/террейн с `objectTypeId` или `terrainType` = `Lava`.
2. В `ReactionConfig` добавить профиль для `Lava`:
   - `outcome`,
   - `speedModifier`,
   - `distanceMultiplier`,
   - `animationTriggerTiming` (`Start`/`End`),
   - (опционально) `animationId` как fallback.
3. В `ReactionAnimationConfig` добавить запись:
   - `obstacleTypeId: Lava`
   - `triggerId: <имя trigger параметра в Animator>`
4. Убедиться, что в `Robot.controller` есть trigger с этим именем.

После этого реакция и анимация должны работать без правок C#.

## 3) Где настраивать конфиги

### В проекте (Assets)
- `Assets/CodeBlocks/Resources/Configs/RobotConfig.asset`
- `Assets/CodeBlocks/Resources/Configs/DefaultReactionConfig.asset`
- `Assets/CodeBlocks/Resources/Configs/ReactionAnimationConfig.asset`

### В samples пакета
- `Packages/com.codeblocks.robotprogramming/Samples~/Assets/CodeBlocks/Resources/Configs/RobotConfig.asset`
- `Packages/com.codeblocks.robotprogramming/Samples~/Assets/CodeBlocks/Resources/Configs/ReactionAnimationConfig.asset`

## 4) Требования к Animator

Для корректной работы у робота должны существовать параметры:
- `IsMoving` (bool),
- `TurnLeft` (trigger),
- `TurnRight` (trigger),
- дополнительные trigger-параметры для реакций (например `Finish`, `Pit`, `Spike`, `OutOfBounds`, `NoTerrain`).

Имена можно переопределять через `RobotConfig`:
- `moveBoolParameter`,
- `turnLeftTriggerParameter`,
- `turnRightTriggerParameter`.

## 5) Anti-pop: как избежать визуального "рывка" при старте

### Что такое anti-pop
`Anti-pop` - это набор настроек/практик, чтобы анимированный объект не "прыгал" из позы префаба в позу анимации в первые кадры после загрузки уровня.

### Рекомендации
1. Настраивать transitions так, чтобы стартовое состояние совпадало с ожидаемым визуальным состоянием объекта.
2. Не делать конфликтующих переходов на `Any State`, если они срабатывают сразу на старте.
3. Проверять `Exit Time`, `Transition Duration`, `Has Exit Time` для стартовых переходов.
4. Для door/button проверить, что начальные bool-параметры (`IsOpen`, `IsPressed`) не вызывают резкой смены позы в первом кадре.
5. Если нужен "мягкий вход" в анимацию - задавать это через контроллер (transition blend), а не через runtime-костыли.

## 6) Пакетное использование (без правок кода)

Чтобы переносить систему в основной проект и добавлять реакции/анимации только конфигами:
1. Импортировать package + samples (или эквивалентные ассеты).
2. Проверить, что `RobotConfig.asset` доступен по одному из путей:
   - `Resources/RobotConfig`,
   - `Resources/Configs/RobotConfig`.
3. Поддерживать актуальный `ReactionAnimationConfig.asset` в проекте.
4. Добавлять новые obstacle и trigger-параметры через `ReactionConfig`/`ReactionAnimationConfig`/Animator.

## 7) Чеклист перед релизом

1. Все нужные `obstacleTypeId` добавлены в `ReactionAnimationConfig`.
2. Все trigger-параметры существуют в `Robot.controller`.
3. Для новых реакций нет C#-веток `if/else`; поведение задано конфигами.
4. Визуально проверены: старт уровня, finish, out-of-bounds, no-terrain, trap-сценарии.
5. В package samples лежат актуальные конфиги.
