#!/usr/bin/env bash

set -e

PROJECT_PATH="Howl.Test.csproj"

NAMESPACE="Howl.Test."
MATH_NAMESPACE="${NAME_SPACE}Math."
ECS_NAMESPACE="${NAME_SPACE}ECS."

VENDORS_NAMESPACE="${NAME_SPACE}Vendors."

MONOGAME_NAMESPACE="${VENDORS_NAME_SPACE}MonoGame."
MONOGAME_MATH_NAMESPACE="${MONOGAME_NAMESPACE}Math."
MONOGAME_GRAPHICS_NAMESPACE="${MONOGAME_NAMESPACE}Graphics."

TEST_NAME=$1

# Declare an associative array
declare -A TEST_MAP
TEST_MAP=(
    ["ComponentRegistry"]="${ECS_NAMESPACE}ComponentRegistryTest"
    ["ECS"]="${ECS_NAMESPACE}"
    ["GenIndex"]="${ECS_NAMESPACE}GenIndexTest"
    ["Math"]="${MATH_NAMESPACE}"
    ["MonoGameColourExt"]="${MONOGAME_GRAPHICS_NAMESPACE}ColourExtensionsTest"
    ["MonoGameRectangleExt"]="${MONOGAME_MATH_NAMESPACE}RectangleExtensionsTest"
    ["MonoGameVector2Ext"]="${MONOGAME_MATH_NAMESPACE}Vector2ExtensionsTest"
    ["MonoGameVector3Ext"]="${MONOGAME_MATH_NAMESPACE}Vector3ExtensionsTest"
    ["Polygon16"]="${MATH_NAMESPACE}Polygon16Test"
    ["Rectangle"]="${MATH_NAMESPACE}RectangleTest"
    ["Vector2"]="${MATH_NAMESPACE}Vector2Test"
    ["Vector2Int"]="${MATH_NAMESPACE}Vector2IntTest"
    ["Vector3"]="${MATH_NAMESPACE}Vector3Test"
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