using System.Collections.Generic;
using EC.Core;
using EC.Data;
using EC.Gameplay;
using UnityEditor;
using UnityEngine;

public static class ChapterBuilder
{
    const string UnitsFolder = "Assets/_Project/ScriptableObjects/Units";
    const string BasesFolder = "Assets/_Project/ScriptableObjects/Bases";
    const string PrefabsFolder = "Assets/_Project/Prefabs";
    const string MaterialsFolder = "Assets/_Project/Art/Materials";
    const string ConfigPath = "Assets/_Project/ScriptableObjects/Lane/LaneConfig_Default.asset";

    class ChapterSpec
    {
        public int number;
        public string baseId;
        public string baseName;
        public int baseResistance;
        public Color baseColor;
        public string envName;
        public Color envTint;
        public string[][] levelEnemies;
        public int[] waveCounts;
        public int[] startCounts;
        public string[] levelNames;
        public string[][] introTexts;
        public string[][] outroTexts;
        public System.Func<BossDefinition> boss;
        public string[] epilogueTexts;
    }

    [MenuItem("EC/Level/Build Chapters 2 to 4")]
    public static void BuildAll()
    {
        BuildChapter(2);
        BuildChapter(3);
        BuildChapter(4);
        AssetDatabase.SaveAssets();
    }

    public static LevelDefinition[] BuildChapter(int number)
    {
        EnemyBaseBuilder.BuildAssets();
        ExtraEnemiesBuilder.BuildAssets();
        LevelBuilder.EnsureFolder("Assets/_Project/ScriptableObjects", "Levels");
        LevelBuilder.EnsureFolder("Assets/_Project/ScriptableObjects", "Stories");

        var spec = Spec(number);
        var baseDefinition = BuildBase(spec);
        var environment = BuildEnvironment(spec);
        var boss = spec.boss();

        var levels = new LevelDefinition[spec.levelNames.Length];
        for (int i = 0; i < levels.Length; i++)
        {
            var key = number + "_" + (i + 1);
            var isLast = i == levels.Length - 1;
            var waves = BuildWaves(key, spec.levelEnemies[i], spec.waveCounts[i], spec.startCounts[i], isLast ? boss : null);

            var level = LevelBuilder.LoadOrCreate<LevelDefinition>(LevelBuilder.LevelsFolder + "/Level_" + key + ".asset");
            level.id = "lv_" + key;
            level.displayName = spec.levelNames[i];
            level.baseDefinition = baseDefinition;
            level.environmentPrefab = environment;
            level.intro = spec.introTexts[i] != null ? LevelBuilder.Story("Intro_" + key, "story_" + key + "_intro", spec.introTexts[i]) : null;
            level.outro = spec.outroTexts[i] != null ? LevelBuilder.Story("Outro_" + key, "story_" + key + "_outro", spec.outroTexts[i]) : null;
            level.epilogue = isLast && spec.epilogueTexts != null ? LevelBuilder.Story("Epilogue_" + key, "story_" + key + "_epilogue", spec.epilogueTexts) : null;
            level.waves = waves;
            level.troopsEnabled = true;
            level.tutorial = false;
            var gold = 120 * number + 40 * i;
            level.reward = new LevelReward { goldFirstClear = gold, goldReplay = gold * 2 / 5, gemsFirstClear = isLast ? 5 : 2, ticketsFirstClear = isLast ? 2 : 1 };
            EditorUtility.SetDirty(level);
            levels[i] = level;
        }
        AssetDatabase.SaveAssets();
        return levels;
    }

    static ChapterSpec Spec(int number)
    {
        switch (number)
        {
            case 2:
                return new ChapterSpec
                {
                    number = 2,
                    baseId = "village_chapel", baseName = "Capilla de la aldea", baseResistance = 700, baseColor = new Color(0.55f, 0.52f, 0.5f),
                    envName = "Env_Village", envTint = new Color(1.3f, 1.1f, 0.9f),
                    levelNames = new[] { "Calles sitiadas", "Tejados y sombras", "Primera sangre", "El Capitán Vampiro" },
                    levelEnemies = new[] { new[] { "warg", "goblin" }, new[] { "bat", "imp" }, new[] { "goblin", "vampire" }, new[] { "warg", "goblin", "vampire", "bat" } },
                    waveCounts = new[] { 5, 6, 6, 7 },
                    startCounts = new[] { 4, 5, 3, 4 },
                    introTexts = new[]
                    {
                        new[] { "El Templario llega a la aldea sitiada. Los aldeanos se refugian en la capilla.", "Debe resistir hasta que alguien le diga hacia dónde llevaron al bebé." },
                        null,
                        null,
                        new[] { "Un capitán de las criaturas sabe hacia dónde llevaron al Bebé de la Profecía.", "Se alza sobre los tejados, rodeado de murciélagos." }
                    },
                    outroTexts = new[]
                    {
                        null,
                        null,
                        null,
                        new[] { "El Capitán cae y confiesa: el bebé fue llevado al bosque maldito.", "En la capilla, el Templario recupera su armadura y la bendición de los aldeanos." }
                    },
                    boss = VampireCaptain
                };
            case 3:
                return new ChapterSpec
                {
                    number = 3,
                    baseId = "forest_camp", baseName = "Campamento en el bosque", baseResistance = 850, baseColor = new Color(0.35f, 0.4f, 0.3f),
                    envName = "Env_CursedForest", envTint = new Color(0.6f, 1.2f, 0.7f),
                    levelNames = new[] { "Senda sin luz", "El pantano hambriento", "Niebla y alas", "El Troll de Pantano Gigante" },
                    levelEnemies = new[] { new[] { "werewolf", "imp" }, new[] { "swamp_troll", "goblin" }, new[] { "vampire", "bat", "werewolf" }, new[] { "swamp_troll", "werewolf", "vampire", "imp" } },
                    waveCounts = new[] { 6, 6, 7, 7 },
                    startCounts = new[] { 3, 2, 3, 3 },
                    introTexts = new[]
                    {
                        new[] { "El bosque maldito se cierra sobre el Templario. Un campamento abandonado es el único refugio.", "Algo antiguo se mueve entre los árboles." },
                        null,
                        null,
                        new[] { "El pantano ruge. Un troll descomunal bloquea el camino hacia la catedral." }
                    },
                    outroTexts = new[]
                    {
                        null,
                        null,
                        null,
                        new[] { "El troll se hunde en el fango. Entre sus restos, el Templario halla un escudo, una maza y una ballesta.", "El rastro del bebé sigue hacia la catedral del Señor Vampiro." }
                    },
                    boss = BogTrollGiant
                };
            default:
                return new ChapterSpec
                {
                    number = 4,
                    baseId = "monastery_gate", baseName = "Portón del monasterio", baseResistance = 1000, baseColor = new Color(0.4f, 0.38f, 0.5f),
                    envName = "Env_Cathedral", envTint = new Color(0.9f, 0.7f, 1.3f),
                    levelNames = new[] { "El portón", "Naves de sombra", "El claustro", "La cripta", "El Señor Vampiro" },
                    levelEnemies = new[] { new[] { "vampire", "goblin" }, new[] { "werewolf", "bat" }, new[] { "swamp_troll", "vampire" }, new[] { "goblin", "imp", "warg", "bat", "vampire", "werewolf", "swamp_troll" }, new[] { "vampire", "werewolf", "swamp_troll", "bat" } },
                    waveCounts = new[] { 7, 7, 8, 8, 4 },
                    startCounts = new[] { 4, 4, 3, 2, 3 },
                    introTexts = new[]
                    {
                        new[] { "La catedral se alza sobre el valle. El portón del monasterio es lo único que separa al Templario del Señor Vampiro." },
                        null,
                        null,
                        null,
                        new[] { "En el altar mayor aguarda el Señor Vampiro, con el Bebé de la Profecía en brazos.", "Esta es la última noche." }
                    },
                    outroTexts = new[]
                    {
                        null,
                        null,
                        null,
                        null,
                        new[] { "El Señor Vampiro se deshace en ceniza. El Templario recoge al bebé.", "Amanece sobre la catedral. La cruzada continúa, pero esta noche ha terminado." }
                    },
                    epilogueTexts = new[] { "Años después, el bebé crece bajo la protección de la Orden.", "La profecía aún no se ha cumplido, pero el Templario sabe que volverá a empuñar la espada si hace falta." },
                    boss = VampireLord
                };
        }
    }

    static BaseDefinition BuildBase(ChapterSpec spec)
    {
        var stages = new[]
        {
            EnemyBaseBuilder.CreateStageSprite("BaseStage_" + spec.baseId + "_0", spec.baseColor),
            EnemyBaseBuilder.CreateStageSprite("BaseStage_" + spec.baseId + "_1", Darken(spec.baseColor, 0.75f)),
            EnemyBaseBuilder.CreateStageSprite("BaseStage_" + spec.baseId + "_2", Darken(spec.baseColor, 0.5f))
        };
        var definition = LevelBuilder.LoadOrCreate<BaseDefinition>(BasesFolder + "/Base_" + spec.baseId + ".asset");
        definition.id = spec.baseId;
        definition.displayName = spec.baseName;
        definition.maxResistance = spec.baseResistance;
        definition.damageStages = stages;
        definition.prefab = EnemyBaseBuilder.CreateBasePrefab(definition, "Base_" + spec.baseId);
        EditorUtility.SetDirty(definition);
        return definition;
    }

    static Color Darken(Color color, float factor)
    {
        return new Color(color.r * factor, color.g * factor, color.b * factor, 1f);
    }

    static GameObject BuildEnvironment(ChapterSpec spec)
    {
        var config = AssetDatabase.LoadAssetAtPath<LaneConfig>(ConfigPath);
        LaneSandboxBuilder.CreateBackground(config);
        var root = GameObject.Find("Background");
        root.name = spec.envName;
        foreach (var renderer in root.GetComponentsInChildren<Renderer>())
        {
            var source = renderer.sharedMaterial;
            var path = MaterialsFolder + "/" + spec.envName + "_" + source.name + ".mat";
            var tinted = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (tinted == null)
            {
                tinted = new Material(source);
                AssetDatabase.CreateAsset(tinted, path);
            }
            var color = source.GetColor("_BaseColor");
            tinted.SetColor("_BaseColor", new Color(color.r * spec.envTint.r, color.g * spec.envTint.g, color.b * spec.envTint.b, color.a));
            EditorUtility.SetDirty(tinted);
            renderer.sharedMaterial = tinted;
        }
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabsFolder + "/" + spec.envName + ".prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    static WaveDefinition[] BuildWaves(string key, string[] enemyIds, int waveCount, int startCount, BossDefinition boss)
    {
        var waves = new WaveDefinition[waveCount];
        for (int i = 0; i < waveCount; i++)
        {
            var entries = new List<SpawnEntry>();
            for (int j = 0; j < enemyIds.Length; j++)
            {
                var count = Mathf.Max(2, startCount + i - j * 2);
                var interval = Mathf.Max(0.6f, 1.8f - 0.1f * i);
                entries.Add(LevelBuilder.Entry(FindEnemy(enemyIds[j]), count, interval, j * 2f));
            }
            var last = i == waveCount - 1 && boss != null;
            waves[i] = LevelBuilder.WaveFor(key, i + 1, last ? 0f : 5f, last ? boss : null, last ? 4f : 0f, entries.ToArray());
        }
        return waves;
    }

    static EnemyDefinition FindEnemy(string id)
    {
        var file = id == "warg" ? "Enemy_Warg" : id == "bat" ? "Enemy_Bat" : "Enemy_" + id;
        return AssetDatabase.LoadAssetAtPath<EnemyDefinition>(UnitsFolder + "/" + file + ".asset");
    }

    static BossDefinition VampireCaptain()
    {
        return BuildBoss("Boss_VampireCaptain", "boss_vampire_captain", "Capitán Vampiro", 700, 25, 2.0f, 1.6f, CreatureTag.Undead, new Vector3(1.4f, 1.6f, 1.4f), new Color(0.5f, 0.05f, 0.15f), 150, go =>
        {
            var blink = go.AddComponent<BlinkModule>();
            blink.distance = 5f;
            blink.interval = 6f;
            var summon = go.AddComponent<SummonModule>();
            summon.minion = FindEnemy("bat");
            summon.count = 3;
            summon.interval = 15f;
        });
    }

    static BossDefinition BogTrollGiant()
    {
        return BuildBoss("Boss_BogTrollGiant", "boss_bog_troll_giant", "Troll de Pantano Gigante", 1400, 30, 0.9f, 2.2f, CreatureTag.None, new Vector3(2.6f, 2.4f, 2.6f), new Color(0.3f, 0.4f, 0.2f), 250, go =>
        {
            go.AddComponent<RegenerationModule>().healthPerSecond = 5f;
            var strike = go.AddComponent<AreaStrikeModule>();
            strike.radius = 3f;
            strike.damage = 45;
            strike.telegraphSeconds = 1.2f;
            strike.vulnerableSeconds = 4f;
        });
    }

    static BossDefinition VampireLord()
    {
        return BuildBoss("Boss_VampireLord", "boss_vampire_lord", "Señor Vampiro", 2500, 40, 2.2f, 2.2f, CreatureTag.Undead, new Vector3(1.6f, 2f, 1.6f), new Color(0.35f, 0f, 0.1f), 500, go =>
        {
            var lord = go.AddComponent<VampireLordModule>();
            lord.orbPrefab = TroopBuilder.CreateBoltPrefab();
            lord.vampireMinion = FindEnemy("vampire");
            lord.batMinion = FindEnemy("bat");
        });
    }

    static BossDefinition BuildBoss(string name, string id, string displayName, int health, int damage, float speed, float range, CreatureTag tags, Vector3 scale, Color color, int gold, System.Action<GameObject> addModules)
    {
        var definition = LevelBuilder.LoadOrCreate<BossDefinition>(UnitsFolder + "/" + name + ".asset");
        definition.id = id;
        definition.displayName = displayName;
        definition.team = Team.Enemy;
        definition.kind = EnemyKind.Ground;
        definition.maxHealth = health;
        definition.attackDamage = damage;
        definition.moveSpeed = speed;
        definition.attackRange = range;
        definition.attackCooldown = 1.2f;
        definition.hurtDuration = 0.1f;
        definition.tags = tags;
        definition.goldDrop = gold;

        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = name;
        go.transform.localScale = scale;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = EnemyBaseBuilder.ColorMaterial(name, color);
        go.AddComponent<EntityController>().definition = definition;
        go.AddComponent<HealthComponent>();
        go.AddComponent<MovementComponent>().lane = AssetDatabase.LoadAssetAtPath<LaneConfig>(ConfigPath);
        go.AddComponent<AttackComponent>();
        go.AddComponent<EnemyBrain>();
        addModules(go);
        definition.prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabsFolder + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
        EditorUtility.SetDirty(definition);
        return definition;
    }
}
