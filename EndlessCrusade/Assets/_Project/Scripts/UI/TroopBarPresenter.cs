using EC.Core;
using EC.Data;
using EC.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EC.UI
{
    public class TroopBarPresenter : MonoBehaviour
    {
        [System.Serializable]
        public class Slot
        {
            public TroopDefinition troop;
            public Button button;
            public Image cooldownFill;
        }

        public GameObject bar;
        public CampaignDefinition campaign;
        public Image leadershipFill;
        public TMP_Text leadershipLabel;
        public Slot[] slots;

        float leadership;
        float[] cooldownEnd;
        int lastShown = -1;

        void Awake()
        {
            if (slots == null)
                slots = new Slot[0];
            cooldownEnd = new float[slots.Length];
            for (int i = 0; i < slots.Length; i++)
            {
                var id = slots[i].troop.id;
                slots[i].button.onClick.AddListener(() => Request(id));
                slots[i].button.gameObject.SetActive(IsUnlocked(slots[i].troop));
            }
            if (bar != null)
                bar.SetActive(false);
        }

        void OnEnable()
        {
            EventBus<TroopsAvailable>.Subscribe(OnTroopsAvailable);
            EventBus<LeadershipChanged>.Subscribe(OnLeadershipChanged);
            EventBus<TroopSummoned>.Subscribe(OnTroopSummoned);
        }

        void OnDisable()
        {
            EventBus<TroopsAvailable>.Unsubscribe(OnTroopsAvailable);
            EventBus<LeadershipChanged>.Unsubscribe(OnLeadershipChanged);
            EventBus<TroopSummoned>.Unsubscribe(OnTroopSummoned);
        }

        void Update()
        {
            if (bar == null || !bar.activeSelf)
                return;
            for (int i = 0; i < slots.Length; i++)
                Refresh(i);
        }

        bool IsUnlocked(TroopDefinition troop)
        {
            if (campaign == null || SaveHost.Service == null)
                return true;
            return TroopUnlockRules.IsUnlocked(troop, campaign, SaveHost.Service.Current.progress.completedLevels);
        }

        public void Request(string troopId)
        {
            EventBus<TroopSummonRequested>.Publish(new TroopSummonRequested(troopId));
        }

        void OnTroopsAvailable(TroopsAvailable evt)
        {
            if (bar != null)
                bar.SetActive(evt.Enabled);
        }

        void OnLeadershipChanged(LeadershipChanged evt)
        {
            leadership = evt.Current;
            if (leadershipFill != null)
                leadershipFill.fillAmount = evt.Max <= 0f ? 0f : Mathf.Clamp01(evt.Current / evt.Max);
            var whole = Mathf.FloorToInt(evt.Current);
            if (leadershipLabel != null && whole != lastShown)
            {
                lastShown = whole;
                leadershipLabel.SetText("Fe {0} / {1}", whole, Mathf.RoundToInt(evt.Max));
            }
        }

        void OnTroopSummoned(TroopSummoned evt)
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].troop.id == evt.TroopId)
                    cooldownEnd[i] = Time.time + evt.Cooldown;
        }

        void Refresh(int index)
        {
            var slot = slots[index];
            var remaining = Mathf.Max(0f, cooldownEnd[index] - Time.time);
            if (slot.cooldownFill != null)
                slot.cooldownFill.fillAmount = slot.troop.summonCooldown <= 0f ? 0f : Mathf.Clamp01(remaining / slot.troop.summonCooldown);
            slot.button.interactable = remaining <= 0f && leadership >= slot.troop.leadershipCost;
        }
    }
}
