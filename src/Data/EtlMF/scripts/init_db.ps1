# PowerShell script to initialize PostgreSQL DB with schema + seed data
# Save this as scripts/init_db.ps1

param (
    [string]$User = "postgres",                       # default user
    [string]$DbName = "mutualfunds",                  # your database name
    [string]$DbHost = "localhost",                    # PostgreSQL server
    [int]$Port = 5432,                                # default port
    [string]$SchemaFile = "..\database\full_schema_details.sql"
   
)

# Path to psql.exe (update version if needed)
$psqlPath = "C:\Program Files\PostgreSQL\18\bin\psql.exe"

if (-Not (Test-Path $psqlPath)) {
    Write-Error "psql.exe not found at $psqlPath. Please update the path."
    exit 1
}

# Step 1: Check if database exists
Write-Output "🔍 Checking if database '$DbName' exists..."
$checkDbCmd = "& `"$psqlPath`" -h $DbHost -p $Port -U $User -tAc `"SELECT 1 FROM pg_database WHERE datname='$DbName';`" postgres"

$dbExists = Invoke-Expression $checkDbCmd

if ([string]::IsNullOrWhiteSpace($dbExists)) {
    Write-Output "📦 Database '$DbName' does not exist. Creating..."
    & $psqlPath -h $DbHost -p $Port -U $User -d postgres -c "CREATE DATABASE $DbName;"
} else {
    Write-Output "✅ Database '$DbName' already exists."
}

# Step 2: Apply schema
Write-Output "📑 Applying schema from $SchemaFile..."
& $psqlPath -h $DbHost -p $Port -U $User -d $DbName -f $SchemaFile



Write-Output "🎉 Database setup complete!"
