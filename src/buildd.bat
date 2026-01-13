@echo off
dotnet build -c Debug || exit /b
REM Loop through all subfolders one level up.
for /d %%D in ("..\..\*") do (
    REM Check if the folder contains a 'nuget-local' subfolder.
    echo [STATUS] Checking folder: %%D
    if exist "%%D\nuget-local\" (
        echo [SUCCESS] nupkg to: %%D\nuget-local\
        copy "bin\Release\Howl.1.0.0.nupkg" "%%D\nuget-local\"
    ) else (
        @REM echo [FAIL] nuget-local folder not found in %%D
    )
)
