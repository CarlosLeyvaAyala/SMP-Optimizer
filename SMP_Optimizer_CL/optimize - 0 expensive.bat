@echo off

:: Read output file/folder from "output_dir.txt"
set /p out=<output_dir.txt 

:: Run program
"%~dp0SMP_Optimizer_CL.exe" %1 -l0 -o %out% -bat

pause