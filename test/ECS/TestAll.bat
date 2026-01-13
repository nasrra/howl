@echo off
dotnet test "..\Howl.Test.csproj" --filter "FullyQualifiedName~Howl.Test.ECS."
