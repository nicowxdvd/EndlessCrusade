namespace EC.Core
{
    public readonly struct PlaySfx
    {
        public readonly AudioCue Cue;

        public PlaySfx(AudioCue cue) { Cue = cue; }
    }

    public readonly struct PlaySfxById
    {
        public readonly string CueId;

        public PlaySfxById(string cueId) { CueId = cueId; }
    }

    public readonly struct PlayMusic
    {
        public readonly string CueId;

        public PlayMusic(string cueId) { CueId = cueId; }
    }

    public readonly struct PlayAmbient
    {
        public readonly string CueId;

        public PlayAmbient(string cueId) { CueId = cueId; }
    }
}
