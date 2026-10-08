using EC.Core;
using EC.Data;
using UnityEngine;

namespace EC.UI
{
    public class LevelFlow : MonoBehaviour
    {
        public StoryPlayer storyPlayer;
        public TutorialController tutorial;

        LevelDefinition level;

        void Start()
        {
            level = LevelSession.Current;
            if (level == null)
            {
                StartWaves();
                return;
            }
            storyPlayer.Play(level.intro, AfterIntro);
        }

        void AfterIntro()
        {
            if (level.tutorial)
                tutorial.Begin(StartWaves);
            else
                StartWaves();
        }

        void StartWaves()
        {
            EventBus<WavesStartRequested>.Publish(new WavesStartRequested());
        }
    }
}
