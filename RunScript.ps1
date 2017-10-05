# Powershell script to run the both client as well as the server two consoles. it' just for windows.
Invoke-Expression 'cmd /c start powershell -Command {write-host "Starting the Server!"; dotnet run --project .\ConsoleApp\ConsoleApp.fsproj }'
Write-Host "Waiting for 3 seconds for server to start.!"
Start-Sleep -s 3 
Invoke-Expression 'cmd /c start powershell -Command {write-host "Starting the Client"; dotnet run --project .\ClientTest\ClientTest.fsproj }'
