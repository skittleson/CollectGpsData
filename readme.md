# CollectGpsData

Should have a deploy script like below
https://github.com/skittleson/GcodeController/blob/master/deploy.sh

`dotnet publish -r linux-x64 -p:PublishSingleFile=true`

## Tool

https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-create 
`dotnet pack`
`dotnet tool install --global --add-source ./nupkg collectgps`