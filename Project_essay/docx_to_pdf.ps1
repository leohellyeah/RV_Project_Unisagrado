<#
Converte o artigo .docx em .pdf usando o Word instalado (COM automation).
Use depois de inserir os screenshots no .docx.

Uso:
  powershell -ExecutionPolicy Bypass -File docx_to_pdf.ps1
  powershell -ExecutionPolicy Bypass -File docx_to_pdf.ps1 -Out "C:\caminho\saida.pdf"
#>
param(
    [string]$In  = "$PSScriptRoot\Artigo_SBC_VR_Folklore.docx",
    [string]$Out = "$PSScriptRoot\Artigo_SBC_VR_Folklore.pdf"
)

$wdExportFormatPDF = 17
$word = New-Object -ComObject Word.Application
$word.Visible = $false
try {
    $doc = $word.Documents.Open($In, $false, $true)   # AddToRecentFiles=false, ReadOnly=true
    $doc.ExportAsFixedFormat($Out, $wdExportFormatPDF)
    $doc.Close($false)
    Write-Output "PDF gerado -> $Out"
}
finally {
    $word.Quit()
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($word) | Out-Null
}
