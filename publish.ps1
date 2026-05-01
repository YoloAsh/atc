Write-Host "Publishing for win-x64..."
dotnet publish -c Release -r win-x64

Write-Host "Publishing for win-x86..."
dotnet publish -c Release -r win-x86

Write-Host "Publishing for win-arm64..."
dotnet publish -c Release -r win-arm64

Write-Host "Done! Executables are located in bin\Release\net10.0-windows\<rid>\publish"
