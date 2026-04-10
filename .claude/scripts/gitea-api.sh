#!/bin/bash
# gitea-api.sh — Gitea API helper with UTF-8 safe Korean support
# Usage:
#   gitea-api.sh issue create "제목" "본문" "label1,label2"
#   gitea-api.sh issue comment 62 "한글 코멘트"
#   gitea-api.sh pr create "team/team-a" "제목" "본문"
#   gitea-api.sh issue close 62
#   gitea-api.sh issue list [state]

set -euo pipefail

GITEA_URL="http://10.11.1.40:7001"
TOKEN="a4cb79626194b34a2d52835de05fb770162af014"
REPO="DR_RnD/Console-GUI"
API_BASE="${GITEA_URL}/api/v1/repos/${REPO}"

api_call() {
    local method="$1"
    local endpoint="$2"
    local json_file="$3"

    curl -s -S \
        -X "${method}" \
        -H "Authorization: token ${TOKEN}" \
        -H "Content-Type: application/json; charset=utf-8" \
        -H "Accept: application/json" \
        --data-binary @"${json_file}" \
        "${API_BASE}${endpoint}"
}

write_json() {
    local filepath="$1"
    shift
    # Use printf to ensure UTF-8 encoding is preserved
    printf '%s' "$@" > "${filepath}"
}

cmd_issue_create() {
    local title="$1"
    local body="$2"
    local labels="${3:-}"

    local tmpfile
    tmpfile=$(mktemp /tmp/gitea_issue_XXXXXX.json)

    if [ -n "${labels}" ]; then
        local label_array
        label_array=$(printf '"%s"' "${labels}" | sed 's/,/","/g')
        write_json "${tmpfile}" "{\"title\":\"${title}\",\"body\":\"${body}\",\"labels\":[${label_array}]}"
    else
        write_json "${tmpfile}" "{\"title\":\"${title}\",\"body\":\"${body}\"}"
    fi

    api_call "POST" "/issues" "${tmpfile}"
    rm -f "${tmpfile}"
}

cmd_issue_comment() {
    local issue_number="$1"
    local body="$2"

    local tmpfile
    tmpfile=$(mktemp /tmp/gitea_comment_XXXXXX.json)

    write_json "${tmpfile}" "{\"body\":\"${body}\"}"

    api_call "POST" "/issues/${issue_number}/comments" "${tmpfile}"
    rm -f "${tmpfile}"
}

cmd_issue_close() {
    local issue_number="$1"

    local tmpfile
    tmpfile=$(mktemp /tmp/gitea_close_XXXXXX.json)

    write_json "${tmpfile}" "{\"state\":\"closed\"}"

    api_call "PATCH" "/issues/${issue_number}" "${tmpfile}"
    rm -f "${tmpfile}"
}

cmd_issue_list() {
    local state="${1:-open}"
    curl -s -H "Authorization: token ${TOKEN}" \
        "${API_BASE}/issues?state=${state}&limit=20" | \
        grep -o '"number":[0-9]*\|"title":"[^"]*"' | \
        paste - - | sed 's/"number":/#/; s/"title":"//; s/"$//'
}

cmd_pr_create() {
    local head="$1"
    local title="$2"
    local body="$3"

    local tmpfile
    tmpfile=$(mktemp /tmp/gitea_pr_XXXXXX.json)

    write_json "${tmpfile}" "{\"head\":\"${head}\",\"base\":\"main\",\"title\":\"${title}\",\"body\":\"${body}\"}"

    api_call "POST" "/pulls" "${tmpfile}"
    rm -f "${tmpfile}"
}

# Main dispatch
case "${1:-help}" in
    issue)
        case "${2:-}" in
            create)  cmd_issue_create "$3" "$4" "${5:-}" ;;
            comment) cmd_issue_comment "$3" "$4" ;;
            close)   cmd_issue_close "$3" ;;
            list)    cmd_issue_list "${3:-open}" ;;
            *)       echo "Usage: gitea-api.sh issue {create|comment|close|list}" ;;
        esac
        ;;
    pr)
        case "${2:-}" in
            create)  cmd_pr_create "$3" "$4" "$5" ;;
            *)       echo "Usage: gitea-api.sh pr create <head> <title> <body>" ;;
        esac
        ;;
    help|*)
        echo "Gitea API Helper — UTF-8 Korean Safe"
        echo ""
        echo "Usage:"
        echo "  gitea-api.sh issue create  <title> <body> [labels]"
        echo "  gitea-api.sh issue comment <number> <body>"
        echo "  gitea-api.sh issue close   <number>"
        echo "  gitea-api.sh issue list    [state]"
        echo "  gitea-api.sh pr create     <head> <title> <body>"
        ;;
esac
