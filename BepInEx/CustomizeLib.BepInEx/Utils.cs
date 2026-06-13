// #define DEBUG_FEATURE__ENABLE_MULTI_LEVEL_BUFF // 启用多级词条

using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx.ExtensionData.Basic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Debug = UnityEngine.Debug;

namespace CustomizeLib.BepInEx
{
    public struct CustomLevelData
    {
        public CustomLevelData()
        {
        }

        public Func<List<int>> AdvBuffs { get; set; } = () => [];
        public MusicType BgmType { get; set; } = MusicType.Day;
        public Board.BoardTag BoardTag { get; set; } = default;
        public Func<List<PlantType>> ConveyBeltPlantTypes { get; set; } = () => [];
        public Func<List<int>> Debuffs { get; set; } = () => [];
        public int ID { get; set; }
        public Sprite Logo { get; set; } = new();
        public Func<string> Name { get; set; } = () => "";
        public bool NeedSelectCard { get; set; } = true;
        public Action<Board> PostBoard { get; set; } = (_) => { };
        public Action<InitBoard> PostInitBoard { get; set; } = (_) => { };
        public Action PreInitBoard { get; set; } = () => { };
        public Func<List<(int, int, PlantType)>> PrePlants { get; set; } = () => [];
        public Func<List<PlantType>> PreSelectCards { get; set; } = () => [];
        public bool RealBoss2 { get; set; } = false;
        public int RowCount { get; set; } = 5;
        public SceneType SceneType { get; set; } = SceneType.Day;
        public Func<List<PlantType>> SeedRainPlantTypes { get; set; } = () => [];
        public Func<int> Sun { get; set; } = () => 500;
        public Func<List<(int, int)>> UltiBuffs { get; set; } = () => [];
        public Func<int> WaveCount { get; set; } = () => 10;
        public Func<int> ZombieHealthRate { get; set; } = () => 1;
        public Func<List<ZombieType>> ZombieList { get; set; } = () => [];
    }

    public struct CustomPlantAlmanac
    {
        public string Description { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public struct CustomPlantData
    {
        public int ID { get; set; }
        public PlantDataManager.PlantData PlantData { get; set; }
        public GameObject Prefab { get; set; }
        public GameObject Preview { get; set; }

        public List<(BulletType, List<GameObject?>)>? BulletList { get; set; }
    }

    /// <summary>
    /// 用于储存皮肤数据
    /// </summary>
    public struct CustomTypeMgrExtraSkin
    {
        public CustomTypeMgrExtraSkin()
        {
        }

        public int BigNut { get; set; } = -1;
        public int BigZombie { get; set; } = -1;
        public int DoubleBoxPlants { get; set; } = -1;
        public int EliteZombie { get; set; } = -1;
        public int FlyingPlants { get; set; } = -1;
        public int IsAirZombie { get; set; } = -1;
        public int IsCaltrop { get; set; } = -1;
        public int IsCustomPlant { get; set; } = -1;
        public int IsFirePlant { get; set; } = -1;
        public int IsIcePlant { get; set; } = -1;
        public int IsMagnetPlants { get; set; } = -1;
        public int IsNut { get; set; } = -1;
        public int IsPlantern { get; set; } = -1;
        public int IsPot { get; set; } = -1;
        public int IsPotatoMine { get; set; } = -1;
        public int IsPuff { get; set; } = -1;
        public int IsPumpkin { get; set; } = -1;
        public int IsSmallRangeLantern { get; set; } = -1;
        public int IsSpecialPlant { get; set; } = -1;
        public int IsSpickRock { get; set; } = -1;
        public int IsTallNut { get; set; } = -1;
        public int IsTangkelp { get; set; } = -1;
        public int IsWaterPlant { get; set; } = -1;
        public int NotRandomBungiZombie { get; set; } = -1;
        public int NotRandomZombie { get; set; } = -1;
        public int UltimateZombie { get; set; } = -1;
        public int UmbrellaPlants { get; set; } = -1;
        public int UselessHypnoZombie { get; set; } = -1;
        public int WaterZombie { get; set; } = -1;
    }

    public struct CustomClickCardOnPlant
    {
        public bool BlockFusion { get; set; } = false;
        public TriggerType Trigger { get; set; } = TriggerType.All;
        public bool SaveOrigin { get; set; } = false;

        public CustomClickCardOnPlant()
        {
            BlockFusion = false;
            Trigger = TriggerType.All;
            SaveOrigin = false;
        }

        public enum TriggerType
        {
            All = 0,
            CardOnly = 1,
            GloveOnly = 2
        }
    }
    public struct BuffBgType
    {
        public int BgType = 0;

        public static BuffBgType Day = new BuffBgType(0);
        public static BuffBgType Night = new BuffBgType(1);
        public static BuffBgType Pool = new BuffBgType(2);

        public BuffBgType() { BgType = 0; }
        public BuffBgType(int bgType) { BgType = bgType; }
        public BuffBgType(TravelBuffOptionButton.BgType bgType) { BgType = (int)bgType; }
        public BuffBgType(TravelStoreWindow.BgType bgType) { BgType = (int)bgType; }

        public static implicit operator int(BuffBgType bgType) => bgType.BgType;
        public static implicit operator TravelBuffOptionButton.BgType(BuffBgType bgType) => (TravelBuffOptionButton.BgType)bgType.BgType;
        public static implicit operator TravelStoreWindow.BgType(BuffBgType bgType) => (TravelStoreWindow.BgType)bgType.BgType;
        public static implicit operator BuffBgType(int bgType) => new BuffBgType(bgType);
        public static implicit operator BuffBgType(TravelBuffOptionButton.BgType bgType) => new BuffBgType(bgType);
        public static implicit operator BuffBgType(TravelStoreWindow.BgType bgType) => new BuffBgType(bgType);
    }

    /// <summary>
    /// 自定义词条类型(在词条图鉴中显示)
    /// </summary>
    public enum AlmanacBuffType
    {
        /// <summary>
        /// 弱究
        /// </summary>
        WeakUltimate,
        /// <summary>
        /// 强究
        /// </summary>
        StrongUltimate,
        /// <summary>
        /// 通用
        /// </summary>
        General,
        /// <summary>
        /// 随机
        /// </summary>
        Random,
        /// <summary>
        /// 诅咒
        /// </summary>
        Curse,
        /// <summary>
        /// 进化
        /// </summary>
        Rogue,
        /// <summary>
        /// 连携
        /// </summary>
        Combo,
        /// <summary>
        /// 小小词条
        /// </summary>
        Tiny,
        /// <summary>
        /// 僵尸
        /// </summary>
        Zombie,
        /// <summary>
        /// 诸神
        /// </summary>
        Shooting
    }

    public class ZombieAttrTimers
    {
        public Zombie zombie;

        #region 黄油
        public float butterTimer
        {
            get => zombie.TryGetEffect<ButterEffect>(EffectType.Butter, out var effect) ? effect.duration : -1f;
            set
            {
                if (zombie.TryGetEffect<ButterEffect>(EffectType.Butter, out var effect))
                    effect.duration = value;
            }
        }
        public bool isButter => butterTimer > 0f;
        #endregion
        #region 寒冷
        public float coldTimer
        {
            get => zombie.TryGetEffect<ColdEffect>(EffectType.Cold, out var effect) ? effect.duration : -1f;
            set
            {
                if (zombie.TryGetEffect<ColdEffect>(EffectType.Cold, out var effect))
                    effect.duration = value;
            }
        }
        public bool isCold => coldTimer > 0f;
        #endregion
        #region 冻结
        public float freezeTimer
        {
            get => zombie.TryGetEffect<FreezeEffect>(EffectType.Freeze, out var effect) ? effect.duration : -1f;
            set
            {
                if (zombie.TryGetEffect<FreezeEffect>(EffectType.Freeze, out var effect))
                    effect.duration = value;
            }
        }
        public bool isFreeze => freezeTimer > 0f;
        #endregion
        #region 免疫
        public float immuneTimer
        {
            get => zombie.TryGetEffect<ImmuneEffect>(EffectType.Immune, out var effect) ? effect.duration : -1f;
            set
            {
                if (zombie.TryGetEffect<ImmuneEffect>(EffectType.Immune, out var effect))
                    effect.duration = value;
            }
        }
        public bool isImmune => immuneTimer > 0f;
        #endregion
        #region 水草
        public float kelpTimer
        {
            get => zombie.TryGetEffect<KelpEffect>(EffectType.Kelp, out var effect) ? effect.duration : -1f;
            set
            {
                if (zombie.TryGetEffect<KelpEffect>(EffectType.Kelp, out var effect))
                    effect.duration = value;
            }
        }
        public bool isKelp => kelpTimer > 0f;
        #endregion
        #region 毒
        public float poisonTimer
        {
            get => zombie.TryGetEffect<PoisonEffect>(EffectType.Poison, out var effect) ? effect.duration : -1f;
            set
            {
                if (zombie.TryGetEffect<PoisonEffect>(EffectType.Poison, out var effect))
                    effect.duration = value;
            }
        }
        public bool isPoison => poisonTimer > 0f;
        #endregion
        #region 超时空
        public float portaledTimer
        {
            get => zombie.TryGetEffect<PortalEffect>(EffectType.Portal, out var effect) ? effect.duration : -1f;
            set
            {
                if (zombie.TryGetEffect<PortalEffect>(EffectType.Portal, out var effect))
                    effect.duration = value;
            }
        }
        public bool isPortaled => portaledTimer > 0f;
        #endregion
        #region 红温
        public bool isJalaed => zombie.TryGetEffect<JalaEffect>(EffectType.Jala, out var _);
        #endregion
        #region 余烬
        public bool isEmbered => zombie.TryGetEffect<EmberEffect>(EffectType.Ember, out var _);
        #endregion
    }

    public struct PlantAlmanac
    {
        public string info = "";
        public string cost = "";
        public string introduce = "";
        public string name = "";
        public PlantType plantType = PlantType.Nothing;
        public PlantAlmanac() { }
    }

    /// <summary>
    /// 旧版兼容
    /// </summary>
    public static class GameExtensions
    {
        public static void UnCold(this Zombie zombie) => zombie.GetAttrTimers().coldTimer = 0f;
        public static void TakeDamage(this Zombie zombie, DamageType theDamageType, int theDamage, PlantType reportType = PlantType.Nothing, bool fix = false)
            => zombie.TakeDamage(theDamage, CustomDamageMaker.DamageMaker, theDamageType, reportType, fix);
        public static void TakeDamage(this Zombie zombie, DmgType theDamageType, int theDamage, PlantType reportType = PlantType.Nothing, bool fix = false)
            => zombie.TakeDamage(theDamage, CustomDamageMaker.DamageMaker, (DamageType)(int)theDamageType, reportType, fix);
        public static void TakeDamage(this Plant plant, int damage, int damageType = 0) =>
            plant.TakeDamage(damage, CustomDamageMaker.DamageMaker, (DamageType)damageType);
        public static void Explode(this BombCherry cherry) => cherry.Explode(CustomDamageMaker.DamageMaker);
    }

    /// <summary>
    /// 旧版本DmgType留存实现
    /// </summary>
    public enum DmgType
    {
        Normal = 0,
        NormalAll = 1,
        Ice = 2,
        IceAll = 3,
        Shieldless = 4,
        IceShieldless = 5,
        RealDamage = 6,
        Explode = 10,
        Squash = 11,
        Carred = 12,
        Hammer = 13,
        MaxDamage = 14,
        CherryExplode = 15,
        JackboxExplode = 16,
        UltimateTallNutAll = 17,
        DoomExplode = 18,
        UltimateBamboo = 19
    }

    #region 无尽额外信息
    public struct CustomEndlessPlantData
    {
        public object value;
        public Type type;
        public int row;
        public int col;
        public PlantType pt;
    }

    public struct CustomEndlessData
    {
        public List<CustomEndlessPlantData> plantDatas;
    }
    #endregion

    public static class TravelExtensions
    {
        public const string BUFF_TYPEDATA = "CustomizeLib_BuffOptionType";
        public const string BUFF_IDDATA = "CustomizeLib_BuffOptionID";

        /// <summary>
        /// 获取TravelBuffOptionButton的类型和ID信息
        /// </summary>
        /// <param name="option"></param>
        /// <returns>(类型, ID)</returns>
        /// <exception cref="InvalidOperationException">TravelBuffOptionButton实例未被设置类型及ID信息</exception>
        public static (BuffType, int) TryGetTypeAndID(this TravelBuffOptionButton option)
        {
            if (option.GetData(BUFF_TYPEDATA) != null && option.GetData(BUFF_IDDATA) != null)
                return (option.GetData<BuffType>(BUFF_TYPEDATA), option.GetData<int>(BUFF_IDDATA));
            throw new InvalidOperationException("Option data is not exist");
        }

        /// <summary>
        /// 设置TravelBuffOptionButton的类型和ID信息，仅为TryGetData提供数据
        /// </summary>
        /// <param name="option"></param>
        /// <param name="type">类型</param>
        /// <param name="id">ID</param>
        public static void SetTypeAndID(this TravelBuffOptionButton option, BuffType type, int id)
        {
            option.SetData(BUFF_TYPEDATA, type);
            option.SetData(BUFF_IDDATA, id);
        }

        /// <summary>
        /// 获取TravelStoreWindow的类型和ID信息
        /// </summary>
        /// <param name="option"></param>
        /// <returns>(类型, ID)</returns>
        /// <exception cref="InvalidOperationException">TravelStoreWindow实例未被设置类型及ID信息</exception>
        public static (BuffType, int) TryGetTypeAndID(this TravelStoreWindow option)
        {
            if (option.GetData(BUFF_TYPEDATA) != null && option.GetData(BUFF_IDDATA) != null)
                return (option.GetData<BuffType>(BUFF_TYPEDATA), option.GetData<int>(BUFF_IDDATA));
            throw new InvalidOperationException("Option data is not exist");
        }

        /// <summary>
        /// 设置TravelStoreWindow的类型和ID信息，仅为TryGetData提供数据
        /// </summary>
        /// <param name="option"></param>
        /// <param name="type">类型</param>
        /// <param name="id">ID</param>
        public static void SetTypeAndID(this TravelStoreWindow option, BuffType type, int id)
        {
            option.SetData(BUFF_TYPEDATA, type);
            option.SetData(BUFF_IDDATA, id);
        }

        public static (BuffType, int) GetTypeAndID(Il2CppSystem.Object buff)
        {
            BuffType buffType = (BuffType)(-1);
            int id = -1;
            if (buff.IsTypeOf<AdvBuff>())
            {
                buffType = BuffType.AdvancedBuff;
                id = (int)buff.Unbox<AdvBuff>();
            }
            else if (buff.IsTypeOf<UltiBuff>())
            {
                buffType = BuffType.UltimateBuff;
                id = (int)buff.Unbox<UltiBuff>();
            }
            else if (buff.IsTypeOf<TravelDebuff>())
            {
                buffType = BuffType.Debuff;
                id = (int)buff.Unbox<TravelDebuff>();
            }
            else if (buff.IsTypeOf<InvestBuff>())
            {
                buffType = BuffType.InvestmentBuff;
                id = (int)buff.Unbox<InvestBuff>();
            }
            else if (buff.IsTypeOf<TravelUnlocks>())
            {
                buffType = BuffType.UnlockPlant;
                id = (int)buff.Unbox<TravelUnlocks>();
            }
            return (buffType, id);
        }

        public static (BuffType, int) GeneralSet(this TravelBuffOptionButton option, Il2CppSystem.Object buff)
        {
            var tuple = GetTypeAndID(buff);
            option.SetTypeAndID(tuple.Item1, tuple.Item2);
            return tuple;
        }

        public static (BuffType, int) GeneralSet(this TravelStoreWindow window, Il2CppSystem.Object buff)
        {
            var tuple = GetTypeAndID(buff);
            window.SetTypeAndID(tuple.Item1, tuple.Item2);
            return tuple;
        }

        public static Type GetBuffType(BuffType buffType)
        {
            switch (buffType)
            {
                case BuffType.AdvancedBuff: return typeof(AdvBuff);
                case BuffType.UltimateBuff: return typeof(UltiBuff);
                case BuffType.Debuff: return typeof(TravelDebuff);
                case BuffType.InvestmentBuff: return typeof(InvestBuff);
                case BuffType.UnlockPlant: return typeof(TravelUnlocks);
            }
            return null;
        }
    }

    public static class Extensions
    {
        public static void AddValueToTypeMgrExtraSkinBackup(this CustomTypeMgrExtraSkin typeMgrExtraSkinFromJson, PlantType plantType)
        {
            CustomCore.TypeMgrExtraSkinBackup.BigNut.Add(plantType, typeMgrExtraSkinFromJson.BigNut);
            //CustomCore.TypeMgrExtraSkinBackup.BigZombie.Add(plantType, typeMgrExtraSkinFromJson.BigZombie);
            CustomCore.TypeMgrExtraSkinBackup.DoubleBoxPlants.Add(plantType, typeMgrExtraSkinFromJson.DoubleBoxPlants);
            //CustomCore.TypeMgrExtraSkinBackup.EliteZombie.Add(plantType, typeMgrExtraSkinFromJson.EliteZombie);
            CustomCore.TypeMgrExtraSkinBackup.FlyingPlants.Add(plantType, typeMgrExtraSkinFromJson.FlyingPlants);
            //CustomCore.TypeMgrExtraSkinBackup.IsAirZombie.Add(plantType, typeMgrExtraSkinFromJson.IsAirZombie);
            CustomCore.TypeMgrExtraSkinBackup.IsCaltrop.Add(plantType, typeMgrExtraSkinFromJson.IsCaltrop);
            CustomCore.TypeMgrExtraSkinBackup.IsCustomPlant.Add(plantType, typeMgrExtraSkinFromJson.IsCustomPlant);
            CustomCore.TypeMgrExtraSkinBackup.IsFirePlant.Add(plantType, typeMgrExtraSkinFromJson.IsFirePlant);
            CustomCore.TypeMgrExtraSkinBackup.IsIcePlant.Add(plantType, typeMgrExtraSkinFromJson.IsIcePlant);
            CustomCore.TypeMgrExtraSkinBackup.IsMagnetPlants.Add(plantType, typeMgrExtraSkinFromJson.IsMagnetPlants);
            CustomCore.TypeMgrExtraSkinBackup.IsNut.Add(plantType, typeMgrExtraSkinFromJson.IsNut);
            CustomCore.TypeMgrExtraSkinBackup.IsPlantern.Add(plantType, typeMgrExtraSkinFromJson.IsPlantern);
            CustomCore.TypeMgrExtraSkinBackup.IsPot.Add(plantType, typeMgrExtraSkinFromJson.IsPot);
            CustomCore.TypeMgrExtraSkinBackup.IsPotatoMine.Add(plantType, typeMgrExtraSkinFromJson.IsPotatoMine);
            CustomCore.TypeMgrExtraSkinBackup.IsPuff.Add(plantType, typeMgrExtraSkinFromJson.IsPuff);
            CustomCore.TypeMgrExtraSkinBackup.IsPumpkin.Add(plantType, typeMgrExtraSkinFromJson.IsPumpkin);
            CustomCore.TypeMgrExtraSkinBackup.IsSmallRangeLantern.Add(plantType, typeMgrExtraSkinFromJson.IsSmallRangeLantern);
            CustomCore.TypeMgrExtraSkinBackup.IsSpecialPlant.Add(plantType, typeMgrExtraSkinFromJson.IsSpecialPlant);
            CustomCore.TypeMgrExtraSkinBackup.IsSpickRock.Add(plantType, typeMgrExtraSkinFromJson.IsSpickRock);
            CustomCore.TypeMgrExtraSkinBackup.IsTallNut.Add(plantType, typeMgrExtraSkinFromJson.IsTallNut);
            CustomCore.TypeMgrExtraSkinBackup.IsTangkelp.Add(plantType, typeMgrExtraSkinFromJson.IsTangkelp);
            CustomCore.TypeMgrExtraSkinBackup.IsWaterPlant.Add(plantType, typeMgrExtraSkinFromJson.IsWaterPlant);
            //CustomCore.TypeMgrExtraSkinBackup.NotRandomBungiZombie.Add(plantType,typeMgrExtraSkinFromJson.NotRandomBungiZombie);
            //CustomCore.TypeMgrExtraSkinBackup.NotRandomZombie.Add(plantType, typeMgrExtraSkinFromJson.NotRandomZombie);
            //CustomCore.TypeMgrExtraSkinBackup.UltimateZombie.Add(plantType, typeMgrExtraSkinFromJson.UltimateZombie);
            CustomCore.TypeMgrExtraSkinBackup.UmbrellaPlants.Add(plantType, typeMgrExtraSkinFromJson.UmbrellaPlants);
            //CustomCore.TypeMgrExtraSkinBackup.UselessHypnoZombie.Add(plantType, typeMgrExtraSkinFromJson.UselessHypnoZombie);
            //CustomCore.TypeMgrExtraSkinBackup.WaterZombie.Add(plantType, typeMgrExtraSkinFromJson.WaterZombie);
        }

        public static void DisableDisMix(this Plant plant) => (plant.firstParent, plant.secondParent) = (PlantType.Nothing, PlantType.Nothing);

        public static void FindCardUIAndChangeSprite(this Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                CardUI cardUI = parent.GetChild(i).GetComponent<CardUI>();
                if (cardUI != null)
                {
                    Mouse.Instance.ChangeCardSprite((PlantType)cardUI.theSeedType, cardUI);
                }

                // 递归查找子物体的子物体
                FindCardUIAndChangeSprite(parent.GetChild(i));
            }
        }


        //递归，找shoot，但是一些奇怪的植物不行
        public static void FindShoot(this Plant plant, Transform parent)
        {
            String name = parent.name.ToLower();
            if (name == "shoot" || name == "shoot1")
                plant.shoot = parent;
            if (name == "shoot2")
                plant.shoot2 = parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                plant.FindShoot(parent.GetChild(i));
            }
        }

        public static T GetAsset<T>(this AssetBundle ab, string name) where T : UnityEngine.Object
        {
            foreach (var ase in ab.LoadAllAssetsAsync().allAssets)
            {
                if (ase.TryCast<T>()?.name == name)
                {
                    return ase.Cast<T>();
                }
            }
            throw new ArgumentException($"Could not find {name} from {ab.name}");
        }

        public static T GetRandomItem<T>(this IList<T> list) => list[UnityEngine.Random.RandomRangeInt(0, list.Count)];

        public static int GetTotalHealth(this Zombie zombie) => (int)zombie.theHealth + zombie.theFirstArmorHealth + zombie.theSecondArmorHealth;

        public static bool ObjectExist<T>(this Board board) => board.GameObject().transform.GetComponentsInChildren<T>().Length > 0;

        /// <summary>
        /// 将Texture2D转换为Sprite
        /// </summary>
        /// <param name="texture2D">Texture2D对象</param>
        /// <returns>Sprite对象</returns>
        public static Sprite ToSprite(this Texture2D texture2D) =>
            Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));

        public static List<string> GetAssetBundleAssetNames(this AssetBundle assetBundle)
        {
            if (assetBundle == null)
            {
                CustomCore.Instance.Value.Log.LogError("Failed to get AssetBundle!");
                return new List<string>();
            }

            List<string> assetNames = new List<string>();

            foreach (var asset in assetBundle.LoadAllAssets())
            {
                assetNames.Add(asset.name);
            }
            return assetNames;
        }

        public static void AddLayer(this Transform transform, int level)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).IsObjExist() && transform.GetChild(i).GetComponent<SpriteRenderer>() != null)
                {
                    transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder += level;
                    AddLayer(transform.GetChild(i), level);
                }
            }
        }

        public static ZombieAttrTimers GetAttrTimers(this Zombie zombie)
        {
            if (zombie.GetData("CustomizeLib_AttrTimers") == null) // 如果尚未被获取过
            {
                var timer = new ZombieAttrTimers
                {
                    zombie = zombie
                };
                zombie.SetData("CustomizeLib_AttrTimers", timer);
                return timer;
            }
            return zombie.GetData<ZombieAttrTimers>("CustomizeLib_AttrTimers"); // 如果获取过就直接返回
        }

        public static ZombieAttrTimers GetAttrTimers(this Zombie zombie, out ZombieAttrTimers timers)
        {
            if (zombie.GetData("CustomizeLib_AttrTimers") == null) // 如果尚未被获取过
            {
                var timer = new ZombieAttrTimers
                {
                    zombie = zombie
                };
                zombie.SetData("CustomizeLib_AttrTimers", timer);
                timers = timer;
                return timer;
            }
            timers = zombie.GetData<ZombieAttrTimers>("CustomizeLib_AttrTimers"); // 如果获取过就直接返回
            return zombie.GetData<ZombieAttrTimers>("CustomizeLib_AttrTimers");
        }

        public static void SwapTypeMgrExtraSkinAndBackup(PlantType plantType)
        {
            // BigNut
            if (CustomCore.TypeMgrExtraSkin.BigNut.TryGetValue(plantType, out int value1))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.BigNut.TryAdd(plantType, value1))
                {
                    CustomCore.TypeMgrExtraSkin.BigNut.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.BigNut.TryGetValue(plantType, out int value2))
            {
                if (CustomCore.TypeMgrExtraSkin.BigNut.TryAdd(plantType, value2))
                {
                    CustomCore.TypeMgrExtraSkinBackup.BigNut.Remove(plantType);
                }
            }

            // // BigZombie
            // if (CustomCore.TypeMgrExtraSkin.BigZombie.TryGetValue(plantType, out int value3))
            // {
            //     if (!CustomCore.TypeMgrExtraSkinBackup.BigZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkinBackup.BigZombie[plantType] = value3;
            //         CustomCore.TypeMgrExtraSkin.BigZombie.Remove(plantType);
            //     }
            // }
            // else if (CustomCore.TypeMgrExtraSkinBackup.BigZombie.TryGetValue(plantType, out int value4))
            // {
            //     if (!CustomCore.TypeMgrExtraSkin.BigZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkin.BigZombie[plantType] = value4;
            //         CustomCore.TypeMgrExtraSkinBackup.BigZombie.Remove(plantType);
            //     }
            // }

            // DoubleBoxPlants
            if (CustomCore.TypeMgrExtraSkin.DoubleBoxPlants.TryGetValue(plantType, out int value5))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.DoubleBoxPlants.TryAdd(plantType, value5))
                {
                    CustomCore.TypeMgrExtraSkin.DoubleBoxPlants.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.DoubleBoxPlants.TryGetValue(plantType, out int value6))
            {
                if (CustomCore.TypeMgrExtraSkin.DoubleBoxPlants.TryAdd(plantType, value6))
                {
                    CustomCore.TypeMgrExtraSkinBackup.DoubleBoxPlants.Remove(plantType);
                }
            }

            // // EliteZombie
            // if (CustomCore.TypeMgrExtraSkin.EliteZombie.TryGetValue(plantType, out int value7))
            // {
            //     if (!CustomCore.TypeMgrExtraSkinBackup.EliteZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkinBackup.EliteZombie[plantType] = value7;
            //         CustomCore.TypeMgrExtraSkin.EliteZombie.Remove(plantType);
            //     }
            // }
            // else if (CustomCore.TypeMgrExtraSkinBackup.EliteZombie.TryGetValue(plantType, out int value8))
            // {
            //     if (!CustomCore.TypeMgrExtraSkin.EliteZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkin.EliteZombie[plantType] = value8;
            //         CustomCore.TypeMgrExtraSkinBackup.EliteZombie.Remove(plantType);
            //     }
            // }

            // FlyingPlants
            if (CustomCore.TypeMgrExtraSkin.FlyingPlants.TryGetValue(plantType, out int value9))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.FlyingPlants.TryAdd(plantType, value9))
                {
                    CustomCore.TypeMgrExtraSkin.FlyingPlants.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.FlyingPlants.TryGetValue(plantType, out int value10))
            {
                if (CustomCore.TypeMgrExtraSkin.FlyingPlants.TryAdd(plantType, value10))
                {
                    CustomCore.TypeMgrExtraSkinBackup.FlyingPlants.Remove(plantType);
                }
            }

            // // IsAirZombie
            // if (CustomCore.TypeMgrExtraSkin.IsAirZombie.TryGetValue(plantType, out int value11))
            // {
            //     if (!CustomCore.TypeMgrExtraSkinBackup.IsAirZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkinBackup.IsAirZombie[plantType] = value11;
            //         CustomCore.TypeMgrExtraSkin.IsAirZombie.Remove(plantType);
            //     }
            // }
            // else if (CustomCore.TypeMgrExtraSkinBackup.IsAirZombie.TryGetValue(plantType, out int value12))
            // {
            //     if (!CustomCore.TypeMgrExtraSkin.IsAirZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkin.IsAirZombie[plantType] = value12;
            //         CustomCore.TypeMgrExtraSkinBackup.IsAirZombie.Remove(plantType);
            //     }
            // }

            // IsCaltrop
            if (CustomCore.TypeMgrExtraSkin.IsCaltrop.TryGetValue(plantType, out int value13))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsCaltrop.TryAdd(plantType, value13))
                {
                    CustomCore.TypeMgrExtraSkin.IsCaltrop.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsCaltrop.TryGetValue(plantType, out int value14))
            {
                if (CustomCore.TypeMgrExtraSkin.IsCaltrop.TryAdd(plantType, value14))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsCaltrop.Remove(plantType);
                }
            }

            // IsCustomPlant
            if (CustomCore.TypeMgrExtraSkin.IsCustomPlant.TryGetValue(plantType, out int value15))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsCustomPlant.TryAdd(plantType, value15))
                {
                    CustomCore.TypeMgrExtraSkin.IsCustomPlant.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsCustomPlant.TryGetValue(plantType, out int value16))
            {
                if (CustomCore.TypeMgrExtraSkin.IsCustomPlant.TryAdd(plantType, value16))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsCustomPlant.Remove(plantType);
                }
            }

            // IsFirePlant
            if (CustomCore.TypeMgrExtraSkin.IsFirePlant.TryGetValue(plantType, out int value17))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsFirePlant.TryAdd(plantType, value17))
                {
                    CustomCore.TypeMgrExtraSkin.IsFirePlant.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsFirePlant.TryGetValue(plantType, out int value18))
            {
                if (CustomCore.TypeMgrExtraSkin.IsFirePlant.TryAdd(plantType, value18))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsFirePlant.Remove(plantType);
                }
            }

            // IsIcePlant
            if (CustomCore.TypeMgrExtraSkin.IsIcePlant.TryGetValue(plantType, out int value19))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsIcePlant.TryAdd(plantType, value19))
                {
                    CustomCore.TypeMgrExtraSkin.IsIcePlant.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsIcePlant.TryGetValue(plantType, out int value20))
            {
                if (CustomCore.TypeMgrExtraSkin.IsIcePlant.TryAdd(plantType, value20))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsIcePlant.Remove(plantType);
                }
            }

            // IsMagnetPlants
            if (CustomCore.TypeMgrExtraSkin.IsMagnetPlants.TryGetValue(plantType, out int value21))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsMagnetPlants.TryAdd(plantType, value21))
                {
                    CustomCore.TypeMgrExtraSkin.IsMagnetPlants.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsMagnetPlants.TryGetValue(plantType, out int value22))
            {
                if (CustomCore.TypeMgrExtraSkin.IsMagnetPlants.TryAdd(plantType, value22))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsMagnetPlants.Remove(plantType);
                }
            }

            // IsNut
            if (CustomCore.TypeMgrExtraSkin.IsNut.TryGetValue(plantType, out int value23))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsNut.TryAdd(plantType, value23))
                {
                    CustomCore.TypeMgrExtraSkin.IsNut.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsNut.TryGetValue(plantType, out int value24))
            {
                if (CustomCore.TypeMgrExtraSkin.IsNut.TryAdd(plantType, value24))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsNut.Remove(plantType);
                }
            }

            // IsPlantern
            if (CustomCore.TypeMgrExtraSkin.IsPlantern.TryGetValue(plantType, out int value25))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsPlantern.TryAdd(plantType, value25))
                {
                    CustomCore.TypeMgrExtraSkin.IsPlantern.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsPlantern.TryGetValue(plantType, out int value26))
            {
                if (CustomCore.TypeMgrExtraSkin.IsPlantern.TryAdd(plantType, value26))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsPlantern.Remove(plantType);
                }
            }

            // IsPot
            if (CustomCore.TypeMgrExtraSkin.IsPot.TryGetValue(plantType, out int value27))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsPot.TryAdd(plantType, value27))
                {
                    CustomCore.TypeMgrExtraSkin.IsPot.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsPot.TryGetValue(plantType, out int value28))
            {
                if (CustomCore.TypeMgrExtraSkin.IsPot.TryAdd(plantType, value28))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsPot.Remove(plantType);
                }
            }

            // IsPotatoMine
            if (CustomCore.TypeMgrExtraSkin.IsPotatoMine.TryGetValue(plantType, out int value29))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsPotatoMine.TryAdd(plantType, value29))
                {
                    CustomCore.TypeMgrExtraSkin.IsPotatoMine.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsPotatoMine.TryGetValue(plantType, out int value30))
            {
                if (CustomCore.TypeMgrExtraSkin.IsPotatoMine.TryAdd(plantType, value30))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsPotatoMine.Remove(plantType);
                }
            }

            // IsPuff
            if (CustomCore.TypeMgrExtraSkin.IsPuff.TryGetValue(plantType, out int value31))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsPuff.TryAdd(plantType, value31))
                {
                    CustomCore.TypeMgrExtraSkin.IsPuff.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsPuff.TryGetValue(plantType, out int value32))
            {
                if (CustomCore.TypeMgrExtraSkin.IsPuff.TryAdd(plantType, value32))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsPuff.Remove(plantType);
                }
            }

            // IsPumpkin
            if (CustomCore.TypeMgrExtraSkin.IsPumpkin.TryGetValue(plantType, out int value33))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsPumpkin.TryAdd(plantType, value33))
                {
                    CustomCore.TypeMgrExtraSkin.IsPumpkin.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsPumpkin.TryGetValue(plantType, out int value34))
            {
                if (CustomCore.TypeMgrExtraSkin.IsPumpkin.TryAdd(plantType, value34))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsPumpkin.Remove(plantType);
                }
            }

            // IsSmallRangeLantern
            if (CustomCore.TypeMgrExtraSkin.IsSmallRangeLantern.TryGetValue(plantType, out int value35))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsSmallRangeLantern.TryAdd(plantType, value35))
                {
                    CustomCore.TypeMgrExtraSkin.IsSmallRangeLantern.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsSmallRangeLantern.TryGetValue(plantType, out int value36))
            {
                if (CustomCore.TypeMgrExtraSkin.IsSmallRangeLantern.TryAdd(plantType, value36))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsSmallRangeLantern.Remove(plantType);
                }
            }

            // IsSpecialPlant
            if (CustomCore.TypeMgrExtraSkin.IsSpecialPlant.TryGetValue(plantType, out int value37))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsSpecialPlant.TryAdd(plantType, value37))
                {
                    CustomCore.TypeMgrExtraSkin.IsSpecialPlant.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsSpecialPlant.TryGetValue(plantType, out int value38))
            {
                if (CustomCore.TypeMgrExtraSkin.IsSpecialPlant.TryAdd(plantType, value38))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsSpecialPlant.Remove(plantType);
                }
            }

            // IsSpickRock
            if (CustomCore.TypeMgrExtraSkin.IsSpickRock.TryGetValue(plantType, out int value39))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsSpickRock.TryAdd(plantType, value39))
                {
                    CustomCore.TypeMgrExtraSkin.IsSpickRock.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsSpickRock.TryGetValue(plantType, out int value40))
            {
                if (CustomCore.TypeMgrExtraSkin.IsSpickRock.TryAdd(plantType, value40))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsSpickRock.Remove(plantType);
                }
            }

            // IsTallNut
            if (CustomCore.TypeMgrExtraSkin.IsTallNut.TryGetValue(plantType, out int value41))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsTallNut.TryAdd(plantType, value41))
                {
                    CustomCore.TypeMgrExtraSkin.IsTallNut.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsTallNut.TryGetValue(plantType, out int value42))
            {
                if (CustomCore.TypeMgrExtraSkin.IsTallNut.TryAdd(plantType, value42))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsTallNut.Remove(plantType);
                }
            }

            // IsTangkelp
            if (CustomCore.TypeMgrExtraSkin.IsTangkelp.TryGetValue(plantType, out int value43))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsTangkelp.TryAdd(plantType, value43))
                {
                    CustomCore.TypeMgrExtraSkin.IsTangkelp.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsTangkelp.TryGetValue(plantType, out int value44))
            {
                if (CustomCore.TypeMgrExtraSkin.IsTangkelp.TryAdd(plantType, value44))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsTangkelp.Remove(plantType);
                }
            }

            // IsWaterPlant
            if (CustomCore.TypeMgrExtraSkin.IsWaterPlant.TryGetValue(plantType, out int value45))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsWaterPlant.TryAdd(plantType, value45))
                {
                    CustomCore.TypeMgrExtraSkin.IsWaterPlant.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsWaterPlant.TryGetValue(plantType, out int value46))
            {
                if (CustomCore.TypeMgrExtraSkin.IsWaterPlant.TryAdd(plantType, value46))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsWaterPlant.Remove(plantType);
                }
            }

            // // NotRandomBungiZombie
            // if (CustomCore.TypeMgrExtraSkin.NotRandomBungiZombie.TryGetValue(plantType, out int value47))
            // {
            //     if (!CustomCore.TypeMgrExtraSkinBackup.NotRandomBungiZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkinBackup.NotRandomBungiZombie[plantType] = value47;
            //         CustomCore.TypeMgrExtraSkin.NotRandomBungiZombie.Remove(plantType);
            //     }
            // }
            // else if (CustomCore.TypeMgrExtraSkinBackup.NotRandomBungiZombie.TryGetValue(plantType, out int value48))
            // {
            //     if (!CustomCore.TypeMgrExtraSkin.NotRandomBungiZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkin.NotRandomBungiZombie[plantType] = value48;
            //         CustomCore.TypeMgrExtraSkinBackup.NotRandomBungiZombie.Remove(plantType);
            //     }
            // }

            // // NotRandomZombie
            // if (CustomCore.TypeMgrExtraSkin.NotRandomZombie.TryGetValue(plantType, out int value49))
            // {
            //     if (!CustomCore.TypeMgrExtraSkinBackup.NotRandomZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkinBackup.NotRandomZombie[plantType] = value49;
            //         CustomCore.TypeMgrExtraSkin.NotRandomZombie.Remove(plantType);
            //     }
            // }
            // else if (CustomCore.TypeMgrExtraSkinBackup.NotRandomZombie.TryGetValue(plantType, out int value50))
            // {
            //     if (!CustomCore.TypeMgrExtraSkin.NotRandomZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkin.NotRandomZombie[plantType] = value50;
            //         CustomCore.TypeMgrExtraSkinBackup.NotRandomZombie.Remove(plantType);
            //     }
            // }

            // // UltimateZombie
            // if (CustomCore.TypeMgrExtraSkin.UltimateZombie.TryGetValue(plantType, out int value51))
            // {
            //     if (!CustomCore.TypeMgrExtraSkinBackup.UltimateZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkinBackup.UltimateZombie[plantType] = value51;
            //         CustomCore.TypeMgrExtraSkin.UltimateZombie.Remove(plantType);
            //     }
            // }
            // else if (CustomCore.TypeMgrExtraSkinBackup.UltimateZombie.TryGetValue(plantType, out int value52))
            // {
            //     if (!CustomCore.TypeMgrExtraSkin.UltimateZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkin.UltimateZombie[plantType] = value52;
            //         CustomCore.TypeMgrExtraSkinBackup.UltimateZombie.Remove(plantType);
            //     }
            // }

            // UmbrellaPlants
            if (CustomCore.TypeMgrExtraSkin.UmbrellaPlants.TryGetValue(plantType, out int value53))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.UmbrellaPlants.TryAdd(plantType, value53))
                {
                    CustomCore.TypeMgrExtraSkin.UmbrellaPlants.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.UmbrellaPlants.TryGetValue(plantType, out int value54))
            {
                if (CustomCore.TypeMgrExtraSkin.UmbrellaPlants.TryAdd(plantType, value54))
                {
                    CustomCore.TypeMgrExtraSkinBackup.UmbrellaPlants.Remove(plantType);
                }
            }

            // // UselessHypnoZombie
            // if (CustomCore.TypeMgrExtraSkin.UselessHypnoZombie.TryGetValue(plantType, out int value55))
            // {
            //     if (!CustomCore.TypeMgrExtraSkinBackup.UselessHypnoZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkinBackup.UselessHypnoZombie[plantType] = value55;
            //         CustomCore.TypeMgrExtraSkin.UselessHypnoZombie.Remove(plantType);
            //     }
            // }
            // else if (CustomCore.TypeMgrExtraSkinBackup.UselessHypnoZombie.TryGetValue(plantType, out int value56))
            // {
            //     if (!CustomCore.TypeMgrExtraSkin.UselessHypnoZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkin.UselessHypnoZombie[plantType] = value56;
            //         CustomCore.TypeMgrExtraSkinBackup.UselessHypnoZombie.Remove(plantType);
            //     }
            // }

            // // WaterZombie
            // if (CustomCore.TypeMgrExtraSkin.WaterZombie.TryGetValue(plantType, out int value57))
            // {
            //     if (!CustomCore.TypeMgrExtraSkinBackup.WaterZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkinBackup.WaterZombie[plantType] = value57;
            //         CustomCore.TypeMgrExtraSkin.WaterZombie.Remove(plantType);
            //     }
            // }
            // else if (CustomCore.TypeMgrExtraSkinBackup.WaterZombie.TryGetValue(plantType, out int value58))
            // {
            //     if (!CustomCore.TypeMgrExtraSkin.WaterZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkin.WaterZombie[plantType] = value58;
            //         CustomCore.TypeMgrExtraSkinBackup.WaterZombie.Remove(plantType);
            //     }
            // }
        }

        public static List<(T1, T2)> ToEnumList<T1, T2>(this List<(int, int)> list) where T1 : Enum where T2 : Enum
        {
            var result = new List<(T1, T2)>();
            foreach (var (v1, v2) in list)
                result.Add((v1.ToEnumVal<T1>(), v2.ToEnumVal<T2>()));
            return result;
        }

        public static List<(int, int)> ToIntegerList<T1, T2>(this List<(T1, T2)> list) where T1 : Enum where T2 : Enum =>
            [.. list.Select(tuple => (tuple.Item1.ToIntVal(), tuple.Item2.ToIntVal()))];

        public static T ToEnumVal<T>(this int value) where T : Enum => (T)Enum.ToObject(typeof(T), value);
        public static int ToIntVal<T>(this T value) where T : Enum => (int)Enum.ToObject(typeof(T), value);
    }

    public static class Utils
    {
        public static bool InGame() => GameAPP.theGameStatus is GameStatus.InGame or GameStatus.Pause;

        public static bool IsCustomLevel(out CustomLevelData levelData)
        {
            if (GameAPP.theBoardType == CustomLevelType)
            {
                levelData = CustomCore.CustomLevels[GameAPP.theBoardLevel];
                return true;
            }
            else
            {
                levelData = default;
                return false;
            }
        }

        public static bool IsGameRunning() => GameAPP.theGameStatus is GameStatus.InGame;

        public static bool IsNotNull<T>(this T obj) => obj is not null;

        public static int ToInt(this bool value) => value ? 1 : 0;

        public static LevelType CustomLevelType => (LevelType)66;

        /// <summary>
        /// 获取卡牌GameObject
        /// </summary>
        /// <returns>卡牌GameObject，Child 0:PacketBg背景图，Child 1：默认展示，1有CardUI组件</returns>
        public static GameObject? GetColorfulCardGameObject()
        {
            if (Board.Instance is not null && !Board.Instance.boardTag.isIZ)
            {
                GameObject? MyCard = null;
                MyCard = InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer/ColorCards/SampleGrid(Clone)").GetChild(0).gameObject;
                return MyCard;
            }
            else if (Board.Instance is not null && Board.Instance.boardTag.isIZ)
            {

                GameObject? MyCard = null;
                MyCard = IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid/ColorfulCards/Page1/CattailGirl").gameObject;
                return MyCard;
            }
            return null;
        }

        /// <summary>
        /// 获取卡牌GameObject
        /// </summary>
        /// <returns>卡牌GameObject，Child 0:PacketBg背景图，Child 1:二次选卡，Child 2：默认展示，12均有CardUI组件</returns>
        public static GameObject? GetNormalCardGameObject()
        {
            if (Board.Instance is not null && !Board.Instance.boardTag.isIZ)
            {
                GameObject? MyCard = null;
                MyCard = InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer/NormalCards/SampleGrid(Clone)").GetChild(0).gameObject;
                return MyCard;
            }
            else if (Board.Instance is not null && Board.Instance.boardTag.isIZ)
            {
                GameObject? MyCard = null;
                MyCard = IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid/Main/Page1/PeaShooter").gameObject;
                return MyCard;
            }
            return null;
        }

        /// <summary>
        /// 获取彩卡选卡父级
        /// </summary>
        /// <returns></returns>
        public static Transform? GetColorfulCardParent()
        {
            if (Board.Instance != null && !Board.Instance.boardTag.isIZ)
            {
                return InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer/ColorCards/SampleGrid(Clone)");
            }
            else if (Board.Instance != null && Board.Instance.boardTag.isIZ)
            {
                return IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid/ColorfulCards/Page1");
            }
            return null;
        }

        public static Transform? GetNormalCardParent()
        {
            if (Board.Instance != null && Board.Instance.boardTag.isTowerDefence)
            {
                return InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer/TowerCards/Page1");
            }
            else if (Board.Instance != null && !Board.Instance.boardTag.isIZ)
            {
                return InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer/NormalCards/SampleGrid(Clone)");
            }
            else if (Board.Instance != null && Board.Instance.boardTag.isIZ)
            {
                return IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid/Pages/Page1");
            }
            return null;
        }

        /// <summary>
        /// 自定义词条是否是多级词条
        /// </summary>
        /// <param name="buffType">词条类型</param>
        /// <param name="returnID">对应的数组的ID（索引），即注册词条是返回的ID</param>
        /// <returns>Item1: 是否是多级词条, Item2: 在CustomBuffsLevel中的索引</returns>
        public static (bool, int) IsMultiLevelBuff(BuffType buffType, int returnID)
        {
            var list = CustomCore.CustomBuffsLevel.Where(kvp => kvp.Key.Item1 == buffType && kvp.Key.Item2 == returnID).ToList();
            if (list.Count > 0)
            {
                var index = list[0].Value.Item1;
                return (list.Count > 0, index);
            }
            return (false, -1);
        }

        public static int TravelCustomBuffLevel(BuffType buffType, int returnID)
        {
            var result = IsMultiLevelBuff(buffType, returnID);
            if (result.Item1)
            {
                if (TravelMgr.Instance is null)
                    return 0;
                var array = (int[])TravelMgr.Instance.GetData("CustomBuffsLevel");
                if (array is null)
                    return 0;
                return array[result.Item2];
            }
            return 0;
        }

        public static bool IsCheat()
        {
            return GameAPP.developerMode;
        }
        public static bool EnableTravelPlant()
        {
            return Board.Instance.boardTag.enableAllTravelPlant || Board.Instance.boardTag.isSuperRandom || Board.Instance.boardTag.isUltimateSuperRandom || IsCheat() || Board.Instance.boardTag.isTravel;
        }
    }

    // json对象
    public class JsonSkinObject
    {
        public Dictionary<int, int> CustomBulletType { get; set; } =
            [];

        public CustomPlantData CustomPlantData { get; set; }
        public CustomPlantAlmanac PlantAlmanac { get; set; }
        public CustomTypeMgrExtraSkin TypeMgrExtraSkin { get; set; }
    }
}