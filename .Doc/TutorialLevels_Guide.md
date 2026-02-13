# Tutorial Levels - Complete Guide

## Overview

5 progressive tutorial levels designed to teach players how to use the robot programming system. Difficulty increases gradually from simple movement to complex maze navigation.

---

## How to Generate Tutorial Levels

### Option 1: Menu Command (Recommended)
1. In Unity Editor, go to: **Tools → CodeBlocks → Generate Tutorial Levels**
2. Levels are created automatically in `Assets/Resources/RobotLevels/`
3. Success dialog confirms generation

### Option 2: Manual Generation
```csharp
// In any editor script or console:
TutorialLevelGenerator.GenerateTutorialLevels();
```

### After Generation
Levels appear in:
```
Assets/
└── Resources/
    └── RobotLevels/
        ├── tutorial_01_move_forward.asset
        ├── tutorial_02_turn_and_move.asset
        ├── tutorial_03_avoid_obstacles.asset
        ├── tutorial_04_buttons_doors.asset
        └── tutorial_05_complex_maze.asset
```

---

## Level Progression

### Level 1: Move Forward ⭐
**Difficulty:** 1/5
**Objective:** Reach the finish point by moving forward
**Size:** 8×8 grid
**Duration:** ~1 minute

#### Layout
```
S → → → → → → F
(S = Start, F = Finish, → = Ground path)
```

#### Learning Goals
- Understand grid-based movement
- Learn "Forward" block
- Understand cardinal directions (East)
- Simple goal: reach the end

#### Hint
*"Move the robot FORWARD to reach the finish point. Use: Forward block"*

#### Solution
```
1. Place [Forward]
2. Place [Forward]
3. Place [Forward]
4. Place [Forward]
5. Place [Forward]
6. Place [Forward]
7. Place [Forward]
(7 forward movements = 7 cells to destination)
```

#### Mechanics Introduced
- ✓ Cardinal directions (N, S, E, W)
- ✓ Forward movement
- ✓ Cell-based grid

---

### Level 2: Turn and Move ⭐
**Difficulty:** 1/5
**Objective:** Move in L-shape pattern to reach finish
**Size:** 8×8 grid
**Duration:** ~2 minutes

#### Layout
```
S → → → → → ↑
            ↑
            F
(L-shaped path)
```

#### Learning Goals
- Learn turning mechanics (90° rotations)
- Combine movement with rotation
- Plan multi-step sequences
- Understand North/East directions

#### Hint
*"Move the robot in an L-shape. Use: Forward + Turn blocks (Go right 5 steps, then up 3 steps)"*

#### Solution
```
1. Place [Forward] × 5  (move East)
2. Place [Turn Left]    (now facing North)
3. Place [Forward] × 3  (move North)
(Total: 8 blocks, 2 operations)
```

#### Mechanics Introduced
- ✓ Turn Left / Turn Right blocks
- ✓ Combining movement and rotation
- ✓ Sequential planning

---

### Level 3: Avoid Obstacles ⭐⭐
**Difficulty:** 2/5
**Objective:** Navigate around walls to reach finish
**Size:** 8×8 grid
**Duration:** ~3-5 minutes

#### Layout
```
S   . . . F
  [walls and gaps creating a path]
```

#### Learning Goals
- Identify and plan around obstacles
- Create alternative routes
- Understand collision mechanics
- Problem-solving and planning

#### Hint
*"Navigate around walls to reach the finish. The direct path is blocked! You must find an alternate route."*

#### Challenge
- Direct path is blocked
- Must navigate through maze corridors
- Multiple possible solutions
- Requires visualization and planning

#### Mechanics Introduced
- ✓ Obstacle avoidance
- ✓ Complex pathfinding
- ✓ Multiple valid solutions

#### Possible Solutions
Multiple paths exist (user must find one):
- Path 1: Go left, then up, then right
- Path 2: Go right, then up, then navigate gaps
- Path 3: Zigzag through corridor system

---

### Level 4: Buttons & Doors ⭐⭐
**Difficulty:** 2/5
**Objective:** Press button to open door, then reach finish
**Size:** 8×8 grid
**Duration:** ~4-6 minutes

#### Layout
```
S → → ↑ → → F
    ↓ [B]
    [Door blocks exit]
```

#### Learning Goals
- Understand action mechanics (press button)
- Conditional logic (button opens door)
- Multi-objective planning
- Block interactions

#### Hint
*"Press the BUTTON to open the DOOR. Sequence: 1. Move to button, 2. Press button (action), 3. Move through door, 4. Reach finish"*

#### Challenge
- Cannot reach finish while door is closed
- Must first navigate to button location
- Action required to proceed
- Sequential dependencies

#### Mechanics Introduced
- ✓ Button interaction
- ✓ Door mechanics
- ✓ Conditional progression
- ✓ Action blocks

#### Solution Steps
```
1. Move forward to button location (x=2, y=4)
2. Place [Press Button] action
3. Move back to main path (y=2)
4. Move forward to finish
5. Door is now open!
```

---

### Level 5: Complex Maze ⭐⭐⭐
**Difficulty:** 3/5
**Objective:** Navigate complex maze to reach finish
**Size:** 10×10 grid
**Duration:** ~10-15 minutes

#### Layout
```
S . . . . . . . . F
  [Complex maze with walls and gaps]
```

#### Learning Goals
- Advanced pathfinding
- Complex planning with multiple constraints
- Optimize solution length
- Strategic thinking

#### Hint
*"Navigate the maze to reach the finish. This level combines: Obstacle avoidance, Multiple turns, Strategic planning"*

#### Challenge
- Large grid with many walls
- Multiple walls creating corridors
- Requires careful planning
- No direct path
- Optimal solutions are significantly different from brute-force paths

#### Mechanics Integrated
- ✓ All previous mechanics
- ✓ Long-sequence planning
- ✓ Complex obstacle patterns
- ✓ Navigation optimization

#### Approach
- Identify corridor structure
- Plan around wall patterns
- Create efficient path
- Test and refine

---

## Level Design Specifications

### Common Elements

| Element | Use |
|---------|-----|
| **Start Point** | Robot's initial position and direction |
| **Finish Point** | Goal destination |
| **Ground** | Walkable terrain |
| **Wall** | Obstacle (cannot pass through) |
| **Button** | Trigger for actions |
| **Door** | Blocks path until button pressed |

### Terrain Types
```
"Ground"  → Walkable terrain (green)
"Road"    → Alternative walkable terrain (gray)
"Pit"     → Dangerous terrain (red, avoid)
```

### Object Types
```
"Wall"    → Solid obstacle
"Button"  → Trigger mechanism
"Door"    → Blockage (opens with button)
"Start"   → Robot spawn point (special)
"Finish"  → Goal location (special)
```

---

## Implementation Details

### Generator Code
Location: `Assets/Scripts/LevelEditor/Editor/TutorialLevelGenerator.cs`

**Key Features:**
- Programmatic level creation
- Consistent level generation
- EditorUtility.MenuItem for easy access
- Automatic directory creation
- Batch asset creation

### Level Data Structure
Each level is a `LevelGridData` ScriptableObject containing:
```csharp
public string levelId;           // Unique identifier
public string levelName;         // Display name
public int difficulty;           // 1-5 scale
public string hintText;          // User guidance
public int gridWidth;            // Grid dimensions
public int gridHeight;
public TerrainCell[] terrain;    // Walkable cells
public GridObject[] objects;     // Obstacles/interactive
public StartPoint start;         // Robot spawn
public FinishPoint finish;       // Goal
```

### Directory Structure
```
Assets/Resources/RobotLevels/
├── tutorial_01_move_forward.asset
├── tutorial_02_turn_and_move.asset
├── tutorial_03_avoid_obstacles.asset
├── tutorial_04_buttons_doors.asset
└── tutorial_05_complex_maze.asset

Assets/Scripts/LevelEditor/Editor/
└── TutorialLevelGenerator.cs
```

---

## Testing the Levels

### In Level Editor
1. **Window → CodeBlocks → Level Editor**
2. **Load Level** → Select tutorial level
3. **Enable Scene Editing**
4. Design solution (place blocks)
5. Test in Play Mode

### Progression Check
- [ ] Level 1 - Completed with 7 Forward blocks
- [ ] Level 2 - Completed with mixed Forward/Turn
- [ ] Level 3 - Found alternate path around obstacles
- [ ] Level 4 - Understand button-door interaction
- [ ] Level 5 - Successfully navigated complex maze

---

## Learning Outcomes by Level

### After Level 1
- [ ] Understand grid movement
- [ ] Know what cardinal directions are
- [ ] Comfortable placing basic blocks
- [ ] See visual feedback

### After Level 2
- [ ] Can combine movement and rotation
- [ ] Understand 90° turns
- [ ] Plan multi-part sequences
- [ ] Visualize paths

### After Level 3
- [ ] Can identify obstacles
- [ ] Plan alternate routes
- [ ] Think strategically about movement
- [ ] Comfortable with complex layouts

### After Level 4
- [ ] Understand interactive objects
- [ ] Know button mechanics
- [ ] Understand doors and triggers
- [ ] Plan conditional sequences

### After Level 5
- [ ] Confident pathfinding
- [ ] Optimize solutions
- [ ] Handle complex scenarios
- [ ] Ready for custom challenges

---

## Extending Tutorial Levels

### Adding More Levels
1. Add new method: `CreateLevel6_YourTopic()`
2. Design terrain, objects, start/finish
3. Add call in `GenerateTutorialLevels()`
4. Recompile and run menu command

### Suggested Advanced Topics
- **Level 6**: Multiple buttons controlling different doors
- **Level 7**: Pit avoidance (deadly terrain)
- **Level 8**: Timed puzzles (steps limited)
- **Level 9**: Robot-to-robot interaction
- **Level 10**: Custom challenge design

---

## File Modifications

### New Files
- `Assets/Scripts/LevelEditor/Editor/TutorialLevelGenerator.cs`
- `Assets/Resources/RobotLevels/tutorial_*.asset` (5 files)

### Modified Files
- `Assembly-CSharp-Editor.csproj` - Added generator to compile list

---

## Compilation Status

✅ **Assembly-CSharp**: Build succeeded (0 errors, 4 warnings)
✅ **Assembly-CSharp-Editor**: Build succeeded (0 errors, 1 warning)

---

## Usage Instructions

### For Players
1. Open Level Editor
2. Load a tutorial level (Start → Level 1)
3. Read the hint
4. Design solution using available blocks
5. Test in Play Mode
6. Progress to next level when complete

### For Instructors
1. Generate tutorial levels: **Tools → CodeBlocks → Generate Tutorial Levels**
2. Introduce one level per session
3. Discuss solution strategies
4. Encourage different approaches
5. Progress based on student understanding

### For Developers
1. Levels are in `Assets/Resources/RobotLevels/`
2. Load at runtime: `Resources.Load<LevelGridData>("RobotLevels/tutorial_01_move_forward")`
3. Modify via Level Editor or code
4. Export/import via JSON for sharing
5. Create custom variants as needed

---

## Summary

5 progressive tutorial levels teach:
1. **Basic movement** (Forward)
2. **Rotation and planning** (Turn + Forward)
3. **Obstacle navigation** (Pathfinding)
4. **Interactive mechanics** (Buttons, Doors)
5. **Complex puzzles** (Advanced pathfinding)

Players graduate from simple grid navigation to solving complex mazes, building programming skills progressively.
