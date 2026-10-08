using System.Collections.Generic;

namespace EC.Data
{
    public enum LevelNodeState { Locked, Available, Completed }

    public static class CampaignRules
    {
        public static LevelNodeState StateOf(CampaignDefinition campaign, string levelId, ICollection<string> completed)
        {
            if (campaign == null || campaign.chapters == null || string.IsNullOrEmpty(levelId))
                return LevelNodeState.Locked;

            string previous = null;
            foreach (var chapter in campaign.chapters)
            {
                if (chapter == null || chapter.levels == null)
                    continue;
                foreach (var level in chapter.levels)
                {
                    if (level == null)
                        continue;
                    if (level.id == levelId)
                    {
                        if (completed.Contains(levelId))
                            return LevelNodeState.Completed;
                        return previous == null || completed.Contains(previous) ? LevelNodeState.Available : LevelNodeState.Locked;
                    }
                    previous = level.id;
                }
            }
            return LevelNodeState.Locked;
        }

        public static bool HasLevels(ChapterDefinition chapter)
        {
            return chapter != null && chapter.levels != null && chapter.levels.Length > 0;
        }
    }
}
