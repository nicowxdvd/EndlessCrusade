using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EC.Core
{
    public static class SceneFlow
    {
        public static void Load(string scene)
        {
            SceneFlowRunner.Instance.Run(scene);
        }
    }

    public class SceneFlowRunner : MonoBehaviour
    {
        const float FadeSeconds = 0.25f;

        static SceneFlowRunner instance;

        Image fade;
        bool loading;

        public static SceneFlowRunner Instance
        {
            get
            {
                if (instance == null)
                    instance = Create();
                return instance;
            }
        }

        static SceneFlowRunner Create()
        {
            var go = new GameObject("SceneFlow");
            DontDestroyOnLoad(go);
            var runner = go.AddComponent<SceneFlowRunner>();

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            var imageObject = new GameObject("Fade");
            imageObject.transform.SetParent(go.transform, false);
            runner.fade = imageObject.AddComponent<Image>();
            runner.fade.color = new Color(0f, 0f, 0f, 0f);
            runner.fade.raycastTarget = false;
            var rect = runner.fade.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return runner;
        }

        public void Run(string scene)
        {
            if (loading)
                return;
            StartCoroutine(LoadRoutine(scene));
        }

        IEnumerator LoadRoutine(string scene)
        {
            loading = true;
            fade.raycastTarget = true;
            yield return Fade(0f, 1f);
            Time.timeScale = 1f;
            var operation = SceneManager.LoadSceneAsync(scene);
            while (!operation.isDone)
                yield return null;
            yield return Fade(1f, 0f);
            fade.raycastTarget = false;
            loading = false;
        }

        IEnumerator Fade(float from, float to)
        {
            for (float t = 0f; t < FadeSeconds; t += Time.unscaledDeltaTime)
            {
                SetAlpha(Mathf.Lerp(from, to, t / FadeSeconds));
                yield return null;
            }
            SetAlpha(to);
        }

        void SetAlpha(float alpha)
        {
            var color = fade.color;
            color.a = alpha;
            fade.color = color;
        }
    }
}
