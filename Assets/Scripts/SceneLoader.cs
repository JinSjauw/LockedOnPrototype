using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class SceneLoader
{
    public static void Load(string sceneName, Action OnFinish = null)
    {
        new GameObject("SceneLoadHelper")
            .AddComponent<LoadingHelper>()
            .StartCoroutine(LoadAsync(sceneName, OnFinish));
    }

    private class LoadingHelper : MonoBehaviour { }

    private static IEnumerator LoadAsync(string sceneName, Action OnFinish)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = true;
        while (!op.isDone) { yield return null; }
        OnFinish();
        Object.Destroy(GameObject.Find("SceneLoadHelper"));
    }
}
