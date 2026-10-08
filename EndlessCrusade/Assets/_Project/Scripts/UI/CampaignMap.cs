using EC.Core;
using EC.Data;
using EC.Services;
using TMPro;
using UnityEngine;

namespace EC.UI
{
    public class CampaignMap : MonoBehaviour
    {
        public CampaignDefinition campaign;
        public RectTransform content;
        public GameObject chapterTemplate;
        public GameObject nodeTemplate;
        public string levelScene = "Level";
        public string menuScene = "Main";

        void Start()
        {
            chapterTemplate.SetActive(false);
            nodeTemplate.SetActive(false);
            var completed = SaveHost.Service.Current.progress.completedLevels;

            foreach (var chapter in campaign.chapters)
            {
                var view = Instantiate(chapterTemplate, content);
                view.name = chapter.id;
                view.SetActive(true);
                view.transform.Find("Title").GetComponent<TMP_Text>().text = chapter.displayName;
                var nodes = view.transform.Find("Nodes");

                if (!CampaignRules.HasLevels(chapter))
                {
                    var soon = Instantiate(nodeTemplate, nodes);
                    soon.SetActive(true);
                    var node = soon.GetComponent<LevelNode>();
                    node.label.text = "Próximamente";
                    node.button.interactable = false;
                    continue;
                }

                foreach (var level in chapter.levels)
                {
                    var instance = Instantiate(nodeTemplate, nodes);
                    instance.SetActive(true);
                    instance.name = level.id;
                    instance.GetComponent<LevelNode>().Bind(level, CampaignRules.StateOf(campaign, level.id, completed), Select);
                }
            }
        }

        public void Select(LevelDefinition level)
        {
            LevelSession.Current = level;
            SceneFlow.Load(levelScene);
        }

        public void Back()
        {
            SceneFlow.Load(menuScene);
        }
    }
}
