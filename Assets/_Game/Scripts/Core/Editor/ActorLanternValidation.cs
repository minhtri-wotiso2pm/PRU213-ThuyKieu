using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ThuyKieu.Gameplay.Chapters.LauTuBa;
using ThuyKieu.Gameplay.Stealth;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ThuyKieu.Core.Editor
{
    // Read-only inspection of actual serialized actors. Does not rebuild or save a scene.
    public static class ActorLanternValidation
    {
        const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        public static void BuildPatchedPlayer()
        {
            var args=System.Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-buildOutput");
            if(index<0 || index+1>=args.Length)throw new Exception("Explicit staging -buildOutput is required");
            string output=Path.GetFullPath(args[index+1]);
            if(File.Exists(output))throw new Exception("Staging executable already exists: "+output);
            var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
            if(!scenes.Contains("Assets/_Game/Scenes/Chapter04_LauTuBaDem.unity"))throw new Exception("Patched Chapter 4 missing from build");
            AudioBankBaker.Bake();
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            Debug.Log("ACTOR LANTERN PLAYER BUILD: "+report.summary.result+", "+report.summary.totalErrors+" errors, "+report.summary.totalSize+" bytes; "+output);
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Windows build failed");
        }
        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Chapter04_LauTuBaDem.unity");
            var world = UnityEngine.Object.FindAnyObjectByType<Chapter04LauWorld>();
            if(world == null) throw new Exception("Chapter 4 world missing");
            var data = new SerializedObject(world);
            var tuBa = (Component)data.FindProperty("_tuBa").objectReferenceValue;
            var lamp = (Light)data.FindProperty("_tuBaLamp").objectReferenceValue;
            if(lamp == null) throw new Exception("Expected existing scene's legacy personal lamp");
            var oldRoot=lamp.transform;
            while(oldRoot.parent!=null && oldRoot.parent!=tuBa.transform) oldRoot=oldRoot.parent;
            typeof(Chapter04LauWorld).GetMethod("HideTuBaLamp",Private).Invoke(world,null);
            if(oldRoot.gameObject.activeSelf || lamp.enabled) throw new Exception("Tu Ba personal lantern remains active");
            Debug.Log("LANTERN PASS: existing TuBa personal prop and light hidden");
            Watchman primary = null;
            foreach(string field in new[]{"_guard","_gateGuard"})
            {
                var guard=(Watchman)data.FindProperty(field).objectReferenceValue;
                var count=guard.GetComponentsInChildren<Light>(true).Length;
                var animator=guard.GetComponentInChildren<Animator>();
                if(animator!=null){animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;animator.Rebind();animator.Update(.05f);}
                HandLantern.Attach(guard);HandLantern.Attach(guard);
                var held=guard.GetComponentsInChildren<HandLantern>(true);
                if(count==0){if(held.Length!=0)throw new Exception("Added unrequested lantern");continue;}
                if(held.Length!=1 || !held[0].gameObject.activeSelf) throw new Exception("Guard lantern missing, duplicated or rig fallback used");
                primary=guard;
                var prop=held[0];var hand=animator.GetBoneTransform(HumanBodyBones.RightHand);
                var knuckle=animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
                foreach(float speed in new[]{0f,1.1f,3.3f})
                {
                    if(animator.parameters.Any(p=>p.name=="Speed"))animator.SetFloat("Speed",speed);
                    for(int frame=0;frame<30;frame++)
                    {
                        animator.Update(1f/30f);
                        guard.transform.rotation=Quaternion.Euler(0,frame*3,0);
                        typeof(HandLantern).GetMethod("LateUpdate",Private).Invoke(prop,null);
                        var grip=(knuckle!=null?Vector3.Lerp(hand.position,knuckle.position,.8f):hand.position)+Vector3.up*.03f;
                        if(Vector3.Distance(grip,prop.transform.position)>.002f)throw new Exception("Handle lost hand during animation/turn");
                    }
                }
                var body=prop.transform.Find("Body").GetComponent<Renderer>().bounds.size;
                if(body.x<.18f || body.x>.25f || body.y<.22f || body.y>.30f)throw new Exception("Incorrect world-space body size: "+body);
                if(prop.transform.Find("Tassel").gameObject.activeSelf || prop.transform.Find("Knot").gameObject.activeSelf)throw new Exception("Dangling decorations remain");
                var finger=animator.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate);
                var curl=finger.localRotation;
                for(int frame=0;frame<90;frame++)typeof(HandLantern).GetMethod("LateUpdate",Private).Invoke(prop,null);
                if(Quaternion.Angle(curl,finger.localRotation)>.05f)throw new Exception("Finger curl accumulates without animator updates");
                prop.enabled=false;
                // This probe runs in Edit Mode, so ordinary MonoBehaviour lifecycle callbacks are invoked explicitly.
                typeof(HandLantern).GetMethod("OnDisable",Private).Invoke(prop,null);
                if(Quaternion.Angle(curl,finger.localRotation)<5)throw new Exception("Finger grip was not restored on disable");
                prop.enabled=true;typeof(HandLantern).GetMethod("LateUpdate",Private).Invoke(prop,null);
                Debug.Log("LANTERN PASS: finger curl stable without animation and released on disable");
                Debug.Log("LANTERN PASS: "+guard.name+" has one hand-held lantern, body "+body+"; 90 animated grip/turn samples correct");
            }
            if(primary==null)throw new Exception("No actual guard model tested");
            string output=System.Environment.GetEnvironmentVariable("TUBA_LANTERN_OUTPUT");
            if(!string.IsNullOrEmpty(output))
            {
                Directory.CreateDirectory(output);
                foreach(var fx in UnityEngine.Object.FindObjectsByType<ThuyKieu.Presentation.ScreenFx>(FindObjectsSortMode.None))
                    fx.GetType().GetMethod("Awake",Private)?.Invoke(fx,null);
                primary.transform.rotation=Quaternion.identity;
                var anim=primary.GetComponentInChildren<Animator>();anim.SetFloat("Speed",0);anim.Play(0,0,0);anim.Update(.1f);
                var prop=primary.GetComponentInChildren<HandLantern>();typeof(HandLantern).GetMethod("LateUpdate",Private).Invoke(prop,null);
                Render(Camera.main,primary.transform.position+primary.transform.forward*2.1f+primary.transform.right*.9f+Vector3.up*1.05f,primary.transform.position+Vector3.up*.95f,Path.Combine(output,"GuardHandLantern.png"));
            }
            Debug.Log("LANTERN VALIDATION PASSED. Existing scene inspected without rebuilding or saving.");
        }
        static void Render(Camera camera,Vector3 at,Vector3 look,string path)
        {
            var target=new RenderTexture(1100,1100,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
            var texture=new Texture2D(1100,1100,TextureFormat.RGB24,false);var previous=RenderTexture.active;
            try
            {
                camera.transform.position=at;camera.transform.LookAt(look);camera.aspect=1;camera.targetTexture=target;
                for(int i=0;i<3;i++)RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
                RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,1100,1100),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());
            }
            finally{camera.targetTexture=null;RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(target);}
        }
    }
}
