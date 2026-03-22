using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SubspeciesEntry.BepInEx
{
    [BepInPlugin("salmon.subspeciesentry", "Subspecies Entry", "1.0")]
    public class Plugin : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }
    }

    public static class Core
    {
        public static IEnumerator Init()
        {
            while (TravelDictionary.advancedBuffsText.Count < Enum.GetValues<AdvBuff>().Length) yield return new WaitForSeconds(1f);
            while (TravelDictionary.ultimateBuffsText.Count < Enum.GetValues<UltiBuff>().Length) yield return new WaitForSeconds(1f);

            CoreTools.Init();
            Load();
            TypeInit.Init();
            CustomBehaviours.Init();
            yield break;
        }

        public static void Load()
        {
            InitBuffText();
            InitAlmanacText();
            InitCardClick();
            InitDataExtra();
        }

        public static void InitBuffText()
        {
            #region 大帝伴侣
            {
                // 流星雨
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("流星雨"),
                    "流星雨：究极杨桃大帝的攻击间隔降低至0.5秒", "流星雨：究极杨桃大帝的攻击间隔降低至0.5秒；亚种五叶草回旋加速降至0.5秒，吸引范围+50%");
                // 众星之力
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("众星之力"),
                    "众星之力：究极杨桃大帝的子弹伤害x2（2级时x3），但发射时不会超过3000（2级时无上限）",
                    "众星之力：究极杨桃大帝的子弹伤害x2，伤害上限增至3000；亚种五叶草子弹伤害x2。2级时，究极杨桃大帝伤害x3，取消伤害上限；亚种五叶草伤害x3，取消储存上限");
                // 斗转星移
                ReplaceText.ReplaceBuff(BuffType.AdvancedBuff, (int)CoreTools.GetAdvBuffByString("斗转星移"),
                    "斗转星移：所有的流星冷却缩短为原来的1/2，且场上每多一个究极杨桃大帝则提升300基础伤害",
                    "斗转星移：所有流星冷却缩短为原来的1/2，所有五叶草储存上限+50%，且场上每多一个究极杨桃大帝则提升300基础伤害，五叶草增加10储存上限");
            }
            #endregion
            #region 金蛋
            {
                // 无尽贪婪
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("无尽贪婪"),
                    "无尽贪婪：究极超时空玉米的黑洞可以吸引一切子弹",
                    "无尽贪婪：究极超时空玉米的黑洞可以吸引一切子弹；亚种超时空坚果每30秒立即回溯一次并回复2000韧性");
                // 万劫不复
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("万劫不复"),
                    "万劫不复：究极超时空玉米的黑洞吸引子弹的范围大幅增加",
                    "万劫不复：究极超时空玉米的黑洞吸引子弹的范围大幅增加；亚种超时空坚果每次回溯都会净化自身状态");
            }
            #endregion
            #region 血月÷子
            {
                // 金光闪闪
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("金光闪闪"),
                    "金光闪闪：太阳神发射子弹时，消耗超过15000部分的阳光的0.5%，使子弹增加消耗阳光数20倍的伤害，亚种月亮神子弹的光照等级增伤×3",
                    "金光闪闪：太阳神发射子弹时，消耗超过15000部分的阳光的0.5%，使子弹增加消耗阳光数20倍的伤害，亚种月亮神子弹的光照等级增伤×3；变种血月神的子弹的光照等级增伤x3，前20级光照等级，每级血月额外提供僵尸的增益提升50%");
                // 人造太阳
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("人造太阳"),
                    "人造太阳：太阳神卷心菜召唤的小太阳伤害x3，亚种月亮神卷心菜召唤的明月治疗量×3",
                    "人造太阳：太阳神卷心菜召唤的小太阳伤害x3，亚种月亮神卷心菜召唤的明月治疗量×3；变种血月神卷心菜召唤的血月持续时间x3，且召唤时间减至5秒");
            }
            #endregion
            #region 曾哥&牢灯
            {
                // 万籁俱寂
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("万籁俱寂"),
                    "万籁俱寂：究极忧郁菇亡语伤害增加到100万，亚种究极路灯花亡语提供的光照等级×3",
                    "万籁俱寂：究极忧郁菇死亡时或对其使用毁灭菇卡片时，造成1000万伤害并额外造成0.5倍韧性的伤害；亚种路灯花为全场提供的光照x3，并使全场目标的冻结值上限降至100");
                // 以爆制爆
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("以爆制爆"),
                    "以爆制爆：究极忧郁菇及亚种免疫非物理爆炸，每吸收10000点爆炸伤害将释放毁灭菇爆炸",
                    "以爆制爆：究极忧郁菇及亚种免疫非物理爆炸，每吸收或受到10000点伤害，释放一次（累计吸收伤害/3.8）伤害的毁灭菇效果，不留坑洞");
            }
            #endregion
            #region 鮟鱇鱼
            {
                // 深渊巨口
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("深渊巨口"),
                    "深渊巨口：提高究极大嘴花的攻击距离",
                    "深渊巨口：究极灯笼巨嘴花对于不可吞食的僵尸，对其造成僵尸韧性的（25+65x范围僵尸总血量/范围僵尸总血量+10^7）%的伤害，该效果会使最终消化时间改为60秒");
                // 光芒四射
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("光芒四射"),
                    "光芒四射：究极灯笼巨嘴咀嚼期间为5x5范围提供2级光照",
                    "光芒四射：究极灯笼巨嘴花吞食后，为自身半径3.7格范围的植物提供（1+吞噬僵尸数量x0.25）点的光照等级，持续15秒");
            }
            #endregion
        }

        public static void InitAlmanacText()
        {
            #region 大帝伴侣
            {
                ReplaceText.ReplaceAlmanac(PlantType.UltimateStar,
                    "子弹伤害×2，伤害上限增至3000。2级，子弹伤害×3，取消伤害上限",
                    "子弹伤害x2，伤害上限增至3000；亚种五叶草子弹伤害x2。2级时，伤害x3，取消伤害上限；亚种五叶草伤害x3，取消储存上限");
                ReplaceText.ReplaceAlmanac(PlantType.UltimateStar,
                    "攻击间隔降低至0.5秒",
                    "攻击间隔降至0.5秒；亚种五叶草回旋加速降至0.5秒，吸引范围+50%");
            }
            #endregion
            #region 金蛋
            {
                // 无尽贪婪
                ReplaceText.ReplaceAlmanac(PlantType.UltimateCorn, "究极黑洞可以吸收绝大多数子弹",
                    "究极黑洞可以吸收绝大多数子弹；亚种超时空坚果每30秒立即回溯一次并回复2000韧性");
                // 万劫不复
                ReplaceText.ReplaceAlmanac(PlantType.UltimateCorn, "究极黑洞吸引子弹的半径翻倍",
                    "究极黑洞吸引子弹的半径翻倍；亚种超时空坚果每次回溯都会净化自身状态");
            }
            #endregion
            #region 血月÷子
            {
                ReplaceText.ReplaceAlmanac(PlantType.UltimateCabbage,
                    "太阳神的子弹会消耗超过15000阳光部分0.5%阳光，使该子弹增加(20×消耗阳光)的伤害；亚种月亮神的子弹的光照等级增伤×3",
                    "太阳神的子弹会消耗超过15000阳光部分0.5%阳光，使该子弹增加（20x消耗阳光）的伤害；亚种月亮神的子弹的光照等级增伤x3；变种血月神的子弹的光照等级增伤x3 ，前20级光照等级，每级血月额外提供僵尸的增益提升50%");
                ReplaceText.ReplaceAlmanac(PlantType.UltimateCabbage,
                    "太阳伤害×3\n月亮回血×3",
                    "太阳伤害x3\n月亮回血x3\n血月持续时间x3，且召唤时间减至5秒");
            }
            #endregion
            #region 曾哥&牢灯
            {
                // 万籁俱寂
                ReplaceText.ReplaceAlmanac(PlantType.UltimateGloom,
                    "究极忧郁菇死亡时造成的伤害增至100万，究极路灯花死亡时为全场提供的光照级数×3",
                    "究极忧郁菇死亡时或对其使用毁灭菇卡片时，造成1000万伤害并额外造成0.5倍韧性的伤害；亚种路灯花为全场提供的光照x3，并使全场目标的冻结值上限降至100");
                // 以爆制爆
                ReplaceText.ReplaceAlmanac(PlantType.UltimateGloom,
                    "免疫非物理爆炸，每吸收10000点伤害释放毁灭菇效果（不留坑洞）",
                    "究极忧郁菇及亚种免疫非物理爆炸，每吸收或受到10000点伤害，释放一次（累计吸收伤害/3.8）伤害的毁灭菇效果，不留坑洞");
            }
            #endregion
            #region 鮟鱇鱼
            {
                // 深渊巨口
                ReplaceText.ReplaceAlmanac(PlantType.UltimateBigChomper,
                    "深渊巨口：究极灯笼巨嘴花的攻击距离增加1.5格",
                    "深渊巨口：究极灯笼巨嘴花对于不可吞食的僵尸，对其造成僵尸韧性的（25+65x范围僵尸总血量/范围僵尸总血量+10^7）%的伤害，该效果会使最终消化时间改为60秒");
                // 光芒四射
                ReplaceText.ReplaceAlmanac(PlantType.UltimateBigChomper,
                    "光芒四射：消化时间内会为5x5范围提供2点光照等级",
                    "光芒四射：究极灯笼巨嘴花吞食后，为自身半径3.7格范围的植物提供（1+吞噬僵尸数量x0.25）点的光照等级，持续15秒");
            }
            #endregion
        }

        public static void InitCardClick()
        {
            #region 曾哥&牢灯
            {
                CardClickMgr.AddCardClickOnPlant(PlantType.DoomShroom, PlantType.UltimateGloom, (plant) =>
                {
                    plant.GetComponent<IceDoomGloom>().DieEvent();
                }, () => CoreTools.TravelUltimate("万籁俱寂"));
                CardClickMgr.AddCardClickOnPlant(PlantType.DoomShroom, PlantType.UltimatePlantern, (plant) =>
                {
                    plant.GetComponent<UltimatePlantern>().DieEvent();
                }, () => CoreTools.TravelUltimate("万籁俱寂"));
            }
            #endregion
        }

        public static void InitDataExtra()
        {
            #region 曾哥&牢灯
            {
                PlantDataMenuExtra.AddExtra(PlantType.UltimateGloom, "储存伤害：{0}", () => CoreTools.TravelUltimate("以爆制爆"),
                    (p) => p.GetOrAddComponent<DataSave>().GetData<int>("UltimateGloom_TotalDamage"));
                PlantDataMenuExtra.AddExtra(PlantType.UltimatePlantern, "储存伤害：{0}", () => CoreTools.TravelUltimate("以爆制爆"),
                    (p) => p.GetOrAddComponent<DataSave>().GetData<int>("UltimatePlantern_TotalDamage"));
            }
            #endregion
        }
    }

    #region 修改文本
    public static class ReplaceText
    {
        #region 字段
        public static bool loadAlmanac = false;
        public static bool loadBuff = false;

        public static Dictionary<PlantType, String> AlmanacStrings = new();
        public static List<(PlantType, String, String)> ReplaceAlmanacStrings = new();

        public static Dictionary<(BuffType, int), String> BuffStrings = new();
        public static List<((BuffType, int), String, String)> ReplaceBuffStrings = new();
        #endregion

        /// <summary>
        /// 替换图鉴文本
        /// </summary>
        /// <param name="almanacType">植物类型</param>
        /// <param name="origin">原来的文本</param>
        /// <param name="replace">替换后的文本</param>
        public static void ReplaceAlmanac(PlantType almanacType, String origin, String replace) =>
            ReplaceAlmanacStrings.Add((almanacType, origin, replace));

        /// <summary>
        /// 替换词条文本
        /// </summary>
        /// <param name="plantType">词条对应的植物类型</param>
        /// <param name="type">词条类型</param>
        /// <param name="index">词条ID</param>
        /// <param name="origin">原来的文本</param>
        /// <param name="replace">替换后的文本</param>
        public static void ReplaceBuff(BuffType type, int index, String origin, String replace) =>
            ReplaceBuffStrings.Add(((type, index), origin, replace));

        public static void InitAlmanacText()
        {
            foreach (var (almanacType, origin, replace) in ReplaceAlmanacStrings)
            {
                var almanac = AlmanacPlantMenu.PlantAlmanacData[almanacType];
                var newStr = almanac.info.Replace(origin, replace);
                almanac.info = newStr;
                AlmanacPlantMenu.PlantAlmanacData[almanacType] = almanac;
            }
        }

        public static void InitBuffText()
        {
            foreach (var ((type, index), origin, replace) in ReplaceBuffStrings)
            {
                var oldStr = "";
                switch (type)
                {
                    case BuffType.AdvancedBuff:
                        oldStr = TravelDictionary.advancedBuffsText[(AdvBuff)index];
                        break;
                    case BuffType.UltimateBuff:
                        oldStr = TravelDictionary.ultimateBuffsText[(UltiBuff)index];
                        break;
                }

                var newStr = oldStr.Replace(origin, replace);
                BuffStrings[(type, index)] = replace;
            }
        }

        public static void InitAlmanac()
        {
            if (!loadAlmanac)
            {
                InitAlmanacText();
                loadAlmanac = true;
            }
            foreach (var (type, str) in AlmanacStrings)
            {
                var almanac = AlmanacPlantMenu.PlantAlmanacData[type];
                almanac.info = str;
                AlmanacPlantMenu.PlantAlmanacData[type] = almanac;
            }
        }

        public static void InitBuff()
        {
            if (!loadBuff)
            {
                InitBuffText();
                loadBuff = true;
            }
            foreach (var ((type, index), str) in BuffStrings)
            {
                switch (type)
                {
                    case BuffType.AdvancedBuff:
                        TravelDictionary.advancedBuffsText[(AdvBuff)index] = str;
                        break;
                    case BuffType.UltimateBuff:
                        TravelDictionary.ultimateBuffsText[(UltiBuff)index] = str;
                        break;
                }
            }
        }
    }
    #endregion

    #region 卡片点击
    public static class CardClickMgr
    {
        public static Dictionary<(PlantType, PlantType), (Func<bool>, Action<Plant>)> OnCardClick { get; set; } = new();

        public static void AddCardClickOnPlant(PlantType card, PlantType plant, Action<Plant> action, Func<bool> can) =>
            OnCardClick.Add((card, plant), (can, action));
    }
    #endregion

    #region 详细信息额外显示
    public static class PlantDataMenuExtra
    {
        public static Dictionary<PlantType, (string, Func<bool>, Func<Plant, object>)> ExtraData { get; set; } = new();

        public static void AddExtra(PlantType plantType, string format, Func<bool> show, Func<Plant, object> datas) =>
            ExtraData.Add(plantType, (format, show, datas));
    }
    #endregion

    #region 基础
    [HarmonyPatch(typeof(AlmanacPlantMenu))]
    public static class AlmanacPlantMenuPatch
    {
        [HarmonyPatch(nameof(AlmanacPlantMenu.InitNameAndInfoFromJson))]
        [HarmonyPostfix]
        public static void PostInitNameAndInfoFromJson()
        {
            ReplaceText.InitAlmanac();
        }
    }
    
    [HarmonyPatch(typeof(TravelLookMenu))]
    public static class TravelLookMenuPatch
    {
        [HarmonyPatch(nameof(TravelLookMenu.Start))]
        [HarmonyPostfix]
        public static void PostStart()
        {
            ReplaceText.InitBuff();
        }
    }

    [HarmonyPatch(typeof(TravelMgr))]
    public static class TravelMgrPatch
    {
        [HarmonyPatch(nameof(TravelMgr.OnBoardStart))]
        [HarmonyPostfix]
        public static void PostOnBoardStart()
        {
            ReplaceText.InitBuff();
        }
    }

    [HarmonyPatch(typeof(Mouse))]
    public static class MousePatch
    {
        [HarmonyPatch(nameof(Mouse.TryToSetPlantByCard))]
        [HarmonyPostfix]
        public static void PostTryToSetPlantByCard(Mouse __instance)
        {
            if (__instance.theCardOnMouse != null)
            {
                Plant? targetPlant = null;
                foreach (var plant in Lawnf.Get1x1Plants(__instance.theMouseColumn, __instance.theMouseRow))
                {
                    if (plant == null || plant.IsDestroyed() || plant.gameObject == null || plant.gameObject.IsDestroyed()) continue;
                    if (CardClickMgr.OnCardClick.ContainsKey((__instance.theCardOnMouse.thePlantType, plant.thePlantType)))
                    {
                        targetPlant = plant;
                        break;
                    }
                }
                if (targetPlant == null) return;
                if (CardClickMgr.OnCardClick[(__instance.theCardOnMouse.thePlantType, targetPlant.thePlantType)].Item1.Invoke())
                {
                    CardClickMgr.OnCardClick[(__instance.theCardOnMouse.thePlantType, targetPlant.thePlantType)].Item2.Invoke(targetPlant);
                    __instance.board.UseSun(__instance.theCardOnMouse.theSeedCost);

                    if (CoreTools.TravelAdvanced("贪婪诅咒"))
                        __instance.board.UseSun(__instance.board.theSun / 2);

                    __instance.theCardOnMouse.CD = 0f;
                    __instance.theCardOnMouse.isPickUp = false;

                    UnityEngine.Object.Destroy(__instance.theItemOnMouse);
                    __instance.ClearItemOnMouse(false);
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlantDataMenu))]
    public static class PlantDataMenuPatch
    {
        [HarmonyPatch(nameof(PlantDataMenu.Start))]
        [HarmonyPostfix]
        public static void PostStart(PlantDataMenu __instance)
        {
            if (__instance != null && __instance.gameObject != null && !__instance.IsDestroyed() && !__instance.gameObject.IsDestroyed() && 
                __instance.plant != null && __instance.plant.gameObject != null && !__instance.plant.IsDestroyed() && !__instance.plant.gameObject.IsDestroyed()
                && PlantDataMenuExtra.ExtraData.ContainsKey(__instance.plant.thePlantType))
            {
                var value = PlantDataMenuExtra.ExtraData[__instance.plant.thePlantType];
                if (value.Item2.Invoke())
                {
                    var str = string.Format(value.Item1, value.Item3.Invoke(__instance.plant));
                    foreach (var text in __instance.infoText)
                        text.text += str;
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameAPP))]
    public static class GameAPPPatch
    {
        [HarmonyPatch(nameof(GameAPP.Start))]
        [HarmonyPostfix]
        public static void PostStart(GameAPP __instance)
        {
            __instance.StartCoroutine(Core.Init());
        }
    }
    #endregion

    #region 大帝伴侣
    [HarmonyPatch(typeof(UltimateStarBlover))]
    public static class UltimateStarBloverPatch
    {
        [HarmonyPatch(nameof(UltimateStarBlover.StarsUpdate))]
        [HarmonyPrefix]
        public static void PreStarsUpdate(UltimateStarBlover __instance)
        {
            if (CoreTools.TravelUltimate("流星雨"))
                __instance.radius = 1.2f;
            if (CoreTools.TravelUltimate("众星之力"))
                __instance.maxBullets = int.MaxValue;
            if (__instance.starBullets == null) return;
            if (CoreTools.TravelUltimate("流星雨"))
            {
                for (int i = 0; i < __instance.starBullets.Length; i++)
                {
                    var star = __instance.starBullets[i];
                    if (star == null || star.IsDestroyed()) continue;
                    star.accelerateTime += Time.deltaTime * 2;
                }
            }
        }

        [HarmonyPatch(nameof(UltimateStarBlover.StarsUpdate))]
        [HarmonyPostfix]
        public static void PostStarsUpdate(UltimateStarBlover __instance)
        {
            if (CoreTools.TravelUltimate("斗转星移") && CoreTools.TravelUltimate("众星之力"))
            {
                for (int i = 0; i < __instance.board.plantStatistics.Count; i++)
                {
                    var plant = __instance.board.plantStatistics[i];
                    if (plant.thePlantType == PlantType.StarPumpkin)
                    {
                        __instance.maxBullets = 360 + 10 * Lawnf.GetPlantCount(PlantType.UltimateStar, __instance.board);
                        return;
                    }
                }
                __instance.maxBullets = 90 + 10 * Lawnf.GetPlantCount(PlantType.UltimateStar, __instance.board);
            }
        }
    }

    [HarmonyPatch(typeof(StarBlover))]
    public static class StarBloverPatch
    {
        [HarmonyPatch(nameof(StarBlover.RemoveNode))]
        [HarmonyPostfix]
        public static void PostRemoveNode(StarBlover __instance, ref Bullet_star starBullet)
        {
            if (__instance.thePlantType == PlantType.UltimateBlover)
            {
                if (CoreTools.TravelUltimate("众星之力"))
                {
                    starBullet.Damage *= (CoreTools.TravelUltimateLevel("众星之力") == 2) ? 3 : 2;
                }
            }
        }

        [HarmonyPatch(nameof(StarBlover.StarsUpdate))]
        [HarmonyPostfix]
        public static void PostStarsUpdate(StarBlover __instance)
        {
            if (Lawnf.TravelAdvanced((AdvBuff)19))
            {
                for (int i = 0; i < __instance.board.plantStatistics.Count; i++)
                {
                    var plant = __instance.board.plantStatistics[i];
                    if (plant.thePlantType == PlantType.StarPumpkin)
                    {
                        __instance.maxBullets = 120 + 10 * Lawnf.GetPlantCount(PlantType.UltimateStar, __instance.board);
                        return;
                    }
                }
                __instance.maxBullets = 30 + 10 * Lawnf.GetPlantCount(PlantType.UltimateStar, __instance.board);
            }
        }
    }
    #endregion

    #region 金蛋
    [HarmonyPatch(typeof(UltimatePortalNut))]
    public static class UltimatePortalNutPatch
    {
        [HarmonyPatch(nameof(UltimatePortalNut.Revive))]
        [HarmonyPostfix]
        public static void PostRevive(UltimatePortalNut __instance)
        {
            if (__instance == null) return;

            if (CoreTools.TravelUltimate("万劫不复"))
            {
                __instance.StartCoroutine(ClearDebuff(__instance));
            }
        }
        
        public static IEnumerator ClearDebuff(UltimatePortalNut __instance)
        {
            float startTime = Time.time;
            while (Time.time - startTime <= 0.1f)
            {
                try
                {
                    if (__instance == null) yield break;
                    __instance.TryBeActive();
                    if (GameAPP.Instance.GetComponent<DelayAction>() == null) yield break;
                    if (GameAPP.Instance.GetComponent<DelayAction>().actions == null) yield break;
                    for (int i = GameAPP.Instance.GetComponent<DelayAction>().actions.Count - 1; i >= 0; i--)
                    {
                        if (GameAPP.Instance.GetComponent<DelayAction>().actions.Count <= 0 || i < 0)
                            break;
                        if (i >= GameAPP.Instance.GetComponent<DelayAction>().actions.Count)
                            continue;
                        var action = GameAPP.Instance.GetComponent<DelayAction>().actions[i];
                        if (action == null) continue;
                        var target = action.action.Target.TryCast<Bullet_doom_ulti.__c__DisplayClass3_0>();
                        if (target != null && target.plant == __instance)
                        {
                            action.active = false;
                            action.action = null;
                        }
                    }
                    if (Lawnf.GetAllZombies().Count > 0)
                    {
                        foreach (Zombie zombie in Lawnf.GetAllZombies())
                        {
                            if (zombie != null)
                            {
                                if (zombie.theZombieType == ZombieType.UltimateHorse)
                                {
                                    var horse = zombie.GetComponent<UltimateHorse>();
                                    if (horse != null && horse.cursedPlants.Contains(__instance))
                                    {
                                        while (horse.cursedPlants.Contains(__instance))
                                            horse.cursedPlants[horse.cursedPlants.IndexOf(__instance)] = null;
                                        if (!horse.cursedPlants.Contains(__instance))
                                            __instance.SetColor(new Color(1f, 1f, 1f));
                                    }
                                }
                                if (zombie.theZombieType == ZombieType.SuperLadderZombie)
                                {
                                    var ladder = zombie.transform.GetComponent<SuperLadderZombie>();
                                    if (ladder != null && ladder.ladder != null && ladder.ladder.theItemRow == __instance.thePlantRow && ladder.ladder.theItemColumn == __instance.thePlantColumn)
                                    {
                                        ladder.ladder.Die();
                                    }
                                }
                            }
                        }
                        foreach (var zombie in Lawnf.GetAllZombies())
                        {
                            if (zombie.theZombieType == ZombieType.UltimateHorse)
                            {
                                var horse = zombie.GetComponent<UltimateHorse>();
                                if (horse != null && horse.cursedPlants.Contains(__instance))
                                {
                                    if (horse.cursedPlants.Contains(__instance))
                                    {
                                        __instance.SetColor(new Color(1f, 0f, 0f, 1f));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (ArgumentOutOfRangeException) { }
                yield return null;
            }
        }
    }

    [HarmonyPatch(typeof(Plant))]
    public static class Plant_UltimatePortalNut_Patch
    {
        [HarmonyPatch(nameof(Plant.Awake))]
        [HarmonyPostfix]
        public static void PostAwake(Plant __instance)
        {
            if (__instance.thePlantType == PlantType.UltimatePortalNut)
            {
                __instance.attributeCountdown = 30f;
            }
        }

        [HarmonyPatch(nameof(Plant.Update))]
        [HarmonyPostfix]
        public static void PostUpdate(Plant __instance)
        {
            if (__instance.thePlantType == PlantType.UltimatePortalNut)
            {
                if (__instance.attributeCountdown - Time.deltaTime <= 0f)
                {
                    if (CoreTools.TravelUltimate("无尽贪婪"))
                    {
                        __instance.GetComponent<UltimatePortalNut>().Revive();
                        __instance.thePlantHealth += 2000;
                        __instance.thePlantHealth = Mathf.Min(__instance.thePlantMaxHealth, __instance.thePlantHealth);
                        __instance.UpdateText();
                    }
                    __instance.attributeCountdown = 30f;
                }
                if (__instance.GetComponent<UltimatePortalNut>().invincibleTimer > 0f)
                {
                    var list = __instance.board.GetComponentsInChildren<UltimateHorse>().ToArray();
                    if (list.Length > 0)
                    {
                        foreach (var zombie in list)
                        {
                            if (zombie.cursedPlants.Contains(__instance))
                                __instance.SetColor(new Color(1f, 0f, 0f, 1f));
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region 血月
    [HarmonyPatch(typeof(Lunar))]
    public static class LunarPatch
    {
        [HarmonyPatch(nameof(Lunar.SummonUpdate))]
        [HarmonyPrefix]
        public static void PreSummonUpdate(Lunar __instance)
        {
            if (CoreTools.TravelUltimate("人造太阳"))
                __instance.summonTimer -= 2 * Time.deltaTime;
        }

        [HarmonyPatch(nameof(Lunar.Init))]
        [HarmonyPostfix]
        public static void PostInit(Lunar __instance)
        {
            if (CoreTools.TravelUltimate("人造太阳"))
                __instance.lifeTimer *= 3;
        }
    }

    [HarmonyPatch(typeof(Lawnf))]
    public static class LawnfPatch
    {
        [HarmonyPatch(nameof(Lawnf.GetPlantCount), new Type[] { typeof(PlantType), typeof(Board) })]
        [HarmonyPostfix]
        public static void PostGetPlantCount(ref PlantType theSeedType, ref int __result)
        {
            var callByUpdate = false;
            for (int i = 0; i < new StackTrace()?.FrameCount; i++)
            {
                if (new StackTrace()?.GetFrame(i)?.GetMethod()?.Name == "DMD<Lunar::Update>" && new StackTrace()?.GetFrame(i)?.GetMethod()?.GetParameters().Length == 1)
                    callByUpdate = true;
            }
            if (CoreTools.TravelUltimate("金光闪闪") && theSeedType == PlantType.UltimateRedLunar && callByUpdate)
            {
                var maxLevel = Lawnf.GetAllPlants().ToArray().ToList().Where(plant => plant.thePlantType == PlantType.UltimateRedLunar).Max(plant => plant.currentLightLevel);
                maxLevel = Mathf.Min(maxLevel, 20);
                __result += maxLevel;
            }
        }
    }
    #endregion

    #region 曾哥&牢灯
    // 曾哥part
    [HarmonyPatch(typeof(IceDoomGloom))]
    public static class IceDoomGloomPatch
    {
        [HarmonyPatch(nameof(IceDoomGloom.DieEvent))]
        [HarmonyPrefix]
        public static bool PostDieEvent(IceDoomGloom __instance, Plant.DieReason reason)
        {
            if (CoreTools.TravelUltimate("万籁俱寂") && reason != Plant.DieReason.ByMix)
            {
                __instance.board.boardAction.SetDoom(__instance.thePlantColumn, __instance.thePlantRow, false, true, damage: 1000_0000, effect: 3, fromType: __instance.thePlantType);
                foreach (var zombie in Lawnf.GetAllZombies())
                {
                    if (zombie == null || zombie.IsDestroyed() || zombie.gameObject == null || zombie.gameObject.IsDestroyed()) continue;
                    zombie.TakeDamage(DmgType.Carred, (int)(zombie.CurrentAllHealth * 0.5f), PlantType.UltimateGloom);
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(IceDoomGloom.TakeDamage))]
        [HarmonyPrefix]
        public static bool PreTakeDamage(IceDoomGloom __instance, ref int damageType, ref int damage)
        {
            if (CoreTools.TravelUltimate("以爆制爆"))
            {
                __instance.power += damage;
                var data = __instance.GetOrAddComponent<DataSave>();
                data.SetData("UltimateGloom_TotalDamage", data.GetData<int>("UltimateGloom_TotalDamage") + damage);
                if (__instance.power >= 10000)
                {
                    __instance.board.boardAction.SetDoom(__instance.thePlantColumn, __instance.thePlantRow,
                        false, false, damage: (int)(data.GetData<int>("UltimateGloom_TotalDamage") / 3.8f), fromType: __instance.thePlantType);

                    __instance.power = 0;
                }

                if ((Plant.DamageType)damageType != Plant.DamageType.Default)
                    return false;
            }
            return true;
        }
    }

    // 牢灯part
    [HarmonyPatch(typeof(UltimatePlantern))]
    public static class UltimatePlanternPatch
    {
        [HarmonyPatch(nameof(UltimatePlantern.DieEvent))]
        [HarmonyPostfix]
        public static void PostDieEvent(UltimatePlantern __instance, Plant.DieReason reason)
        {
            if (CoreTools.TravelUltimate("万籁俱寂") && reason != Plant.DieReason.ByMix)
            {
                foreach (var zombie in Lawnf.GetAllZombies())
                {
                    if (zombie == null || zombie.IsDestroyed() || zombie.gameObject == null || zombie.gameObject.IsDestroyed()) continue;
                    zombie.freezeMaxLevel = 100;
                }
            }
        }

        [HarmonyPatch(nameof(UltimatePlantern.TakeDamage))]
        [HarmonyPrefix]
        public static bool PreTakeDamage(UltimatePlantern __instance, ref int damageType, ref int damage)
        {
            if (CoreTools.TravelUltimate("以爆制爆"))
            {
                __instance.attributeCount += damage;
                var data = __instance.GetOrAddComponent<DataSave>();
                data.SetData("UltimatePlantern_TotalDamage", data.GetData<int>("UltimatePlantern_TotalDamage") + damage);
                if (__instance.attributeCount >= 10000)
                {
                    __instance.board.boardAction.SetDoom(__instance.thePlantColumn, __instance.thePlantRow,
                        false, false, damage: (int)(data.GetData<int>("UltimatePlantern_TotalDamage") / 3.8f), fromType: __instance.thePlantType);

                    __instance.attributeCount = 0;
                }

                if ((Plant.DamageType)damageType != Plant.DamageType.Default)
                    return false;
            }
            return true;
        }
    }
    #endregion

    #region 鮟鱇鱼
    [HarmonyPatch(typeof(UltimateBigChomper))]
    public static class UltimateBigChomperPatch
    {
        [HarmonyPatch(nameof(UltimateBigChomper.Awake))]
        [HarmonyPostfix]
        public static void PostAwake(UltimateBigChomper __instance)
        {
            __instance.attributeFloat = 5f;
        }

        [HarmonyPatch(nameof(UltimateBigChomper.Chomp))]
        [HarmonyPrefix]
        public static void PreChomp(UltimateBigChomper __instance, ref Zombie zombie, out (int, int) __state)
        {
            var center = __instance.axis.transform.position;
            center.x += __instance.centerOffset.x;
            center.y += __instance.centerOffset.y;
            Collider2D[] colliders = Physics2D.OverlapBoxAll(center, new Vector2(__instance.range.x + __instance.attributeFloat, __instance.range.y), 0f, LayerMask.GetMask("Zombie"));
            var boxZombies = 0;
            var totalHealth = 0;
            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent<Zombie>(out var z) && z.IsObjExist())
                {
                    if (z.theZombieRow == __instance.thePlantRow)
                    {
                        if (!TypeMgr.IsBossZombie(z.theZombieType) && __instance.CheckZombie(z))
                            boxZombies++;
                        totalHealth += z.CurrentAllHealth;
                    }
                }
            }
            __state = (boxZombies, totalHealth);
        }

        [HarmonyPatch(nameof(UltimateBigChomper.Chomp))]
        [HarmonyPostfix]
        public static void PostChomp(UltimateBigChomper __instance, ref Zombie zombie, (int, int) __state)
        {
            if (CoreTools.TravelUltimate("光芒四射"))
            {
                CreatePlant.Instance.AdjustLightLevel(__instance.thePlantColumn, __instance.thePlantRow, -2, 2); // 抵消原来的

                __instance.add = true;
                var boxZombies = __state.Item1;
                var light = new Light(__instance.axis.transform.position, 1 + (int)(boxZombies * 0.25f), PlantTools.ColumnX * 3.7f);
                __instance.GetOrAddComponent<Timer>().AddTimer(15f, () =>
                {
                    light.Die();
                    __instance.add = false;
                });
                __instance.SetData("UltimateBigChomper_Light", light);
            }
            if (CoreTools.TravelUltimate("深渊巨口"))
            {
                Collider2D[] colliders = Physics2D.OverlapBoxAll(__instance.axis.transform.position, new Vector2(__instance.range.x + __instance.attributeFloat, __instance.range.y), 0f, LayerMask.GetMask("Zombie"));
                var zombies = new List<Zombie>();
                foreach (var collider in colliders)
                    if (collider.TryGetComponent<Zombie>(out var z) && z.IsObjExist() && z.theZombieRow == __instance.thePlantRow)
                        zombies.Add(z);
                if (zombies.Count <= 0)
                    return;
                var totalHealth = __state.Item2;
                var timer = 0.25f + 0.65f * totalHealth / (totalHealth + Mathf.Pow(10, 7));
                foreach (var z in zombies)
                {
                    z.TakeDamage(DmgType.Normal, (int)(z.TotalAllHealth * timer), PlantType.UltimateBigChomper);
                }
                __instance.canToChew = true;

                __instance.anim.ResetTrigger("back");
                __instance.attributeCountdown = 60f;
                var runner = __instance.GetOrAddComponent<ComponentRunner>();
                runner.OnUpdate = () =>
                {
                    if (__instance.anim.speed != 0f)
                    {
                        if (__instance.attributeCountdown > 0f)
                        {
                            var t = __instance.attributeCountdown;
                            __instance.attributeCountdown += Time.deltaTime * (__instance.attributeSpeed - 1); // 抵消受attributeSpeed影响的countdown
                        }
                        else
                        {
                            UnityEngine.Object.Destroy(runner);
                        }
                    }
                };
            }
        }

        [HarmonyPatch(nameof(UltimateBigChomper.OnMove))]
        [HarmonyPostfix]
        public static void PostOnMove(UltimateBigChomper __instance, int originalColumn, int originalRow, int newColumn, int newRow)
        {
            if (__instance.add)
            {
                // 抵消原来的效果
                CreatePlant.Instance.AdjustLightLevel(originalColumn, originalRow, 2, 2);
                CreatePlant.Instance.AdjustLightLevel(newColumn, newRow, -2, 2);

                // 新内容
                __instance.GetOrAddComponent<Timer>().AddTimer(0.01f, () =>
                {
                    __instance.GetData<Light>("UltimateBigChomper_Light").MoveTo(__instance.axis.transform.position);
                });
            }
        }

        [HarmonyPatch(nameof(UltimateBigChomper.TakeDamage))]
        [HarmonyPrefix]
        public static void PreTakeDamage(ref int damage)
        {
            if (damage > 2000)
                damage = 2000;
        }
    }

    [HarmonyPatch(typeof(UltimateFootballZombie))]
    public static class UltimateFootBallZombiePatch
    {
        [HarmonyPatch(nameof(UltimateFootballZombie.AttackEffect))]
        [HarmonyPrefix]
        public static void PreAttackEffect(ref Plant plant, out (bool, Plant.PlantTag) __state)
        {
            if (plant.IsObjExist() && plant.thePlantType is PlantType.UltimateBigChomper)
            {
                __state = (true, plant.plantTag);
                var tag = plant.plantTag;
                tag.nutPlant = true;
                plant.plantTag = tag;
            }
            __state = (false, default);
        }

        [HarmonyPatch(nameof(UltimateFootballZombie.AttackEffect))]
        [HarmonyPostfix]
        public static void PostAttackEffect(ref Plant plant, (bool, Plant.PlantTag) __state)
        {
            if (__state.Item1)
            {
                plant.plantTag = __state.Item2;
            }
        }
    }

    [HarmonyPatch(typeof(Chomper))]
    public static class ChomperPatch
    {
        [HarmonyPatch(nameof(Chomper.ChompBack))]
        [HarmonyPrefix]
        public static void PreChompBack(Chomper __instance, out (bool, int) __state)
        {
            if (__instance.thePlantType is PlantType.UltimateBigChomper && CoreTools.TravelUltimate("深渊巨口"))
            {
                var center = __instance.axis.transform.position;
                center.x += __instance.centerOffset.x;
                center.y += __instance.centerOffset.y;
                Collider2D[] colliders = Physics2D.OverlapBoxAll(center, new Vector2(__instance.range.x + __instance.attributeFloat, __instance.range.y), 0f, LayerMask.GetMask("Zombie"));
                var totalHealth = 0;
                foreach (var collider in colliders)
                {
                    if (collider.TryGetComponent<Zombie>(out var z) && z.IsObjExist())
                    {
                        if (z.theZombieRow == __instance.thePlantRow)
                        {
                            totalHealth += z.CurrentAllHealth;
                        }
                    }
                }
                __state = (true, totalHealth);
                return;
            }
            __state = (false, 0);
        }

        [HarmonyPatch(nameof(Chomper.ChompBack))]
        [HarmonyPostfix]
        public static void PostChompBack(Chomper __instance, (bool, int) __state)
        {
            if (__state.Item1)
            {
                Collider2D[] colliders = Physics2D.OverlapBoxAll(__instance.axis.transform.position, new Vector2(__instance.range.x + __instance.attributeFloat, __instance.range.y), 0f, LayerMask.GetMask("Zombie"));
                var zombies = new List<Zombie>();
                foreach (var collider in colliders)
                    if (collider.TryGetComponent<Zombie>(out var z) && z.IsObjExist() && z.theZombieRow == __instance.thePlantRow)
                        zombies.Add(z);
                if (zombies.Count <= 0)
                    return;
                var totalHealth = __state.Item2;
                var timer = 0.25f + 0.65f * totalHealth / (totalHealth + Mathf.Pow(10, 7));
                foreach (var z in zombies)
                {
                    z.TakeDamage(DmgType.Normal, (int)(z.TotalAllHealth * timer), PlantType.UltimateBigChomper);
                }
                __instance.canToChew = true;

                __instance.anim.ResetTrigger("back");
                __instance.attributeCountdown = 60f;
                var runner = __instance.GetOrAddComponent<ComponentRunner>();
                runner.OnUpdate = () =>
                {
                    if (__instance.anim.speed != 0f)
                    {
                        if (__instance.attributeCountdown > 0f)
                        {
                            var t = __instance.attributeCountdown;
                            __instance.attributeCountdown += Time.deltaTime * (__instance.attributeSpeed - 1); // 抵消受attributeSpeed影响的countdown
                        }
                        else
                        {
                            UnityEngine.Object.Destroy(runner);
                        }
                    }
                };
            }
        }
    }
    #endregion
}
