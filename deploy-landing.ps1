# =====================================================
# Deploy script – LANDING PAGE (paybysharepay.dk)
# Deployer den statiske landing page til Azure Static Web Apps.
# Kør fra: C:\Users\Michael\source\repos\PayBySharePPay\
# Brug:    .\deploy-landing.ps1
#
# OBS: Erstat PLACEHOLDER-værdien med deployment token
#      fra din landing page Static Web App i Azure.
#      Se docs/miljoer-opsaetning.md for vejledning.
# =====================================================

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

# ── Landing page token (udfyld efter oprettelse i Azure) ─────────────────────
$landingToken = "<INDSÆT_LANDING_PAGE_DEPLOYMENT_TOKEN>"

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host " DEPLOY LANDING PAGE" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

if ($landingToken -like "*INDSÆT*") {
	Write-Host "FEJL: Udfyld landing token i deploy-landing.ps1 før du kører scriptet." -ForegroundColor Red
	Write-Host "Se docs/miljoer-opsaetning.md for vejledning." -ForegroundColor Yellow
	exit 1
}

Write-Host "Deployer Landing Page til Azure Static Web Apps..." -ForegroundColor Cyan
Set-Location "$root"
npx @azure/static-web-apps-cli deploy "./src/Landing.PayBySharePay" `
	--deployment-token $landingToken `
	--env production
if ($LASTEXITCODE -ne 0) { Write-Host "Landing page deploy fejlede!" -ForegroundColor Red; exit 1 }

Write-Host ""
Write-Host "======================================" -ForegroundColor Green
Write-Host " LANDING PAGE DEPLOY FAERDIG!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green
Write-Host "Landing page: https://paybysharepay.dk" -ForegroundColor Green
Write-Host ""
