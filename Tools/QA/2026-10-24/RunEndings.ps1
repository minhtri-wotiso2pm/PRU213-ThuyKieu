$ErrorActionPreference = 'Stop'
function Invoke-QaUnity {
    param([string[]]$UnityArguments)
    $qaRaw = & unity @UnityArguments
    if ($LASTEXITCODE -ne 0) { throw ($qaRaw -join "`n") }
    $qaResponse = ($qaRaw -join "`n") | ConvertFrom-Json
    if (-not $qaResponse.success) { throw ($qaRaw -join "`n") }
    return $qaResponse
}
foreach ($qaEnding in @('nghich_menh', 'thang', 'mat_minh', 'dinh_menh')) {
    Invoke-QaUnity -UnityArguments @('command', 'editor_stop', '--format', 'json') | Out-Null
    $qaReport = "D:/ThuyKieuKiemTien_Ne/PRU213-ThuyKieu/Tools/QA/2026-10-24/Ending_$qaEnding.txt"
    if (Test-Path -LiteralPath $qaReport) { throw "Report already exists: $qaReport" }
    $qaCode = 'System.Environment.SetEnvironmentVariable("THUYKIEU_AUTOPILOT_ARGS", "-storyAutopilot -autopilotEnding {ENDING} -autopilotReport {REPORT}"); UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Game/Scenes/Chapter07_HuyenDuong.unity"); return "Ready";'.Replace('{ENDING}', $qaEnding).Replace('{REPORT}', $qaReport)
    Invoke-QaUnity -UnityArguments @('command', 'eval', $qaCode, '--format', 'json') | Out-Null
    Invoke-QaUnity -UnityArguments @('command', 'editor_play', '--format', 'json') | Out-Null
    $qaDeadline = [DateTime]::UtcNow.AddSeconds(90)
    while (-not (Test-Path -LiteralPath $qaReport) -and [DateTime]::UtcNow -lt $qaDeadline) { Start-Sleep -Seconds 2 }
    if (-not (Test-Path -LiteralPath $qaReport)) { throw "Ending timeout: $qaEnding" }
    $qaVerdict = Get-Content -LiteralPath $qaReport -First 1
    Write-Output "$qaEnding : $qaVerdict"
    if ($qaVerdict -ne 'AUTOPILOT PASS') { throw "Ending runtime failure: $qaEnding" }
    $qaState = Invoke-QaUnity -UnityArguments @('command', 'eval', 'var d=UnityEngine.Object.FindAnyObjectByType<ThuyKieu.Core.Story.StoryDirector>(); return new { verdict=d.GetInt("c7_ket"), soul=d.GetInt("dong_dieu"), fate=d.GetInt("dinh_menh"), classification=ThuyKieu.Core.EndingRecord.Classify(d.GetInt) };', '--format', 'json')
    $qaState.data.result.result | ConvertTo-Json -Compress | Write-Output
    $qaState.data.result.result | ConvertTo-Json | Set-Content -LiteralPath ("Tools/QA/2026-10-24/Ending_" + $qaEnding + '.json') -Encoding utf8
}
Invoke-QaUnity -UnityArguments @('command', 'editor_stop', '--format', 'json') | Out-Null
