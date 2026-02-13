using UnityEngine;
using UnityEditor;
using System.IO;

public class TutorialLevelGenerator
{
    private const string LEVELS_PATH = "Assets/Resources/RobotLevels";

    [MenuItem("Tools/CodeBlocks/Generate Tutorial Levels")]
    public static void GenerateTutorialLevels()
    {
        // Ensure directory exists
        if (!Directory.Exists(LEVELS_PATH))
        {
            Directory.CreateDirectory(LEVELS_PATH);
        }

        Debug.Log("✓ Generating tutorial levels...");

        CreateLevel1_MoveForward();
        CreateLevel2_TurnAndMove();
        CreateLevel3_AvoidObstacles();
        CreateLevel4_ButtonsAndDoors();
        CreateLevel5_ComplexMaze();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✓ Tutorial levels created successfully!");
        EditorUtility.DisplayDialog("Success", "5 tutorial levels created in:\n" + LEVELS_PATH, "OK");
    }

    private static void CreateLevel1_MoveForward()
    {
        LevelGridData level = ScriptableObject.CreateInstance<LevelGridData>();
        level.levelId = "tutorial_01_move_forward";
        level.levelName = "Tutorial 1: Move Forward";
        level.difficulty = 1;
        level.hintText = "Move the robot FORWARD to reach the finish point.\n\nUse: Forward block";
        level.gridWidth = 8;
        level.gridHeight = 8;

        // Create ground path from start to finish
        level.terrain = new TerrainCell[8];
        for (int i = 0; i < 8; i++)
        {
            level.terrain[i] = new TerrainCell
            {
                position = new Vector2Int(i, 0),
                terrainType = "Ground"
            };
        }

        // NEW: Add start point as GridObject
        var startObj1 = new GridObject
        {
            position = new Vector2Int(0, 0),
            objectTypeId = "StartPoint",
            objectInstanceId = "start_tutorial_01"
        };
        startObj1.AddParameter("direction", CardinalDirection.East.ToString());
        ArrayUtility.Add(ref level.objects, startObj1);

        // NEW: Add finish point as GridObject
        var finishObj1 = new GridObject
        {
            position = new Vector2Int(7, 0),
            objectTypeId = "FinishPoint",
            objectInstanceId = "finish_tutorial_01"
        };
        ArrayUtility.Add(ref level.objects, finishObj1);

        SaveLevel(level, "tutorial_01_move_forward.asset");
    }

    private static void CreateLevel2_TurnAndMove()
    {
        LevelGridData level = ScriptableObject.CreateInstance<LevelGridData>();
        level.levelId = "tutorial_02_turn_and_move";
        level.levelName = "Tutorial 2: Turn and Move";
        level.difficulty = 1;
        level.hintText = "Move the robot in an L-shape.\n\nUse: Forward + Turn blocks\n(Go right 5 steps, then up 3 steps)";
        level.gridWidth = 8;
        level.gridHeight = 8;

        // Create L-shaped path
        var terrainList = new System.Collections.Generic.List<TerrainCell>();

        // Horizontal part (East)
        for (int x = 0; x < 6; x++)
        {
            terrainList.Add(new TerrainCell
            {
                position = new Vector2Int(x, 0),
                terrainType = "Ground"
            });
        }

        // Vertical part (North from end of horizontal)
        for (int y = 1; y < 4; y++)
        {
            terrainList.Add(new TerrainCell
            {
                position = new Vector2Int(5, y),
                terrainType = "Ground"
            });
        }

        level.terrain = terrainList.ToArray();

        // NEW: Add start point as GridObject
        var startObj2 = new GridObject
        {
            position = new Vector2Int(0, 0),
            objectTypeId = "StartPoint",
            objectInstanceId = "start_tutorial_02"
        };
        startObj2.AddParameter("direction", CardinalDirection.East.ToString());
        ArrayUtility.Add(ref level.objects, startObj2);

        // NEW: Add finish point as GridObject
        var finishObj2 = new GridObject
        {
            position = new Vector2Int(5, 3),
            objectTypeId = "FinishPoint",
            objectInstanceId = "finish_tutorial_02"
        };
        ArrayUtility.Add(ref level.objects, finishObj2);

        SaveLevel(level, "tutorial_02_turn_and_move.asset");
    }

    private static void CreateLevel3_AvoidObstacles()
    {
        LevelGridData level = ScriptableObject.CreateInstance<LevelGridData>();
        level.levelId = "tutorial_03_avoid_obstacles";
        level.levelName = "Tutorial 3: Avoid Obstacles";
        level.difficulty = 2;
        level.hintText = "Navigate around walls to reach the finish.\n\nThe direct path is blocked!\nYou must find an alternate route.";
        level.gridWidth = 8;
        level.gridHeight = 8;

        // Create maze-like path with obstacles
        var terrainList = new System.Collections.Generic.List<TerrainCell>();

        // Create ground everywhere except blocked areas
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                // Create a path that avoids center obstacles
                if ((x < 3 && y < 5) || (x >= 5 && y >= 3) || (x == 4 && y < 3) || (x < 5 && y >= 6))
                {
                    terrainList.Add(new TerrainCell
                    {
                        position = new Vector2Int(x, y),
                        terrainType = "Ground"
                    });
                }
            }
        }

        level.terrain = terrainList.ToArray();

        // Create wall obstacles in the middle
        var objectsList = new System.Collections.Generic.List<GridObject>();

        // Vertical wall in the middle
        for (int y = 2; y < 7; y++)
        {
            if (y != 4) // Leave gap at y=4
            {
                objectsList.Add(new GridObject
                {
                    position = new Vector2Int(3, y),
                    objectTypeId = "Wall",
                    objectInstanceId = $"wall_{3}_{y}"
                });
            }
        }

        // Horizontal wall
        for (int x = 4; x < 7; x++)
        {
            if (x != 5) // Leave gap at x=5
            {
                objectsList.Add(new GridObject
                {
                    position = new Vector2Int(x, 3),
                    objectTypeId = "Wall",
                    objectInstanceId = $"wall_{x}_3"
                });
            }
        }

        // NEW: Add start point as GridObject
        var startObj3 = new GridObject
        {
            position = new Vector2Int(0, 0),
            objectTypeId = "StartPoint",
            objectInstanceId = "start_tutorial_03"
        };
        startObj3.AddParameter("direction", CardinalDirection.East.ToString());
        objectsList.Add(startObj3);

        // NEW: Add finish point as GridObject
        var finishObj3 = new GridObject
        {
            position = new Vector2Int(7, 7),
            objectTypeId = "FinishPoint",
            objectInstanceId = "finish_tutorial_03"
        };
        objectsList.Add(finishObj3);

        level.objects = objectsList.ToArray();

        SaveLevel(level, "tutorial_03_avoid_obstacles.asset");
    }

    private static void CreateLevel4_ButtonsAndDoors()
    {
        LevelGridData level = ScriptableObject.CreateInstance<LevelGridData>();
        level.levelId = "tutorial_04_buttons_doors";
        level.levelName = "Tutorial 4: Buttons & Doors";
        level.difficulty = 2;
        level.hintText = "Press the BUTTON to open the DOOR.\n\nSequence:\n1. Move to button\n2. Press button (action)\n3. Move through door\n4. Reach finish";
        level.gridWidth = 8;
        level.gridHeight = 8;

        // Create ground path
        var terrainList = new System.Collections.Generic.List<TerrainCell>();
        for (int x = 0; x < 8; x++)
        {
            terrainList.Add(new TerrainCell
            {
                position = new Vector2Int(x, 2),
                terrainType = "Ground"
            });
        }

        // Add branch to button
        for (int y = 2; y < 5; y++)
        {
            terrainList.Add(new TerrainCell
            {
                position = new Vector2Int(2, y),
                terrainType = "Ground"
            });
        }

        level.terrain = terrainList.ToArray();

        // Place button and door
        var objectsList = new System.Collections.Generic.List<GridObject>();

        objectsList.Add(new GridObject
        {
            position = new Vector2Int(2, 4),
            objectTypeId = "Button",
            objectInstanceId = "button_001"
        });

        objectsList.Add(new GridObject
        {
            position = new Vector2Int(5, 2),
            objectTypeId = "Door",
            objectInstanceId = "door_001"
        });

        // NEW: Add start point as GridObject
        var startObj4 = new GridObject
        {
            position = new Vector2Int(0, 2),
            objectTypeId = "StartPoint",
            objectInstanceId = "start_tutorial_04"
        };
        startObj4.AddParameter("direction", CardinalDirection.East.ToString());
        objectsList.Add(startObj4);

        // NEW: Add finish point as GridObject
        var finishObj4 = new GridObject
        {
            position = new Vector2Int(7, 2),
            objectTypeId = "FinishPoint",
            objectInstanceId = "finish_tutorial_04"
        };
        objectsList.Add(finishObj4);

        level.objects = objectsList.ToArray();

        SaveLevel(level, "tutorial_04_buttons_doors.asset");
    }

    private static void CreateLevel5_ComplexMaze()
    {
        LevelGridData level = ScriptableObject.CreateInstance<LevelGridData>();
        level.levelId = "tutorial_05_complex_maze";
        level.levelName = "Tutorial 5: Complex Maze";
        level.difficulty = 3;
        level.hintText = "Navigate the maze to reach the finish.\n\nThis level combines:\n- Obstacle avoidance\n- Multiple turns\n- Strategic planning";
        level.gridWidth = 10;
        level.gridHeight = 10;

        // Create maze terrain
        var terrainList = new System.Collections.Generic.List<TerrainCell>();

        // Fill entire grid with ground
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                terrainList.Add(new TerrainCell
                {
                    position = new Vector2Int(x, y),
                    terrainType = "Ground"
                });
            }
        }

        level.terrain = terrainList.ToArray();

        // Create maze walls
        var objectsList = new System.Collections.Generic.List<GridObject>();

        // Vertical walls
        for (int y = 1; y < 9; y++)
        {
            objectsList.Add(new GridObject
            {
                position = new Vector2Int(3, y),
                objectTypeId = "Wall",
                objectInstanceId = $"wall_v1_{y}"
            });

            objectsList.Add(new GridObject
            {
                position = new Vector2Int(6, y),
                objectTypeId = "Wall",
                objectInstanceId = $"wall_v2_{y}"
            });
        }

        // Horizontal walls with gaps
        for (int x = 1; x < 9; x++)
        {
            if (x != 3 && x != 6)
            {
                objectsList.Add(new GridObject
                {
                    position = new Vector2Int(x, 4),
                    objectTypeId = "Wall",
                    objectInstanceId = $"wall_h1_{x}"
                });

                objectsList.Add(new GridObject
                {
                    position = new Vector2Int(x, 7),
                    objectTypeId = "Wall",
                    objectInstanceId = $"wall_h2_{x}"
                });
            }
        }

        // NEW: Add start point as GridObject
        var startObj5 = new GridObject
        {
            position = new Vector2Int(0, 0),
            objectTypeId = "StartPoint",
            objectInstanceId = "start_tutorial_05"
        };
        startObj5.AddParameter("direction", CardinalDirection.East.ToString());
        objectsList.Add(startObj5);

        // NEW: Add finish point as GridObject
        var finishObj5 = new GridObject
        {
            position = new Vector2Int(9, 9),
            objectTypeId = "FinishPoint",
            objectInstanceId = "finish_tutorial_05"
        };
        objectsList.Add(finishObj5);

        level.objects = objectsList.ToArray();

        SaveLevel(level, "tutorial_05_complex_maze.asset");
    }

    private static void SaveLevel(LevelGridData level, string filename)
    {
        string assetPath = Path.Combine(LEVELS_PATH, filename);
        AssetDatabase.CreateAsset(level, assetPath);
        Debug.Log($"✓ Created: {level.levelName} ({assetPath})");
    }
}
