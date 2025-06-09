call .\set_params.bat
set LIB_ROOT=%ROOT%\lib\

xcopy /y c:\RooTrax\RooTrax.Utilities\Deploy\BasicUtilities.dll %LIB_ROOT%*.*
xcopy /y c:\RooTrax\RooTrax.Utilities\Deploy\WebAppUtilities.dll %LIB_ROOT%*.*

xcopy /y c:\RooTrax\RooTrax.Utilities\Deploy\RooTrax.Cache.dll %LIB_ROOT%*.*
xcopy /y c:\RooTrax\RooTrax.Utilities\Deploy\RooTrax.Common.dll %LIB_ROOT%*.*
xcopy /y c:\RooTrax\RooTrax.Utilities\Deploy\RooTrax.Common.DB.dll %LIB_ROOT%*.*

xcopy /y c:\RooTrax\RooTrax.Utilities\Deploy\RooTrax.VDB.DB.dll %LIB_ROOT%*.*
xcopy /y c:\RooTrax\RooTrax.Utilities\Deploy\RooTrax.VDB.UI.dll %LIB_ROOT%*.*
xcopy /y c:\RooTrax\RooTrax.Utilities\Deploy\RooTrax.VDB.dll %LIB_ROOT%*.*


xcopy /y c:\RooTrax\RooTrax.Utilities\Deploy\kScript.dll %LIB_ROOT%*.*


xcopy /y C:\dev\BuffalyNet6\Deploy\Buffaly.*.dll %LIB_ROOT%*.*

