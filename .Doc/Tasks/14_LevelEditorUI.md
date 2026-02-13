# Task #14: Level Editor UI - редактор в Editor режиме

**Status**: Ready for Implementation
**Priority**: 🔴 CRITICAL
**Blockers**: #13 (нужны структуры данных)
**Timeline**: 13-15 января (2-3 дня)

---

## Описание

Создать EditorWindow для редактирования уровней CodeBlocks в Unity Editor.

Главное окно с 4 компонентами:
1. **Миникарта** - preview всего уровня
2. **Палитра** - выбор terrain и objects блоков
3. **Сцена** - сетка с размещением блоков (Scene view)
4. **Информация** - имя, ID, сложность, подсказка

---

## Компоненты для реализации

### 1. CodeBlocksLevelEditorWindow.cs (главное EditorWindow)

```csharp
public class CodeBlocksLevelEditorWindow : EditorWindow
{
    private LevelGridData currentLevel;
    private Vector2Int gridSize;
    private Vector2Int selectedTerrainType;  // Ground/Road/Pit
    private Vector2Int selectedObjectType;   // Wall/Button/Door

    private GUILayoutOption[] gridCellOptions;

    [MenuItem("Window/CodeBlocks/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<CodeBlocksLevelEditorWindow>("CodeBlocks Level Editor");
    }

    private void OnGUI()
    {
        DrawHeader();
        DrawPalette();
        DrawLevelInfo();
        DrawMiniMap();
    }

    private void DrawHeader() { ... }      // Кнопки Load/Save/New
    private void DrawPalette() { ... }     // Выбор блоков
    private void DrawLevelInfo() { ... }   // Имя, ID, сложность
    private void DrawMiniMap() { ... }     // Preview уровня
}
```

### 2. GridVisualizer.cs (рисование на Scene)

```csharp
[ExecuteInEditMode]
public class GridVisualizer : MonoBehaviour
{
    public LevelGridData levelData;
    public float cellSize = 1f;
    public Color groundColor = Color.green;
    public Color roadColor = Color.gray;
    public Color pitColor = Color.red;
    public Color obstacleColor = Color.black;

    private void OnDrawGizmosSelected()
    {
        if (levelData == null) return;

        // Рисовать сетку
        // Рисовать блоки terrain
        // Рисовать объекты
        // Рисовать Start/Finish
    }

    public Vector3 GridToWorldPos(Vector2Int gridPos) { ... }
    public Vector2Int WorldToGridPos(Vector3 worldPos) { ... }
}
```

### 3. MiniMap.cs (preview уровня)

```csharp
public class MiniMap
{
    public Texture2D GeneratePreview(LevelGridData level, int textureSize)
    {
        // Создать текстуру
        // Пиксели: земля=зелёный, дорога=серый, пропасть=красный
        // Объекты: чёрные точки
        // Start/Finish: специальные символы
        return texture;
    }
}
```

### 4. BlockPalette.cs (выбор блоков)

```csharp
public class BlockPalette
{
    public void DrawTerrainSelector()
    {
        // Buttons: [Ground] [Road] [Pit]
        // Активная кнопка подсвечена
    }

    public void DrawObjectSelector()
    {
        // Buttons: [Wall] [Button] [Door] [DestructibleWall]
        // Активная кнопка подсвечена
        // При выборе Button/Door показать диалог параметров
    }
}
```

### 5. LevelInfoPanel.cs (информация об уровне)

```csharp
public class LevelInfoPanel
{
    public void DrawLevelInfo(LevelGridData level)
    {
        // TextField: levelName
        // TextField: levelId
        // Slider: difficulty (1-5)
        // TextArea: hintText
        // Button: [Clear Level]
    }
}
```

---

## Функциональность

### На Scene view (через GridVisualizer)

1. **Размещение Terrain**
   - Левый клик (с выбранным Ground/Road/Pit) → размещает блок
   - Правый клик → удаляет блок
   - Сетка показывает где можно кликать

2. **Размещение Objects**
   - Левый клик (с выбранным Wall/Button/Door) → размещает на proходимом terrain
   - Если клик на Pit → сообщение об ошибке
   - Параметры: для Button/Door нужно выбрать Color

3. **Установка Start/Finish**
   - Кнопка "[Set Start]" в pallete → режим выбора
   - Клик на сетку → диалог "Choose direction"
   - Аналогично Finish (без выбора направления)

4. **Visual Feedback**
   - Наводка на клетку → подсвечивается
   - Ground = зелёная ячейка
   - Road = серая ячейка
   - Pit = красная ячейка
   - Objects = чёрные точки/иконки

---

## Шаги реализации

### День 1 (13 янв)

1. Создать CodeBlocksLevelEditorWindow.cs
2. Создать GridVisualizer.cs
3. Базовое отображение сетки (Gizmos)

### День 2 (14 янв)

4. Реализовать размещение Terrain блоков
5. Реализовать размещение Objects блоков
6. Валидация (объекты на проходимом)

### День 3 (15 янв)

7. Установка Start/Finish
8. MiniMap preview
9. LevelInfoPanel
10. Тестирование на сцене

---

## Критерии готовности

- ✅ EditorWindow открывается из меню
- ✅ Сетка рисуется в Scene view
- ✅ Размещение terrain работает
- ✅ Размещение objects работает (с валидацией)
- ✅ Start/Finish можно установить
- ✅ Миникарта обновляется при изменении уровня
- ✅ Информация уровня редактируется
- ✅ Visual feedback работает
