Write-Host "Converting Markdown files to HTML..."

$markdownFiles = (Get-ChildItem . -Recurse -Include *.md -Exclude .git)
For ($i = 0; $i -lt $markdownFiles.Length; $i++) {
    $markdownFile = $markdownFiles[$i]
	$markdownFolder = [System.IO.Path]::GetDirectoryName($markdownFile)
    $fileName = [System.IO.Path]::GetFileNameWithoutExtension($markdownFile)
    $htmlFolder = [System.IO.Path]::Combine($markdownFolder, "html")
    if (![System.IO.Directory]::Exists($htmlFolder)) {
        [System.IO.Directory]::CreateDirectory($htmlFolder) > $null
    }
    $htmlFile = [System.IO.Path]::Combine($htmlFolder, "$fileName.html")
    
    # Using to generate html https://www.npmjs.com/package/markdown-to-html
    & github-markdown $markdownFile > $htmlFile 
    
    (Get-Content $htmlFile) -replace '.md', '.html' | Set-Content $htmlFile
}