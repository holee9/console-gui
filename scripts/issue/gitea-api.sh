#!/bin/bash
# gitea-api.sh — Korean-safe Gitea API wrapper
# Usage:
#   ./gitea-api.sh issue-create "Title here" "Body here" "label1,label2"
#   ./gitea-api.sh issue-comment 61 "Comment text"
#   ./gitea-api.sh issue-close 61
#   ./gitea-api.sh issue-open 61
#   ./gitea-api.sh pr-create "Title" "body" "head_branch" "base_branch"
#
# Korean text is written to temp file first, then sent via --data-binary @file
# This avoids curl inline Korean encoding corruption (U+FFFD bug)

set -euo pipefail

GITEA_URL="${GITEA_URL:-http://10.11.1.40:7001}"
GITEA_TOKEN="${GITEA_TOKEN:-a4cb79626194b34a2d52835de05fb770162af014}"
GITEA_OWNER="${GITEA_OWNER:-DR_RnD}"
GITEA_REPO="${GITEA_REPO:-Console-GUI}"
API_BASE="$GITEA_URL/api/v1/repos/$GITEA_OWNER/$GITEA_REPO"

# Korean-safe JSON payload sender
send_json() {
    local method="$1"
    local endpoint="$2"
    local json_content="$3"

    local tmpfile
    tmpfile=$(mktemp /tmp/gitea_api_XXXXXX.json)
    echo "$json_content" > "$tmpfile"

    curl -s -X "$method" \
        -H "Authorization: token $GITEA_TOKEN" \
        -H "Content-Type: application/json; charset=utf-8" \
        --data-binary "@$tmpfile" \
        "$endpoint"

    rm -f "$tmpfile"
}

cmd_issue_create() {
    local title="$1"
    local body="$2"
    local labels_csv="${3:-}"

    local labels_json="[]"
    if [[ -n "$labels_csv" ]]; then
        # Convert label names to IDs via grep
        labels_json=$(echo "$labels_csv" | tr ',' '\n' | while read -r lbl; do
            id=$(curl -s -H "Authorization: token $GITEA_TOKEN" "$API_BASE/labels" | \
                node -e "const d=JSON.parse(require('fs').readFileSync(0,'utf8'));const l=d.find(x=>x.name==='$lbl');console.log(l?l.id:'')" 2>/dev/null || echo "")
            [[ -n "$id" ]] && echo "$id"
        done | tr '\n' ',' | sed 's/,$//')
        if [[ -n "$labels_json" ]]; then
            labels_json="[$labels_json]"
        else
            labels_json="[]"
        fi
    fi

    local json
    json=$(printf '{"title":"%s","body":"%s","labels":%s}' \
        "$(echo "$title" | sed 's/"/\\"/g')" \
        "$(echo "$body" | sed 's/"/\\"/g' | sed 's/$/\\n/g' | sed 's/\\n$//')" \
        "$labels_json")

    send_json POST "$API_BASE/issues" "$json"
}

cmd_issue_comment() {
    local issue_num="$1"
    local comment_body="$2"

    local json
    json=$(printf '{"body":"%s"}' "$(echo "$comment_body" | sed 's/"/\\"/g' | sed 's/$/\\n/g' | sed 's/\\n$//')")

    send_json POST "$API_BASE/issues/$issue_num/comments" "$json"
}

cmd_issue_close() {
    local issue_num="$1"
    send_json PATCH "$API_BASE/issues/$issue_num" '{"state":"closed"}'
}

cmd_issue_open() {
    local issue_num="$1"
    send_json PATCH "$API_BASE/issues/$issue_num" '{"state":"open"}'
}

cmd_pr_create() {
    local title="$1"
    local body="$2"
    local head="$3"
    local base="${4:-main}"

    local json
    json=$(printf '{"title":"%s","body":"%s","head":"%s","base":"%s"}' \
        "$(echo "$title" | sed 's/"/\\"/g')" \
        "$(echo "$body" | sed 's/"/\\"/g' | sed 's/$/\\n/g' | sed 's/\\n$//')" \
        "$head" "$base")

    send_json POST "$API_BASE/pulls" "$json"
}

# Main dispatch
case "${1:-help}" in
    issue-create)
        cmd_issue_create "${2:?title required}" "${3:?body required}" "${4:-}"
        ;;
    issue-comment)
        cmd_issue_comment "${2:?issue# required}" "${3:?comment required}"
        ;;
    issue-close)
        cmd_issue_close "${2:?issue# required}"
        ;;
    issue-open)
        cmd_issue_open "${2:?issue# required}"
        ;;
    pr-create)
        cmd_pr_create "${2:?title required}" "${3:?body required}" "${4:?head branch required}" "${5:-main}"
        ;;
    help|*)
        echo "Usage: $0 {issue-create|issue-comment|issue-close|issue-open|pr-create} [args...]"
        echo "  issue-create TITLE BODY [LABELS_CSV]"
        echo "  issue-comment ISSUE# COMMENT"
        echo "  issue-close ISSUE#"
        echo "  issue-open ISSUE#"
        echo "  pr-create TITLE BODY HEAD_BRANCH [BASE_BRANCH]"
        ;;
esac
