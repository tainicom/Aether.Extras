@echo off
setlocal

cd %~dp0

SET MGFX="C:\Program Files (x86)\MSBuild\MonoGame\v3.0\Tools\2mgfx.exe"
SET XNAFX="..\..\Tools\CompileEffect\CompileEffect.exe"

@echo Build dx11
@for /f %%f IN ('dir /b *.fx') do (
    call %MGFX% %%~nf.fx ..\Resources\%%~nf.dx11.mgfxo.9 /Profile:DirectX_11
)

@echo Build ogl
@for /f %%f IN ('dir /b *.fx') do (
    call %MGFX% %%~nf.fx ..\Resources\%%~nf.ogl.mgfxo.9
)

@echo Build dx9/xna Reach
@for /f %%f IN ('dir /b *.fx') do (
    call %XNAFX% Windows Reach %%~nf.fx ..\Resources\%%~nf.xna.WinReach
)

endlocal
@pause
