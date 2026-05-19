# =====================================================
# Deploy script – PRODUKTION
# Deployer Angular, MerchantDemo og .NET API til PROD miljøet i Azure.
# Kør fra: C:\Users\Michael\source\repos\PayBySharePPay\
# Brug:    .\deploy-prod.ps1
# =====================================================

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

# ── PROD tokens & navne ───────────────────────────────────────────────────────
$frontendToken   = "acad8ed89e853e5624beb518c6d37aab861ed6958fe2affb30d085fb21820b5c07-15d77b89-25e6-4195-a4b1-7ef61f20a7c900305250750d2703"
$merchantToken   = "37c702bfe9ec68958e87eb3bdb8470a3cb3126e73910d869a94517bd97177f1107-354904cb-ccc4-4a6f-adcf-c401582b999b00300010e753db03"
$apiResourceGroup = "paybysharepay-rg"
$apiAppName       = "paybysharepay-api-win"

Write-Host ""
Write-Host "======================================" -ForegroundColor Yellow
Write-Host " DEPLOY TIL PRODUKTION" -ForegroundColor Yellow
Write-Host "======================================" -ForegroundColor Yellow
Write-Host ""

Write-Host "Trin 1/6 - Bygger Angular (production)..." -ForegroundColor Cyan
Set-Location "$root\src\Frontend.PayBySharePay"
npm ci --silent
npx ng build --configuration production
if ($LASTEXITCODE -ne 0) { Write-Host "Angular build fejlede!" -ForegroundColor Red; exit 1 }

Write-Host "Trin 2/6 - Deployer Angular til Azure Static Web Apps (PROD)..." -ForegroundColor Cyan
npx @azure/static-web-apps-cli deploy ./dist/frontend.paybysharepay `
	--deployment-token $frontendToken `
	--env production
if ($LASTEXITCODE -ne 0) { Write-Host "Frontend deploy fejlede!" -ForegroundColor Red; exit 1 }

Write-Host "Trin 3/6 - Deployer Frontend.MerchantDemo til Azure Static Web Apps (PROD)..." -ForegroundColor Cyan
Set-Location "$root"
npx @azure/static-web-apps-cli deploy "$root\src\Frontend.MerchantDemo" `
	--deployment-token $merchantToken `
	--env production
if ($LASTEXITCODE -ne 0) { Write-Host "MerchantDemo deploy fejlede!" -ForegroundColor Red; exit 1 }

Write-Host "Trin 4/6 - Bygger og publisher .NET 9 API..." -ForegroundColor Cyan
Set-Location "$root"
dotnet publish src\Api.PayBySharePay\Api.PayBySharePay.csproj `
	--configuration Release `
	--output "./publish-output"
if ($LASTEXITCODE -ne 0) { Write-Host ".NET build fejlede!" -ForegroundColor Red; exit 1 }

Write-Host "Trin 5/6 - Pakker til zip..." -ForegroundColor Cyan
Compress-Archive -Path "./publish-output/*" -DestinationPath "./publish-output.zip" -Force

Write-Host "Trin 6/6 - Deployer API til Azure App Service (PROD)..." -ForegroundColor Cyan
az webapp deploy `
	--resource-group $apiResourceGroup `
	--name $apiAppName `
	--src-path "./publish-output.zip" `
	--type zip
if ($LASTEXITCODE -ne 0) { Write-Host "API deploy fejlede!" -ForegroundColor Red; exit 1 }

Write-Host ""
Write-Host "======================================" -ForegroundColor Green
Write-Host " PROD DEPLOY FAERDIG!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green
Write-Host "Frontend:      https://icy-water-0750d2703.7.azurestaticapps.net  (paybysharepay.dk)" -ForegroundColor Green
Write-Host "MerchantDemo:  https://ashy-bay-0e753db03.7.azurestaticapps.net" -ForegroundColor Green
Write-Host "API:           https://paybysharepay-api-win.azurewebsites.net" -ForegroundColor Green
Write-Host ""
