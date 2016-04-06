
:create

call :delete
sqllocaldb c "templateTests"

exit /b 0

:delete
sqllocaldb p "templateTests" 2> nul
sqllocaldb d "templateTests" 2> nul
del %userprofile%\aspnet-StarterWeb*.* 2> nul

exit /b 0