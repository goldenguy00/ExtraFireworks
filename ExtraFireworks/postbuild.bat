REM original version https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/C%23-Programming/Networking/UNet/
REM open this in vs it'll be so much nicer

REM call with ./postbuild.bat $(ConfigurationName)
set Target=ExtraFireworks
set Output=bin\%1\netstandard2.1
set Zip=..\Thunderstore\Release.zip

REM that's it. This is meant to pretend we just built a dll like any other time except this one is networked
REM add your postbuilds in vs like it's any other project

xcopy %Output%\%Target%.dll ..\Thunderstore\ /y
xcopy %Output%\%Target%.pdb ..\Thunderstore\ /y
xcopy .\extrafireworks ..\Thunderstore\ /y
xcopy ..\Thunderstore\README.md ..\README.md /y

if exist %Zip% Del %Zip%

powershell Compress-Archive -Path '..\Thunderstore\*' -DestinationPath '%Zip%' -Force