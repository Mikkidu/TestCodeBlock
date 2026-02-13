# Task #26 Step 2: InputPoint Setup Instructions

**Status**: Manual setup required in Unity Editor
**Time**: 5-10 minutes

---

## 📋 MANUAL SETUP (Unity Editor)

### Step 1: Open ProgramArea prefab
1. Navigate to: `Assets/CodeBlocks/Prefabs/UI/ProgramArea.prefab`
2. Double-click to open prefab in Editor

### Step 2: Create InputPoint GameObject
1. Right-click on **ProgramArea** GameObject in Hierarchy
2. Select **Create Empty**
3. Rename it to **"InputPoint"**

### Step 3: Add RectTransform (should be added automatically)
- Verify that InputPoint has a **RectTransform** component
- If not, add it manually

### Step 4: Position InputPoint
**Option A - Top-Left corner** (recommended):
- Anchors: Min(0, 1), Max(0, 1)
- Pivot: (0, 1)
- Anchored Position: (20, -20) — small offset from corner

**Option B - Center**:
- Anchors: Min(0.5, 0.5), Max(0.5, 0.5)
- Pivot: (0.5, 0.5)
- Anchored Position: (0, 100) — near top

### Step 5: Add Image component (optional for visualization)
1. Click **Add Component** → **UI** → **Image**
2. Set color to **Green** or **Orange** (for visibility)
3. Set size to **20x20** pixels (small circle/dot)
4. Optional: Use a circle sprite for better look
5. Set **Raycast Target** = **false** (important!)

### Step 6: Assign InputPoint to ProgramArea
1. Select **ProgramArea** GameObject in prefab
2. Find **Program Area** script component in Inspector
3. Locate the **"Input Point (Task #26)"** section
4. Drag **InputPoint** GameObject into the **Input Point** field

### Step 7: Save prefab
1. Press **Ctrl+S** or **File → Save**
2. Close prefab editing mode

---

## 🧪 TESTING

### Test 1: Run test from Inspector
1. Enter Play mode
2. Find GameObject with **GameManagerAPITest** component
3. Right-click on component → **"Test InputPoint (Task #26 Step 2)"**
4. Check Console logs

### Test 2: Verify positions
Console should show:
```
HasInputPoint() = True
GetInputPointTransform() = InputPoint
GetInputPointWorldPosition() = (x, y, z)
GetInputPointScreenPosition() = (x, y)
```

### Test 3: Visual check
- InputPoint should be visible in Game view (if Image added)
- Should be positioned at chosen location (top-left or center)

---

## ✅ SUCCESS CRITERIA

- [ ] InputPoint GameObject exists in ProgramArea prefab
- [ ] InputPoint assigned to ProgramArea.inputPoint field
- [ ] Test passes: `HasInputPoint() = True`
- [ ] Positions returned correctly (world and screen)
- [ ] No errors in Console

---

## 🚀 NEXT STEP

After successful setup:
- **Step 3**: Magnetism to InputPoint (SnapManager integration)

---

**Version**: 1.0
**Date**: 29 Jan 2026
