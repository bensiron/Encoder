$base = $env:UGTSSHARE;
$bin = Join-Path $base "\dev\UGTS\Encoder\src\Encoder\bin\debug";
$dst = Join-Path $base "\run\Encoder";

copy "$bin\Encoder.exe" $dst;
copy "$bin\UGTS.WPF.dll" $dst;
