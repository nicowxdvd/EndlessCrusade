using System;
using EC.Core;
using TMPro;
using UnityEngine;

namespace EC.UI
{
    public enum TutorialStep { Move, Sword, Whip, HolyWater }

    public class TutorialController : MonoBehaviour
    {
        public GameObject root;
        public TMP_Text label;
        public string[] messages =
        {
            "Muévete con los botones de dirección",
            "Ataca con la espada",
            "Ataca con el látigo",
            "Lanza agua bendita"
        };

        Action onFinished;
        int step = -1;

        public bool IsActive => step >= 0;
        public TutorialStep Current => (TutorialStep)step;

        void Awake()
        {
            root.SetActive(false);
        }

        void OnEnable()
        {
            EventBus<HeroActed>.Subscribe(OnHeroActed);
            EventBus<AbilityRequested>.Subscribe(OnAbilityRequested);
        }

        void OnDisable()
        {
            EventBus<HeroActed>.Unsubscribe(OnHeroActed);
            EventBus<AbilityRequested>.Unsubscribe(OnAbilityRequested);
        }

        public void Begin(Action finished)
        {
            onFinished = finished;
            step = 0;
            root.SetActive(true);
            ShowStep();
        }

        public void Notify(TutorialStep performed)
        {
            if (!IsActive || performed != Current)
                return;
            step++;
            if (step >= Enum.GetValues(typeof(TutorialStep)).Length)
            {
                step = -1;
                root.SetActive(false);
                EventBus<TutorialCompleted>.Publish(new TutorialCompleted());
                var callback = onFinished;
                onFinished = null;
                callback?.Invoke();
                return;
            }
            ShowStep();
        }

        void OnHeroActed(HeroActed evt)
        {
            switch (evt.Action)
            {
                case HeroAction.Move: Notify(TutorialStep.Move); break;
                case HeroAction.Sword: Notify(TutorialStep.Sword); break;
                case HeroAction.Whip: Notify(TutorialStep.Whip); break;
            }
        }

        void OnAbilityRequested(AbilityRequested evt)
        {
            if (evt.Slot == 0)
                Notify(TutorialStep.HolyWater);
        }

        void ShowStep()
        {
            label.text = messages[step];
        }
    }
}
