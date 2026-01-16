@echo off
setlocal

dotnet restore || exit /b 1
dotnet pack -c Debug || exit /b 1

REM ---- Find nupkg ----
for %%F in ("bin\Debug\Howl.*.nupkg") do (
    set "NUPKG_FILE=%%F"
)

if not defined NUPKG_FILE (
    echo [ERROR] No nupkg file found
    exit /b 1
)

for /d %%D in ("..\..\*") do (
    if exist "%%D\nuget-local\" (
        echo [SUCCESS] Copying %NUPKG_FILE% to %%D\nuget-local\
        copy "%NUPKG_FILE%" "%%D\nuget-local\" >nul
    )
)

echo [DONE] %NUPKG_FILE%
