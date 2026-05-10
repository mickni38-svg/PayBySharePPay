# =====================================================
# Deploy script – bygger Angular + .NET og deployer til Azure
# Kør fra: C:\Users\Michael\source\repos\PayBySharePPay\
# Brug:    .\deploy-azure.ps1
# =====================================================

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

Write-Host "Trin 1/5 - Bygger Angular (production)..." -ForegroundColor Cyan
Set-Location "$root\src\Frontend.PayBySharePay"
npm ci
npx ng build --configuration production
if ($LASTEXITCODE -ne 0) { Write-Host "Angular build fejlede!" -ForegroundColor Red; exit 1 }

Write-Host "Trin 2/5 - Deployer Angular til Azure Static Web Apps..." -ForegroundColor Cyan
npx @azure/static-web-apps-cli deploy ./dist/frontend.paybysharepay `
    --deployment-token "acad8ed89e853e5624beb518c6d37aab861ed6958fe2affb30d085fb21820b5c07-15d77b89-25e6-4195-a4b1-7ef61f20a7c900305250750d2703" `
    --env production

if ($LASTEXITCODE -ne 0) { Write-Host "Frontend deploy fejlede!" -ForegroundColor Red; exit 1 }

Write-Host "Trin 3/5 - Bygger og publisher .NET 9 API..." -ForegroundColor Cyan
Set-Location "$root"
dotnet publish src\Api.PayBySharePay\Api.PayBySharePay.csproj `
    --configuration Release `
    --runtime linux-x64 `
    --self-contained false `
    --output "./publish-output"
if ($LASTEXITCODE -ne 0) { Write-Host ".NET build fejlede!" -ForegroundColor Red; exit 1 }

Write-Host "Trin 4/5 - Pakker til zip..." -ForegroundColor Cyan
Compress-Archive -Path "./publish-output/*" -DestinationPath "./publish-output.zip" -Force

Write-Host "Trin 5/5 - Deployer API til Azure App Service..." -ForegroundColor Cyan
az webapp deploy `
    --resource-group paybysharepay-rg `
    --name paybysharepay-api-win `
    --src-path "./publish-output.zip" `
    --type zip
if ($LASTEXITCODE -ne 0) { Write-Host "API deploy fejlede!" -ForegroundColor Red; exit 1 }

Write-Host ""
Write-Host "Deploy faerdig!" -ForegroundColor Green
Write-Host "Frontend: https://icy-water-0750d2703.7.azurestaticapps.net" -ForegroundColor Green
Write-Host "API:      https://paybysharepay-api-win.azurewebsites.net" -ForegroundColor Green
