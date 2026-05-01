using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace SubspeciesEntry.BepInEx
{
    public static class Tools
    {
        /// <summary>
        /// 获取嵌入dll里的ab包
        /// </summary>
        /// <param name="assembly">要获取ab包的dll</param>
        /// <param name="name">名称</param>
        /// <returns>ab包</returns>
        /// <exception cref="ArgumentException"></exception>
        public static AssetBundle GetAssetBundle(Assembly assembly, string name)
        {
            try
            {
                using Stream stream =
                    assembly.GetManifestResourceStream(assembly.FullName!.Split(",")[0] + "." + name) ??
                    assembly.GetManifestResourceStream(name)!;
                using MemoryStream stream1 = new();
                stream.CopyTo(stream1);
                var ab = AssetBundle.LoadFromMemory(stream1.ToArray());
                ArgumentNullException.ThrowIfNull(ab);
                return ab;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to load {name} \n{e}");
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

        public static bool IsObjExist(this Component component) => !(component == null || component.IsDestroyed() || component.gameObject == null || component.gameObject.IsDestroyed());
    }

    public static class CoreTools
    {
        public static Dictionary<string, AdvBuff> AdvBuffPair = new();
        public static Dictionary<string, UltiBuff> UltiBuffPair = new();

        public static void Init()
        {
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
        }

        public static AdvBuff GetAdvBuffByString(string name)
        {
            if (AdvBuffPair.ContainsKey(name))
                return AdvBuffPair[name];
            return (AdvBuff)(-1);
        }

        public static UltiBuff GetUltiBuffByString(string name)
        {
            if (UltiBuffPair.ContainsKey(name))
                return UltiBuffPair[name];
            return (UltiBuff)(-1);
        }

        public static bool TravelAdvanced(string name) => Lawnf.TravelAdvanced(GetAdvBuffByString(name));
        public static bool TravelUltimate(string name) => Lawnf.TravelUltimate(GetUltiBuffByString(name));
        public static int TravelUltimateLevel(string name) => Lawnf.TravelUltimateLevel(GetUltiBuffByString(name));

        /// <summary>
        /// 获取 若要将僵尸的护甲值视为(armor - reducedArmor)，应增加造成多少伤害
        /// </summary>
        /// <param name="armor">僵尸原护甲量</param>
        /// <param name="originDamage">原伤害</param>
        /// <param name="reducedArmor">要减免的护甲系数</param>
        /// <returns>应增加的伤害</returns>
        public static float GetReducedArmorDamage(float armor, float originDamage, float reducedArmor)
        {
            float b = 100 + armor - reducedArmor;
            if (b == 0) b = 1E-7f;
            // 增加伤害 = (原伤害 * 免疫护甲量) / (100 + 护甲 - 免疫护甲量)
            return (reducedArmor * originDamage) / b;
        }
    }

    public static class PlantTools
    {
        public static float ColumnX
        {
            get
            {
                if (Mouse.Instance != null && !Mouse.Instance.IsDestroyed())
                    return Mouse.Instance.GetBoxXFromColumn(1) - Mouse.Instance.GetBoxXFromColumn(0);
                return 1f;
            }
        }
        public static float RowY
        {
            get
            {
                if (Mouse.Instance != null && !Mouse.Instance.IsDestroyed())
                    return Mouse.Instance.GetBoxYFromRow(1) - Mouse.Instance.GetBoxYFromRow(0);
                return 1f;
            }
        }
    }
}
