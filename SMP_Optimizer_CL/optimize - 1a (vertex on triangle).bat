@REM @echo off
set /p out=<output_dir.txt
"%~dp0SMP_Optimizer_CL.exe" %1 -l1a -o %out% -v
pause