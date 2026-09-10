using UnityEngine;

/// 主菜单：开始游戏 / 退出（挂在 Start 场景）
/// 按钮 OnClick 分别绑定 StartGame / QuitGame
public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        var scene = FindObjectOfType<Scene>();
        if (scene != null) scene.Enter("MainScene");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
