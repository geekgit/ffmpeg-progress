function get-csc
{
	$PathAL=New-Object System.Collections.ArrayList
	#VS2015
	$PathAL.Add("C:\Program Files (x86)\MSBuild\14.0\Bin\amd64\csc.exe") | Out-Null
	$PathAL.Add("C:\Program Files (x86)\MSBuild\14.0\Bin\csc.exe") | Out-Null
	#
	foreach($path in $PathAL)
	{
		$flag=(Test-Path $path)
		return $path
	}
	return "N/A"
}
$csc=(get-csc)
if($csc -ne "N/A")
{
	& $csc /define:DEBUG /optimize /out:ffmpeg-progress.exe Program.cs
}
