#!/usr/bin/env bash

# exit on error.
set -e

BUILD_TYPE=$1
declare -A BUILD_TYPE_DIRECTORY
BUILD_TYPE_DIRECTORY=(
    ["Debug"]="bin/Debug/"
    ["PreRelease"]="bin/Release/"
    ["Release"]="bin/Release/"
)

declare -A BUILD_TYPE_COMMAND
BUILD_TYPE_COMMAND=(
    ["Debug"]="dotnet pack -c Debug"
    ["PreRelease"]="dotnet pack -c Release -p:PreRelease=true"
    ["Release"]="dotnet pack -c Release"
)

# Check if the build type is valid.
if [[ -z "${BUILD_TYPE_DIRECTORY[$BUILD_TYPE]}" ]]; then
    echo "[ERROR] Unkown build type: $BUILD_TYPE"
    echo "Available build types:"
    for key in "${!BUILD_TYPE_DIRECTORY[@]}"; do
        echo "  $key"
    done
    exit 1
fi

# pack using the correct command

echo "[INFO] Running: ${BUILD_TYPE_COMMAND[$BUILD_TYPE]}"
eval "${BUILD_TYPE_COMMAND[$BUILD_TYPE]}"

# == Find nupkg ==

OUTPUT_DIR=${BUILD_TYPE_DIRECTORY[$BUILD_TYPE]}

# list matching nuget packages and order from latest modified: ls bin/Debug/Howl.*.nupkg
# ls -t lists files sorted newest first
# head -n 1 picks the first file only — the newest one
NUPKG_FILE=$(ls -t "$OUTPUT_DIR"Howl.*.nupkg 2>/dev/null | head -n 1)

if [[ -z "$NUPKG_FILE" ]]; then
echo "[ERROR] No nupkg file found at ${OUTPUT_DIR}."
exit 1
fi

# == Copy nupkg ==

# loop over directories.
for D in ../../*; do

# check if the local destination folder exists
if [[ -d "$D/nuget-local/howl" ]]; then

# copy the package into the directory
cp "$NUPKG_FILE" "$D/nuget-local/howl/"

# print success message.
echo "[SUCCESS] Copied $NUPKG_FILE to $D/nuget-local/howl/"

fi

done