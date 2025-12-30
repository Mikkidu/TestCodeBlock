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
- Note: Параметры (типы данных) вынесены в отдельную задачу #12
- Detailed plan: [.Doc/Tasks/9_SnapToOutputs.md](Tasks/9_SnapToOutputs.md)

## #10a Снап в начало цепи
- Status: [✓] Done (2025-12-30)
- Description: Реализовать снап выхода перетаскиваемого блока ко входу первого блока - позволяет вставлять блоки в начало цепи
- Progress:
  - ✓ Этапы 1-7: Основная реализация (FindNearestInput, ApplySnapToInput, визуальный feedback)
  - ✓ Этап 8: Исправить разрыв входящих соединений при перемещении существующих блоков
  - ✓ Этап 9: Исправить визуальный feedback для новых блоков с палитры
- Bugs Fixed:
  - ✓ При перемещении СУЩЕСТВУЮЩЕГО блока в начало цепи входящие соединения не разрываются → зацикливание программы
  - ✓ При перетаскивании НОВОГО блока (с палитры) коннекторы не меняют цвет при визуализации снепа
- Detailed plan: [.Doc/Tasks/10a_SnapToBeginning.md](Tasks/10a_SnapToBeginning.md)

## #10b Снап в середину цепи
- Status: [→] Testing (исправления реализованы, ожидание верификации)
- Description: Реализовать вставку блока в середину цепи (между двумя блоками) с правильным переконектированием соединений
- Progress:
  - ✓ FindConnectedOutput() - добавлен для поиска входящих соединений
  - ✓ ApplySnapToInput() обновлена для переконектирования при вставке в середину
  - ✓ Исправлены все 3 критических бага:
    - ✓ #10b-1: cf8703f - Добавлена проверка inProgramArea в OnEndDrag
    - ✓ #10b-2: aa46156 - Удален дублированный ReturnToOriginalPosition
    - ✓ #10b-3: cf8703f + aa46156 - Разделение ответственности палитра/программа
- Bugs Fixed:
  - ✓ #10b-3: Палитровые блоки теперь копируются, не переносятся
  - ✓ #10b-2: Визуальный snap применяется для всех типов соединений
  - ✓ #10b-1: Блоки в конце цепи выполняются один раз
- Test Plan: [.Doc/BugFix_10b_TestPlan.md](BugFix_10b_TestPlan.md)
- Detailed plan: [.Doc/Tasks/10b_SnapToMiddle.md](Tasks/10b_SnapToMiddle.md)

## #11 Блок цикла
- Status: Pending
- Description: Создать блок цикла с поддержкой вложенных блоков и выполнением нужное количество раз
- Blockers: #10b (нужна полная snap система для подключения блоков внутри цикла)
- Detailed plan: [.Doc/Tasks/11_LoopBlock.md](Tasks/11_LoopBlock.md)

## #12 Параметры блоков
- Status: Pending
- Description: Добавить возможность задавать параметры к блокам (выпадающий список). Начать с числовых параметров (количество повторений)
- Blockers: #11 (для реализации параметра "количество повторений" для цикла)
- Detailed plan: [.Doc/Tasks/12_BlockParameters.md](Tasks/12_BlockParameters.md)
