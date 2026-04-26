#!/usr/bin/env bash

set -e

PROJECT_PATH="Howl.Test.csproj"

NAMESPACE="Howl.Test."
MATH_NAMESPACE="${NAMESPACE}Math."
SHAPES_NAMESPACE="${MATH_NAMESPACE}Shapes."
ECS_NAMESPACE="${NAMESPACE}Ecs."
PHYSICS_NAMESPACE="${NAMESPACE}Physics."
GRAPHICS_NAMESPACE="${NAMESPACE}Graphics."
DATA_STRUCTURES_NAMESPACE="${NAMESPACE}DataStructures."
ALGORITHMS_NAMESPACE="${NAMESPACE}Algorithms."
SORTING_NAMESPACE="${ALGORITHMS_NAMESPACE}Sorting."
COLLECTIONS_NAMESPACE="${NAMESPACE}Collections."
IO_NAMESPACE="${NAMESPACE}Io."
TEXT_NAMESPACE="${NAMESPACE}Text."

VENDORS_NAMESPACE="${NAMESPACE}Vendors."

MONOGAME_NAMESPACE="${VENDORS_NAMESPACE}MonoGame."
MONOGAME_MATH_NAMESPACE="${MONOGAME_NAMESPACE}Math."
MONOGAME_SHAPES_NAMESPACE="${MONOGAME_MATH_NAMESPACE}Shapes."
MONOGAME_GRAPHICS_NAMESPACE="${MONOGAME_NAMESPACE}Graphics."

FONTSTASHSHARP_NAMESPACE="${MONOGAME_NAMESPACE}FontStashSharp."

LEVEL_MANAGEMENT_NAMESPACE="${NAMESPACE}LevelManagement."
LDTK_NAMESPACE="${LEVEL_MANAGEMENT_NAMESPACE}Ldtk."

TEST_NAME=$1

# Declare an associative array
declare -A TEST_MAP
TEST_MAP=(
    ["AABB"]="${SHAPES_NAMESPACE}AABBTest"
    ["BoundingVolumeHierarchy"]="${DATA_STRUCTURES_NAMESPACE}Bvh.Test_BoundingVolumeHierarchy"
    ["ComponentArray"]="${ECS_NAMESPACE}Test_ComponentArray"
    ["ComponentRegistry"]="${ECS_NAMESPACE}Test_ComponentRegistry"
    ["ComponentType"]="${ECS_NAMESPACE}Test_ComponentType"
    ["Circle"]="${SHAPES_NAMESPACE}CircleTest"
    ["DataStructuresNamespace"]="${DATA_STRUCTURES_NAMESPACE}"
    ["ECSNamespace"]="${ECS_NAMESPACE}"
    ["EntityRegistry"]="${ECS_NAMESPACE}Test_EntityRegistry"
    ["FsSoa_Vector2"]="${MATH_NAMESPACE}Test_FsSoa_Vector2"
    ["FontManager"]="${FONTSTASHSHARP_NAMESPACE}Test_FontManager"
    ["DopeVector"]="${COLLECTIONS_NAMESPACE}Test_DopeVector"
    ["GenId"]="${ECS_NAMESPACE}Test_GenId"
    ["GuiText16"]="${GRAPHICS_NAMESPACE}Text.GuiText16Test"
    ["GuiText4096"]="${GRAPHICS_NAMESPACE}Text.GuiText4096Test"
    ["LdtkParser"]="${LDTK_NAMESPACE}Test_LdtkParser"
    ["LeafBufferSlice"]="${DATA_STRUCTURES_NAMESPACE}Bvh.LeafBufferSliceTest"
    ["LevelManager"]="${LDTK_NAMESPACE}Test_LevelManager"
    ["MathNamespace"]="${MATH_NAMESPACE}"
    ["Math"]="${MATH_NAMESPACE}MathTest"
    ["MonoGameColourExt"]="${MONOGAME_GRAPHICS_NAMESPACE}ColourExtensionsTest"
    ["MonoGameRectangleExt"]="${MONOGAME_SHAPES_NAMESPACE}Test_RectangleExtensions"
    ["MonoGameVector2Ext"]="${MONOGAME_MATH_NAMESPACE}Test_Vector2Extensions"
    ["MonoGameVector3Ext"]="${MONOGAME_MATH_NAMESPACE}Test_Vector3Extensions"
    ["MortonCode"]="${ALGORITHMS_NAMESPACE}MortonCode"
    ["PathUtils"]="${IO_NAMESPACE}Test_PathUtils"
    ["Polygon16"]="${SHAPES_NAMESPACE}Polygon16Test"
    ["PolygonRectangle"]="${SHAPES_NAMESPACE}PolygonRectangleTest"
    ["RadixSort"]="${SORTING_NAMESPACE}RadixSortTest"
    ["RadixSortF"]="${SORTING_NAMESPACE}RadixSortFTest"
    ["RadixSortBuffer"]="${SORTING_NAMESPACE}RadixSortBuffer"
    ["Rectangle"]="${SHAPES_NAMESPACE}RectangleTest"
    ["SAT"]="${SHAPES_NAMESPACE}SATTest"
    ["Soa_Aabb"]="${SHAPES_NAMESPACE}Soa_AabbTest"
    ["Soa_AabbSlice"]="${SHAPES_NAMESPACE}Soa_AabbSliceTest"
    ["Soa_Branch"]="${DATA_STRUCTURES_NAMESPACE}Bvh.Test_Soa_Branch"
    ["Soa_Leaf"]="${DATA_STRUCTURES_NAMESPACE}Bvh.Test_Soa_Leaf"
    ["Soa_Overlap"]="${DATA_STRUCTURES_NAMESPACE}Bvh.Test_Soa_Overlap"
    ["Soa_PhysicsMaterial"]="${PHYSICS_NAMESPACE}Test_Soa_PhysicsMaterial"
    ["Soa_QueryResult"]="${DATA_STRUCTURES_NAMESPACE}Bvh.Test_Soa_QueryResult"
    ["Soa_Transform"]="${MATH_NAMESPACE}Test_Soa_Transform"
    ["Soa_Vector2"]="${MATH_NAMESPACE}Test_Soa_Vector2"
    ["Soa_Vector2Slice"]="${MATH_NAMESPACE}Soa_Vector2SliceTest"
    ["StackArray"]="${COLLECTIONS_NAMESPACE}Test_StackArray"
    ["StringAllocatorState"]="${TEXT_NAMESPACE}Test_StringAllocatorState"
    ["StringAllocator"]="${TEXT_NAMESPACE}Test_StringAllocator"
    ["StringRegistry"]="${TEXT_NAMESPACE}Test_StringRegistry"
    ["StringRegistryState"]="${TEXT_NAMESPACE}Test_StringRegistryState"
    ["Swap"]="${ALGORITHMS_NAMESPACE}SwapTest"
    ["SwapBackArray"]="${COLLECTIONS_NAMESPACE}Test_SwapBackArray"
    ["TeloPhysics"]="${PHYSICS_NAMESPACE}Telo.Test_TeloPhysics"
    ["TeloPhysicsState"]="${PHYSICS_NAMESPACE}Telo.Test_TeloPhysicsState"
    ["Text16"]="${GRAPHICS_NAMESPACE}Text.Text16Test"
    ["Text4096"]="${GRAPHICS_NAMESPACE}Text.Text4096Test"
    ["TextureManager"]="${MONOGAME_NAMESPACE}Test_TextureManager"
    ["Transform"]="${MATH_NAMESPACE}TransformTest"
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