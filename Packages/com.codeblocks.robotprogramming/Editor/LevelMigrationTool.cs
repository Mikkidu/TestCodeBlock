using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CodeBlocks.Editor
{
    /// <summary>
    /// Migration tool for converting legacy StartPoint/FinishPoint fields to unified objects[] array.
    /// Menu: Tools → CodeBlocks → Migrate Levels (Start/Finish)
    /// </summary>
    public static class LevelMigrationTool
    {
        [MenuItem("Tools/CodeBlocks/Migrate Levels (Start-Finish)")]
        public static void MigrateLevels()
        {
            // Find all LevelGridData assets in project
            string[] guids = AssetDatabase.FindAssets("t:LevelGridData");
            int migratedCount = 0;
            int skippedCount = 0;
            List<string> migratedLevels = new List<string>();

            EditorUtility.DisplayProgressBar("Migration", "Finding levels...", 0f);

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                LevelGridData level = AssetDatabase.LoadAssetAtPath<LevelGridData>(assetPath);

                if (level == null) continue;

                bool changed = false;
                var objectsList = new List<GridObject>(level.objects);

                float progress = (float)migratedCount / guids.Length;
                EditorUtility.DisplayProgressBar("Migration", $"Migrating: {level.levelName}", progress);

                // Suppress obsolete warnings - Migration Tool is allowed to use legacy fields
                #pragma warning disable CS0618

                // Migrate StartPoint
                if (level.start != null)
                {
                    // Check if already migrated (avoid duplicates)
                    bool alreadyHasStart = false;
                    foreach (var obj in objectsList)
                    {
                        if (obj != null && obj.objectTypeId == "StartPoint")
                        {
                            alreadyHasStart = true;
                            break;
                        }
                    }

                    if (!alreadyHasStart)
                    {
                        var startObj = new GridObject
                        {
                            position = level.start.position,
                            objectTypeId = "StartPoint",
                            objectInstanceId = $"start_{level.levelId}"
                        };
                        startObj.AddParameter("direction", level.start.direction.ToString());
                        objectsList.Add(startObj);

                        Debug.Log($"Migration: Added StartPoint to '{level.levelName}' at {level.start.position}, direction {level.start.direction}");
                        changed = true;
                    }

                    // Clear legacy field (will be removed in v1.1.0)
                    level.start = null;
                }

                // Migrate FinishPoint
                if (level.finish != null)
                {
                    // Check if already migrated (avoid duplicates)
                    bool alreadyHasFinish = false;
                    foreach (var obj in objectsList)
                    {
                        if (obj != null && obj.objectTypeId == "FinishPoint")
                        {
                            alreadyHasFinish = true;
                            break;
                        }
                    }

                    if (!alreadyHasFinish)
                    {
                        var finishObj = new GridObject
                        {
                            position = level.finish.position,
                            objectTypeId = "FinishPoint",
                            objectInstanceId = $"finish_{level.levelId}"
                        };
                        objectsList.Add(finishObj);

                        Debug.Log($"Migration: Added FinishPoint to '{level.levelName}' at {level.finish.position}");
                        changed = true;
                    }

                    // Clear legacy field (will be removed in v1.1.0)
                    level.finish = null;
                }

                #pragma warning restore CS0618

                if (changed)
                {
                    level.objects = objectsList.ToArray();
                    EditorUtility.SetDirty(level);
                    migratedCount++;
                    migratedLevels.Add(level.levelName);
                }
                else
                {
                    skippedCount++;
                }
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Show summary dialog
            string summary = $"Migration complete!\n\n" +
                           $"✅ Migrated: {migratedCount} levels\n" +
                           $"⏭️ Skipped: {skippedCount} levels (already migrated)\n\n";

            if (migratedLevels.Count > 0)
            {
                summary += "Migrated levels:\n";
                foreach (var levelName in migratedLevels)
                {
                    summary += $"  • {levelName}\n";
                }
            }

            summary += "\nCheck Console for detailed migration log.";

            EditorUtility.DisplayDialog("Level Migration", summary, "OK");

            Debug.Log($"=== Level Migration Summary ===");
            Debug.Log($"Migrated: {migratedCount} levels");
            Debug.Log($"Skipped: {skippedCount} levels");
            Debug.Log($"All StartPoint/FinishPoint data moved to objects[] array.");
        }
    }
}
