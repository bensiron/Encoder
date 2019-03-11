$base = $env:UGTSSHARE;
$bin = Join-Path $base "\dev\UGTS\Encoder\src\Encoder\bin\debug";
$dst = Join-Path $base "\run\Encoder";

copy "$bin\Encoder.exe" $dst;
copy "$bin\UGTS.WPF.dll" $dst;
copy "$bin\log4net.dll" $dst;
copy "$bin\UGTS.Log4NetCleaner.dll" $dst;