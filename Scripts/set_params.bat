set SITE=Buffaly

for /f "tokens=1,2,3,4 delims=/ " %%a in ('DATE /T') do set date=%%d_%%b_%%c

set DEPLOY_ROOT_DIR=c:\Deployments

set ROOT=c:\dev\ai\Ontology8
set PROJECT_NAME=Ontology
set PORTAL_NAMESPACE=Buffaly.Customer.Portal
set BUSINESS_LAYER_NAMESPACE=Buffaly.Business
set UI_LAYER_NAMESPACE=Buffaly.UI