# Реестр задач: Визуальное программирование робота

## #1 Базовая инфраструктура
- Status: [✓] Done (2025-12-23)
- Description: Создать структуру папок и базовые enum/data классы для системы команд
- Blockers: None
- Detailed plan: `.Doc/Tasks/1_BasicInfrastructure.md`

## #2 Core интерфейсы
- Status: [✓] Done (2025-12-23)
- Description: Определить контракты (ICommand, IRobotController, ICommandExecutor) и базовый класс
- Blockers: None
- Detailed plan: `.Doc/Tasks/2_CoreInterfaces.md`

## #3 RobotController и конфигурация
- Status: [✓] Done (2025-12-23)
- Description: Реализовать управление роботом с плавной lerp-анимацией через Promises
- Blockers: None
- Detailed plan: `.Doc/Tasks/3_RobotController.md`

## #4 Реализация команд
- Status: [✓] Done (2025-12-23)
- Description: Создать 4 базовых команды (MoveForward, MoveBackward, TurnLeft, TurnRight)
- Blockers: None
- Detailed plan: `.Doc/Tasks/4_CommandImplementation.md`

## #5 Система выполнения
- Status: [✓] Done (2025-12-23)
- Description: Реализовать CommandExecutor с рекурсивной цепочкой Promise
- Blockers: None
- Detailed plan: `.Doc/Tasks/5_ExecutionSystem.md`

## #6 UI - BlockUI и BlockFactory
- Status: [✓] Done (2025-12-23)
- Description: Создать визуальное представление блока с drag-drop функционалом
- Blockers: None
- Detailed plan: `.Doc/Tasks/6_BlockUI.md`

## #7 UI - ProgramArea и BlockPalette
- Status: [✓] Done (2025-12-23)
- Description: Реализовать рабочую область и палитру блоков с snap-логикой
- Blockers: None
- Detailed plan: `.Doc/Tasks/7_ProgramUI.md`

## #8 GameManager и интеграция
- Status: [✓] Done (2025-12-23)
- Description: Связать все системы, добавить кнопки Run/Stop/Reset, финальное тестирование
- Blockers: None
- Detailed plan: `.Doc/Tasks/8_Integration.md`

## #9 Магнитный снап блоков к выходам
- Status: [✓] Done (2025-12-30)
- Description: Реализовать визуальные входы/выходы блоков с магнитным снапом и выполнение по физическим соединениям
- Progress:
  - ✓ Этап 1: Инфраструктура (BlockConnector)
  - ✓ Этап 2: Визуальные точки (Inspector assignment)
  - ✓ Этап 3: SnapManager и поиск (FindNearestOutput)
  - ✓ Этап 4: Визуальный feedback (Colors: Green/Red/Yellow)
  - ✓ Этап 5: Применение снапа (ApplySnap при OnDrop с палитры)
  - ✓ Этап 6: Выполнение по соединениям (GetNextBlock, навигация по connectedTo, правильный стартовый блок)
- Blockers: None
- Note: Параметры (типы данных) вынесены в отдельную задачу #10
- Detailed plan: [.Doc/Tasks/9_SnapToOutputs.md](Tasks/9_SnapToOutputs.md)
