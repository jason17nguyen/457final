# 457 Final - Unity Project

## Setup
1. Install Git LFS: https://git-lfs.com/
2. Clone repo: `git clone ...`
3. Open Unity Hub → Add project → select folder
4. Unity settings must be:
   - Version Control: Visible Meta Files
   - Asset Serialization: Force Text

## Rules
- Do NOT commit Library/, Temp/, Logs/, UserSettings/
- One person edits `Main.unity` at a time
- Make changes in branches and open PRs
- Prefer prefabs over editing the main scene

## Branch naming
- feature/...
- bugfix/...
- ui/...

## Large assets
Tracked with Git LFS (.gitattributes). Don't bypass it.
