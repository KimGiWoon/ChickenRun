#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class ForcePlayFromFirstScene
{
    static ForcePlayFromFirstScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode) {
            string firstScenePath = SceneUtility.GetScenePathByBuildIndex(0);

            if (!EditorSceneManager.GetActiveScene().path.Equals(firstScenePath)) {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                    EditorSceneManager.OpenScene(firstScenePath);
                }
                else {
                    EditorApplication.isPlaying = false;
                }
            }
        }
    }
}
#endif
