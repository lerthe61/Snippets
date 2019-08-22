echo off
set JAVA=C:\Java\jdk1.8.0\bin\java.exe
set POSTJAR=C:\Java\solr-6.4.1\example\exampledocs\post.jar
set COLLECTION_URL=%1
set SOURCE_FILE=%2

echo Posting to Solr...
%JAVA% -Durl="%COLLECTION_URL%" -jar "%POSTJAR%" "%SOURCE_FILE%"