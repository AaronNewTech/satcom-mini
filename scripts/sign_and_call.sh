#!/usr/bin/env bash
set -euo pipefail

# sign_and_call.sh
# Usage:
#   ./scripts/sign_and_call.sh METHOD URL [BODY_FILE]
# Examples:
#   ./scripts/sign_and_call.sh POST http://localhost:5143/v1/satellites/<id>/telemetry payload.json
#   ./scripts/sign_and_call.sh GET http://localhost:5143/v1/satellites/<id>/location

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
# load repo root .env if present
if [ -f "$SCRIPT_DIR/../.env" ]; then
  # shellcheck disable=SC1090
  source "$SCRIPT_DIR/../.env"
fi

if [ -z "${ApiSigningSecret-}" ]; then
  echo "ApiSigningSecret not set. Please set it in .env or environment." >&2
  exit 1
fi

METHOD=${1:-GET}
URL=${2:?URL required}
BODY_FILE=${3:-}

TIMESTAMP=$(date +%s)

# compute body SHA256 hex
if [ -n "$BODY_FILE" ]; then
  if [ ! -f "$BODY_FILE" ]; then
    echo "Body file not found: $BODY_FILE" >&2
    exit 2
  fi
  BODY_CONTENT=$(cat "$BODY_FILE")
else
  BODY_CONTENT=""
fi

# body hash hex (lowercase)
BODY_HASH_HEX=$(printf '%s' "$BODY_CONTENT" | openssl dgst -sha256 -binary | xxd -p -c 256 | tr '[:upper:]' '[:lower:]')

# build canonical string: METHOD \n PATH \n TIMESTAMP \n BODY_HASH_HEX
# we must pass the path (URI path + query) to canonical. Extract path+query from URL.
PATH_AND_QUERY=$(printf '%s' "$URL" | awk -F/ '{print substr($0, index($0,$3))}' | sed 's|^[^/]*/||')
# Simpler: use python to parse
if command -v python3 >/dev/null 2>&1; then
  PATH_AND_QUERY=$(python3 - <<PY
from urllib.parse import urlparse
u = urlparse('$URL')
path = u.path or '/'
if u.query:
    path += '?' + u.query
print(path)
PY
)
fi

CANONICAL=$(printf '%s\n%s\n%s\n%s' "${METHOD^^}" "$PATH_AND_QUERY" "$TIMESTAMP" "$BODY_HASH_HEX")

# compute HMAC-SHA256 base64
SIGNATURE=$(printf '%s' "$CANONICAL" | openssl dgst -sha256 -hmac "$ApiSigningSecret" -binary | openssl base64)

# build curl command
CURL_OPTS=( -sS -w "\nHTTP_STATUS:%{http_code}\n" -X "$METHOD" -H "x-timestamp: $TIMESTAMP" -H "x-signature: $SIGNATURE" )
if [ -n "$BODY_FILE" ]; then
  CURL_OPTS+=( -H "Content-Type: application/json" --data-binary @$BODY_FILE )
fi

curl "${CURL_OPTS[@]}" "$URL"
