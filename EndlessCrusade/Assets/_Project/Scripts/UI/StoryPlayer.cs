using System;
using EC.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EC.UI
{
    public class StoryPlayer : MonoBehaviour
    {
        public GameObject root;
        public Image image;
        public TMP_Text text;
        public Button advanceButton;
        public Button skipButton;

        StorySequence sequence;
        Action onFinished;
        int index;

        public bool IsPlaying { get; private set; }

        void Awake()
        {
            advanceButton.onClick.AddListener(Advance);
            skipButton.onClick.AddListener(Finish);
            root.SetActive(false);
        }

        public void Play(StorySequence newSequence, Action finished)
        {
            if (newSequence == null || newSequence.panels == null || newSequence.panels.Length == 0)
            {
                finished?.Invoke();
                return;
            }
            sequence = newSequence;
            onFinished = finished;
            index = 0;
            IsPlaying = true;
            root.SetActive(true);
            Show();
        }

        public void Advance()
        {
            if (!IsPlaying)
                return;
            index++;
            if (index >= sequence.panels.Length)
            {
                Finish();
                return;
            }
            Show();
        }

        public void Finish()
        {
            if (!IsPlaying)
                return;
            IsPlaying = false;
            root.SetActive(false);
            var callback = onFinished;
            onFinished = null;
            callback?.Invoke();
        }

        void Show()
        {
            var panel = sequence.panels[index];
            text.text = panel.text;
            image.sprite = panel.image;
            image.enabled = panel.image != null;
        }
    }
}
