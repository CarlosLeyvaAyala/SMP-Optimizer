@echo off

CD /D "%~dp0

:: Read output file/folder from "output_dir.txt"
set /p out=<output_dir.txt 

:: Run program
"%~dp0SMP_Optimizer_CL.exe" %1 -l1b -o "%out%" -bat

pause