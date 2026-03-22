using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
#pragma warning disable
namespace CustomizeLib.BepInEx
{
    public static class Tools
    {
        public static Assembly GetAssembly() => Assembly.GetCallingAssembly();
        public static Assembly Assembly {
            get {
                return Assembly.GetCallingAssembly();
            }
        }

        public static void InitMod(bool skipRegister = false) => InitMod(Assembly.GetCallingAssembly(), skipRegister);

        public static void InitMod(Assembly assembly, bool skipRegister = false)
        {
            Console.OutputEncoding = Encoding.UTF8;
            if (!skipRegister)
            {
                var types = GetAllMonoBehaviourTypes(assembly);
                foreach (var type in types)
                    if (!ClassInjector.IsTypeRegisteredInIl2Cpp(type))
                        ClassInjector.RegisterTypeInIl2Cpp(type);
            }
            Harmony.CreateAndPatchAll(assembly);
        }

        public static Type[] GetAllMonoBehaviourTypes(Assembly assembly)
        {
            try
            {
                // 获取所有类型
                Type[] allTypes = assembly.GetTypes();

                // 筛选继承自MonoBehaviour的类型
                return allTypes
                    .Where(type => typeof(MonoBehaviour).IsAssignableFrom(type) &&
                                  !type.IsAbstract &&
                                  !type.IsInterface)
                    .ToArray();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // 处理类型加载异常
                return ex.Types
                    .Where(type => type != null &&
                                  typeof(MonoBehaviour).IsAssignableFrom(type) &&
                                  !type.IsAbstract &&
                                  !type.IsInterface)
                    .ToArray();
            }
        }
    }
    public struct ID
    {
        public int id = 0;
        public ID(int id) { this.id = id; }
        public ID(PlantType id) { this.id = (int)id; }
        public ID(ZombieType id) { this.id = (int)id; }
        public ID(ParticleType id) { this.id = (int)id; }
        public ID(BulletType id) { this.id = (int)id; }
        public ID(CherryBombType id) { this.id = (int)id; }
        public static implicit operator int(ID id) => id.id;
        public static implicit operator PlantType(ID id) => (PlantType)id.id;
        public static implicit operator ZombieType(ID id) => (ZombieType)id.id;
        public static implicit operator ParticleType(ID id) => (ParticleType)id.id;
        public static implicit operator BulletType(ID id) => (BulletType)id.id;
        public static implicit operator CherryBombType(ID id) => (CherryBombType)id.id;
        public static implicit operator ID(int i) => new ID(i);
        public static implicit operator ID(PlantType id) => new ID(id);
        public static implicit operator ID(ZombieType id) => new ID(id);
        public static implicit operator ID(ParticleType id) => new ID(id);
        public static implicit operator ID(BulletType id) => new ID(id);
        public static implicit operator ID(CherryBombType id) => new ID(id);

        public override string ToString()
        {
            return id.ToString();
        }
    }

    public struct BuffID
    {
        public int id = 0;
        public BuffID(int id) { this.id = id; }
        public BuffID(AdvBuff id) { this.id = (int)id; }
        public BuffID(UltiBuff id) { this.id = (int)id; }
        public BuffID(TravelDebuff id) { this.id = (int)id; }
        public BuffID(TravelUnlocks id) { this.id = (int)id; }

        public static implicit operator AdvBuff(BuffID id) => (AdvBuff)id.id;
        public static implicit operator UltiBuff(BuffID id) => (UltiBuff)id.id;
        public static implicit operator TravelDebuff(BuffID id) => (TravelDebuff)id.id;
        public static implicit operator TravelUnlocks(BuffID id) => (TravelUnlocks)id.id;
        public static implicit operator BuffID(AdvBuff i) => new BuffID(i);
        public static implicit operator BuffID(UltiBuff id) => new BuffID(id);
        public static implicit operator BuffID(TravelDebuff id) => new BuffID(id);
        public static implicit operator BuffID(TravelUnlocks id) => new BuffID(id);
        public static implicit operator BuffID(int id) => new BuffID(id);
    }

    public static class Extension
    {
        public static T? GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject != null && gameObject.TryGetComponent<T>(out var component) && component != null)
                return component;
            else if (gameObject != null)
                return gameObject.AddComponent<T>();
            return null;
        }

        public static T? GetOrAddComponent<T>(this Transform gameObject) where T : Component
        {
            if (gameObject != null && gameObject.TryGetComponent<T>(out var component) && component != null)
                return component;
            else if (gameObject != null)
                return gameObject.AddComponent<T>();
            return null;
        }

        public static T? GetOrAddComponent<T>(this Component gameObject) where T : Component
        {
            if (gameObject != null && gameObject.TryGetComponent<T>(out var component) && component != null)
                return component;
            else if (gameObject != null)
                return gameObject.AddComponent<T>();
            return null;
        }

        public static Coroutine StartCoroutine(this MonoBehaviour self, IEnumerator routine)
        {
            return global::BepInEx.Unity.IL2CPP.Utils.MonoBehaviourExtensions.StartCoroutine(self, routine);
        }
    }

    public class CorePlugin : BasePlugin
    {
        public static List<Action> OnGameInitAction = new();

        public ManualLogSource Logger;

        public override void Load()
        {
            Logger = base.Log;
            Tools.InitMod(GetType().Assembly);
            OnStart();
            OnGameInitAction.Add(OnGameInit);
        }

        public virtual void OnStart() { }

        public virtual void OnGameInit() { }
    }

    public static class CoreTools
    {
        public static Dictionary<string, AdvBuff> AdvBuffPair = new();
        public static Dictionary<string, UltiBuff> UltiBuffPair = new();

        public static void InitBuffDic()
        {
            TravelDictionary.advancedBuffsText = new();
            TravelDictionary.AdvBuffPlantPairs = new();
            TravelDictionary.allStrongUltimtePlant = new();
            TravelDictionary.CurseBuffs = new();
            TravelDictionary.debuffData = new();
            TravelDictionary.PlantInfo = new();
            TravelDictionary.RandomBuffs = new();
            TravelDictionary.RogueBuffs = new();
            TravelDictionary.ultimateBuffsText = new();
            TravelDictionary.unlocksText = new();
        }

        public static IEnumerator Init()
        {
            while (TravelDictionary.advancedBuffsText.Count < Enum.GetValues<AdvBuff>().Length) yield return new WaitForSeconds(1f);
            foreach (var (buff, str) in TravelDictionary.advancedBuffsText)
            {
                int index = str.IndexOf('：');
                if (index == -1)
                    index = str.IndexOf(":");
                if (index != -1)
                {
                    if (!AdvBuffPair.ContainsKey(str.Substring(0, index)))
                        AdvBuffPair.Add(str.Substring(0, index), buff);
                }
            }

            while (TravelDictionary.ultimateBuffsText.Count < Enum.GetValues<UltiBuff>().Length) yield return new WaitForSeconds(1f);
            foreach (var (buff, str) in TravelDictionary.ultimateBuffsText)
            {
                int index = str.IndexOf('：');
                if (index == -1)
                    index = str.IndexOf(":");
                if (index != -1)
                {
                    if (!UltiBuffPair.ContainsKey(str.Substring(0, index)))
                        UltiBuffPair.Add(str.Substring(0, index), buff);
                }
            }
            yield break;
        }

        /// <summary>
        /// 根据词条名称获取普通词条,请在GameAPP实例化后调用
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static AdvBuff GetAdvBuffByString(string name)
        {
            if (AdvBuffPair.ContainsKey(name))
                return AdvBuffPair[name];
            return (AdvBuff)(-1);
        }

        /// <summary>
        /// 根据词条名称获取究极词条,请在GameAPP实例化后调用
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static UltiBuff GetUltiBuffByString(string name)
        {
            if (UltiBuffPair.ContainsKey(name))
                return UltiBuffPair[name];
            return (UltiBuff)(-1);
        }


        public static TravelDebuff GetTravelDebuffByString(string name)
        {
            var id = -1;
            #region 映射
            switch (name)
            {
                case "二爷1":
                    id = 0;
                    break;
                case "二爷2":
                    id = 1;
                    break;
                case "黑橄榄1":
                    id = 2;
                    break;
                case "黑橄榄2":
                    id = 3;
                    break;
                case "机鱼1":
                    id = 4;
                    break;
                case "机鱼2":
                    id = 5;
                    break;
                case "基洛夫1":
                    id = 6;
                    break;
                case "基洛夫2":
                    id = 7;
                    break;
                case "丑跳1":
                    id = 8;
                    break;
                case "丑跳2":
                    id = 9;
                    break;
                case "三叉戟1":
                    id = 10;
                    break;
                case "三叉戟2":
                    id = 11;
                    break;
                case "白舞王1":
                    id = 12;
                    break;
                case "白舞王2":
                    id = 13;
                    break;
                case "尸王1":
                    id = 14;
                    break;
                case "尸王2":
                    id = 15;
                    break;
                case "冲车1":
                    id = 16;
                    break;
                case "植物概率死亡":
                    id = 17;
                    break;
                case "阳光归零":
                    id = 18;
                    break;
                case "血量翻倍":
                    id = 19;
                    break;
                case "更多僵尸":
                    id = 20;
                    break;
                case "黑车1":
                    id = 21;
                    break;
                case "黑车2":
                    id = 22;
                    break;
                case "博士":
                    id = 23;
                    break;
                case "究极马超":
                    id = 24;
                    break;
                case "究极鱼丸":
                    id = 25;
                    break;
                case "究极将军":
                    id = 26;
                    break;
                case "究极裂空":
                    id = 27;
                    break;
                case "究极白车":
                    id = 28;
                    break;
                case "究极读报":
                    id = 29;
                    break;
                case "蹦极":
                    id = 30;
                    break;
                case "究极丑皇":
                    id = 31;
                    break;
                case "复活":
                    id = 32;
                    break;
                case "植物数量限制":
                    id = 33;
                    break;
                case "究极阿尔法":
                    id = 34;
                    break;
                case "阿尔法1":
                    id = 35;
                    break;
                case "冲车2":
                    id = 36;
                    break;
                case "究极大帅":
                    id = 37;
                    break;
                case "旅行飞碟1":
                    id = 38;
                    break;
                case "旅行飞碟2":
                    id = 39;
                    break;
                case "特种巨人1":
                    id = 40;
                    break;
                case "特种巨人2":
                    id = 41;
                    break;
                case "堡垒巨人1":
                    id = 42;
                    break;
                case "特种黄金巨人":
                    id = 43;
                    break;
                case "毁灭机枪二爷":
                    id = 44;
                    break;
                case "鱼丸1":
                    id = 45;
                    break;
                case "鱼丸2":
                    id = 46;
                    break;
                case "重症难题":
                    id = 47;
                    break;
                case "永久创伤":
                    id = 48;
                    break;
            }
            #endregion
            return (TravelDebuff)id;
        }

        public static TravelUnlocks GetTravelUnlocksByString(string name)
        {
            var id = -1;
            #region 映射
            switch (name)
            {
                case "UltimateChomper":
                    id = 0;
                    break;
                case "UltimateGatling":
                    id = 1;
                    break;
                case "UltimateFume":
                    id = 2;
                    break;
                case "UltimateTorch":
                    id = 3;
                    break;
                case "UltimateStar":
                    id = 4;
                    break;
                case "UltimateGloom":
                    id = 5;
                    break;
                case "UltimateMelon":
                    id = 6;
                    break;
                case "UltimateCannon":
                    id = 7;
                    break;
                case "UltimateTallNut":
                    id = 8;
                    break;
                case "UltimateHypno":
                    id = 9;
                    break;
                case "UltimateBigGatling":
                    id = 10;
                    break;
                case "UltimateCabbage":
                    id = 11;
                    break;
                case "UltimatePumpkin":
                    id = 12;
                    break;
                case "UltimateSpring":
                    id = 13;
                    break;
                case "UltimateKelp":
                    id = 14;
                    break;
                case "UltimateCorn":
                    id = 15;
                    break;
                case "UltimateSpruce":
                    id = 16;
                    break;
                case "UltimateBigChomper":
                    id = 17;
                    break;
                case "UltimateExplodeCannon":
                    id = 18;
                    break;
                case "UltimateSunflower":
                    id = 19;
                    break;
                case "UltimateWinterMelon":
                    id = 20;
                    break;
                case "UltimateCattail":
                    id = 21;
                    break;
                case "UltimateSeaShroom":
                    id = 22;
                    break;
                case "UltimateJalapeno":
                    id = 23;
                    break;
            }
            #endregion
            return (TravelUnlocks)id;
        }

        public static bool TravelAdvanced(string name) => Lawnf.TravelAdvanced(GetAdvBuffByString(name));

        public static bool TravelUltimate(string name) => Lawnf.TravelUltimate(GetUltiBuffByString(name));
        public static int TravelUltimateLevel(string name) => Lawnf.TravelUltimateLevel(GetUltiBuffByString(name));

        public static List<int> Range(int end = 1)
        {
            var result = new List<int>();
            for (int i = 0; i < end; i++)
                result.Add(i);
            return result;
        }

        public static GameObject CreateCherryExplodeCustom(Vector2 v, int theRow, CherryBombType bombType = CherryBombType.Normal,
            int damage = 1800, PlantType fromType = PlantType.Nothing, Il2CppSystem.Action<Zombie> action = null, bool immediately = true,
            bool shake = true)
        {
            var particle = CreateParticle.SetParticle(CustomCore.CustomCherryStartID + (int)bombType, v, 11);
            if (shake)
                ScreenShake.TriggerShake(0.15f);
            GameAPP.PlaySound(40, 0.5f, 1.0f);

            BombCherry cherry = new BombCherry();
            cherry.board = Board.Instance;
            cherry.damageToZombie = damage;
            cherry.bombRow = theRow;
            cherry.bombType = bombType;
            cherry.zombieAction = action;
            cherry.bombPosition = v;
            cherry.fromType = fromType;
            cherry.targetPlant = null;

            if (immediately)
            {
                cherry.Explode();
            }

            return particle;
        }

        /// <summary>
        /// 创建樱桃爆炸
        /// </summary>
        /// <param name="v">位置</param>
        /// <param name="theRow">行</param>
        /// <param name="bombType">樱桃爆炸类型</param>
        /// <param name="damage">伤害</param>
        /// <param name="fromType">来源</param>
        /// <param name="action">僵尸操作</param>
        /// <param name="immediately">是否立即引爆</param>
        /// <param name="shake">是否晃动屏幕</param>
        /// <param name="soundID">音效ID</param>
        /// <param name="volume">音量(子弹填0.2f)</param>
        /// <param name="pitch"></param>
        /// <returns></returns>
        public static (BombCherry, GameObject) CreateCherryExplode(Vector2 v, int theRow, CherryBombType bombType = CherryBombType.Normal,
            int damage = 1800, PlantType fromType = PlantType.Nothing, Il2CppSystem.Action<Zombie> action = null, bool immediately = true,
            bool shake = true, int soundID = 40, float volume = 0.5f, float pitch = 1f)
        {
            if (Board.Instance == null || Board.Instance.IsDestroyed()) return (null, null);
            GameObject particle = null;

            if (CustomCore.CustomCherrys.ContainsKey(bombType))
            {
                particle = CreateParticle.SetParticle(CustomCore.CustomCherryStartID + (int)bombType, v, 11);
                GameAPP.PlaySound(soundID, volume, pitch);
            }
            else
            {
                if (bombType == CherryBombType.Sun)
                {
                    particle = CreateParticle.SetParticle((int)ParticleType.SunBombCloud, v, 11).gameObject;
                }
                else if (bombType == CherryBombType.Bullet || bombType == CherryBombType.BulletAll)
                {
                    particle = CreateParticle.SetParticle((int)ParticleType.BombCloudSmall, v, 11).gameObject;
                }
                else if (bombType == CherryBombType.IceCharry)
                {
                    particle = CreateParticle.SetParticle((int)ParticleType.BombCloud_blue, v, 11).gameObject;
                }
                else
                {
                    particle = CreateParticle.SetParticle((int)ParticleType.BombCloud, v, 11).gameObject;
                }
                GameAPP.PlaySound(soundID, volume, pitch);
            }

            if (shake)
                ScreenShake.TriggerShake(0.15f);

            // 创建樱桃炸弹对象
            BombCherry cherryExplode = new BombCherry();

            // 设置炸弹属性
            cherryExplode.board = Board.Instance;
            cherryExplode.damageToZombie = damage;
            cherryExplode.bombRow = theRow;
            cherryExplode.bombType = bombType;
            cherryExplode.zombieAction = action;
            cherryExplode.bombPosition = v;
            cherryExplode.fromType = fromType;
            cherryExplode.targetPlant = null;

            // 如果设置为立即爆炸，则直接引爆
            if (immediately)
            {
                cherryExplode.Explode();
            }

            return (cherryExplode, particle);
        }
    }
}
