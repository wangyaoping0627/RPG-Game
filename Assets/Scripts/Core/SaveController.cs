using UnityEngine;

/// 存档挂载点：进场景自动读档，F5 存档 / F9 读档，退出时自动存档
/// 用法：MainScene 里建一个空物体（如 "SaveController"）挂上本脚本即可
public class SaveController : MonoBehaviour
{
    [Header("行为")]
    public bool loadOnStart = true;      // 进入场景时自动读档
    public bool saveOnQuit = true;       // 退出游戏时自动存档
    public KeyCode saveKey = KeyCode.F5;
    public KeyCode loadKey = KeyCode.F9;

    private void Start()
    {
        if (loadOnStart) SaveSystem.Load();
    }

    private void Update()
    {
        if (Input.GetKeyDown(saveKey)) SaveSystem.Save();
        if (Input.GetKeyDown(loadKey)) SaveSystem.Load();
    }

    private void OnApplicationQuit()
    {
        if (saveOnQuit) SaveSystem.Save();
    }
}
