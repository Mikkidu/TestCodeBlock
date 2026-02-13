# How to Use Tutorial Levels - 30 Second Setup

## One-Click Generation

**In Unity Editor:**
```
Tools → CodeBlocks → Generate Tutorial Levels
```

**Done!** 5 levels are created automatically.

---

## The Levels

| Level | Name | Goal | Difficulty |
|-------|------|------|-----------|
| 1 | Move Forward | Go straight to finish | ⭐ |
| 2 | Turn and Move | Navigate L-shape | ⭐ |
| 3 | Avoid Obstacles | Pathfind around walls | ⭐⭐ |
| 4 | Buttons & Doors | Press button, go through door | ⭐⭐ |
| 5 | Complex Maze | Solve 10×10 maze | ⭐⭐⭐ |

---

## To Use in Level Editor

1. **Window → CodeBlocks → Level Editor**
2. **Load Level** → Find `tutorial_0X_*.asset`
3. **Enable Scene Editing**
4. Design solution using blocks
5. Test in Play Mode

---

## To Use in Code

```csharp
LevelGridData tutorial1 = Resources.Load<LevelGridData>(
    "RobotLevels/tutorial_01_move_forward"
);
```

---

## Files Location

```
Assets/Resources/RobotLevels/
├── tutorial_01_move_forward.asset
├── tutorial_02_turn_and_move.asset
├── tutorial_03_avoid_obstacles.asset
├── tutorial_04_buttons_doors.asset
└── tutorial_05_complex_maze.asset
```

---

## More Info

- **Full Guide:** `.Doc/TutorialLevels_Guide.md`
- **Quick Ref:** `.Doc/TutorialLevels_QuickStart.md`
- **Summary:** `.Doc/Stage10b_Tutorial_Levels_Summary.md`

---

## Done!

That's it. 5 complete tutorial levels ready to use.
