#!/bin/bash
# Get Unity Personal License for CI/CD
# Run this ONCE on a machine with Unity installed to get your license key

set -e

echo "=== Unity License Generator for CI ==="
echo ""
echo "This script generates a Unity Personal license file for GitHub Actions."
echo "Run it on a machine where Unity Hub is installed and you're signed in."
echo ""

# Find Unity installation
UNITY_PATHS=(
    "/Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity"
    "/opt/unity/Editor/Unity"
    "/usr/local/unity/Editor/Unity"
    "$HOME/Unity/Hub/Editor/*/Editor/Unity"
    "C:/Program Files/Unity/Hub/Editor/*/Editor/Unity.exe"
)

UNITY_EXE=""
for path in "${UNITY_PATHS[@]}"; do
    for found in $path; do
        if [ -f "$found" ]; then
            UNITY_EXE="$found"
            break 2
        fi
    done
done

if [ -z "$UNITY_EXE" ]; then
    echo "❌ Unity not found in standard locations."
    echo "Please run Unity manually with:"
    echo "  /path/to/Unity -batchmode -nographics -logFile - -quit"
    echo "Then copy the license from ~/.local/share/unity3d/Unity/Unity_lic.ulf"
    exit 1
fi

echo "✅ Found Unity at: $UNITY_EXE"
echo ""

# Generate license
echo "Generating license (this may take 30-60 seconds)..."
"$UNITY_EXE" -batchmode -nographics -logFile - -quit

# Find license file
LICENSE_FILES=(
    "$HOME/.local/share/unity3d/Unity/Unity_lic.ulf"
    "$HOME/.config/unity3d/Unity/Unity_lic.ulf"
    "$UNITY_EXE/../Unity_lic.ulf"
)

LICENSE_FILE=""
for f in "${LICENSE_FILES[@]}"; do
    if [ -f "$f" ]; then
        LICENSE_FILE="$f"
        break
    fi
done

if [ -z "$LICENSE_FILE" ]; then
    echo "❌ License file not found. Check Unity logs above."
    exit 1
fi

echo ""
echo "✅ License generated at: $LICENSE_FILE"
echo ""
echo "=== COPY THIS FOR GITHUB SECRETS ==="
echo ""
cat "$LICENSE_FILE"
echo ""
echo "=== END LICENSE ==="
echo ""
echo "Next steps:"
echo "1. Go to your GitHub repo → Settings → Secrets and variables → Actions"
echo "2. Add new repository secret: UNITY_LICENSE"
echo "3. Paste the entire license content above (including BEGIN/END lines)"
echo "4. Push to main branch to trigger build"