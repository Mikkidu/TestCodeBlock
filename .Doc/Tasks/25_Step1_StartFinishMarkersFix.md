# Task #25 - Step 1: Fix Start/Finish Markers Duplication

**Status:** TODO
**Priority:** 🔴 CRITICAL
**Estimated Time:** 1.5 hours
**Created:** 2026-01-28

## Problem Analysis

### Root Cause
В `LevelRuntimeManager.cs` маркеры старт/финиш создаются двумя способами:
1. **Fallback** (без префаба): `GameObject.CreatePrimitive()` → **НЕ устанавливает parent**
2. **Prefab** (с префабом): `Instantiate(prefab, levelContainer.transform)` → **устанавливает parent**

При вызове `ClearLevel()`:
- Удаляется `levelContainer` → удаляются только дочерние объекты
- Маркеры созданные через CreatePrimitive **не дочерние** → не удаляются
- При загрузке нового уровня создаются новые маркеры → **дублирование**

### Affected Code Locations
**File:** `Packages/com.codeblocks.robotprogramming/Runtime/Managers/LevelRuntimeManager.cs`

**Problem Lines:**
- Line 141: `startVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);` - NO SetParent
- Line 176: `finishVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);` - NO SetParent

**Cleanup Method:**
- Lines 193-212: `ClearLevel()` - только зануление ссылок, нет явного Destroy

## Solution Strategy

### Approach 1: Add SetParent (Quick Fix - 30 min)
Добавить SetParent после CreatePrimitive:
```csharp
// Line 141-142
startVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
startVisual.transform.SetParent(levelContainer.transform); // ADD THIS
```

**Pros:** Минимальное изменение
**Cons:** Зависит от корректности parent-child иерархии

### Approach 2: Explicit Destroy in ClearLevel (Robust - 30 min)
Добавить явное удаление в `ClearLevel()`:
```csharp
// Before line 195 (before levelContainer cleanup)
if (startVisual != null)
{
    Destroy(startVisual);
    startVisual = null;
}

if (finishVisual != null)
{
    Destroy(finishVisual);
    finishVisual = null;
}
```

**Pros:** Надёжная защита, работает даже если parent не установлен
**Cons:** Чуть больше кода

### ✅ Recommended: Hybrid Approach (Both fixes - 1 hour)
Применить **оба** исправления для максимальной надёжности:
1. SetParent при создании (lines 142, 177)
2. Explicit Destroy в ClearLevel (before line 195)

## Implementation Steps

### Step 1.1: Add SetParent for StartVisual (15 min)
- **File:** `LevelRuntimeManager.cs`
- **Location:** Line 142 (after `startVisual = GameObject.CreatePrimitive(...)`)
- **Change:**
  ```csharp
  // Line 141-143
  startVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
  startVisual.transform.SetParent(levelContainer.transform); // ADD THIS LINE
  startVisual.transform.localScale = new Vector3(0.3f, 0.1f, 0.3f);
  ```

### Step 1.2: Add SetParent for FinishVisual (15 min)
- **File:** `LevelRuntimeManager.cs`
- **Location:** Line 177 (after `finishVisual = GameObject.CreatePrimitive(...)`)
- **Change:**
  ```csharp
  // Line 176-178
  finishVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
  finishVisual.transform.SetParent(levelContainer.transform); // ADD THIS LINE
  finishVisual.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
  ```

### Step 1.3: Add Explicit Destroy in ClearLevel (30 min)
- **File:** `LevelRuntimeManager.cs`
- **Location:** Before line 195 (before `if (levelContainer != null)`)
- **Change:**
  ```csharp
  public void ClearLevel()
  {
      // Explicitly destroy marker visuals FIRST (before container cleanup)
      if (startVisual != null)
      {
          Destroy(startVisual);
          startVisual = null;
      }

      if (finishVisual != null)
      {
          Destroy(finishVisual);
          finishVisual = null;
      }

      // Then destroy container (existing code)
      if (levelContainer != null)
      {
          Destroy(levelContainer);
          levelContainer = null;
      }

      // ... rest of existing cleanup code
  }
  ```

### Step 1.4: Compile and Test (30 min)
1. **Compile Check:**
   - Run `dotnet build` or check Unity console
   - Ensure 0 errors, 0 warnings

2. **Manual Testing:**
   - Open Unity Editor
   - Create test scene with GameManager + LevelRuntimeManager
   - Load Level 1 → check Hierarchy (1 StartVisual, 1 FinishVisual)
   - Call `InitLevel(Level 2)` → check Hierarchy (старые удалены, только новые)
   - Repeat with Levels 3-5 → каждый раз проверять count в Hierarchy

3. **Automated Test (optional):**
   - Create `LevelRuntimeManagerMarkerTest.cs`
   - Test: Load 5 levels sequentially → Assert только 1 StartVisual, 1 FinishVisual

## Testing Checklist

### Test Case 1: Initial Load
- [ ] Load Level 1 → Hierarchy shows **exactly 1** StartVisual, **exactly 1** FinishVisual
- [ ] StartVisual is child of LevelRuntime
- [ ] FinishVisual is child of LevelRuntime

### Test Case 2: Reload Same Level
- [ ] Load Level 1 → load Level 1 again
- [ ] Hierarchy shows **exactly 1** StartVisual, **exactly 1** FinishVisual (no duplication)

### Test Case 3: Sequential Load (5 levels)
- [ ] Load Level 1 → 2 → 3 → 4 → 5
- [ ] After each load: Hierarchy shows **exactly 1** StartVisual, **exactly 1** FinishVisual
- [ ] No accumulation of markers in scene

### Test Case 4: Fallback Mode (no prefabs)
- [ ] Delete `Resources/LevelEditor/Markers/` folder (force fallback)
- [ ] Load Level 1 → CreatePrimitive creates markers with SetParent
- [ ] Load Level 2 → old markers removed correctly

### Test Case 5: Prefab Mode (with prefabs)
- [ ] Restore `Resources/LevelEditor/Markers/` folder
- [ ] Load Level 1 → Instantiate creates markers (already has parent)
- [ ] Load Level 2 → old markers removed correctly (explicit Destroy works)

## Acceptance Criteria

- [ ] SetParent added after CreatePrimitive (lines 142, 177)
- [ ] Explicit Destroy added in ClearLevel (before line 195)
- [ ] Code compiles without errors/warnings
- [ ] All 5 test cases pass
- [ ] No visual artifacts (markers not floating, correctly positioned)
- [ ] Memory: no leaked GameObjects in Hierarchy after multiple InitLevel calls

## Notes

### Why Hybrid Approach?
- **SetParent** - профилактика, гарантирует правильную иерархию
- **Explicit Destroy** - надёжность, гарантирует очистку даже если parent неправильный
- Комбинация = максимальная защита от дублирования

### Performance Impact
- Negligible (2 extra SetParent calls + 2 extra Destroy calls)
- Only happens on level load/unload (не в runtime loop)

### Alternative Considered (Rejected)
- **Only SetParent**: Недостаточно надёжно если кто-то меняет parent позже
- **Only Explicit Destroy**: Не использует Unity parent-child систему (best practice)

## Next Steps
После завершения Step 1:
1. Commit changes: "Fix: start/finish markers duplication in LevelRuntimeManager"
2. Proceed to Step 2: Background positioning fix (#25 BUG-2)
