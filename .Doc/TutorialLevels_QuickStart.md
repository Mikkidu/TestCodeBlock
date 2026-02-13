# Tutorial Levels - Quick Start

## Generate Levels (One Command!)

```
Tools → CodeBlocks → Generate Tutorial Levels
```

Done! 5 levels are now in `Assets/Resources/RobotLevels/`

---

## The 5 Levels

| # | Name | Difficulty | Size | Goal |
|---|------|-----------|------|------|
| 1 | Move Forward | ⭐ | 8×8 | Reach finish going straight |
| 2 | Turn and Move | ⭐ | 8×8 | Navigate L-shaped path |
| 3 | Avoid Obstacles | ⭐⭐ | 8×8 | Pathfind around walls |
| 4 | Buttons & Doors | ⭐⭐ | 8×8 | Press button to open door |
| 5 | Complex Maze | ⭐⭐⭐ | 10×10 | Solve large maze |

---

## Quick Summary

### Level 1: Move Forward
- **Learn:** Basic movement
- **Teach:** "Forward" block
- **Solution:** 7× Forward block

### Level 2: Turn and Move
- **Learn:** Turning and planning
- **Teach:** Rotation + movement
- **Solution:** 5× Forward + Turn + 3× Forward

### Level 3: Avoid Obstacles
- **Learn:** Pathfinding around walls
- **Teach:** Strategic navigation
- **Solution:** Find alternate route (multiple solutions)

### Level 4: Buttons & Doors
- **Learn:** Interactive mechanics
- **Teach:** Button-door interaction
- **Solution:** Navigate to button, press it, proceed through door

### Level 5: Complex Maze
- **Learn:** Advanced pathfinding
- **Teach:** Complex problem-solving
- **Solution:** Navigate 10×10 maze with corridor system

---

## How to Use in Your Game

### Load from Resources
```csharp
LevelGridData level1 = Resources.Load<LevelGridData>("RobotLevels/tutorial_01_move_forward");
```

### In Level Editor
1. Window → CodeBlocks → Level Editor
2. Load Level → Select tutorial_0X
3. Edit and test

### Export/Import
1. Load level in editor
2. Click "Export to JSON"
3. Share JSON file with anyone
4. Import into another project

---

## What Each Level Teaches

```
Level 1 ──> Basic Movement
              ↓
Level 2 ──> Rotation + Planning
              ↓
Level 3 ──> Pathfinding
              ↓
Level 4 ──> Interactions
              ↓
Level 5 ──> Complex Problem Solving
              ↓
Ready for Advanced Puzzles!
```

---

## File Locations

```
Levels:
Assets/Resources/RobotLevels/
├── tutorial_01_move_forward.asset
├── tutorial_02_turn_and_move.asset
├── tutorial_03_avoid_obstacles.asset
├── tutorial_04_buttons_doors.asset
└── tutorial_05_complex_maze.asset

Generator:
Assets/Scripts/LevelEditor/Editor/TutorialLevelGenerator.cs
```

---

## Documentation

- **Full Guide:** `.Doc/TutorialLevels_Guide.md`
- **This File:** `.Doc/TutorialLevels_QuickStart.md`
- **Level Editor:** `.Doc/LevelEditor_JSON_System.md`

---

## Next Steps

1. Generate levels: **Tools → CodeBlocks → Generate Tutorial Levels**
2. Load in Level Editor to preview
3. Test in Play Mode
4. Share with team/players
5. Extend with more custom levels

---

## That's It!

5 complete tutorial levels, automatically generated, ready to use.

Difficulty progression: ⭐ → ⭐ → ⭐⭐ → ⭐⭐ → ⭐⭐⭐
