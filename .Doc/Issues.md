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

## #10b Снап в середину цепи + улучшение магнитного притяжения
- Status: [✓] Done (2026-01-15)
- Description: Реализовать вставку блока в середину цепи + улучшить удобство магнитного притяжения с приоритизацией
- Latest Commits:
  - ✓ 533cc0b - Базовая логика snap (возврат в ProgramArea, без сдвижения)
  - ✓ a647dbf - Исправлена позиция для вставки в начало (OUTPUT к INPUT)
  - ✓ 4b96bfe - Добавлен метод AlignToInputConnection() для каскадного выравнивания
  - ✓ be44b6d - Замена ShiftBlockChain на систему каскадного выравнивания
  - ✓ [NEW] Добавлена приоритизация магнитного притяжения (Priority-based snapping)
- Implementation Details:
  - ✓ Алгоритм каскадного выравнивания: каждый блок выравнивается к своему входящему соединению
  - ✓ AlignToInputConnection() рекурсивно вызывается по цепи: B → C → D...
  - ✓ ApplySnapToInput() инициирует cascade выполняя nextBlock.AlignToInputConnection()
  - ✓ [NEW] Приоритизированный поиск с двумя уровнями приоритета
- Features Working:
  - ✓ Вставка в начало цепи (X перед A): OUTPUT X совпадает с INPUT A
  - ✓ Вставка в середину цепи (X между A и B):
    - INPUT X = OUTPUT A
    - OUTPUT X = INPUT B
    - B, C, D... автоматически выравниваются через cascade
  - ✓ Блоки возвращаются в ProgramArea после snap
  - ✓ Выполнение по физическим соединениям (GetNextBlock)
  - ✓ Нет визуальных наложений благодаря правильному выравниванию
  - ✓ [NEW] Магнитное притяжение с приоритизацией:
    - Входом: сначала к концам цепей, потом к середине
    - Выходом: только к начаткам новых участков (блокам без входящей связи)
- Test Plan: [.Doc/BugFix_10b_TestPlan.md](BugFix_10b_TestPlan.md)
- Detailed plan: [.Doc/Tasks/10b_SnapToMiddle.md](Tasks/10b_SnapToMiddle.md)
- Verification Checklist:
  - [ ] Тест 1: Вставка в начало цепи (X перед A)
  - [ ] Тест 2: Вставка в конец цепи (X после B)
  - [ ] Тест 3: Вставка в середину цепи (X между A и B, все блоки выравниваются)
  - [ ] Тест 4: Множественные вставки (A→X→Y→B→C)
  - [ ] Тест 5: Выполнение программы в правильном порядке
  - [ ] Тест 6: Отсутствие визуальных наложений

## #10b.1 BUG: Сдвиг блоков при вставке из ProgramArea не работает
- Status: [✓] Done (2026-01-15)
- Priority: 🔴 CRITICAL
- Description: При вставке СУЩЕСТВУЮЩЕГО блока из ProgramArea в середину цепи сдвиг не срабатывает. При вставке НОВОГО блока (с палитры) сдвиг работает правильно.
- Symptoms:
  - Новый блок (из палитры) вставляется в середину → блоки сдвигаются + выравниваются ✓
  - Существующий блок (перетаскивание в ProgramArea) вставляется в середину → блоки НЕ сдвигаются ✗
  - Визуальное наложение блоков при вставке существующего блока
- Root Cause: Вероятно разница в логике OnEndDrag между новыми блоками (с палитры) и существующими (в ProgramArea)
- Detailed plan: [.Doc/BugFix_10b1_ExistingBlockShift.md](BugFix_10b1_ExistingBlockShift.md)

## #10b.2 FEATURE: Визуализация линии магнитизма + Переписана логика выбора snap'а
- Status: [✓] Done (2026-01-15)
- Priority: 🟠 HIGH
- Description: Добавить визуальную линию между магнитящимися коннекторами (от коннектора перетаскиваемого блока к целевому коннектору) + исправить неточный выбор snap точек
- Implementation:
  - ✓ SnapLineRenderer.cs с RectTransformUtility координатным преобразованием
  - ✓ BlockUI.cs интеграция с SnapLineRenderer (поддержка палитры + существующих блоков)
  - ✓ SnapManager.cs переписана логика выбора snap'а (простое геометрическое сравнение расстояний)
- Critical Fixes (2026-01-14):
  - ✓ FIX #1: Линия с смещением вправо-вверх → RectTransformUtility.ScreenPointToLocalPointInRectangle()
  - ✓ FIX #2: Snap выбирает неправильную точку → Переписана на геометрический выбор (ближайший = выбран)
    - Удалена приоритизация (Priority 1/2)
    - Удален гистерезис (priorityThreshold)
    - Новая логика: INPUT→OUTPUT vs OUTPUT→INPUT, выбираем меньшее расстояние
- Compilation Status: ✓ Build succeeded (0 errors)
- Key Changes:
  - FindNearestSnap() - новый метод для объединённого поиска (INPUT→OUTPUT + OUTPUT→INPUT)
  - FindNearestOutput/Input() - deprecated, используют FindNearestSnap() для совместимости
  - HasIncomingConnection(BlockUI) - новая перегрузка для проверки входящих всего блока
- Acceptance Criteria:
  - [ ] Линия видна в Game окне без смещения
  - [ ] Линия ровная и соединяет обе точки корректно
  - [ ] Snap выбирает геометрически ближайшую точку (не "прилипает" к далёким)
  - [ ] Линия работает для палитра блоков и существующих блоков
- Test Plan: [.Doc/TESTING_Both_Fixes.md](TESTING_Both_Fixes.md)
- Implementation Report: [.Doc/SNAP_Logic_Refactor_Report.md](SNAP_Logic_Refactor_Report.md)
- Detailed plan: [.Doc/Features_10b2_MagnetLine.md](Features_10b2_MagnetLine.md)

## #11 Блок цикла
- Status: [✓] Done (2026-01-19)
- Note: Phase 5 пройдена, вложенные циклы работают корректно
- Description: Создать блок цикла с 4 коннекторами, динамическим размером и логикой выполнения через возврат управления
- Blockers: None
- Latest Updates:
  - ✓ (2026-01-16) Основные баги исправлены - Loop работает правильно!
  - ✓ (2026-01-16) Code cleanup: удалён закомментированный код
  - ✓ (2026-01-16) Документация создана: Analysis_11_CodeCleanup.md
- Progress:
  - ✓ Phase 1: Базовая инфраструктура (CommandType.Loop, LoopCommand.cs, LoopBlockUI.cs)
  - ✓ Phase 2: Интеграция в BlockFactory, GameManager, BlockPalette
  - ✓ Phase 3: Префаб создан в Unity
  - ✓ Phase 4: Новая архитектура с 4 коннекторами (DONE)
    - ✓ Step 8.1: BlockConnector - ConnectorRole enum
    - ✓ Step 8.2: LoopBlockUI.cs - 4 коннектора, динамическая высота
    - ✓ Step 8.3: LoopCommand.cs - определение источника вызова, маршрутизация
    - ✓ Step 8.4: Snap логика - поддержка внутренних коннекторов Loop
    - ✓ Step 8.5: Обновлен префаб - 4 коннектора, sliced left panel
  - [ ] Phase 5: Полное тестирование всех сценариев (NEXT)
- Known Limitations & Future Improvements:
  1. 🔴 Пересчёт при вставке в начало/середину - Loop размер не обновляется автоматически
  2. 🔴 Размер вложенного Loop - Outer Loop не пересчитывает при изменении inner Loop
  3. 🟠 Stop при reset - Нужна остановка программы при Reset robot state
  4. 🟠 Lock UI во время выполнения - Блокировка перетаскивания и палитры во время выполнения
  5. 🟠 Размер при удалении - Пересчёт при удалении блока из Loop
- Architecture:
  ```
  ┌──●────────────────────────────┐  ← внешний INPUT
  │          HEADER (300x50)      │
  ├──────┬──●─────────────────────┘  ← внутренний OUTPUT (→ первый блок)
  │ LEFT │  [вложенные блоки]
  │ SLICE│
  ├──────┴──○───────────────┐        ← внутренний INPUT (← последний блок)
  │        FOOTER (250x25)  │
  └──○──────────────────────┘        ← внешний OUTPUT
  ```
- Detailed plan: [.Doc/Tasks/11_LoopBlock.md](Tasks/11_LoopBlock.md)

## #11a Архитектурный рефактор BlockUI - Гибридный подход
- Status: [✓] Done (2026-01-21)
- Priority: 🟠 HIGH (подготовка к #12 и будущим If/IfElse блокам)
- Description: Переработка архитектуры BlockUI с Composition на гибридный подход (BlockUIBase + наследование + Map коннекторов)
- Motivation:
  - Унифицировать интерфейс для SnapManager, BlockFactory
  - Подготовить архитектуру к If, IfElse, Switch блокам
  - Map коннекторов вместо List для гибкости
  - Полиморфизм через наследование
- Progress:
  - [✓] Шаг 1: Создать BlockUIBase abstract class
    - Dictionary<string, BlockConnector> connectors
    - GetConnector(), GetAllConnectors(), GetInputConnectors(), GetOutputConnectors()
    - GetPrimaryInput(), GetPrimaryOutput() - virtual
    - SetCommand(), AlignToInputConnection(), DisconnectAllConnections()
    - UpdateSnapVisuals(), ResetAllConnectorColors()
  - [✓] Шаг 2: Создать BlockDragHandler компонент
    - Отдельный MonoBehaviour для Drag & Drop логики
    - IBeginDragHandler, IDragHandler, IEndDragHandler
    - Работает с любым BlockUIBase через GetComponent
  - [✓] Шаг 3: Переделать BlockUI : BlockUIBase
    - Минимальный класс, только InitializeConnectors()
    - Использует connectors[INPUT] и connectors[OUTPUT]
  - [✓] Шаг 4: Изменить BlockConnector.parentBlock на BlockUIBase
  - [✓] Шаг 5: Обновить SnapManager для работы с BlockUIBase
    - FindNearestSnap(BlockUIBase, List<BlockUI>)
    - GetInputConnectors(), GetOutputConnectors() вместо inputPoints/outputPoints
  - [✓] Шаг 6: Переделать LoopBlockUI : BlockUIBase
    - Наследование вместо sibling component
    - 4 коннектора через AddConnector()
    - Override GetPrimaryInput/Output для внешних коннекторов
    - Override RecalculateSize()
  - [✓] Шаг 7: Упростить API SnapManager
    - ApplySnap(block, targetConnector, area) - без inputPoint параметра
    - ApplySnapToInput(block, targetConnector, area) - без outputPoint параметра
    - Исправлен баг LogBlockState (GetOutputConnectors)
    - Удалены мёртвые методы и закомментированный код
  - [✓] Шаг 8: Обновить BlockFactory, ProgramArea
    - BlockFactory возвращает BlockUIBase (уже было)
    - ProgramArea.GetBlocks() → List<BlockUIBase> (уже было)
    - Исправлен баг в GetFirstBlock(): GetInputConnectors → GetOutputConnectors
  - [✓] Шаг 9: Централизовать SnapLineRenderer в ProgramArea
    - ProgramArea.SnapLineRenderer property добавлен
    - BlockUIBase использует programArea.SnapLineRenderer
  - [✓] Шаг 10: Обновить Loop prefab в Unity
    - Удалить компонент BlockUI
    - Добавить BlockDragHandler
    - Назначить blockImage, blockLabel в Inspector
  - [✓] Шаг 11: Тестирование всех сценариев
- Detailed plan: [.Doc/Tasks/11a_BlockUI_Refactor.md](Tasks/11a_BlockUI_Refactor.md)
- Architecture Analysis: [.Doc/Architecture_BlockUI_Strategy.md](Architecture_BlockUI_Strategy.md)

## #11b UPM пакет - подготовка и интеграция (Гибридный подход)
- Status: [→] In Progress (2026-01-21)
- Priority: 🔴 CRITICAL (подготовка к интеграции в основной проект)
- Description: Преобразование проекта в UPM пакет (гибрид: код в Packages/, ассеты в Assets/) и тестирование интеграции через Git URL
- Architecture:
  - **Packages/com.codeblocks.robotprogramming/** — только скрипты (.cs), обновляется через UPM
  - **Assets/CodeBlocks/** — префабы, уровни, конфиги, видимы команде
- Progress:
  - [✓] Создана структура UPM пакета в `Packages/com.codeblocks.robotprogramming/`
    - package.json с правильными ссылками (github.com/mikkiducher/TestCodeBlock)
    - Runtime/CodeBlocks.Runtime.asmdef
    - Editor/CodeBlocks.Editor.asmdef
    - CHANGELOG.md для версионирования
    - README.md с инструкциями по установке
    - MIGRATION_GUIDE_HYBRID.md — гибридный подход
    - PRIVATE_REPO_GUIDE.md — работа с приватным репо
  - [ ] Шаг 1: Создать Assets/CodeBlocks/ структуру
    - Assets/CodeBlocks/Prefabs/UI/
    - Assets/CodeBlocks/Prefabs/LevelEditor/Terrain/
    - Assets/CodeBlocks/Prefabs/LevelEditor/Objects/
    - Assets/CodeBlocks/Resources/Levels/
    - Assets/CodeBlocks/Resources/Configs/
  - [ ] Шаг 1.5: Переименовать namespace (ОПЦИОНАЛЬНО, но рекомендуется)
    - RobotProgramming.* → CodeBlocks.*
    - LevelEditor → CodeBlocks.LevelEditor
    - Promises остаётся БЕЗ изменений!
    - Инструкция: NAMESPACE_RENAME_GUIDE.md
  - [ ] Шаг 2: Перенос Runtime скриптов → Packages/
    - Assets/Scripts/RobotProgramming/* → Packages/.../Runtime/
    - ⚠️ Assets/Scripts/Promises/* — НЕ переносить! (остаётся в Assets, внешняя зависимость)
    - Assets/Scripts/LevelEditor/* (runtime) → Packages/.../Runtime/LevelEditor/
  - [ ] Шаг 3: Перенос Editor скриптов → Packages/
    - Assets/Scripts/LevelEditor/Editor/* → Packages/.../Editor/LevelEditor/
  - [ ] Шаг 4: Перенос ассетов → Assets/CodeBlocks/
    - Assets/PrefabsUI/* → Assets/CodeBlocks/Prefabs/UI/
    - Assets/Resources/CodeBlocks/* → Assets/CodeBlocks/Prefabs/LevelEditor/
    - Assets/Resources/RobotLevels/* → Assets/CodeBlocks/Resources/Levels/
    - Assets/Resources/Configs/* → Assets/CodeBlocks/Resources/Configs/
  - [ ] Шаг 5: Обновление путей Resources.Load в коде
    - "RobotLevels/tutorial_01" → "Levels/tutorial_01"
    - "CodeBlocks/Terrain/Ground" → "LevelEditor/Terrain/Ground"
  - [ ] Шаг 6: Полное тестирование в TestCodeBlock
    - Компиляция без ошибок
    - Функциональность UI (drag-drop блоков)
    - Функциональность Level Editor
    - Запуск игры и выполнение программ
  - [ ] Шаг 7: Подготовка Git репозитория
    - Коммит структуры пакета
    - Создание тега v1.0.0
    - Push с тегом в origin/master
  - [ ] Шаг 8: Тестирование интеграции в другом проекте
    - Создать тестовый Unity проект
    - Добавить пакет через git URL: `https://github.com/mikkiducher/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming#v1.0.0`
    - Вручную скопировать Assets/CodeBlocks/ в новый проект (или через .unitypackage)
    - Тестировать основные функции
  - [ ] Шаг 9: Документирование процесса обновлений
    - Как делать новые релизы (git tag)
    - Как обновлять скрипты (Package Manager)
    - Как обновлять ассеты (Export/Import)
- Git URL для интеграции:
  ```
  https://github.com/mikkiducher/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming#v1.0.0
  ```
- Resources:
  - MIGRATION_GUIDE_HYBRID.md — пошаговая инструкция (гибридный подход)
  - PRIVATE_REPO_GUIDE.md — работа с приватным репо
  - README.md — Quick Start и документация

## #12 Параметры блоков
- Status: Pending
- Description: Добавить возможность задавать параметры к блокам (выпадающий список). Начать с числовых параметров (количество повторений)
- Blockers: #11 (для реализации параметра "количество повторений" для цикла)
- Detailed plan: [.Doc/Tasks/12_BlockParameters.md](Tasks/12_BlockParameters.md)

---

## #13 Level Editor инфраструктура - структуры данных
- Status: [✓] Done (2026-01-13)
- Priority: 🔴 CRITICAL (требуется для #14-16)
- Description: Создать структуры данных для редактора уровней (Terrain + Objects + Points)
- Detailed plan: [.Doc/Tasks/13_LevelEditorInfra.md](Tasks/13_LevelEditorInfra.md)
- Steps:
  - [✓] Создать папку `Assets/Scripts/LevelEditor/`
  - [✓] Создать `CardinalDirection.cs` (enum: North, East, South, West)
  - [✓] Создать `TerrainCell.cs` (position, terrainType, IsPassable)
  - [✓] Создать `GridObject.cs` (position, objectTypeId, objectInstanceId, parameters)
  - [✓] Создать `StartPoint.cs` (position, direction)
  - [✓] Создать `FinishPoint.cs` (position)
  - [✓] Создать `LevelGridData.cs` как ScriptableObject (gridWidth, gridHeight, terrain[], objects[], start, finish)
  - [✓] Добавить методы в LevelGridData: GetTerrainAt(), GetObjectAt(), IsPassable()
  - [✓] Скомпилировать проект (зелёные галочки в Assets)
  - [✓] Создать тестовый asset через Create меню (Create → CodeBlocks → Level Grid Data)
  - [✓] Заполнить тестовый уровень (5-10 terrain, 1-2 objects, Start/Finish)
  - [✓] Проверить Console - нет Serialization warnings

## #14 Level Editor UI - редактор в Editor режиме
- Status: [✓] Phase 4 DONE (2026-01-13) - Полная система с Gizmos + Prefabs поддержкой
- Priority: 🔴 CRITICAL
- Description: Создать EditorWindow для редактирования уровней в Unity Editor
- Detailed plan: [.Doc/Tasks/14_LevelEditorUI.md](Tasks/14_LevelEditorUI.md)
- Phase 1-2 (DONE):
  - [✓] CodeBlocksLevelEditorWindow + GridVisualizer
  - [✓] Click handler с SceneView.duringSceneGui
  - [✓] PlaceTerrain / RemoveTerrain функции
- Phase 3 - Fixes (2026-01-13):
  - [✓] Ray casting debug логирование (проверка точности попадания)
  - [✓] Защита от случайных кликов: ignore middle-mouse, Ctrl/Shift/Alt
  - [✓] MiniMap исправлена: отображение совпадает с Scene View (top-down)
  - [✓] Виджеты сетки встали по центру ячеек (уже было)
- Phase 4 - Prefabs & Visualization (2026-01-13) - DONE:
  - [✓] LevelVisualizationManager.cs - управление префабами и GameObjects
  - [✓] TerrainBlockVisual.cs - компонент для terrain префабов
  - [✓] ObjectBlockVisual.cs - компонент для object префабов
  - [✓] PrefabGenerator.cs - Editor tool для генерации префабов (Tools/CodeBlocks/Generate Level Editor Prefabs)
  - [✓] Интеграция с GridVisualizer: PlaceTerrain/RemoveTerrain вызывают визуализацию
  - [✓] Toggle usePrefabs в Inspector для выбора: Gizmos-only или Prefabs mode
- Phase 4 - Bugfixes Round 1 (2026-01-13):
  - [✓] Редактирование работает только когда Level Editor окно открыто (reflection check)
  - [✓] Правый клик работает на MouseUp без drag (отличает от поворота камеры)
  - [✓] Префабы позиционируются в центре клеток (добавлен cellSize * 0.5f offset)
- Phase 4 - Bugfixes Round 2 (2026-01-13):
  - [✓] Префабы сразу создаются при включении usePrefabs toggle для существующих блоков
  - [✓] Исправлена ошибка ray casting: WorldToGridPos теперь использует FloorToInt вместо RoundToInt
- Phase 4 - Bugfixes Round 3 (2026-01-13):
  - [✓] ИСПРАВЛЕНО: Префабы сразу создаются при toggle - синхронизация usePrefabs перед rebuild
  - [✓] ИСПРАВЛЕНО: Объекты теперь размещаются корректно - добавлен placeTerrainMode флаг
  - [✓] ДОБАВЛЕНО: UI для выбора режима (Terrain Mode / Object Mode) с кнопками
- Usage:
  - 1. Open Level Editor (Window → CodeBlocks → Level Editor)
  - 2. Generate prefabs: Tools → CodeBlocks → Generate Level Editor Prefabs
  - 3. Enable "usePrefabs" toggle on GridVisualizer component
  - 4. Place/remove blocks in Scene View - увидите реальные GameObjects вместо Gizmos
- Features:
  - ✓ Gizmos-only mode (быстро, без создания объектов)
  - ✓ Prefabs mode (красивая 3D визуализация с реальными GameObjects)
  - ✓ Синхронизация визуализации при размещении/удалении блоков
  - ✓ Автоматическое позиционирование префабов по координатам сетки

## #15 Level Editor Tools - сохранение и загрузка
- Status: [✓] Done (2026-01-14)
- Priority: 🟠 HIGH
- Description: Реализовать экспорт/импорт JSON и загрузку уровней в игру
- Blockers: None
- Implementation:
  - [✓] LevelJsonData.cs - JSON-сериализуемые структуры данных
  - [✓] LevelJsonSerializer.cs - export/import функции
  - [✓] Export to JSON кнопка в Level Editor window
  - [✓] Import from JSON кнопка (импортирует в текущий уровень)
  - [✓] Полная поддержка terrain, objects, start/finish points
  - [✓] Человекочитаемый JSON формат
  - [✓] Версионирование и timestamp метаданные
- Documentation:
  - [✓] LevelEditor_JSON_System.md - полная спецификация
  - [✓] Bidirectional conversion (Unity ↔ JSON)

## #16 Level Editor примеры и интеграция
- Status: [✓] Done (2026-01-14)
- Priority: 🟠 HIGH
- Description: Создать 5 примеров уровней и протестировать всю цепь
- Implementation:
  - [✓] TutorialLevelGenerator.cs - автоматическая генерация 5 уровней
  - [✓] Menu: Tools → CodeBlocks → Generate Tutorial Levels
  - [✓] Level 1 (⭐): Move Forward - простое движение вперед
  - [✓] Level 2 (⭐): Turn and Move - L-образный путь с поворотами
  - [✓] Level 3 (⭐⭐): Avoid Obstacles - навигация вокруг стен
  - [✓] Level 4 (⭐⭐): Buttons & Doors - интерактивные элементы
  - [✓] Level 5 (⭐⭐⭐): Complex Maze - сложный лабиринт 10×10
- Assets Location:
  - [✓] Assets/Resources/RobotLevels/ - все 5 уровней
  - [✓] Сохраняются как ScriptableObject assets
  - [✓] Загружаются через Resources.Load() в игре
- Documentation:
  - [✓] TutorialLevels_Guide.md - полное описание каждого уровня
  - [✓] TutorialLevels_QuickStart.md - быстрая справка
  - [✓] HOW_TO_USE_TUTORIAL_LEVELS.md - инструкция (30 сек)
  - [✓] Stage10b_Tutorial_Levels_Summary.md - полный обзор

## #17 Level Editor (Phase 5 optional) - Prefab Configuration System
- Status: [○] Planning
- Priority: 🟢 LOW (optional enhancement)
- Description: Создать гибкую систему маппинга BlockType → Prefab через ScriptableObject конфиг
- Detailed plan: [.Doc/Phase5_PrefabConfigSystem_Plan.md](Phase5_PrefabConfigSystem_Plan.md)
- Motivation: Текущая система жестко привязана к именам (Ground → Ground.prefab). Конфиг позволит менять визуалы без кода.
- When: После #15-16, перед интеграцией в play-united

---

## СТАТУС DEVELOPMENT PIPELINE (неделя 2 янв 2026)

```
Текущий статус (13 янв):
┌─────────────────────────────────────────────────────┐
│ ИГРА (CORE) - СТАБИЛЬНО ✅                         │
├─────────────────────────────────────────────────────┤
│ #1-9: Базовая механика                      DONE   │
│ #10a: Снап в начало                         DONE   │
│ #10b: Снап в середину                    TESTING   │
│ #11: Блок цикла                          PENDING  │
│ #12: Параметры блоков                     PENDING  │
└─────────────────────────────────────────────────────┘

Неделя 2 (12-16 янв):
┌─────────────────────────────────────────────────────┐
│ РЕДАКТОР УРОВНЕЙ (NEW FEATURE) - ПОЛНОСТЬЮ ГОТОВ ✅ │
├─────────────────────────────────────────────────────┤
│ #13: Инфраструктура                       [✓] DONE  │
│ #14: UI редактора (Phase 1-4)             [✓] DONE  │
│ #15: Инструменты (JSON/Load)              [✓] DONE  │
│ #16: Примеры и интеграция                 [✓] DONE  │
│ #17: Prefab Config (optional Phase 5) [○] Planning  │
└─────────────────────────────────────────────────────┘

Затем в play-united:
┌─────────────────────────────────────────────────────┐
│ ИНТЕГРАЦИЯ В PLAY-UNITED (Week 3-4)                │
├─────────────────────────────────────────────────────┤
│ Копирование кода CodeBlocks из TestCodeBlock      │
│ Интеграция Level Editor в MiniGameManager         │
│ Тестирование цепи: Editor → Game → Results       │
└─────────────────────────────────────────────────────┘
```

## КЛЮЧЕВЫЕ ЗАВИСИМОСТИ

- #10b (снап в середину) → нужна для корректного редактирования
- #11 (цикл) + #12 (параметры) → можно добавить позже
- Level Editor (#13-16) → параллельная разработка с #10b-12
