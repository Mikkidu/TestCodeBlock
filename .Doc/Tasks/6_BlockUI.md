# Задача #6: UI - BlockUI и BlockFactory

## Цель
Создать визуальное представление блока с drag-drop функционалом и фабрику для создания блоков.

## Контекст
Зависит от: Задача #4
Это начало UI слоя.

## Ключевые шаги
1. Создать Canvas в SampleScene с Event System

2. Создать prefab BlockUI (Assets/Prefabs/UI/BlockUI.prefab):
   - Panel с LayoutElement
   - Image для цвета
   - TextMeshProUGUI для названия команды
   - RectTransform для drag-drop

3. Создать `UI/BlockUI.cs`:
   - Реализовать IBeginDragHandler, IDragHandler, IEndDragHandler
   - Initialize(ICommand cmd)
   - Методы: OnBeginDrag, OnDrag, OnEndDrag
   - ReturnToOriginalPosition()
   - SetHighlight(bool)

4. Создать `UI/BlockFactory.cs`:
   - BlockUI blockPrefab (SerializeField)
   - CreateBlock(CommandType type, Transform parent) → BlockUI
   - Private CreateCommand(CommandType type) используя switch

5. Тестировать создание одного блока и его перетаскивание

## Критерии завершения
- [ ] Canvas создан с Event System в сцене
- [ ] BlockUI prefab создан с нужными UI элементами
- [ ] BlockUI.cs реализует drag-drop интерфейсы
- [ ] BlockFactory создаёт блоки из типов команд
- [ ] Блок можно перетаскивать по экрану
- [ ] Блок возвращается на место при отпускании
- [ ] Цвет блока соответствует типу команды
- [ ] Имя блока отображается корректно
