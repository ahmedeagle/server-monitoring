# =========================================
# Generate Secure Secrets Script
# Creates cryptographically secure random secrets
# =========================================

Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host "Secure Secrets Generator" -ForegroundColor Cyan
Write-Host "=========================================`n" -ForegroundColor Cyan

Write-Host "Generating secure random secrets...`n" -ForegroundColor White

# Function to generate random password
function New-SecurePassword {
    param(
        [int]$Length = 32,
        [int]$NonAlphanumericChars = 8
    )
    
    Add-Type -AssemblyName 'System.Web'
    return [System.Web.Security.Membership]::GeneratePassword($Length, $NonAlphanumericChars)
}

# Function to generate alphanumeric string
function New-AlphanumericString {
    param([int]$Length = 64)
    
    $chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789'
    $result = -join ((1..$Length) | ForEach-Object { $chars[(Get-Random -Maximum $chars.Length)] })
    return $result
}

# Generate secrets
$dbPassword = New-SecurePassword -Length 32 -NonAlphanumericChars 8
$redisPassword = New-SecurePassword -Length 32 -NonAlphanumericChars 8
$jwtSecret = New-AlphanumericString -Length 64

# Display secrets
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Generated Secrets (COPY THESE NOW!)" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

Write-Host "`nDatabase Password (SQL Server):" -ForegroundColor Yellow
Write-Host $dbPassword -ForegroundColor White

Write-Host "`nRedis Password:" -ForegroundColor Yellow
Write-Host $redisPassword -ForegroundColor White

Write-Host "`nJWT Secret Key:" -ForegroundColor Yellow
Write-Host $jwtSecret -ForegroundColor White

# Create a sample .env snippet
$envSnippet = @"

# =========================================
# PRODUCTION SECRETS (Generated $((Get-Date).ToString('yyyy-MM-dd HH:mm:ss')))
# =========================================

DB_SA_PASSWORD=$dbPassword
REDIS_PASSWORD=$redisPassword
JWT_SECRET_KEY=$jwtSecret

"@

# Save to file
$secretsFile = "secrets-$(Get-Date -Format 'yyyyMMdd-HHmmss').txt"
$envSnippet | Out-File $secretsFile -Encoding UTF8

Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host "Secrets saved to: $secretsFile" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan

Write-Host "`n⚠️  IMPORTANT SECURITY NOTES:" -ForegroundColor Yellow
Write-Host "   1. Copy these secrets to your .env file NOW" -ForegroundColor White
Write-Host "   2. DELETE the $secretsFile file after copying" -ForegroundColor White
Write-Host "   3. NEVER commit secrets to version control" -ForegroundColor White
Write-Host "   4. Store production secrets in a secure password manager" -ForegroundColor White
Write-Host "   5. Rotate secrets regularly (every 90 days recommended)" -ForegroundColor White

Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host "How to Use These Secrets" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

Write-Host "`n1. Open your .env file:" -ForegroundColor White
Write-Host "   notepad .env" -ForegroundColor Gray

Write-Host "`n2. Replace these variables with the generated values:" -ForegroundColor White
Write-Host "   DB_SA_PASSWORD=<paste database password>" -ForegroundColor Gray
Write-Host "   REDIS_PASSWORD=<paste redis password>" -ForegroundColor Gray
Write-Host "   JWT_SECRET_KEY=<paste jwt secret>" -ForegroundColor Gray

Write-Host "`n3. Validate your configuration:" -ForegroundColor White
Write-Host "   .\validate-env.ps1" -ForegroundColor Gray

Write-Host "`n4. Deploy your application:" -ForegroundColor White
Write-Host "   docker-compose up -d" -ForegroundColor Gray

Write-Host "`n5. DELETE the secrets file:" -ForegroundColor White
Write-Host "   Remove-Item $secretsFile" -ForegroundColor Gray

Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host "Alternative: Direct Update" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

Write-Host "`nYou can also run this command to update .env directly:" -ForegroundColor White
Write-Host "(Only do this if .env file exists)`n" -ForegroundColor Yellow

$updateCommand = @"
`$dbPass = "$dbPassword"
`$redisPass = "$redisPassword"
`$jwtSecret = "$jwtSecret"

(Get-Content .env) -replace 'DB_SA_PASSWORD=.*', "DB_SA_PASSWORD=`$dbPass" |
    Set-Content .env -Encoding UTF8

(Get-Content .env) -replace 'REDIS_PASSWORD=.*', "REDIS_PASSWORD=`$redisPass" |
    Set-Content .env -Encoding UTF8

(Get-Content .env) -replace 'JWT_SECRET_KEY=.*', "JWT_SECRET_KEY=`$jwtSecret" |
    Set-Content .env -Encoding UTF8
"@

Write-Host $updateCommand -ForegroundColor Gray

Write-Host "`n=========================================" -ForegroundColor Green
Write-Host "Secrets Generation Complete!" -ForegroundColor Green
Write-Host "=========================================`n" -ForegroundColor Green

# Warn about the secrets file
Write-Host "⚠️  Remember to delete $secretsFile after use!`n" -ForegroundColor Red

exit 0
