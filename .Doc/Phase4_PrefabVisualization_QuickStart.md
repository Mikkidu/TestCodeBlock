# Phase 4 - Prefabs & Real Visualization - Quick Start

## Overview

Task #14 Phase 4 добавляет поддержку реальной 3D визуализации уровней с GameObjects вместо только Gizmos. Теперь редактор поддерживает два режима:

1. **Gizmos-only mode** (по умолчанию)
   - Быстро и легко
   - Работает без создания GameObject'ов
   - Хорошо для быстрого прототипирования

2. **Prefabs mode** (новый)
   - Красивая 3D визуализация с реальными объектами
   - Видно как выглядит готовый уровень
   - Проще понять дизайн уровня

## Quick Start

### 1. Генерация префабов

**Первый раз перед использованием:**

```
Tools → CodeBlocks → Generate Level Editor Prefabs
```

Это создаст директорию `Assets/Resources/CodeBlocks/` со всеми необходимыми префабами:

```
Assets/Resources/CodeBlocks/
├── Terrain/
│   ├── Ground.prefab
│   ├── Road.prefab
│   └── Pit.prefab
└── Objects/
    ├── Wall.prefab
    ├── Button.prefab
    └── Door.prefab
```

### 2. Использование в редакторе

1. Откройте Level Editor:
   ```
   Window → CodeBlocks → Level Editor
   ```

2. Выберите или создайте уровень

3. Найдите GridVisualizer в Scene Hierarchy

4. В Inspector найдите **usePrefabs** toggle

   - ✓ **Checked** = Prefabs mode (реальные объекты)
   - ✗ **Unchecked** = Gizmos-only mode (быстрый режим)

5. Включите "Enable Scene Editing"

6. **Размещайте блоки как обычно:**
   - Левый клик = разместить selected terrain type
   - Правый клик = удалить блок

### 3. Режимы визуализации

#### Gizmos Mode (по умолчанию)
- Быстро переключается
- Никаких объектов в сцене
- Использует цветные кубы (Gizmos)
- Хорошо для итерирования

#### Prefabs Mode
- Переключает на реальные GameObjects
- Создает визуальный контейнер "LevelVisuals" в сцене
- Можно видеть материалы и освещение
- Медленнее, но красивее

## Архитектура

### Основные классы

**LevelVisualizationManager.cs**
- Управляет GameObjects в сцене
- Методы: `PlaceTerrainVisual()`, `RemoveTerrainVisual()`, `PlaceObjectVisual()`, `RemoveObjectVisual()`
- Работает только если `usePrefabs = true`

**TerrainBlockVisual.cs**
- Компонент на terrain префабе
- Метод: `SetTerrain(Vector2Int pos, string terrainType)`
- Обновляет цвет в зависимости от типа

**ObjectBlockVisual.cs**
- Компонент на object префабе
- Метод: `SetObject(Vector2Int pos, string objectTypeId)`
- Устанавливает цвет по типу объекта

**GridVisualizer.cs** (обновлен)
- Добавлены вызовы `visualizationManager.PlaceTerrainVisual()` в методе `PlaceTerrain()`
- Добавлены вызовы `visualizationManager.RemoveTerrainVisual()` в методе `RemoveTerrain()`
- Аналогично для объектов

## Цвета в Prefabs Mode

### Terrain
- **Ground** (зеленый): Проходимая земля
- **Road** (серый): Дорога для движения
- **Pit** (красный): Яма (непроходима)

### Objects
- **Wall** (черный): Стена-препятствие
- **Button** (оранжевый): Кнопка
- **Door** (голубой): Дверь

## Примеры использования

### Быстрое прототипирование

```
1. Откройте редактор с usePrefabs = OFF (Gizmos mode)
2. Быстро размещайте блоки
3. Когда довольны - включите usePrefabs = ON
4. Увидите реальную визуализацию
```

### Детальная разработка уровня

```
1. Включите usePrefabs = ON с самого начала
2. Размещайте блоки и сразу видите результат
3. Проверяйте освещение и внешний вид
4. При необходимости измените материалы в префабах
```

## Расширение системы

### Добавление новых типов terrain

1. Отредактируйте `PrefabGenerator.cs`:
   ```csharp
   // Добавьте в методе GeneratePrefabs():
   GenerateTerrainPrefab("NewType", new Color(...), terrainPath);
   ```

2. Запустите `Tools → CodeBlocks → Generate Level Editor Prefabs`

3. Отредактируйте префаб (цвет, геометрию) вручную

### Добавление новых типов объектов

1. Отредактируйте `PrefabGenerator.cs`:
   ```csharp
   // Добавьте в методе GeneratePrefabs():
   GenerateObjectPrefab("NewObject", Color.someColor, objectsPath);
   ```

2. Запустите генератор

3. Отредактируйте префаб вручную

4. Добавьте цвет в `ObjectBlockVisual.GetObjectColor()`

## Troubleshooting

### Префабы не появляются?
- Проверьте что `usePrefabs = true` в Inspector
- Проверьте что префабы существуют в `Assets/Resources/CodeBlocks/`
- Посмотрите Console на ошибки загрузки Resources

### GameObjects не позиционируются правильно?
- Проверьте что GridVisualizer имеет правильный cellSize (должен совпадать с сеткой)
- TerrainBlockVisual должен быть на префабе
- ObjectBlockVisual должен быть на объектном префабе

### Производительность падает с Prefabs mode?
- Это нормально - реальные объекты дороже чем Gizmos
- Переключитесь на Gizmos mode если нужна скорость
- Для большых уровней (100+ блоков) используйте Gizmos

## Дальнейшие улучшения (Phase 5+)

- Object pooling для быстрого создания/удаления
- Поддержка префабов в Resources (можно редактировать внешнейI)
- Сохранение состояния prefab mode между сессиями
- Экспорт уровня с визуализацией в готовую сцену
