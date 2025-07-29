#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class ClearPlayerPrefsMenu
{
    [MenuItem("Tools/Clear PlayerPrefs")]
    private static void ClearPlayerPrefs()
    {
        if (EditorUtility.DisplayDialog(
            "Clear PlayerPrefs",
            "정말로 모든 PlayerPrefs 데이터를 삭제하시겠습니까?",
            "삭제", "취소")) {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("PlayerPrefs 삭제 완료");
        }
    }
}
#endif
