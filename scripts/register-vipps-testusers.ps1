#!/usr/bin/env pwsh
# Registrerer Vipps testbrugere i PayNSync-databasen.
# Kør EFTER API'et er startet i Visual Studio (port 5071).

$API = "http://localhost:5071"

function Register($name, $email, $phone, $password = "Test1234!") {
	$body = @{ name = $name; email = $email; phone = $phone; password = $password } | ConvertTo-Json
	try {
		$r = Invoke-RestMethod -Method Post -Uri "$API/api/auth/register" `
			-Body $body -ContentType "application/json" -ErrorAction Stop
		Write-Host "✅ Oprettet: $name (id=$($r.participantId))"
		return $r
	} catch {
		$msg = $_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue
		Write-Host "⚠️  $name — $($msg.message ?? $_.Exception.Message)"
	}
}

Write-Host ""
Write-Host "=== Registrerer Vipps testbrugere ==="
Write-Host ""

# Host — bruger telefon 635 50 321 (testbruger 1)
$host1 = Register "Test Host"        "host@vippstest.dk"    "+4563550321"

# Deltager 1 — telefon 662 86 865
$p1    = Register "Test Deltager 1"  "delta1@vippstest.dk"  "+4566286865"

# Deltager 2 — telefon 201 67 183
$p2    = Register "Test Deltager 2"  "delta2@vippstest.dk"  "+4520167183"

# Deltager 3 — telefon 242 11 628
$p3    = Register "Test Deltager 3"  "delta3@vippstest.dk"  "+4524211628"

Write-Host ""
Write-Host "=== Login som Host og hent JWT ==="
Write-Host ""

$login = Invoke-RestMethod -Method Post -Uri "$API/api/auth/login" `
	-Body (@{ email = "host@vippstest.dk"; password = "Test1234!" } | ConvertTo-Json) `
	-ContentType "application/json"

Write-Host "✅ Host JWT: $($login.token.Substring(0,40))..."
Write-Host ""
Write-Host "Brug dette token i Swagger (Authorize-knappen):"
Write-Host $login.token
