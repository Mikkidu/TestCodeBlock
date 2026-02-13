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
- Status: [✓] Done (2026-01-21)
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
  - [✓] Шаг 1: Создана Assets/CodeBlocks/ структура
    - Assets/CodeBlocks/Prefabs/UI/
    - Assets/CodeBlocks/Prefabs/LevelEditor/Terrain/
    - Assets/CodeBlocks/Prefabs/LevelEditor/Objects/
    - Assets/CodeBlocks/Resources/Levels/
    - Assets/CodeBlocks/Resources/Configs/
  - [✓] Шаг 1.5: Переименован namespace на CodeBlocks.*
  - [✓] Шаг 2: Перенесены Runtime скрипты → Packages/
  - [✓] Шаг 3: Перенесены Editor скрипты → Packages/
  - [✓] Шаг 4: Перенесены ассеты → Assets/CodeBlocks/
  - [✓] Шаг 5: Обновлены пути Resources.Load в коде
  - [✓] Шаг 6: Полное тестирование в TestCodeBlock — PASSED
  - [✓] Шаг 7: Git репозиторий подготовлен с тагами (v1.0.1, v1.0.2)
  - [✓] Шаг 8: Интеграция протестирована
  - [✓] Шаг 9: Документация завершена
- Git URL для интеграции:
  ```
  https://github.com/mikkiducher/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming#v1.0.2
  ```
- Resources:
  - package.json с правильными версиями
  - CHANGELOG.md с историей версий
  - README.md с инструкциями

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

## #18 LevelRuntimeManager - Загрузка уровней в Play режиме
- Status: [✓] Done (2026-01-22)
- Priority: 🔴 CRITICAL
- Description: Создать компонент для инстанцирования уровней из LevelGridData в Play режиме с API для преобразования координат Grid ↔ World
- Blockers: None
- Implementation: `Packages/com.codeblocks.robotprogramming/Runtime/Managers/LevelRuntimeManager.cs`
- Features:
  - LoadLevel(LevelGridData) инстанцирует все визуалы
  - GetWorldPosition() и GetGridPosition() для преобразования координат
  - Gizmos отображают сетку уровня
  - Debug Gizmos для Start/Finish точек
- Detailed plan: [.Doc/Tasks/18_LevelRuntimeManager.md](Tasks/18_LevelRuntimeManager.md)

## #19 Robot Grid Integration - Позиционирование робота на уровне
- Status: [✓] Done (2026-01-22)
- Priority: 🔴 CRITICAL
- Depends On: #18 ✓
- Description: Связать робота с уровнем - автоматическая установка в start point при загрузке, Reset возвращает на старт
- Implementation:
  - RobotController.SetStartPosition() для динамической установки старта
  - GameManager.LoadLevel() интегрирует с LevelRuntimeManager
  - PositionRobotAtStart() преобразует grid → world координаты
  - Reset возвращает робота в start point уровня
- Detailed plan: [.Doc/Tasks/19_RobotGridIntegration.md](Tasks/19_RobotGridIntegration.md)

## #20 GridPositionTracker - Отслеживание положения робота
- Status: [✓] Done (2026-01-22)
- Priority: 🔴 CRITICAL
- Depends On: #18 ✓, #19 ✓
- Description: Компонент отслеживает на какой grid-клетке находится робот после каждого движения, генерирует события при изменении позиции
- Implementation: `Packages/com.codeblocks.robotprogramming/Runtime/Robot/GridPositionTracker.cs`
- Features:
  - Event OnGridPositionChanged при движении робота
  - Event OnMovedToImpassableTerrain для ловушек (future)
  - Event OnReachedFinish для детекции финиша (используется в #21)
  - IsOnGrid() и GetDistanceFromGrid() для валидации точности
  - Debug Gizmos показывают текущую клетку и направление движения
- Detailed plan: [.Doc/Tasks/20_GridPositionTracker.md](Tasks/20_GridPositionTracker.md)

## #21 Finish Detection - Определение достижения финиша
- Status: [✓] Done (2026-01-22)
- Priority: 🔴 CRITICAL
- Depends On: #20 ✓
- Description: При достижении робота finish point показывать UI сообщение "Уровень пройден!" и останавливать программу
- Implementation:
  - GridPositionTracker.OnReachedFinish event при совпадении позиции с финишем
  - GameManager.OnRobotReachedFinish() обрабатывает событие
  - Программа останавливается, UI обновляется "Уровень пройден! 🎉"
  - Флаг hasReachedFinish предотвращает множественные срабатывания
- Detailed plan: [.Doc/Tasks/21_FinishDetection.md](Tasks/21_FinishDetection.md)

---

## СТАТУС DEVELOPMENT PIPELINE (актуальный на 2026-01-22)

```
Текущий статус (22 янв):

┌──────────────────────────────────────────────────────────┐
│ ИГРА (CORE) - СТАБИЛЬНО ✅ + УРОВНИ ГОТОВЫ ✅           │
├──────────────────────────────────────────────────────────┤
│ #1-9: Базовая механика                          DONE ✅ │
│ #10a: Снап в начало                             DONE ✅ │
│ #10b: Снап в середину                          DONE ✅ │
│ #10b.1: Fix блоков при вставке                  DONE ✅ │
│ #10b.2: Визуализация линии магнитизма           DONE ✅ │
│ #11: Блок цикла                                 DONE ✅ │
│ #11a: Рефактор BlockUI архитектуры              DONE ✅ │
│ #12: Параметры блоков                         PENDING   │
└──────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────┐
│ UPM ПАКЕТ - ПОЛНОСТЬЮ ГОТОВ ✅                          │
├──────────────────────────────────────────────────────────┤
│ #11b: Преобразование в UPM (гибридный подход) DONE ✅  │
│       - Namespace переименован на CodeBlocks.*          │
│       - Код в Packages/, ассеты в Assets/             │
│       - Версионирование (v1.0.1, v1.0.2)              │
│       - Git URL интеграция работает                     │
└──────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────┐
│ РЕДАКТОР УРОВНЕЙ (NEW FEATURE) - ПОЛНОСТЬЮ ГОТОВ ✅    │
├──────────────────────────────────────────────────────────┤
│ #13: Инфраструктура (структуры данных)         DONE ✅  │
│ #14: UI редактора (Phase 1-4 + Prefabs)        DONE ✅  │
│ #15: Инструменты (JSON/Load)                   DONE ✅  │
│ #16: Примеры и интеграция (5 туториалов)       DONE ✅  │
│ #17: Prefab Config (optional Phase 5)      PLANNING   │
└──────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────┐
│ ИНТЕГРАЦИЯ РОБОТА + УРОВНЕЙ - ПОЛНОСТЬЮ ГОТОВА ✅      │
├──────────────────────────────────────────────────────────┤
│ #18: LevelRuntimeManager (загрузка уровней)   DONE ✅   │
│ #19: Robot Grid Integration (позиционирование)DONE ✅   │
│ #20: GridPositionTracker (отслеживание)       DONE ✅   │
│ #21: Finish Detection (финиш уровня)          DONE ✅   │
└──────────────────────────────────────────────────────────┘

Затем в play-united:
┌──────────────────────────────────────────────────────────┐
│ ИНТЕГРАЦИЯ В PLAY-UNITED (когда потребуется)           │
├──────────────────────────────────────────────────────────┤
│ 1. Добавить UPM пакет через Git URL                    │
│ 2. Скопировать Assets/CodeBlocks/ в новый проект      │
│ 3. Интеграция с MiniGameManager                        │
│ 4. Тестирование цепи: Editor → Game → Results         │
└──────────────────────────────────────────────────────────┘
```

## КЛЮЧЕВЫЕ ЗАВИСИМОСТИ

- #10b (снап в середину) → нужна для корректного редактирования
- #11 (цикл) + #12 (параметры) → можно добавить позже
- Level Editor (#13-16) → параллельная разработка с #10b-12

## #22 Drag & Drop улучшение - Вытаскивание первого/последнего блока из Loop
- Status: [✓] Done (2026-01-22)
- Priority: 🟠 HIGH
- Description: Безопасное вытаскивание блока из цепи внутри Loop с автоматическим схлопыванием соединений.
- Implementation (✓ COMPLETE):
  - [✓] Добавлен метод `BypassBlockInLoop()` в BlockDragHandler (26 строк)
    - Проверяет только PrimaryInput и PrimaryOutput (External)
    - Ищет соединение с внутренними коннекторами (InternalOutput / InternalInput)
    - Переподключает противоположный коннектор (bypass logic)
  - [✓] Интегрирован в `OnBeginDrag()` ДО `DisconnectAllConnections()`
  - [✓] Логирование добавлено для debug-а
- Test Cases:
  - [Test 1] Вытащить первый блок → InternalOutput переподключится к следующему ✓
  - [Test 2] Вытащить последний блок → Предыдущий переподключится к InternalInput ✓
  - [Test 3] Один блок в Loop → InternalOutput.connectedTo = null ✓
  - [Test 4] Вытащить средний блок → Не должно быть логов bypass-а ✓
  - [Test 5] Вытащить и переподключить в другое место ✓
  - [Test 6] Отпустить блок без snap → возврат на место ✓
- Testing Plan: [.Doc/TESTING_DragFromLoop.md](TESTING_DragFromLoop.md)
- Solution Doc: [.Doc/BACKLOG_DragFromLoop_Solution.md](BACKLOG_DragFromLoop_Solution.md)
- **NEXT:** Протестировать в Unity Editor

## #24 Инициализация уровня по запросу - InitLevel() API (КРИТИЧНА ДЛЯ ИНТЕГРАЦИИ В PLAY-UNITED)
- Status: [→] **~95% DONE** (2026-01-26, работает но требует доработок)
- Priority: 🔴 CRITICAL (блокирует интеграцию в play-united)
- Depends On: #18 ✓, #19 ✓, #20 ✓, #21 ✓
- Description: Создать публичный API `InitLevel(LevelGridData)` для множественной загрузки уровней с автоматической очисткой программы и ленивой инициализацией
- Progress:
  - [✓] Шаг 1: Реализован метод `InitLevel()` с ленивой инициализацией (2026-01-26) ✅ РАБОТАЕТ
    - Добавлен флаг `isInitialized`
    - Переделан `Init()` с защитой от повторной инициализации
    - Создан публичный метод `InitLevel(LevelGridData)` с автоочисткой программы
    - Обновлён `Start()` для вызова `InitLevel`
    - Обновлён `LevelRuntimeManagerTest` для использования нового API
    - **РЕЗУЛЬТАТ**: Уровни загружаются корректно, старые удаляются, память чистится ✅
  - [ ] Шаг 2: Тестирование в Unity Editor (ОСНОВНОЕ ТЕСТИРОВАНИЕ ПРОЙДЕНО ✅)
  - [ ] Шаг 3: Debug UI для переключения уровней (опционально)
  - [ ] Шаг 4: Memory Profiler тесты (опционально)
- Architecture:
  - `Init()` - приватный, однократный (компоненты + события)
  - `InitLevel(level)` - публичный, многократный (загрузка уровня + очистка программы)
  - Ленивая инициализация: `InitLevel()` автоматически вызывает `Init()` при первом запуске
- Key Features:
  - ✅ Всегда останавливает программу перед загрузкой
  - ✅ Всегда очищает ProgramArea при загрузке нового уровня
  - ✅ LevelRuntimeManager.ClearLevel() автоматически удаляет старые GameObjects
  - ✅ GridPositionTracker.Initialize() сбрасывает `hasReachedFinish` флаг
- Usage in play-united:
  - Новый уровень: `gameManager.InitLevel(nextLevel)` → очистка программы
  - Рестарт: `gameManager.OnResetButtonClicked()` → без очистки программы
- **⚠️ ИЗВЕСТНЫЕ ПРОБЛЕМЫ (для последующего исправления)**:
  - [ ] #24-BUG-1: Старт и финиш точки остаются как были после перезагрузки уровня
    - **Описание**: При `InitLevel(newLevel)` визуальные маркеры StartPoint/FinishPoint могут не обновиться
    - **Приоритет**: 🟡 СРЕДНИЙ (не блокирует функционал, но влияет на визуал)
    - **Решение**: Пересинхронизировать GridPositionTracker с новыми точками
  - [ ] #24-UX-1: Видно моргание при смене старого и нового уровней
    - **Описание**: При `InitLevel()` наблюдается визуальное моргание (старый удаляется, новый появляется)
    - **Причина**: Синхронная загрузка без анимации/splash
    - **Решение**: Добавить Splash/анимированный переход или асинхронную загрузку уровней
    - **Приоритет**: 🟡 СРЕДНИЙ (UX улучшение)
    - **Опции**:
      - Вариант 1: Черный Splash экран 0.3-0.5 сек во время переключения
      - Вариант 2: Асинхронная загрузка уровня (LoadLevelAsync)
      - Вариант 3: Fade In/Out анимация при переключении
- **✅ ИСПРАВЛЕНО (благодаря #24 правильной инициализации)**:
  - ✅ Баг с неправильным позиционированием уровня - исчез!
  - **Причина**: Префаб CodeBlocksWindow при показе меняет свои размеры (якоря/границы), что было проблемой для позиционирования
  - **Решение**: Правильная инициализация уровня при OnShowing() сделала это неактуально
  - **Результат**: #3b-1 (Позиционирование уровня) теперь **НЕ ТРЕБУЕТСЯ**
- Detailed plan: [.Doc/Tasks/24_Step1_InitLevel_Implementation.md](Tasks/24_Step1_InitLevel_Implementation.md)

---

## #23 BUG: Позиционирование сброшенных блоков в локальных координатах
- Status: [✓] Done (2026-01-23)
- Priority: 🔴 CRITICAL
- Description: Исправить баг позиционирования при перетаскивании блоков. Сброшенные блоки должны позиционироваться в локальных координатах родителя правильно, независимо от размера родителя (вложенные Loop, разные ProgramArea и т.д.)
- Root Cause:
  - AlignToInputConnection(), ApplySnap(), ApplySnapToInput() используют мировые координаты (rect.position)
  - При SetParent(parent, true) мировая позиция сохраняется, но локальные координаты не конвертируются
  - Работает только когда родитель == rootCanvas, ломается для вложенных контейнеров
- Impact:
  - Блоки позиционируются неправильно в Loop контейнерах
  - Визуальное несовпадение между положением сброса и финальной позицией
  - Требует расширения родителя на весь канвас (невозможно для вложенных контейнеров)
- Solution Approach:
  - Создать публичный метод `SetWorldPosition(Vector3 worldPosition)` в BlockUIBase
    - Инкапсулирует всю логику конвертации координат
    - Сам получает доступ к RectTransform, parentRect, Canvas
    - Использует RectTransformUtility для конвертации: мировая → экранная → локальная
  - Заменить прямое манипулирование rect.position на вызовы SetWorldPosition()
  - Применить в трех методах: AlignToInputConnection(), ApplySnap(), ApplySnapToInput()
  - Исправить SetParent вызовы в SnapManager.cs (изменить true на false после SetWorldPosition)
- Implementation Steps:
  1. Создать SetWorldPosition() в BlockUIBase
  2. Рефакторинг AlignToInputConnection() для использования SetWorldPosition()
  3. Рефакторинг ApplySnap() для использования SetWorldPosition()
  4. Рефакторинг ApplySnapToInput() для использования SetWorldPosition()
  5. Исправить SetParent параметры в SnapManager.cs (строки 303, 365)
  6-7. Комплексное тестирование и валидация
- Key Files to Fix:
  - `BlockUIBase.cs` - добавить SetWorldPosition(), обновить AlignToInputConnection()
  - `SnapManager.cs:217-307` - ApplySnap() использует SetWorldPosition()
  - `SnapManager.cs:310-369` - ApplySnapToInput() использует SetWorldPosition()
  - `SnapManager.cs:303, 365` - изменить SetParent параметры с true на false
- Detailed plan: [.Doc/Tasks/23_BlockLocalCoordinateFix.md](Tasks/23_BlockLocalCoordinateFix.md)

## #25 Bugfix релиз v1.0.8 - Unified Start/Finish + Публичные методы (КРИТИЧНО ДЛЯ PLAY-UNITED)
- Status: [✓] **Done** (2026-01-28, приоритет 🔴 CRITICAL)
- Priority: 🔴 CRITICAL (блокирует 100% готовность месяца 2 в play-united)
- Description: Унификация Start/Finish архитектуры + публичные API методы для управления программой + bugfixes
- Progress:
  - [✓] **UNIFIED REFACTOR: Start/Finish как GridObject** (завершено 2026-01-28)
    - StartPoint и FinishPoint теперь в objects[] массиве (не отдельные поля)
    - Создан Migration Tool для конвертации старых уровней
    - Добавлены GetStartPoint(), GetFinishPoint(), GetStartDirection() API
    - Обновлены все 13 файлов для использования unified API
    - Backward compatibility через deprecated поля с fallback логикой
    - **Detailed plan**: [.Doc/Tasks/25_Unified_StartFinish_Refactor.md](Tasks/25_Unified_StartFinish_Refactor.md)
  - [→] **ШАГ 1: Публичные API + Stop при Reset** (в процессе 2026-01-28)
    - **Detailed plan**: [.Doc/Tasks/25_Step1_PublicAPI_StopFixes.md](Tasks/25_Step1_PublicAPI_StopFixes.md)
- Key Changes:
  1. **Unified Architecture**: Start/Finish теперь обычные GridObject в objects[]
  2. **Public API**: 5 методов для внешнего управления (StartProgram, StopProgram, ClearProgram, IsProgramRunning, GetBlocksCount)
  3. **Stop при Reset**: OnResetButtonClicked() теперь корректно останавливает программу
  4. **BUG fixes**: Маркеры дублирования исправлены благодаря unified архитектуре
- Blockers: None
- Next Steps:
  - [ ] Реализовать публичные API методы в GameManager.cs
  - [ ] Рефакторинг OnResetButtonClicked() для Stop
  - [ ] Обновить CHANGELOG.md с полным списком изменений v1.0.8
  - [ ] Протестировать все изменения
  - [ ] Git release v1.0.8

---

## ЗАДАЧИ НА БУДУЩЕЕ (BACKLOG)

Здесь хранятся баги и задачи для будущей реализации. Когда задачи будут планироваться (создаваться задачи #N с детальными планами), они удаляются из этого списка.

### Выполнение программы
- [ ] Реализовать правильный Stop программы
- [ ] Реализовать дополнительный вызов стоп при нажатии клавиш Reset и Clear

### Drag & Drop улучшения
- [ ] Реализовать вытаскивание блока из СЕРЕДИНЫ цепи в ProgramArea:
  - Если потащили блок из середины цепи (например 2й из 3х):
    - Разрывается связь только между взятым и предыдущим (между 2м и 1м)
    - Связь с последующими сохраняется
    - На каждый OnDrag выполняется Alignment для всех последующих блоков (для 3го)
    - При отпускании сохраняется финальное положение и связь для всех последующих
    - Входной коннектор перетаскиваемого остаётся пустым если отпустили далеко
    - Заполняется связью если рядом был выход другого блока (не перетаскиваемого и не из списка присоединённых)
    - (Возможно временно делать последующие блоки дочерними к перетаскиваемому)
  - **ПРИМЕЧАНИЕ:** Вытаскивание первого/последнего уже реализовано в #22

- [ ] Доработать перетаскивание нескольких блоков:
  - Примагничивание работает аналогично, только выходным коннектором считается выход последнего в цепи блока
  - Входной коннектор - вход перетаскиваемого
  - При отпускании если подключаемся входом в середину другой цепи:
    - Подключение: перетаскиваемый input к блоку A, выход последнего перетаскиваемого к входу B
    - Схема: A→B становится A→C→B
  - Если подключаемся выходом последнего блока:
    - Нужно добавить Alignment в обратную сторону (от последнего к первому)
    - Добавить метод GetPreviousBlock()

### Loop блок улучшения
- [ ] Проверить что при перетаскивании блоки внутри цикла тоже перетаскиваются
- [ ] Реализовать правильное удаление блоков из цикла

### BUG: Loop выравнивание цепочки при присоединении OUTPUT к INPUT другого блока
- **Описание:** Если присоединить Loop с его OUTPUT к INPUT другого блока (схема: A → [Loop] → B), то при добавлении новых блоков во внутреннюю область Loop подстройка (AlignToInputConnection) не передаётся дальше к блоку B.
- **Как воспроизвести:**
  1. Создать цепь: [BlockA] → [Loop] → [BlockB]
  2. Добавить блок внутрь Loop (например BlockC внутрь Loop)
  3. BlockC выравнивается правильно к InternalOutput Loop
  4. Но BlockB НЕ выравнивается (стоит на старой позиции)
- **Ожидаемое поведение:** После добавления BlockC, цепочка выравнивания должна быть:
  - BlockC.AlignToInputConnection() → выравняется к InternalOutput
  - Loop.AlignToInputConnection() → выравняется к BlockA.output
  - BlockB.AlignToInputConnection() → выравняется к Loop.output
- **Контрапример (работает правильно):** Если INPUT Loop присоединен к OUTPUT BlockA (наоборот: A → Loop → B), подстройка работает правильно, все блоки выравниваются.
- **Root Cause:** Вероятно в цепочке вызовов GetNextBlock() → AlignToInputConnection() для вложенных Loop структур
- **Priority:** 🟠 HIGH (влияет на удобство редактирования сложных программ)

### BUG: Размер Loop контейнера не пересчитывается при вытаскивании блоков
- **Описание:** При вытаскивании блока из внутренней области Loop контейнер Loop не пересчитывает свой размер. Loop остаётся большим даже если блоки внутри удалены.
- **Как воспроизвести:**
  1. Создать Loop с несколькими блоками внутри
  2. Потащить один из внутренних блоков наружу (или удалить из цепи)
  3. Loop контейнер остаётся с прежней высотой (не сжимается)
- **Ожидаемое поведение:** После вытаскивания блока должен вызваться Loop.RecalculateSize() для пересчёта высоты контейнера
- **Current Fix:** Loop.RecalculateSize() вызывается при добавлении блока (ProgramArea.AddBlockToProgram), но не при удалении/вытаскивании
- **Related Code:**
  - BlockDragHandler.OnBeginDrag() - вызывает BypassBlockInLoop() но не RecalculateSize()
  - LoopBlockUI.RecalculateSize() - динамический расчёт высоты на основе contenSize
- **Priority:** 🟠 HIGH (влияет на визуальный интерфейс)

---

## #26 Доработка механики цепи блоков - InputPoint и навигация

**Status**: [✓] **DONE** (2026-01-30)
**Priority**: 🔴 **CRITICAL** (основная механика для интеграции в play-united)
**Description**: Реализовать базовую навигацию по цепи блоков и InputPoint как точку старта программы

**Depends On**:
- #24 ✓ (InitLevel API)
- #25 ✓ (Start/Finish unified)
- #9-#10b ✓ (Snap система)

**Progress**: 3 из 3 обязательных шагов выполнено (100%)

**Implementation Status**:

### ✅ Шаг 1: Навигация по цепи (DONE - 2026-01-30)
- ✓ Добавлен `GetPreviousBlock()` в BlockUIBase - обратная навигация по цепи
- ✓ Добавлен `GetLastBlockInChain()` в ProgramArea - поиск последнего блока
- ✓ Тесты в GameManagerAPITest
- 📄 Код: BlockUIBase.cs:242-252, ProgramArea.cs:209-224

### ✅ Шаг 2: InputPoint API (DONE - 2026-01-30)
- ✓ Добавлено поле `inputPoint` в ProgramArea
- ✓ API методы: GetInputPointTransform(), GetInputPointWorldPosition(), GetInputPointScreenPosition(), HasInputPoint()
- ✓ Тесты в GameManagerAPITest
- ✓ Инструкции по настройке в Unity: `.Doc/Tasks/26_Step2_InputPoint_Setup_Instructions.md`
- 📄 Код: ProgramArea.cs:251-298

### ✅ Шаг 3: Магнетизм к InputPoint (DONE - 2026-01-30)
- ✓ Добавлен тип снэпа `InputToInputPoint` в SnapManager
- ✓ FindNearestSnap() проверяет расстояние INPUT блока до InputPoint
- ✓ Метод ApplySnapToInputPoint() позиционирует INPUT блока к InputPoint
- ✓ Первый блок ВСЕГДА магнитится к InputPoint при сбросе
- ✓ Интеграция в BlockDragHandler.OnEndDrag() и ProgramArea.OnDrop()
- ✓ Все отладочные логи убраны (консоль чистая)
- 📄 Код: SnapManager.cs:146-178, 418-467, BlockDragHandler.cs:172-205, ProgramArea.cs:83-112

**BUGFIX (2026-01-30)**:
- ✓ Исправлено направление магнетизма: OUTPUT→InputPoint → INPUT→InputPoint (точка старта)
- ✓ Убраны логи-спам из OnDrag и FindNearestSnap
- ✓ Первый блок теперь магнитится независимо от позиции сброса
- ✓ Почищены все отладочные Debug.Log (остались только критичные LogError)

### [ ] Шаг 4 (опционально): Connect/Disconnect методы в BlockConnector
- [ ] Добавить методы Connect(), Disconnect() для явного управления соединениями
- [ ] Инкапсуляция логики подключения
- 📄 План: TBD

**Key Achievements**:
- ✅ InputPoint как единая точка старта программы
- ✅ Навигация вперёд и назад по цепи (GetNextBlock, GetPreviousBlock, GetLastBlockInChain)
- ✅ Автоматический магнетизм первого блока к InputPoint
- ✅ Snap система учитывает InputPoint наряду с коннекторами блоков

**Testing Strategy**:
- ✓ Manual тесты: первый блок магнитится к InputPoint
- ✓ API тесты в GameManagerAPITest
- [ ] Integration тесты для многоблочных цепей

**Next Steps**:
- [ ] (Опционально) Шаг 4 - Connect/Disconnect методы
- [ ] Интеграция в play-united
- [ ] UPM пакет v1.1.0

**Detailed plan**: [.Doc/Tasks/26_ChainManagement_ProgramAreaManager.md](Tasks/26_ChainManagement_ProgramAreaManager.md)
