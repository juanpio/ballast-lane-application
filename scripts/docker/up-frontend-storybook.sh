#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$REPO_ROOT"

docker compose --profile frontend --profile storybook up -d frontend storybook

echo "Frontend is running at http://localhost:5173"
echo "Storybook is running at http://localhost:6006"
