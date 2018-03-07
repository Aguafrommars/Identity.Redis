#!/bin/bash 2> nul

echo off

set result=0
tools\dotCover\dotCover.exe cover /TargetExecutable="C:\Program Files\dotnet\dotnet.exe" /TargetArguments="test" /TargetWorkingDir="test\Aguacongas.Redis.Test" /Output="coverage\Aguacongas.Redis.Test.snapshot" /Filters="+:Aguacongas.Redis"
if %errorlevel% neq 0 set result=%errorlevel%
tools\dotCover\dotCover.exe cover /TargetExecutable="C:\Program Files\dotnet\dotnet.exe" /TargetArguments="test" /TargetWorkingDir="test\Aguacongas.Identity.Redis.Test" /Output="coverage\Aguacongas.Identity.Redis.Test.snapshot" /Filters="+:Aguacongas.Identity.Redis"
if %errorlevel% neq 0 set result=%errorlevel%

tools\dotCover\dotCover.exe cover /TargetExecutable="C:\Program Files\dotnet\dotnet.exe" /TargetArguments="test" /TargetWorkingDir="test\Aguacongas.Identity.Redis.IntegrationTest" /Output="coverage\Aguacongas.Identity.Redis.IntegrationTest.snapshot" /Filters="+:Aguacongas.Identity.Redis"

tools\dotCover\dotCover.exe merge /Source="coverage\Aguacongas.Identity.Redis.Test.snapshot;coverage\Aguacongas.Identity.Redis.IntegrationTest.snapshot;coverage\Aguacongas.Redis.Test.snapshot" /Output="coverage\coverage.snapshot"
tools\dotCover\dotCover.exe report /Source="coverage\coverage.snapshot" /Output="coverage\coverage.xml" /ReportType="DetailedXML"
tools\dotCover\dotCover.exe report /Source="coverage\coverage.snapshot" /Output="coverage\docs\index.html" /ReportType="HTML"

tools\ReportGenerator\ReportGenerator.exe "-reports:coverage\coverage.xml" "-targetdir:coverage\docs" "-reporttypes:Badges"

exit /b %result%