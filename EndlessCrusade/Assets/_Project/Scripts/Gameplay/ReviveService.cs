using System;
using System.Collections.Generic;
using EC.Core;
using EC.Data;
using EC.Services;
using UnityEngine;

namespace EC.Gameplay
{
    public class ReviveService : MonoBehaviour
    {
        public const float ReviveHealthFraction = 0.5f;

        public static readonly List<Func<bool>> Sources = new List<Func<bool>>();

        bool used;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetSources()
        {
            Sources.Clear();
            Sources.Add(ConsumeElixir);
        }

        static bool ConsumeElixir()
        {
            return SaveHost.Service != null && UpgradeService.Instance.TryConsume(ConsumableDefinition.ReviveElixirId);
        }

        public bool TryRevive(GameObject hero)
        {
            if (used || hero == null)
                return false;
            if (!hero.TryGetComponent<EntityController>(out var controller) || controller.Health == null)
                return false;

            for (int i = 0; i < Sources.Count; i++)
            {
                if (!Sources[i]())
                    continue;
                used = true;
                controller.ResetState();
                controller.Health.Revive(Mathf.RoundToInt(controller.Health.maxHealth * ReviveHealthFraction));
                return true;
            }
            return false;
        }
    }
}
