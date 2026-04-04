#!/usr/bin/env bash

set -e

PROJECT_PATH="Howl.Test.csproj"

NAMESPACE="Howl.Test."
MATH_NAMESPACE="${NAMESPACE}Math."
SHAPES_NAMESPACE="${MATH_NAMESPACE}Shapes."
ECS_NAMESPACE="${NAMESPACE}ECS."
PHYSICS_NAMESPACE="${NAMESPACE}Physics."
GRAPHICS_NAMESPACE="${NAMESPACE}Graphics."
DATA_STRUCTURES_NAMESPACE="${NAMESPACE}DataStructures."
ALGORITHMS_NAMESPACE="${NAMESPACE}Algorithms."
SORTING_NAMESPACE="${ALGORITHMS_NAMESPACE}Sorting."
COLLECTIONS_NAMESPACE="${NAMESPACE}Collections."

VENDORS_NAMESPACE="${NAMESPACE}Vendors."

MONOGAME_NAMESPACE="${VENDORS_NAMESPACE}MonoGame."
MONOGAME_MATH_NAMESPACE="${MONOGAME_NAMESPACE}Math."
MONOGAME_SHAPES_NAMESPACE="${MONOGAME_MATH_NAMESPACE}Shapes."
MONOGAME_GRAPHICS_NAMESPACE="${MONOGAME_NAMESPACE}Graphics."

TEST_NAME=$1

# Declare an associative array
declare -A TEST_MAP
TEST_MAP=(
    ["AABB"]="${SHAPES_NAMESPACE}AABBTest"
    ["BoundingVolumeHierarchy"]="${DATA_STRUCTURES_NAMESPACE}Bvh.BoundingVolumeHierarchyTest"
    ["ComponentRegistry"]="${ECS_NAMESPACE}ComponentRegistryTest"
    ["ComponentType"]="${ECS_NAMESPACE}ComponentTypeTest"
    ["Circle"]="${SHAPES_NAMESPACE}CircleTest"
    ["DataStructuresNamespace"]="${DATA_STRUCTURES_NAMESPACE}"
    ["ECSNamespace"]="${ECS_NAMESPACE}"
    ["GenId"]="${ECS_NAMESPACE}Test_GenId"
    ["GenIndex"]="${ECS_NAMESPACE}GenIndexTest"
    ["GenIndexArray"]="${ECS_NAMESPACE}Test_GenIndexArray"
    ["GuiText16"]="${GRAPHICS_NAMESPACE}Text.GuiText16Test"
    ["GuiText4096"]="${GRAPHICS_NAMESPACE}Text.GuiText4096Test"
    ["LeafBufferSlice"]="${DATA_STRUCTURES_NAMESPACE}Bvh.LeafBufferSliceTest"
    ["MathNamespace"]="${MATH_NAMESPACE}"
    ["Math"]="${MATH_NAMESPACE}MathTest"
    ["MonoGameColourExt"]="${MONOGAME_GRAPHICS_NAMESPACE}ColourExtensionsTest"
    ["MonoGameRectangleExt"]="${MONOGAME_SHAPES_NAMESPACE}RectangleExtensionsTest"
    ["MonoGameVector2Ext"]="${MONOGAME_MATH_NAMESPACE}Vector2ExtensionsTest"
    ["MonoGameVector3Ext"]="${MONOGAME_MATH_NAMESPACE}Vector3ExtensionsTest"
    ["MortonCode"]="${ALGORITHMS_NAMESPACE}MortonCode"
    ["PhysicsSystem"]="${PHYSICS_NAMESPACE}PhysicsSystemTest"
    ["Polygon16"]="${SHAPES_NAMESPACE}Polygon16Test"
    ["PolygonRectangle"]="${SHAPES_NAMESPACE}PolygonRectangleTest"
    ["RadixSort"]="${SORTING_NAMESPACE}RadixSortTest"
    ["RadixSortF"]="${SORTING_NAMESPACE}RadixSortFTest"
    ["RadixSortBuffer"]="${SORTING_NAMESPACE}RadixSortBuffer"
    ["Rectangle"]="${SHAPES_NAMESPACE}RectangleTest"
    ["RigidBody"]="${PHYSICS_NAMESPACE}RigidBodyTest"
    ["SAT"]="${SHAPES_NAMESPACE}SATTest"
    ["Soa_Aabb"]="${SHAPES_NAMESPACE}Soa_AabbTest"
    ["Soa_AabbSlice"]="${SHAPES_NAMESPACE}Soa_AabbSliceTest"
    ["Soa_Branch"]="${DATA_STRUCTURES_NAMESPACE}Bvh.Soa_BranchTest"
    ["Soa_GenIndex"]="${ECS_NAMESPACE}Soa_GenIndexTest"
    ["Soa_GenIndexSlice"]="${ECS_NAMESPACE}Soa_GenIndexSliceTest"
    ["Soa_Leaf"]="${DATA_STRUCTURES_NAMESPACE}Bvh.Soa_LeafTest"
    ["Soa_QueryResult"]="${DATA_STRUCTURES_NAMESPACE}Bvh.Soa_QueryResultTest"
    ["Soa_SpatialPair"]="${DATA_STRUCTURES_NAMESPACE}Bvh.Soa_SpatialPairTest"
    ["Soa_Transform"]="${MATH_NAMESPACE}Soa_TransformTest"
    ["Soa_Vector2"]="${MATH_NAMESPACE}Soa_Vector2Test"
    ["Soa_Vector2Slice"]="${MATH_NAMESPACE}Soa_Vector2SliceTest"
    ["StackArray"]="${COLLECTIONS_NAMESPACE}StackArray_Test"
    ["Swap"]="${ALGORITHMS_NAMESPACE}SwapTest"
    ["SwapBackArray"]="${COLLECTIONS_NAMESPACE}SwapBackArray_Test"
    ["Text16"]="${GRAPHICS_NAMESPACE}Text.Text16Test"
    ["Text4096"]="${GRAPHICS_NAMESPACE}Text.Text4096Test"
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
    dotnet test "$PROJECT_PATH" -c Debug --filter "FullyQualifiedName~${TEST_MAP[$TEST_NAME]}"
else
    echo "Unknown test: $TEST_NAME"
    echo "Please run the script with no test name to see available tests."
    exit 1
fi