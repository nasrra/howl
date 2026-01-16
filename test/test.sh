#!/bin/bash

PROJECT_PATH="Howl.Test.csproj"

NAMESPACE="Howl.Test."

VENDORS_NAMESPACE="${NAME_SPACE}Vendors."

MONOGAME_NAMESPACE="${VENDORS_NAME_SPACE}MonoGame."
MONOGAME_MATH_NAMESPACE="${MONOGAME_NAMESPACE}Math."
MONOGAME_GRAPHICS_NAMESPACE="${MONOGAME_NAMESPACE}Graphics."

TEST_NAME=$1

# Declare an associative array
declare -A TEST_MAP
TEST_MAP=(
    ["ECS"]="${NAMESPACE}ECS."
    ["Math"]="${NAMESPACE}Math."
    ["MonoGameColorExt"]="${MONOGAME_GRAPHICS_NAMESPACE}ColorExtensionsTest"
    ["MonoGameRectangleExt"]="${MONOGAME_MATH_NAMESPACE}RectangleExtensionsTest"
    ["MonoGameVector2Ext"]="${MONOGAME_MATH_NAMESPACE}Vector2ExtensionsTest"
    ["Rectangle"]="RectangleTest"
    ["Vector2"]="Vector2Test"
    ["Vector2Int"]="Vector2IntTest"
)

# print the help menu if the test name variable has zero length.
# meaning that the user didnt input anything.
if [ -z "$TEST_NAME" ]; then
    echo ""
    echo "Usage:"
    echo "  ./run.sh <TestName>"
    echo ""
    echo "Available Tests:"
    for key in "${!TEST_MAP[@]}"; do # "${!TEST_MAP[@]}" gets all keys in the map. 
        echo "  $key"
    done
    exit 1
fi

# check if the test exists in the map
if [[ -n "${TEST_MAP[$TEST_NAME]}" ]]; then
    dotnet test "$PROJECT_PATH" --filter "FullyQualifiedName~${TEST_MAP[$TEST_NAME]}"
else
    echo "Unknown test: $TEST_NAME"
    echo "Please run the script with no test name to see available tests."
    exit 1
fi