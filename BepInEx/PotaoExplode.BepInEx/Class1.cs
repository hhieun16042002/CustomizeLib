using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

namespace PotaoExplode.BepInEx
{
    [BepInPlugin("salmon.potaoexplode", "PotaoExplode", "1.0")]
    public class Core : BasePlugin
    {
        public static GameObject PotatoPrefab = null;

        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            var ab = GetAssetBundle();
            foreach (var ase in ab.LoadAllAssetsAsync().allAssets)
            {
                if (ase.TryCast<GameObject>()?.name == "PotaoExplode")
                    PotatoPrefab = ase.Cast<GameObject>();
            }
        }

        public static AssetBundle GetAssetBundle()
        {
            using Stream stream =
                Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().FullName!.Split(",")[0] + "." + "potaoexplode") ??
                Assembly.GetExecutingAssembly().GetManifestResourceStream("potaoexplode")!;
            using MemoryStream stream1 = new();
            stream.CopyTo(stream1);
            var ab = AssetBundle.LoadFromMemory(stream1.ToArray());
            ArgumentNullException.ThrowIfNull(ab);
            return ab;
        }
    }

    [HarmonyPatch(typeof(GameAPP), nameof(GameAPP.Start))]
    public static class GameAPPPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            GameAPP.resourcesManager.particlePrefabs[ParticleType.PotaoExplode] = Core.PotatoPrefab;
        }
    }
}
