call set_params.bat


set JSONWSCMD=C:\Rootrax\RooTrax.Utilities\JsonWsStubGenerator.Cmd\bin\Debug\net9.0\JsonWsStubGenerator.Cmd.exe

set PORTAL_NAMESPACE=Buffaly.Ontology.Portal
set JSONWS=%ROOT%\%PORTAL_NAMESPACE%\wwwroot\JsonWs

set DLL=%ROOT%\%PORTAL_NAMESPACE%\bin\Debug\net9.0\ProtoScript.Extensions.dll

%JSONWSCMD% -js %DLL% * %JSONWS% Buffaly
%JSONWSCMD% -ashx %DLL% * %JSONWS%
