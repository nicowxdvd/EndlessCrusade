using UnityEngine;

namespace EC.Core
{
    public class AudioManager : MonoBehaviour
    {
        public const int SfxSourceCount = 8;
        public const float CrossfadeSeconds = 1.5f;
        public const string LibraryResource = "AudioLibrary";

        static AudioManager instance;

        public static AudioManager Instance => instance;

        public AudioLibrary library;

        AudioSource[] sfx;
        AudioSource[] music;
        AudioSource ambient;
        int activeMusic;
        string currentMusicId;
        string currentAmbientId;
        float fadeProgress = 1f;
        float musicCueVolume = 1f;
        float ambientCueVolume = 1f;

        public float MusicVolume { get; private set; } = 0.8f;
        public float SfxVolume { get; private set; } = 1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()
        {
            if (instance != null)
                return;
            var go = new GameObject("AudioManager");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<AudioManager>();
            instance.library = Resources.Load<AudioLibrary>(LibraryResource);
        }

        void Awake()
        {
            sfx = new AudioSource[SfxSourceCount];
            for (int i = 0; i < sfx.Length; i++)
                sfx[i] = CreateSource(false);
            music = new[] { CreateSource(true), CreateSource(true) };
            ambient = CreateSource(true);
        }

        AudioSource CreateSource(bool loop)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            return source;
        }

        void OnEnable()
        {
            EventBus<PlaySfx>.Subscribe(OnPlaySfx);
            EventBus<PlaySfxById>.Subscribe(OnPlaySfxById);
            EventBus<PlayMusic>.Subscribe(OnPlayMusic);
            EventBus<PlayAmbient>.Subscribe(OnPlayAmbient);
            EventBus<EntityHurt>.Subscribe(OnHurt);
            EventBus<EntityDied>.Subscribe(OnDied);
            EventBus<HeroActed>.Subscribe(OnHeroActed);
            EventBus<BossSpawned>.Subscribe(OnBossSpawned);
            EventBus<TroopSummoned>.Subscribe(OnTroopSummoned);
            EventBus<AbilityRequested>.Subscribe(OnAbilityRequested);
            EventBus<GoldDropped>.Subscribe(OnGoldDropped);
            EventBus<LevelEnded>.Subscribe(OnLevelEnded);
        }

        void OnDisable()
        {
            EventBus<PlaySfx>.Unsubscribe(OnPlaySfx);
            EventBus<PlaySfxById>.Unsubscribe(OnPlaySfxById);
            EventBus<PlayMusic>.Unsubscribe(OnPlayMusic);
            EventBus<PlayAmbient>.Unsubscribe(OnPlayAmbient);
            EventBus<EntityHurt>.Unsubscribe(OnHurt);
            EventBus<EntityDied>.Unsubscribe(OnDied);
            EventBus<HeroActed>.Unsubscribe(OnHeroActed);
            EventBus<BossSpawned>.Unsubscribe(OnBossSpawned);
            EventBus<TroopSummoned>.Unsubscribe(OnTroopSummoned);
            EventBus<AbilityRequested>.Unsubscribe(OnAbilityRequested);
            EventBus<GoldDropped>.Unsubscribe(OnGoldDropped);
            EventBus<LevelEnded>.Unsubscribe(OnLevelEnded);
        }

        void OnHurt(EntityHurt evt) { PlayById("hit"); }
        void OnDied(EntityDied evt) { PlayById("death"); }
        void OnBossSpawned(BossSpawned evt) { PlayById("boss_roar"); }
        void OnTroopSummoned(TroopSummoned evt) { PlayById("summon"); }
        void OnAbilityRequested(AbilityRequested evt) { PlayById("ability"); }
        void OnGoldDropped(GoldDropped evt) { PlayById("coin"); }
        void OnLevelEnded(LevelEnded evt) { PlayById(evt.Outcome == LevelOutcome.Victory ? "victory" : "defeat"); }

        void OnHeroActed(HeroActed evt)
        {
            if (evt.Action == HeroAction.Sword)
                PlayById("attack_sword");
            else if (evt.Action == HeroAction.Whip)
                PlayById("attack_whip");
        }

        void OnPlaySfx(PlaySfx evt) { PlaySfxCue(evt.Cue); }
        void OnPlaySfxById(PlaySfxById evt) { PlayById(evt.CueId); }
        void OnPlayMusic(PlayMusic evt) { SwitchMusic(evt.CueId); }
        void OnPlayAmbient(PlayAmbient evt) { SwitchAmbient(evt.CueId); }

        void PlayById(string id)
        {
            if (library != null)
                PlaySfxCue(library.Find(id));
        }

        public void SetVolumes(float musicVolume, float sfxVolume)
        {
            MusicVolume = Mathf.Clamp01(musicVolume);
            SfxVolume = Mathf.Clamp01(sfxVolume);
            ApplyMusicVolumes();
        }

        public static int FindFreeSource(bool[] busy)
        {
            for (int i = 0; i < busy.Length; i++)
                if (!busy[i])
                    return i;
            return -1;
        }

        public static void CrossfadeVolumes(float progress, out float outgoing, out float incoming)
        {
            progress = Mathf.Clamp01(progress);
            outgoing = 1f - progress;
            incoming = progress;
        }

        readonly bool[] busyBuffer = new bool[SfxSourceCount];

        public bool PlaySfxCue(AudioCue cue)
        {
            if (cue == null || !cue.HasClips || sfx == null)
                return false;
            var now = Time.unscaledTime;
            if (now - cue.lastPlayTime < cue.minInterval)
                return false;

            for (int i = 0; i < sfx.Length; i++)
                busyBuffer[i] = sfx[i].isPlaying;
            var index = FindFreeSource(busyBuffer);
            if (index < 0)
                return false;

            cue.lastPlayTime = now;
            var source = sfx[index];
            source.clip = cue.clips[Random.Range(0, cue.clips.Length)];
            source.pitch = Random.Range(cue.pitchRange.x, cue.pitchRange.y);
            source.volume = cue.volume * SfxVolume;
            source.loop = cue.loop;
            source.Play();
            return true;
        }

        void SwitchMusic(string id)
        {
            if (id == currentMusicId)
                return;
            currentMusicId = id;
            var cue = library != null ? library.Find(id) : null;

            activeMusic = 1 - activeMusic;
            var incoming = music[activeMusic];
            if (cue != null && cue.HasClips)
            {
                incoming.clip = cue.clips[Random.Range(0, cue.clips.Length)];
                incoming.Play();
                musicCueVolume = cue.volume;
            }
            else
            {
                incoming.Stop();
            }
            fadeProgress = 0f;
            ApplyMusicVolumes();
        }

        void SwitchAmbient(string id)
        {
            if (id == currentAmbientId)
                return;
            currentAmbientId = id;
            var cue = library != null ? library.Find(id) : null;
            if (cue != null && cue.HasClips)
            {
                ambient.clip = cue.clips[0];
                ambient.Play();
                ambientCueVolume = cue.volume;
            }
            else
            {
                ambient.Stop();
            }
            ambient.volume = ambientCueVolume * SfxVolume;
        }

        void Update()
        {
            if (fadeProgress < 1f)
            {
                fadeProgress = Mathf.Min(1f, fadeProgress + Time.unscaledDeltaTime / CrossfadeSeconds);
                ApplyMusicVolumes();
                if (fadeProgress >= 1f)
                    music[1 - activeMusic].Stop();
            }
            if (ambient != null)
                ambient.volume = ambientCueVolume * SfxVolume;
        }

        void ApplyMusicVolumes()
        {
            if (music == null)
                return;
            CrossfadeVolumes(fadeProgress, out var outgoing, out var incoming);
            music[activeMusic].volume = incoming * musicCueVolume * MusicVolume;
            music[1 - activeMusic].volume = outgoing * musicCueVolume * MusicVolume;
        }
    }
}
