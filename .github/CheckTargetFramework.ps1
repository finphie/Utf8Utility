$files = Get-ChildItem Source -Include *.csproj -Recurse -Depth 1

foreach ($file in $files) {
    $xml = [Xml](Get-Content $file -Encoding UTF8)
    $node = $xml.Project.SelectSingleNode('//Project/PropertyGroup/TargetFramework')

    if (!$node) {
        $node = $xml.Project.SelectSingleNode('//Project/PropertyGroup/TargetFrameworks')
    }

    [string] $text = $node.InnerText
    Write-Output "$($file.Name) $text"

    $targetFrameworks = $text -Split ';'

    if (!($targetFrameworks -in 'net6.0')) {
        Write-Output 'DOTNET_MULTILEVEL_LOOKUP=1' | Out-File $Env:GITHUB_ENV -Encoding utf8 -Append
    }
}
