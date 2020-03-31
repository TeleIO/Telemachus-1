rd /s /q  "%1..\publish\GameData"
rd /s /q "%1..\ksp-telemachus-dev\GameData\Telemachus"

xcopy "%2Servers.dll" "%1..\publish\GameData\Telemachus\Plugins\" /e /y /i /r
xcopy "%2Telemachus.dll" "%1..\publish\GameData\Telemachus\Plugins\" /e /y /i /r
xcopy "%2websocket-sharp.dll" "%1..\publish\GameData\Telemachus\Plugins\" /e /y /i /r

xcopy "%1..\Parts\*" "%1..\publish\GameData\Telemachus\Parts\"  /e /y /i /r
xcopy "%1..\WebPages\WebPages\src\*" "%1..\publish\GameData\Telemachus\Plugins\PluginData\Telemachus\" /e /y /i /r
xcopy "%1..\licences\*" "%1..\publish\GameData\Telemachus\" /e /y /i /r
copy "%1..\readme.md" "%1..\publish\GameData\Telemachus\"

xcopy "%1..\WebPages\WebPagesTest\src\*" "%1..\ksp-telemachus-dev\GameData\Telemachus\Plugins\PluginData\Telemachus\test" /e /y /i /r
xcopy "%1..\publish\GameData\*"  "%1..\ksp-telemachus-dev\GameData\" /e /y /i /r
powershell -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; $assets = Invoke-WebRequest -Uri https://api.github.com/repos/TeleIO/houston/releases/latest | ConvertFrom-Json; $assets = $assets.assets; iwr $assets.browser_download_url -OutFile %2h.zip; Expand-Archive -Path %2h.zip -DestinationPath %2houston -Force;"


mkdir "%1..\publish\GameData\Telemachus\Plugins\PluginData\Telemachus\houston"
xcopy "%2houston\*" "%1..\publish\GameData\Telemachus\Plugins\PluginData\Telemachus\houston"  /e /y /i /r
powershell -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; iwr https://github.com/TeleIO/mkon/archive/master.zip -OutFile %2mkon.zip; Expand-Archive -Path %2mkon.zip -DestinationPath %2mkon -Force;"      
mkdir "%1..\publish\GameData\Telemachus\Plugins\PluginData\Telemachus\mkon"
xcopy "%2mkon\mkon-master\*" "%1..\publish\GameData\Telemachus\Plugins\PluginData\Telemachus\mkon"  /e /y /i /r
dir "%1..\publish\GameData\Telemachus\Plugins\PluginData\Telemachus\"