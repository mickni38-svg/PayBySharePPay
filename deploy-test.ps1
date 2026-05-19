# =====================================================
# Deploy script – TEST MILJØ
# Deployer Angular, MerchantDemo og .NET API til TEST miljøet i Azure.
# Kør fra: C:\Users\Michael\source\repos\PayBySharePPay\
# Brug:    .\deploy-test.ps1
#
# OBS: Erstat PLACEHOLDER-værdierne med de rigtige tokens/navne
#      fra dit nye Azure TEST-miljø. Se docs/miljoer-opsaetning.md.
# =====================================================

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

# ── TEST tokens & navne (udfyld disse efter oprettelse i Azure) ───────────────
$frontendToken    = "<INDSÆT_FRONTEND_TEST_DEPLOYMENT_TOKEN>"
$merchantToken    = "<INDSÆT_MERCHANT_TEST_DEPLOYMENT_TOKEN>"
$apiResourceGroup = "paybysharepay-test-rg"      # ret til dit test resource group navn
$apiAppName       = "paybysharepay-api-test"      # ret til dit test App Service navn

Write-Host ""
Write-Host "======================================" -ForegroundColor Magenta
Write-Host " DEPLOY TIL TEST MILJØ" -ForegroundColor Magenta
Write-Host "======================================" -ForegroundColor Magenta
Write-Host ""

# Validér at placeholders er udfyldt
if ($frontendToken -like "*INDSÆT*" -or $merchantToken -like "*INDSÆT*") {
	Write-Host "FEJL: Udfyld tokens i deploy-test.ps1 før du kører scriptet." -ForegroundColor Red
	Write-Host "Se docs/miljoer-opsaetning.md for vejledning." -ForegroundColor Yellow
	exit 1
}

Write-Host "Trin 1/6 - Bygger Angular (production)..." -ForegroundColor Cyan
Set-Location "$root\src\Frontend.PayBySharePay"
npm ci --silent
npx ng build --configuration production
if ($LASTEXITCODE -ne 0) { Write-Host "Angular build fejlede!" -ForegroundColor Red; exit 1 }

Write-Host "Trin 2/6 - Deployer Angular til Azure Static Web Apps (TEST)..." -ForegroundColor Cyan
npx @azure/static-web-apps-cli deploy ./dist/frontend.paybysharepay `
	--deployment-token $frontendToken `
	--env production
if ($LASTEXITCODE -ne 0) { Write-Host "Frontend deploy fejlede!" -ForegroundColor Red; exit 1 }

Write-Host "Trin 3/6 - Deployer Frontend.MerchantDemo til Azure Static Web Apps (TEST)..." -ForegroundColor Cyan
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

Write-Host "Trin 6/6 - Deployer API til Azure App Service (TEST)..." -ForegroundColor Cyan
az webapp deploy `
	--resource-group $apiResourceGroup `
	--name $apiAppName `
	--src-path "./publish-output.zip" `
	--type zip
if ($LASTEXITCODE -ne 0) { Write-Host "API deploy fejlede!" -ForegroundColor Red; exit 1 }

Write-Host ""
Write-Host "======================================" -ForegroundColor Green
Write-Host " TEST DEPLOY FAERDIG!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green
Write-Host "Frontend (TEST):      https://<din-test-swa-url>.azurestaticapps.net" -ForegroundColor Green
Write-Host "MerchantDemo (TEST):  https://<din-test-merchant-swa-url>.azurestaticapps.net" -ForegroundColor Green
Write-Host "API (TEST):           https://$apiAppName.azurewebsites.net" -ForegroundColor Green
Write-Host ""
