using System;
using System.Text;
using EC.Core;
using EC.Data;
using EC.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EC.UI
{
    public class PachinkoScreen : MonoBehaviour
    {
        public PrizeTable table;
        public PachinkoBall ball;
        public Transform spawnPoint;
        public float boardHalfWidth = 4.5f;
        public float slotY = -4.5f;
        public TMP_Text ticketsLabel;
        public TMP_Text resultLabel;
        public TMP_Text oddsLabel;
        public Button launchButton;
        public Button dailyButton;
        public string backScene = "CampaignMap";

        PachinkoService service;
        int pendingIndex = -1;

        public static float SlotX(int index, int count, float halfWidth)
        {
            if (count <= 1)
                return 0f;
            var slotWidth = 2f * halfWidth / count;
            return -halfWidth + slotWidth * (index + 0.5f);
        }

        public static string FormatOdds(PrizeTable table)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < table.entries.Length; i++)
                builder.AppendLine(table.entries[i].displayName + ": " + (table.Probability(i) * 100f).ToString("0.#") + "%");
            return builder.ToString();
        }

        void Start()
        {
            service = new PachinkoService(SaveHost.Service, CurrencyService.Instance, UpgradeService.Instance, new System.Random(), () => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            ball.gameObject.SetActive(false);
            ball.Landed += OnLanded;
            oddsLabel.text = FormatOdds(table);
            resultLabel.text = "";
            Refresh();
        }

        public void Launch()
        {
            if (pendingIndex >= 0)
                return;
            var index = service.TryPlay(table);
            if (index < 0)
            {
                resultLabel.text = "Sin boletos";
                return;
            }

            pendingIndex = index;
            resultLabel.text = "";
            ball.gameObject.SetActive(true);
            ball.Launch(spawnPoint.position, SlotX(index, table.entries.Length, boardHalfWidth), slotY);
            Refresh();
        }

        public void ClaimDaily()
        {
            resultLabel.text = service.ClaimDaily() ? "Boleto diario reclamado" : "Ya reclamaste el boleto de hoy";
            Refresh();
        }

        public void Back()
        {
            SceneFlow.Load(backScene);
        }

        void OnLanded(PachinkoBall landed)
        {
            var entry = table.entries[pendingIndex];
            pendingIndex = -1;
            landed.gameObject.SetActive(false);
            resultLabel.text = "Premio: " + entry.displayName;
            Refresh();
        }

        void Refresh()
        {
            ticketsLabel.text = "Boletos: " + service.Tickets;
            launchButton.interactable = pendingIndex < 0 && service.Tickets > 0;
            dailyButton.interactable = pendingIndex < 0 && service.CanClaimDaily();
        }
    }
}
