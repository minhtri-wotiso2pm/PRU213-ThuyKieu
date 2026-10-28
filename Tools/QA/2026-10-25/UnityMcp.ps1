$ErrorActionPreference = 'Stop'
function Invoke-UnityMcp {
    param([string]$Method, [hashtable]$Params = @{})
    $headers = @{ Accept = 'application/json, text/event-stream' }
    if ($script:McpSession) { $headers['Mcp-Session-Id'] = $script:McpSession }
    $payload = @{ jsonrpc = '2.0'; id = 1; method = $Method; params = $Params } | ConvertTo-Json -Depth 40 -Compress
    $response = Invoke-WebRequest -Uri 'http://127.0.0.1:8080/mcp' -Method Post -ContentType 'application/json' -Headers $headers -Body ([System.Text.Encoding]::UTF8.GetBytes($payload)) -TimeoutSec 90
    if ($response.Headers['Mcp-Session-Id']) { $script:McpSession = [string]$response.Headers['Mcp-Session-Id'][0] }
    $raw = [string]$response.Content
    if ($raw -match '(?m)^data:') { $raw = ($raw -split "`n" | Where-Object { $_.StartsWith('data:') } | Select-Object -Last 1).Substring(5) }
    $result = $raw | ConvertFrom-Json -Depth 60
    if ($result.error) { throw ($result.error | ConvertTo-Json -Compress) }
    return $result.result
}
$null = Invoke-UnityMcp 'initialize' @{protocolVersion='2024-11-05';capabilities=@{};clientInfo=@{name='codex-player-review';version='1.0'}}
function Invoke-UnityTool {
    param([string]$Name, [hashtable]$Arguments = @{})
    Invoke-UnityMcp 'tools/call' @{name=$Name;arguments=$Arguments}
}
function Invoke-UnityCode {
    param([string]$Code)
    (Invoke-UnityTool 'execute_code' @{action='execute';code=$Code}).content.text
}
