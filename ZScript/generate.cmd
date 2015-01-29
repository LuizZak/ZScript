SET /P FILENAME=Enter the grammar file name: 
SET /P OUTPUTDIR=Enter the output directory: 

java -jar antlr-4.5-complete.jar -Dlanguage=CSharp %FILENAME% -o "%OUTPUTDIR%"
pause