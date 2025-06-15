call set_params.bat
call add_jststubs.bat

set PROJECT=Buffaly.SemanticDB.Portal
set SOURCE_DIR=%ROOT%\%PROJECT%
set DEPLOY_DIR=c:\Deployments\%PROJECT%_%date%\
set FILE_NAME="%DEPLOY_DIR%%PROJECT%_%date%.zip"


del /s /q "%DEPLOY_DIR%*.*"
rmdir /s /q "%DEPLOY_DIR%"

mkdir "%DEPLOY_DIR%"


set FLAG=%1

if "%FLAG%"=="kscripts" GOTO kScripts


dotnet publish -c Release %SOURCE_DIR% -o %DEPLOY_DIR%Site 

:kScripts

xcopy /y /e "%SOURCE_DIR%\wwwroot\kScripts\*.*" "%DEPLOY_DIR%Site\kScripts\"

ren "%DEPLOY_DIR%Site\appsettings.json"  "appsettings.json.back"


:zip 

cd "%DEPLOY_DIR%"

if "%FLAG%"=="lite" GOTO lite

 "c:\Program Files\7-Zip\7z.exe" a -tzip "%FILE_NAME%" ".\*" -r

 if "%FLAG%"=="" GOTO end

 :lite

del /q /s "%DEPLOY_DIR%Site\wwwroot\assets\*.*"
del /q /s "%DEPLOY_DIR%Site\wwwroot\js\*.*"
del /q /s "%DEPLOY_DIR%Site\wwwroot\images\*.*"
del /q /s "%DEPLOY_DIR%Site\wwwroot\css\*.*"

"c:\program files (x86)\GnuWin32\bin\zip.exe" -q -r "%SITE%_%date%_portal_lite.zip" "*.*"

:end

c:


cd %ROOT%\Scripts