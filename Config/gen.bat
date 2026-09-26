set WORKSPACE=..
set LUBAN_DLL=%WORKSPACE%/Tools/Luban/Luban.dll
set CONF_ROOT=.

dotnet %LUBAN_DLL% ^
    -t client ^
    -c cs-dotnet-json ^
    -d json ^
    --conf %CONF_ROOT%/luban.conf ^
    -x outputCodeDir=../Client/Assets/Scripts/Demo/Model/Client/Generage/Table ^
    -x outputDataDir=../Client/Assets/Bundles/Table

pause