$ErrorActionPreference = "Stop"

dotnet build .\src\ILLightenComparer.Benchmarks\ILLightenComparer.Benchmarks.csproj -c release -o .\publish

.\publish\ILLightenComparer.Benchmarks.exe
