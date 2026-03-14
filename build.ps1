$compiler = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if (-not (Test-Path $compiler)) {
    throw "C# compiler not found at $compiler"
}

& $compiler /nologo /target:exe /out:logomake.exe Logomake.cs /r:System.Drawing.dll

if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE"
}

Write-Host "Built logomake.exe"
