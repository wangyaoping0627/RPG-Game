using UnityEngine;

/// 暂停菜单：ESC 开关，暂停时 timeScale = 0
/// 注意：HitstopController 也会改 timeScale，暂停瞬间若正好命中会互相覆盖，
///       演示时避免「暂停的同时被打中」即可
public class PauseMenu : MonoBehaviour
{
    public GameObject panel;

    private void Start()
    {
        if (panel != null) panel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Toggle();
    }

    private void Toggle()
    {
        if (panel == null) return;
        panel.SetActive(!panel.activeSelf);
        Time.timeScale = panel.activeSelf ? 0f : 1f;
    }

    /// 继续游戏（按钮 OnClick）
    public void Resume() => Toggle();

    /// 回主菜单（按钮 OnClick）
    public void BackToMenu()
    {
        Time.timeScale = 1f; // 先恢复，否则切场景后仍是 0
        var scene = FindObjectOfType<Scene>();
        if (scene != null) scene.Enter("Start");
    }
}
