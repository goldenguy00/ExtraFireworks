REM call with ./postbuild.bat $(TargetDir)
set Target=%1ExtraFireworks
set Store=..\Thunderstore\
set Zip=..\Thunderstore\Release.zip
echo %Target%
REM copy stuff into the thunderstore folder
xcopy %Target%.dll      %Store% /y
xcopy %Target%.pdb      %Store% /y
xcopy .\extrafireworks  %Store% /y
xcopy ..\README.md      %Store% /y
xcopy ..\manifest.json  %Store% /y

REM remove old zip, replace with new one

if exist %Zip% Del %Zip%

powershell Compress-Archive -Path '%Store%*' -DestinationPath '%Zip%' -Force