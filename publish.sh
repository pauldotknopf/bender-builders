#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION="$SCRIPT_DIR/project/BenderBuilders.slnx"

case "$(uname -s)" in
  Darwin)
    case "$(uname -m)" in
      arm64) RID="osx-arm64" ;;
      *) RID="osx-x64" ;;
    esac
    ;;
  MINGW*|MSYS*|CYGWIN*)
    RID="win-x64"
    ;;
  *)
    echo "Unsupported OS: $(uname -s)" >&2
    exit 1
    ;;
esac

echo "Publishing BenderBuilders for $RID..."
dotnet publish "$SOLUTION" -c Release -r "$RID" -p:RuntimeIdentifier="$RID"
