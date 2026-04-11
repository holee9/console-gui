#!/bin/bash
# CC Dispatch All Teams Script
# Usage: ./scripts/cc-dispatch-all.sh <round>
# Example: ./scripts/cc-dispatch-all.sh S04-R3
#
# Expects dispatch files in project root named:
#   DISPATCH-COORDINATOR-YYYY-MM-DD.md
#   DISPATCH-TEAM-A-YYYY-MM-DD.md
#   DISPATCH-TEAM-B-YYYY-MM-DD.md
#   DISPATCH-DESIGN-YYYY-MM-DD.md
#   DISPATCH-QA-YYYY-MM-DD.md
#   DISPATCH-RA-YYYY-MM-DD.md  (or falls back to .moai/dispatches/active/S0X-R1-ra.md)

set -e

ROUND=$1
if [ -z "$ROUND" ]; then
  echo "Usage: $0 <round>  (e.g. S04-R3)"
  exit 1
fi

DATE=$(date +%Y-%m-%d)

declare -A TEAM_FILES=(
  [coordinator]="DISPATCH-COORDINATOR-${DATE}.md"
  [team-a]="DISPATCH-TEAM-A-${DATE}.md"
  [team-b]="DISPATCH-TEAM-B-${DATE}.md"
  [team-design]="DISPATCH-DESIGN-${DATE}.md"
  [qa]="DISPATCH-QA-${DATE}.md"
  [ra]="DISPATCH-RA-${DATE}.md"
)

ROUND_LOWER=$(echo "$ROUND" | tr 'A-Z' 'a-z')
ROUND_NUM=$(echo "$ROUND" | grep -o 'R[0-9]*' | tr -d 'R')
SPRINT=$(echo "$ROUND" | grep -o 'S[0-9]*')
SPRINT_LOWER=$(echo "$SPRINT" | tr 'A-Z' 'a-z')
PREV_NUM=$((ROUND_NUM - 1))

SUCCESS=0
FAILED=0

for TEAM in coordinator team-a team-b team-design qa ra; do
  SRC="${TEAM_FILES[$TEAM]}"
  WORKTREE=".worktrees/$TEAM"
  ACTIVE_DIR="$WORKTREE/.moai/dispatches/active"

  # Fallback for RA
  if [ ! -f "$SRC" ] && [ "$TEAM" = "ra" ]; then
    SRC=".moai/dispatches/active/${SPRINT_LOWER}-r1-ra.md"
  fi

  if [ ! -f "$SRC" ]; then
    echo "[SKIP] $TEAM: source not found ($SRC)"
    FAILED=$((FAILED+1))
    continue
  fi

  echo "--- Deploying to $TEAM ---"

  # Write DISPATCH.md
  sed 's/Console-GUI\.sln/HnVue.sln/g' "$SRC" > "$WORKTREE/DISPATCH.md"

  # Write active dispatch
  sed 's/Console-GUI\.sln/HnVue.sln/g' "$SRC" > "$ACTIVE_DIR/${ROUND_LOWER}-${TEAM}.md"

  # Ensure previous round history
  if [ $PREV_NUM -gt 0 ]; then
    PREV_FILE="${ACTIVE_DIR}/${SPRINT_LOWER}-r${PREV_NUM}-${TEAM}.md"
    PREV_SRC=".moai/dispatches/active/$(basename "$PREV_FILE")"
    if [ ! -f "$PREV_FILE" ] && [ -f "$PREV_SRC" ]; then
      cp "$PREV_SRC" "$PREV_FILE"
      echo "  + Previous round history added"
    fi
  fi

  # Git commit in worktree
  cd "$WORKTREE"
  git add DISPATCH.md .moai/dispatches/active/
  COMMIT_MSG=$(git commit -m "chore: $ROUND DISPATCH 배포 (CC)" 2>&1 | tail -1)
  echo "  $COMMIT_MSG"
  cd - > /dev/null

  SUCCESS=$((SUCCESS+1))
done

echo ""
echo "=== DISPATCH DEPLOY COMPLETE: $SUCCESS 성공, $FAILED 실패 ==="
