using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CodeBlocks.Editor.Tools
{
    public class PackageVersionTool : EditorWindow
    {
        private const string PACKAGE_PATH = "Packages/com.codeblocks.robotprogramming";
        private const string PACKAGE_JSON = "package.json";
        private const string SAMPLES_JSON = "Samples~/package.json";
        private const string CHANGELOG_MD = "CHANGELOG.md";
        private const string README_MD = "README.md";

        private string currentVersion = "";
        private string newVersion = "";
        private bool updateSamplesVersion = false;
        private string changelogDescription = "";
        private Vector2 scrollPosition;

        [MenuItem("Tools/CodeBlocks/Package Version Tool")]
        public static void ShowWindow()
        {
            var window = GetWindow<PackageVersionTool>("Package Version Tool");
            window.minSize = new Vector2(500, 600);
            window.Show();
        }

        private void OnEnable()
        {
            ReadCurrentVersion();
        }

        private void ReadCurrentVersion()
        {
            string packageJsonPath = Path.Combine(PACKAGE_PATH, PACKAGE_JSON);
            if (File.Exists(packageJsonPath))
            {
                string json = File.ReadAllText(packageJsonPath);
                Match match = Regex.Match(json, @"""version"":\s*""([^""]+)""");
                if (match.Success)
                {
                    currentVersion = match.Groups[1].Value;
                    newVersion = currentVersion; // Initialize with current version
                }
            }
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("UPM Package Version Tool", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Current Version (read-only)
            EditorGUILayout.LabelField("Current Version:", currentVersion);
            GUILayout.Space(5);

            // New Version (editable)
            EditorGUILayout.LabelField("New Version:");
            newVersion = EditorGUILayout.TextField(newVersion);
            GUILayout.Space(5);

            // Update Samples checkbox
            updateSamplesVersion = EditorGUILayout.Toggle("Update Samples Version", updateSamplesVersion);
            EditorGUILayout.HelpBox(
                "If checked, the version in Samples~/package.json will also be updated to match the new version.",
                MessageType.Info
            );
            GUILayout.Space(10);

            // Changelog description (large text area)
            EditorGUILayout.LabelField("Changelog Description:");
            EditorGUILayout.HelpBox(
                "Describe the changes in this version. Use markdown format:\n" +
                "### Added\n- New feature\n\n" +
                "### Fixed\n- Bug fix\n\n" +
                "### Changed\n- Modification",
                MessageType.Info
            );
            changelogDescription = EditorGUILayout.TextArea(changelogDescription, GUILayout.Height(200));
            GUILayout.Space(10);

            // Validation
            bool isValid = !string.IsNullOrEmpty(newVersion) &&
                           !string.IsNullOrEmpty(changelogDescription) &&
                           newVersion != currentVersion;

            if (!isValid)
            {
                if (newVersion == currentVersion)
                {
                    EditorGUILayout.HelpBox("New version must be different from current version!", MessageType.Warning);
                }
                else if (string.IsNullOrEmpty(newVersion))
                {
                    EditorGUILayout.HelpBox("Please enter a new version number!", MessageType.Warning);
                }
                else if (string.IsNullOrEmpty(changelogDescription))
                {
                    EditorGUILayout.HelpBox("Please provide a changelog description!", MessageType.Warning);
                }
            }

            // Confirm button
            GUI.enabled = isValid;
            if (GUILayout.Button("Confirm and Update Files", GUILayout.Height(40)))
            {
                OnConfirmClicked();
            }
            GUI.enabled = true;

            EditorGUILayout.EndScrollView();
        }

        private void OnConfirmClicked()
        {
            try
            {
                // Update package.json
                UpdatePackageJson();

                // Update Samples~/package.json if checkbox is checked
                if (updateSamplesVersion)
                {
                    UpdateSamplesJson();
                }

                // Update CHANGELOG.md
                UpdateChangelog();

                // Update README.md
                UpdateReadme();

                // Generate git commands
                GenerateGitCommands();

                EditorUtility.DisplayDialog(
                    "Success",
                    $"Package version updated to {newVersion}!\n\n" +
                    "Git commands have been logged to the console.\n" +
                    "Please review changes and execute git commands manually.",
                    "OK"
                );

                // Refresh current version
                ReadCurrentVersion();
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to update version:\n{ex.Message}", "OK");
                Debug.LogError($"PackageVersionTool Error: {ex}");
            }
        }

        private void UpdatePackageJson()
        {
            string path = Path.Combine(PACKAGE_PATH, PACKAGE_JSON);
            string json = File.ReadAllText(path);

            // Replace version using regex
            json = Regex.Replace(json, @"""version"":\s*""[^""]+""", $"\"version\": \"{newVersion}\"");

            File.WriteAllText(path, json);
            Debug.Log($"✅ Updated {PACKAGE_JSON} to version {newVersion}");
        }

        private void UpdateSamplesJson()
        {
            string path = Path.Combine(PACKAGE_PATH, SAMPLES_JSON);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Samples package.json not found at {path}");
                return;
            }

            string json = File.ReadAllText(path);

            // Replace version using regex
            json = Regex.Replace(json, @"""version"":\s*""[^""]+""", $"\"version\": \"{newVersion}\"");

            File.WriteAllText(path, json);
            Debug.Log($"✅ Updated {SAMPLES_JSON} to version {newVersion}");
        }

        private void UpdateChangelog()
        {
            string path = Path.Combine(PACKAGE_PATH, CHANGELOG_MD);
            string changelog = File.ReadAllText(path);

            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string newEntry = $"## [{newVersion}] - {today}\n\n{changelogDescription}\n\n";

            // Find the position to insert (after the header, before first version entry)
            int insertPos = changelog.IndexOf("## [");
            if (insertPos != -1)
            {
                changelog = changelog.Insert(insertPos, newEntry);
            }
            else
            {
                // If no version entries exist, append after header
                changelog += "\n" + newEntry;
            }

            File.WriteAllText(path, changelog);
            Debug.Log($"✅ Updated {CHANGELOG_MD} with version {newVersion}");
        }

        private void UpdateReadme()
        {
            string path = Path.Combine(PACKAGE_PATH, README_MD);
            string readme = File.ReadAllText(path);

            // Replace HTTPS git URL (line ~24)
            readme = Regex.Replace(
                readme,
                @"https://github\.com/mikkiducher/TestCodeBlock\.git\?path=Packages/com\.codeblocks\.robotprogramming#v[\d\.]+",
                $"https://github.com/mikkiducher/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming#v{newVersion}"
            );

            // Replace SSH git URL (line ~29)
            readme = Regex.Replace(
                readme,
                @"git@github\.com:mikkiducher/TestCodeBlock\.git\?path=Packages/com\.codeblocks\.robotprogramming#v[\d\.]+",
                $"git@github.com:mikkiducher/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming#v{newVersion}"
            );

            File.WriteAllText(path, readme);
            Debug.Log($"✅ Updated {README_MD} with version {newVersion} git URLs");
        }

        private void GenerateGitCommands()
        {
            string repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

            // Message 1: TODO Checklist
            string checklist = $@"
═══════════════════════════════════════════════════════════
📦 PACKAGE VERSION {newVersion} UPDATED
═══════════════════════════════════════════════════════════

✅ TODO: Review changes before commit:
  [ ] CHANGELOG.md - new version {newVersion} added
  [ ] package.json - version updated to {newVersion}
  {(updateSamplesVersion ? $"[ ] Samples~/package.json - version updated to {newVersion}\n  " : "")}[ ] README.md - git URLs updated to v{newVersion}
  [ ] All changes added to git (use git status to verify)

Execute commands below for release ↓
";
            Debug.Log(checklist);

            // Message 2: Git Commands
            string gitCommands = $@"
═══════════════════════════════════════════════════════════
📝 GIT COMMANDS FOR RELEASE v{newVersion}
═══════════════════════════════════════════════════════════

# 1. Check changes
cd ""{repoRoot}""
git status
git diff

# 2. Add ALL modified files
git add .

# 3. Create commit
git commit -m ""Release v{newVersion}""

# 4. Create and push tag
git tag v{newVersion}
git push origin v{newVersion}

# 5. Push changes to master
git push origin master

═══════════════════════════════════════════════════════════
";
            Debug.Log(gitCommands);

            // Message 3: Success
            Debug.Log($"✅ Version {newVersion} is ready to be released! Copy commands above.");
        }
    }
}
