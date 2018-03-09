$result = 0
gci -rec `
| ? { $_.Name -like "*.IntegrationTest.csproj" `
       -Or $_.Name -like "*.Test.csproj" `
     } `
| % { 
    $testArgs = "test " + $_.FullName
    Write-Host "testargs" $testArgs
    dotnet $testArgs
	
    if ($LASTEXITCODE -ne 0) {
        $result = $LASTEXITCODE
    }
  }
  
  exit $result
  