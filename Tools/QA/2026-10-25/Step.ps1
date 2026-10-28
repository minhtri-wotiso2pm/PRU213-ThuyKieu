param([string[]]$Keys = @(), [double]$Seconds = 0.15, [double]$LookX = 0, [double]$LookY = 0, [string]$Shot = '', [string]$Label = 'input')
. "$PSScriptRoot/UnityMcp.ps1"
$keyList = ($Keys | ForEach-Object { 'UnityEngine.InputSystem.Key.' + $_ }) -join ','
$code = @'
Application.runInBackground = true;
var keys = new UnityEngine.InputSystem.Key[] {KEYS};
var end = EditorApplication.timeSinceStartup + SECONDS;
var lastFrame = -1;
var frames = new List<float>();
var player = UnityEngine.Object.FindAnyObjectByType<ThuyKieu.Player.PlayerMovement>();
var startPosition = player == null ? Vector3.zero : player.transform.position;
Action input = null;
input = () => {
    if (UnityEngine.InputSystem.LowLevel.InputState.currentUpdateType != UnityEngine.InputSystem.LowLevel.InputUpdateType.Dynamic) return;
    bool done = EditorApplication.timeSinceStartup >= end;
    var keyboard = UnityEngine.InputSystem.Keyboard.current;
    if (keyboard != null) UnityEngine.InputSystem.LowLevel.InputState.Change(keyboard, new UnityEngine.InputSystem.LowLevel.KeyboardState(done ? new UnityEngine.InputSystem.Key[0] : keys));
    ThuyKieu.Core.InputReader.SimulatedLook = done ? Vector2.zero : new Vector2(LOOKXf, LOOKYf);
    if (lastFrame != Time.frameCount) { frames.Add(Time.unscaledDeltaTime * 1000f); lastFrame = Time.frameCount; }
    if (done) {
        UnityEngine.InputSystem.InputSystem.onAfterUpdate -= input;
        var record = new { label = "LABEL", scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, seconds = SECONDS, keys = keys.Select(k => k.ToString()).ToArray(), start = startPosition.ToString(), end = player == null ? "" : player.transform.position.ToString(), framesMs = frames };
        System.IO.File.AppendAllText("Tools/QA/2026-10-25/InputLog.jsonl", Newtonsoft.Json.JsonConvert.SerializeObject(record) + "\n");
    }
};
UnityEngine.InputSystem.InputSystem.onAfterUpdate += input;
return "input queued";
'@
$code = $code.Replace('KEYS', $keyList).Replace('SECONDS', $Seconds.ToString([cultureinfo]::InvariantCulture)).Replace('LOOKX', $LookX.ToString([cultureinfo]::InvariantCulture)).Replace('LOOKY', $LookY.ToString([cultureinfo]::InvariantCulture)).Replace('LABEL', $Label)
Invoke-UnityCode $code
Start-Sleep -Milliseconds ([int]($Seconds * 1000 + 350))
if ($Shot) { (Invoke-UnityTool 'manage_camera' @{action='screenshot';output_folder='Tools/QA/2026-10-25/Captures';screenshot_file_name=$Shot}).content.text }
Invoke-UnityCode 'var d = UnityEngine.Object.FindAnyObjectByType<ThuyKieu.Core.Story.StoryDirector>(); return new { scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, sequence = d == null ? null : d.CurrentSequence, explore = d != null && d.InExplore, position = d == null ? null : d.Player.transform.position.ToString(), text = UnityEngine.Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsSortMode.None).Where(t=>t.gameObject.activeInHierarchy && !string.IsNullOrWhiteSpace(t.text)).Select(t=>new {path=t.transform.parent.name+"/"+t.name,text=t.text}).ToArray() };'
