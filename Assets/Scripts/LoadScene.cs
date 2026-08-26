using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;


/// 场景切换：加载Build列表中下一个场景
public class Scene : MonoBehaviour
{
    public float fadeDuration = 0.4f;

    private static Image _blackImage;
    private static MonoBehaviour _runner;

    // 场景历史栈：Enter 时入栈，GoBack 时出栈
    public static Stack<string> sceneStack = new Stack<string>();

    public void Enter(string sceneName)
    {
        EnsureSetup();

        // 入栈当前场景，记录来路
        string currentScene = SceneManager.GetActiveScene().name;
        sceneStack.Push(currentScene);

        // 后台加载线程优先级降到最低，保证淡入动画不被抢占
        Application.backgroundLoadingPriority = ThreadPriority.Low;

        _runner.StartCoroutine(RunTransition(sceneName, fadeDuration));
    }

    /// <summary>
    /// 实例包装，供 Unity 按钮事件调用
    /// </summary>
    public void Back()
    {
        // 返回前静音所有视频
        var videoPlayers = FindObjectsOfType<VideoPlayer>(true);
        foreach (var vp in videoPlayers)
        {
            vp.SetDirectAudioMute(0, true);
            vp.Stop();
        }
        GoBack();
    }

    /// <summary>
    /// 出栈并返回上一个场景。栈为空时忽略。
    /// </summary>
    public static void GoBack()
    {
        if (sceneStack.Count == 0)
        {
            Debug.LogWarning("[EnterotherScenes] 场景栈为空，没有可返回的场景。");
            return;
        }

        string previousScene = sceneStack.Pop();
        EnsureSetup();

        Application.backgroundLoadingPriority = ThreadPriority.Low;
        _runner.StartCoroutine(RunTransition(previousScene, 0.4f));
    }

    private static void EnsureSetup()
    {
        if (_blackImage != null) return;

        // 创建持久化的协程运行器（不随场景销毁）
        GameObject runnerObj = new GameObject("SceneTransition");
        DontDestroyOnLoad(runnerObj);
        _runner = runnerObj.AddComponent<TransitionRunner>();

        // 创建黑色遮罩 Canvas
        GameObject canvasObj = new GameObject("TransitionCanvas");
        canvasObj.transform.SetParent(runnerObj.transform);

        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject imgObj = new GameObject("BlackImage");
        imgObj.transform.SetParent(canvasObj.transform, false);

        _blackImage = imgObj.AddComponent<Image>();
        _blackImage.color = new Color(0, 0, 0, 0);
        _blackImage.raycastTarget = false;

        var rt = _blackImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static IEnumerator RunTransition(string sceneName, float fadeDuration)
    {
        _blackImage.raycastTarget = true;

        // 1. 淡入黑屏 (OutQuad 缓动：先快后慢)
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / fadeDuration);
            float a = 1f - (1f - p) * (1f - p); // OutQuad
            _blackImage.color = new Color(0, 0, 0, a);
            yield return null;
        }
        _blackImage.color = new Color(0, 0, 0, 1f);

        // 2. 黑屏已全覆盖，开始后台异步加载（allowSceneActivation=false → 加载完不激活）
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // 3. 等待场景加载完成（此时屏幕全黑，用户看不到任何卡顿）
        while (op.progress < 0.9f) yield return null;

        // 4. 激活新场景
        //    注意：激活这一帧 Unity 会同步创建所有 GameObject + 调用 Awake/Start，会卡一帧
        //    但此时屏幕全黑，用户看不到
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        // 5. 激活后等几帧，让 Awake/Start/OnEnable 全部跑完，视频第一帧也渲染出来
        //    这段"滞留黑屏"把激活尖峰完全吸收在黑屏里
        yield return null;
        yield return null;
        yield return null;

        // 6. 找到新场景里的 MediaPlayer 并确保开始播放
        var mp = Object.FindObjectOfType<RenderHeads.Media.AVProVideo.MediaPlayer>();
        if (mp != null)
        {
            if (mp.Control != null && !mp.Control.IsPlaying())
            {
                mp.Play();
            }
            yield return null; // 等一帧让视频第一帧渲染出来
        }

        // 7. 淡出黑屏 (InQuad 缓动：先慢后快)
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / fadeDuration);
            float a = 1f - p * p; // InQuad
            _blackImage.color = new Color(0, 0, 0, a);
            yield return null;
        }
        _blackImage.color = new Color(0, 0, 0, 0);

        _blackImage.raycastTarget = false;
    }
}

/// <summary>
/// 最小化 MonoBehaviour，仅用于在 DontDestroyOnLoad 对象上运行协程
/// </summary>
public class TransitionRunner : MonoBehaviour { }
