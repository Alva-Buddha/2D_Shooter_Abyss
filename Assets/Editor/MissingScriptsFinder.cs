#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class MissingScriptsFinder : MonoBehaviour
{
    [MenuItem("Tools/Find Missing Scripts")]
    public static void FindMissingScripts()
    {
        Debug.Log("FindMissingScripts started.");

        // Check all GameObjects in the scene, including inactive ones
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        Debug.Log("Checking " + allObjects.Length + " GameObjects in the scene.");
        foreach (var go in allObjects)
        {
            // Only check GameObjects that are part of the scene (this excludes prefabs)
            if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)
                continue;

            CheckGameObjectForMissingScripts(go);
        }

        // Check all prefabs in the Assets directory
        string[] allPrefabPaths = AssetDatabase.GetAllAssetPaths();
        Debug.Log("Checking " + allPrefabPaths.Length + " assets for prefabs.");
        foreach (string path in allPrefabPaths)
        {
            if (path.Contains(".prefab"))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                CheckGameObjectForMissingScripts(prefab);
            }
        }

        Debug.Log("FindMissingScripts completed.");
    }

    private static void CheckGameObjectForMissingScripts(GameObject go)
    {
        MonoBehaviour[] components = go.GetComponents<MonoBehaviour>();
        foreach (var c in components)
        {
            if (c == null)
            {
                Debug.LogError("Missing script found on " + go.name, go);
            }
        }

        // Check all children
        foreach (Transform child in go.transform)
        {
            CheckGameObjectForMissingScripts(child.gameObject);
        }
    }
}
#endif
