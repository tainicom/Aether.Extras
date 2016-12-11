@echo off
setlocal

cd %~dp0

SET MGFX="C:\Program Files (x86)\MSBuild\MonoGame\v3.0\Tools\2mgfx.exe"
SET XNAFX="CompileEffect\CompileEffect.exe"

@echo Build dx11
@for /f %%f IN ('dir /b *.fx') do (
    call %MGFX% %%~nf.fx %%~nf.dx11.mgfxo /Profile:DirectX_11
)

@echo Build ogl
@for /f %%f IN ('dir /b *.fx') do (
    call %MGFX% %%~nf.fx %%~nf.ogl.mgfxo
)

@echo Build dx9/xna Reach
@for /f %%f IN ('dir /b *.fx') do (
    call %XNAFX% Windows Reach %%~nf.fx %%~nf.xna.WinReach
)

endlocal
@pause
