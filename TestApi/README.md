# TestApi

Minimal ASP.NET Core endpoint:

Run (PowerShell):

```powershell
# from repository root
$env:ASPNETCORE_URLS = "http://localhost:5000"; dotnet run --project .\TestApi\TestApi.csproj
```

Test (PowerShell using curl alias or Invoke-RestMethod):

```powershell
# with curl
curl http://localhost:5000/test
```
