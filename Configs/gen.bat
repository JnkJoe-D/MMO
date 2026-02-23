@echo off
set LUBAN_EXE=..\Tools\Luban\Luban.exe
set CONF_FILE=luban.conf

echo [Luban] 开始导出客户端配置 (使用 MiniTemplate 示例结构)...

%LUBAN_EXE% --conf %CONF_FILE% -t all -c cs-simple-json -d json -x outputCodeDir=..\Assets\GameClient\Generated\Config -x outputDataDir=..\Assets\Configs

if %ERRORLEVEL% NEQ 0 (
    echo [Luban] 导出失败！
) else (
    echo [Luban] 导出成功！
)
pause