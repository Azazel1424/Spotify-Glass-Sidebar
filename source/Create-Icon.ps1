Add-Type -AssemblyName System.Drawing
$bitmap = New-Object System.Drawing.Bitmap 64,64
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.Clear([System.Drawing.Color]::Transparent)
$brush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(48,235,142))
$graphics.FillEllipse($brush,2,2,60,60)
$pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(6,44,25)),4
$pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$graphics.DrawBezier($pen,15,24,26,19,40,20,50,26)
$graphics.DrawBezier($pen,18,34,27,29,39,30,47,35)
$graphics.DrawBezier($pen,22,43,29,39,37,40,43,44)
$buffer = New-Object System.IO.MemoryStream
$bitmap.Save($buffer,[System.Drawing.Imaging.ImageFormat]::Png)
$bytes = $buffer.ToArray()
$file = [System.IO.File]::Create((Join-Path $PSScriptRoot 'app.ico'))
$writer = New-Object System.IO.BinaryWriter $file
$writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]1)
$writer.Write([byte]64); $writer.Write([byte]64); $writer.Write([byte]0); $writer.Write([byte]0)
$writer.Write([uint16]1); $writer.Write([uint16]32); $writer.Write([uint32]$bytes.Length); $writer.Write([uint32]22)
$writer.Write($bytes)
$writer.Dispose(); $buffer.Dispose(); $pen.Dispose(); $brush.Dispose(); $graphics.Dispose(); $bitmap.Dispose()