$base = $env:UGTSSHARE;
$bin = Join-Path $base "\dev\UGTS\Encoder\bin\debug";
$doc = Join-Path $base "\dev\UGTS\Encoder\doc";
$dst = Join-Path $base "\staging\Encoder";

copy "$bin\Encoder.exe" $dst;
copy "$bin\UGTS.dll" $dst;
copy "$doc\ChangeLog.xlsm" $dst;
