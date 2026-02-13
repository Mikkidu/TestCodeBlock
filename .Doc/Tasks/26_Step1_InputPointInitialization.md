# Шаг 1.1: Инициализация входной точки (InputPoint)

**Этап**: Группа 1 - Базовое управление цепью
**Статус**: 📋 **ПЛАНИРОВАНИЕ**
**Метрика готовности**: InputPoint доступна из ProgramAreaManager, позиция возвращается в мировых координатах
**Время**: 2-3 часа

---

## 💡 АРХИТЕКТУРА РЕШЕНИЯ

- **InputPoint** - отдельный GameObject, вручную расположен в префабе ProgramArea
- Прокинут в инспектор ProgramAreaManager как ссылка на Transform
- **ProgramAreaManager** возвращает мировую позицию InputPoint (блоки сами конвертируют в локальные для своего родителя)
- При drop блока без магнетизма к началу цепи → блок выравнивается к InputPoint
- Визуализация: спрайт на самом GameObject InputPoint (для будущего коннектора)
- Магнетизм InputPoint только при отпускании (DROP), не во время движения

---

## 📝 ПОДРОБНЫЙ ПЛАН

### Подшаг 1.1.1: Создать ProgramAreaManager компонент

**Файл**: `Assets/Scripts/Windows/CodeBlocks/ProgramAreaManager.cs`

```csharp
public class ProgramAreaManager : MonoBehaviour
{
    // Ссылка на Transform InputPoint (отдельный GO в префабе)
    [SerializeField] private Transform inputPointTransform;

    // Ссылка на RectTransform самого ProgramArea
    [SerializeField] private RectTransform programAreaRect;

    private void OnValidate()
    {
        // Автоматический поиск при установке в инспекторе
        if (inputPointTransform == null && transform.Find("InputPoint") != null)
            inputPointTransform = transform.Find("InputPoint");

        if (programAreaRect == null)
            programAreaRect = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Получить позицию входной точки программы (в мировых координатах)
    /// </summary>
    public Vector3 GetInputPointWorldPosition()
    {
        if (inputPointTransform == null)
        {
            Debug.LogError("[ProgramAreaManager] InputPoint не назначена в инспекторе!");
            return Vector3.zero;
        }
        return inputPointTransform.position;
    }

    /// <summary>
    /// Получить позицию входной точки в экранных координатах
    /// </summary>
    public Vector2 GetInputPointScreenPosition()
    {
        Vector3 worldPos = GetInputPointWorldPosition();
        return RectTransformUtility.WorldToScreenPoint(null, worldPos);
    }

    /// <summary>
    /// Получить RectTransform самого ProgramArea (для выравнивания блоков)
    /// </summary>
    public RectTransform GetProgramAreaRect()
    {
        return programAreaRect;
    }

    /// <summary>
    /// Получить Transform самого InputPoint (для визуальной отрисовки, коннекторов)
    /// </summary>
    public Transform GetInputPointTransform()
    {
        return inputPointTransform;
    }
}
```

**Чек-лист**:
- [ ] Файл создан в папке `Assets/Scripts/Windows/CodeBlocks/`
- [ ] Компонент добавлен на ProgramArea GameObject
- [ ] В инспекторе назначена ссылка на Transform InputPoint (отдельный GameObject)
- [ ] Метод GetInputPointWorldPosition() возвращает корректную позицию
- [ ] Методы возвращают мировые и экранные координаты

---

### Подшаг 1.1.2: Интегрировать ProgramAreaManager в CodeBlocksWindow

**Файл**: `Assets/Scripts/Windows/CodeBlocks/CodeBlocksWindow.cs`

Добавить в класс:

```csharp
public partial class CodeBlocksWindow : BaseWindow<CodeBlocksGameInitData>
{
    [SerializeField] private ProgramAreaManager programAreaManager;

    public override void OnInit(CodeBlocksGameInitData initData)
    {
        base.OnInit(initData);

        // Найти и инициализировать ProgramAreaManager
        if (programAreaManager == null)
            programAreaManager = GetComponentInChildren<ProgramAreaManager>();

        if (programAreaManager == null)
        {
            Debug.LogError("[CodeBlocksWindow] ProgramAreaManager не найдена!");
            return;
        }

        Debug.Log($"[CodeBlocksWindow] InputPoint инициализирована: {programAreaManager.GetInputPointWorldPosition()}");
    }

    /// <summary>
    /// Получить доступ к ProgramAreaManager для других компонентов (BlockDragHandler и т.д.)
    /// </summary>
    public ProgramAreaManager GetProgramAreaManager()
    {
        return programAreaManager;
    }
}
```

**Чек-лист**:
- [ ] Метод GetComponentInChildren<ProgramAreaManager>() находит компонент на ProgramArea
- [ ] В консоли при OnInit() выводится логи инициализации
- [ ] Метод `GetProgramAreaManager()` доступен для других компонентов
- [ ] При отсутствии компонента выводится ошибка в консоль

---

### Подшаг 1.1.3: Создать InputPoint GameObject в префабе

**Размещение**: `Assets/Resources/Windows/Client/CodeBlocksWindow.prefab`

1. **В иерархии ProgramArea GameObject**:
   - Создать дочерний GameObject: `InputPoint`
   - Добавить компонент **Image** (для визуализации спрайтом)
   - Добавить компонент **RectTransform** (если будет UI-элемент)

2. **Визуализация спрайтом**:
   - Назначить спрайт (иконка точки входа, круг или специальный спрайт)
   - Размер: ~50x50 пикселей (достаточно видно)
   - Цвет: светло-зелёный или оранжевый (контраст с фоном)
   - Установить RaycastTarget = false (чтобы не блокировала клики)

3. **Позиция**:
   - Расположить вручную в центре ProgramArea (или сверху)
   - Позиция фиксируется в префабе
   - Можно отредактировать впоследствии через инспектор ProgramArea

**Чек-лист**:
- [ ] GameObject "InputPoint" создан как дочерний для ProgramArea
- [ ] Назначен спрайт (любой визуальный элемент)
- [ ] Transform правильно расположен в префабе
- [ ] RaycastTarget = false для Image компонента

---

### Подшаг 1.1.4: Интегрировать InputPoint в ProgramAreaManager (инспектор)

**Файл**: `Assets/Scripts/Windows/CodeBlocks/ProgramAreaManager.cs`

В инспекторе CodeBlocksWindow.prefab:
- Найти компонент **ProgramAreaManager** на ProgramArea
- В поле **inputPointTransform** перетащить GameObject **InputPoint**

Или через код (автоматический поиск в OnValidate):
```csharp
// Если InputPoint не назначена, поискать дочерний GO
if (inputPointTransform == null)
    inputPointTransform = transform.Find("InputPoint");
```

**Чек-лист**:
- [ ] InputPoint назначена в инспекторе ProgramAreaManager
- [ ] Debug.Log в Start() выводит позицию InputPoint без ошибок
- [ ] При запуске игры видна спрайт InputPoint на экране

---

## 🧪 ТЕСТИРОВАНИЕ

### Тест 1: InputPoint видна в иерархии и на экране
1. Открыть `Assets/Resources/Windows/Client/CodeBlocksWindow.prefab`
2. В Scene View должен быть виден спрайт InputPoint в составе ProgramArea
3. Запустить игру, в Game View видна спрайт InputPoint

**Результат**: ✅ / ❌

### Тест 2: Позиция InputPoint возвращается корректно
1. Добавить временный Debug.Log в CodeBlocksWindow.OnInit():

```csharp
var inputWorldPos = programAreaManager.GetInputPointWorldPosition();
var inputScreenPos = programAreaManager.GetInputPointScreenPosition();
Debug.Log($"[InputPoint] World: {inputWorldPos}, Screen: {inputScreenPos}");
```

2. Запустить игру, проверить логи в консоли
3. Позиция должна соответствовать видимой спрайте на экране

**Результат**: ✅ / ❌

### Тест 3: Доступ из других компонентов
1. Убедиться что ProgramAreaManager доступна через CodeBlocksWindow:

```csharp
var window = Root.Root.UIManager.GetCurrentWindow() as CodeBlocksWindow;
var inputPos = window.GetProgramAreaManager().GetInputPointWorldPosition();
Debug.Log($"InputPoint доступна: {inputPos}");
```

2. Позиция логируется без ошибок

**Результат**: ✅ / ❌

### Тест 4: Координаты конвертируются правильно
1. Взять блок (любой BlockUI компонент) и попробовать конвертировать мировую позицию InputPoint в локальную для блока:

```csharp
Vector3 inputWorldPos = manager.GetInputPointWorldPosition();
Vector3 blockLocalPos = block.transform.parent.InverseTransformPoint(inputWorldPos);
Debug.Log($"InputPoint в локальных координатах блока: {blockLocalPos}");
```

2. Проверить что результат логируется без ошибок

**Результат**: ✅ / ❌

---

## 📦 АРТЕФАКТЫ ЭТАПА

**Созданные файлы**:
- `Assets/Scripts/Windows/CodeBlocks/ProgramAreaManager.cs` ← основной компонент (методы для доступа к InputPoint)

**Модифицированные файлы**:
- `Assets/Scripts/Windows/CodeBlocks/CodeBlocksWindow.cs` ← инициализация ProgramAreaManager в OnInit()
- `Assets/Resources/Windows/Client/CodeBlocksWindow.prefab` ← добавлен дочерний GameObject InputPoint со спрайтом

**Конфигурация префаба**:
- ProgramArea → добавлен компонент ProgramAreaManager
- ProgramArea → InputPoint (дочерний GO) с Image компонентом и спрайтом
- ProgramAreaManager → поле inputPointTransform указывает на InputPoint GameObject

---

## 🚀 СЛЕДУЮЩИЙ ШАГ (1.2)

После завершения этого шага:
- ProgramAreaManager полностью готов для использования в шаге 1.2 (подключение блока к цепи)
- BlockDragHandler будет получать позицию InputPoint через `GetInputPointWorldPosition()`
- Блоки смогут конвертировать мировую позицию в локальную для своего родителя
- Визуальная спрайта InputPoint обозначает точку, где будут "цепляться" блоки при drop без магнетизма

---

## 📝 ЗАМЕТКИ

- InputPoint это реальный GameObject с Transform, расположенный вручную в префабе
- Позиция задается в инспекторе (может быть в центре, сверху или в любом месте ProgramArea)
- Методы возвращают мировые координаты - это позволяет блокам самостоятельно конвертировать в локальные
- Спрайт на InputPoint будет видна на экране как визуальный ориентир для пользователя
- При drop блока без магнетизма (вне зон притяжения других выходов) блок выравнивается к InputPoint
- Позже на InputPoint можно добавить коннектор (визуальный элемент для подключения)

---

**Версия**: 1.0
**Дата**: 29 янв 2026
