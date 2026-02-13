# Git Commands for Release v1.0.8

## 📋 Summary of Changes
- **13 files modified** (unified Start/Finish architecture)
- **5 public API methods added** (StartProgram, StopProgram, etc.)
- **1 migration tool added** (LevelMigrationTool.cs)
- **Bug fixes:** marker duplication, background positioning, Reset button

---

## 🔍 Step 1: Review Changes

```bash
# Check current status
git status

# Review diff of key files
git diff Packages/com.codeblocks.robotprogramming/Runtime/Managers/GameManager.cs
git diff Packages/com.codeblocks.robotprogramming/CHANGELOG.md
git diff .Doc/Issues.md
```

---

## 📦 Step 2: Stage Changes

```bash
# Stage package changes (core functionality)
git add Packages/com.codeblocks.robotprogramming/Runtime/Managers/GameManager.cs
git add Packages/com.codeblocks.robotprogramming/CHANGELOG.md
git add Packages/com.codeblocks.robotprogramming/package.json

# Stage documentation
git add .Doc/Issues.md
git add .Doc/Release_v1.0.8_Summary.md
git add .Doc/Tasks/25_Step1_PublicAPI_StopFixes.md

# Stage test script (optional, can be excluded if prefer)
git add Assets/Scripts/GameManagerAPITest.cs
git add Assets/Scripts/GameManagerAPITest.cs.meta

# NOTE: Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset
# This is Unity-generated metadata, you may want to exclude it or include separately
```

**Alternative (stage all at once):**
```bash
git add -A
```

---

## 💾 Step 3: Commit

```bash
git commit -m "Release v1.0.8: Unified Start/Finish + Public API + Bug fixes

Major Changes:
- Unified StartPoint/FinishPoint architecture (now GridObject in objects[] array)
- Added 5 public API methods (StartProgram, StopProgram, ClearProgram, IsProgramRunning, GetBlocksCount)
- Added Migration Tool (Tools → CodeBlocks → Migrate Levels)
- Refactored OnResetButtonClicked() to reuse Stop logic (DRY)

Bug Fixes:
- Fixed start/finish marker duplication
- Fixed background positioning
- Reset button now stops running program

Breaking Changes (v1.1.0):
- start/finish fields deprecated (use GetStartPoint/GetFinishPoint)
- Migration Tool available for converting legacy levels

🤖 Generated with Claude Code (https://claude.com/claude-code)

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## 🏷️ Step 4: Create Tag

```bash
# Create annotated tag for v1.0.8
git tag -a v1.0.8 -m "Release v1.0.8: Unified Start/Finish + Public API

Highlights:
- Unified StartPoint/FinishPoint architecture
- Public API for external control (5 methods)
- Migration Tool for legacy levels
- Bug fixes: marker duplication, background positioning

Integration ready for play-united.

Full changelog: Packages/com.codeblocks.robotprogramming/CHANGELOG.md"
```

---

## 🚀 Step 5: Push to Remote

```bash
# Push commits
git push origin master

# Push tags
git push origin v1.0.8

# Or push everything at once
git push origin master --tags
```

---

## 🔍 Step 6: Verify Release

```bash
# Verify tag exists locally
git tag -l

# Verify tag exists on remote
git ls-remote --tags origin

# Check tag details
git show v1.0.8
```

---

## 📦 Step 7: Update play-united

In **play-united** project's `manifest.json`:

```json
{
  "dependencies": {
    "com.codeblocks.robotprogramming": "https://github.com/mikkiducher/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming#v1.0.8"
  }
}
```

Then in Unity:
1. Window → Package Manager
2. Wait for package to update
3. Verify version shows **1.0.8**

---

## 🧪 Optional: Test in Fresh Clone

```bash
# Clone fresh copy to verify
cd /tmp
git clone https://github.com/mikkiducher/TestCodeBlock.git test-v1.0.8
cd test-v1.0.8
git checkout v1.0.8

# Verify files
cat Packages/com.codeblocks.robotprogramming/package.json | grep version
cat Packages/com.codeblocks.robotprogramming/CHANGELOG.md | head -50
```

---

## 📝 Alternative: One-liner Commands

```bash
# If you want to do everything in one go:
git add -A && \
git commit -m "Release v1.0.8: Unified Start/Finish + Public API + Bug fixes" && \
git tag -a v1.0.8 -m "Release v1.0.8" && \
git push origin master --tags
```

---

## ⚠️ Troubleshooting

### If you need to redo the tag:
```bash
# Delete local tag
git tag -d v1.0.8

# Delete remote tag
git push origin :refs/tags/v1.0.8

# Recreate tag
git tag -a v1.0.8 -m "Release v1.0.8"
git push origin v1.0.8
```

### If commit message has typo:
```bash
# Amend last commit (only if not pushed yet!)
git commit --amend -m "New message"
```

---

## ✅ Post-Release Checklist

- [ ] Commit pushed to master
- [ ] Tag v1.0.8 created and pushed
- [ ] GitHub release visible: https://github.com/mikkiducher/TestCodeBlock/releases/tag/v1.0.8
- [ ] Package Manager in Unity shows v1.0.8
- [ ] play-united manifest.json updated
- [ ] play-united compiles without errors
- [ ] Test basic functionality in play-united

---

## 🎯 Next Steps

After releasing v1.0.8:
1. Test integration in play-united
2. Collect feedback
3. Plan v1.1.0 (breaking changes - remove deprecated fields)
