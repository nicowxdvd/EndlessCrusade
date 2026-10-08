using System;
using EC.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EC.UI
{
    public class LevelNode : MonoBehaviour
    {
        public Button button;
        public TMP_Text label;

        public static string Caption(string name, LevelNodeState state)
        {
            switch (state)
            {
                case LevelNodeState.Completed: return name + "\nCompletado";
                case LevelNodeState.Locked: return name + "\nBloqueado";
                default: return name;
            }
        }

        public void Bind(LevelDefinition level, LevelNodeState state, Action<LevelDefinition> onSelected)
        {
            label.text = Caption(level.displayName, state);
            button.interactable = state != LevelNodeState.Locked;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onSelected(level));
        }
    }
}
