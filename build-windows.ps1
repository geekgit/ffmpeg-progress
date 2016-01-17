function get-csc
{
	$PathAL=New-Object System.Collections.ArrayList
	#System
	$PathAL.Add("$env:SystemRoot\Microsoft.NET\Framework64\v4.0.30319\csc.exe") | Out-Null
	$PathAL.Add("$env:SystemRoot\Framework\v4.0.30319\csc.exe") | Out-Null
	#VS2015
    $PathAL.Add("$env:ProgramFiles (x86)\MSBuild\14.0\Bin\amd64\csc.exe") | Out-Null
	$PathAL.Add("$env:ProgramFiles (x86)\MSBuild\14.0\Bin\csc.exe") | Out-Null
	$PathAL.Add("$env:ProgramFiles\MSBuild\14.0\Bin\amd64\csc.exe") | Out-Null
	$PathAL.Add("$env:ProgramFiles\MSBuild\14.0\Bin\csc.exe") | Out-Null
	#VS2013
	$PathAL.Add("$env:ProgramFiles (x86)\MSBuild\12.0\Bin\amd64\csc.exe") | Out-Null
	$PathAL.Add("$env:ProgramFiles (x86)\MSBuild\12.0\Bin\csc.exe") | Out-Null
	$PathAL.Add("$env:ProgramFiles\MSBuild\12.0\Bin\amd64\csc.exe") | Out-Null
	$PathAL.Add("$env:ProgramFiles\MSBuild\12.0\Bin\csc.exe") | Out-Null
	#
	foreach($path in $PathAL)
	{
		$flag=(Test-Path $path)
        if($flag)
		{
			return $path
		}
	}
	return "N/A"
}
$csc=(get-csc)
if($csc -ne "N/A")
{
	& $csc /define:DEBUG /optimize /out:ffmpeg-progress.exe Program.cs
}
