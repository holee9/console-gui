#!/bin/bash
# CC Dispatch Deployment Script
# Usage: ./scripts/cc-dispatch.sh <round> <team> <dispatch-src-file>
# Example: ./scripts/cc-dispatch.sh S04-R3 coordinator DISPATCH-COORDINATOR-2026-04-12.md

set -e

ROUND=$1      # e.g. S04-R3
TEAM=$2       # e.g. coordinator, team-a, team-b, team-design, qa, ra
SRC_FILE=$3   # source dispatch file in project root

WORKTREE=".worktrees/$TEAM"
ACTIVE_DIR="$WORKTREE/.moai/dispatches/active"
ROUND_LOWER=$(echo "$ROUND" | tr '[:upper:]' '[:lower:]')

if [ -z "$ROUND" ] || [ -z "$TEAM" ] || [ -z "$SRC_FILE" ]; then
  echo "Usage: $0 <round> <team> <dispatch-src-file>"
  exit 1
fi

if [ ! -f "$SRC_FILE" ]; then
  echo "ERROR: Source file not found: $SRC_FILE"
  exit 1
fi

if [ ! -d "$WORKTREE" ]; then
  echo "ERROR: Worktree not found: $WORKTREE"
  exit 1
fi

echo "=== Deploying $ROUND dispatch to $TEAM ==="

# Step 1: Fix sln name and write DISPATCH.md
sed 's/Console-GUI\.sln/HnVue.sln/g' "$SRC_FILE" > "$WORKTREE/DISPATCH.md"
echo "[1/4] DISPATCH.md written"

# Step 2: Write active dispatch file
ACTIVE_FILE="$ACTIVE_DIR/${ROUND_LOWER}-${TEAM}.md"
sed 's/Console-GUI\.sln/HnVue.sln/g' "$SRC_FILE" > "$ACTIVE_FILE"
echo "[2/4] Active dispatch written: $ACTIVE_FILE"

# Step 3: Ensure previous round history exists
PREV_ROUND=$(echo "$ROUND" | sed 's/R\([0-9]*\)$/R$((\1-1))/' 2>/dev/null || echo "")
PREV_FILE="$ACTIVE_DIR/$(echo "$ROUND" | sed 's/R[0-9]*$//' | tr '[:upper:]' '[:lower:]')r$(($(echo "$ROUND" | grep -o 'R[0-9]*' | tr -d 'R') - 1))-${TEAM}.md"
if [ ! -f "$PREV_FILE" ] && [ -f ".moai/dispatches/active/$(basename "$PREV_FILE")" ]; then
  cp ".moai/dispatches/active/$(basename "$PREV_FILE")" "$PREV_FILE"
  echo "[3/4] Previous round history added: $(basename "$PREV_FILE")"
else
  echo "[3/4] Previous round history: OK"
fi

# Step 4: Git add + commit in worktree
cd "$WORKTREE"
git add DISPATCH.md .moai/dispatches/active/
git commit -m "chore: $ROUND DISPATCH 배포 (CC)" 2>&1 | tail -2
cd - > /dev/null

echo "=== DONE: $TEAM dispatch deployed and committed ==="
