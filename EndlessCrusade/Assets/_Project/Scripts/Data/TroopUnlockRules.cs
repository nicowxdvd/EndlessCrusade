using System.Collections.Generic;

namespace EC.Data
{
    public static class TroopUnlockRules
    {
        public static bool IsUnlocked(TroopDefinition troop, CampaignDefinition campaign, ICollection<string> completed)
        {
            if (troop == null || string.IsNullOrEmpty(troop.unlockChapterId))
                return true;
            if (campaign == null || campaign.chapters == null)
                return false;

            for (int i = 0; i < campaign.chapters.Length; i++)
            {
                var chapter = campaign.chapters[i];
                if (chapter == null || chapter.id != troop.unlockChapterId)
                    continue;
                if (i == 0)
                    return true;
                return IsChapterCompleted(campaign.chapters[i - 1], completed);
            }
            return false;
        }

        public static bool IsChapterCompleted(ChapterDefinition chapter, ICollection<string> completed)
        {
            if (!CampaignRules.HasLevels(chapter))
                return false;
            var last = chapter.levels[chapter.levels.Length - 1];
            return last != null && completed.Contains(last.id);
        }
    }
}
