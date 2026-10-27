using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Test-copy entry point only. The game's code, assertions and error collector remain unchanged.
public static class MergeBatchQA
{
    public static void Playtest()
    {
        int removed = 0;
        foreach (Delegate callback in EditorApplication.delayCall?.GetInvocationList() ?? Array.Empty<Delegate>())
        {
            if (callback.Method.Name != "IndexationOnStartup" ||
                callback.Method.DeclaringType?.FullName != "UnityEditor.Search.SearchInit") continue;
            EditorApplication.delayCall -= (EditorApplication.CallbackFunction)callback;
            removed++;
        }
        Debug.Log("[MergeQA] Removed batch-only Editor Search startup callbacks: " + removed);
        Type entry = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType("ThuyKieu.Core.Editor.StoryBuild", false))
            .First(type => type != null);
        entry.GetMethod("PlaytestBatch", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
    }
}
