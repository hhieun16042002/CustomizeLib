using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace UltimateCornFume.BepInEx
{
    [BepInPlugin("salmon.ultimatecornfume", "UltimateCornFume", "1.0")]
    public class Core : CorePlugin
    {
        public override void OnStart()
        {
            var ab = CustomCore.GetAssetBundle(Tools.GetAssembly(), "abname");
            CustomCore.RegisterCustomPlant<TBase, UltimateCornFume>(UltimateCornFume.PlantID, ab.GetAsset<GameObject>("Prefab"),
                ab.GetAsset<GameObject>("Preview"), fusions, attackInterval, produceInterval, attackDamage, maxHealth, cd, sun);
            CustomCore.AddPlantAlmanacStrings(UltimateCornFume.PlantID, $"AlmanacName",
                "xxx\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>韧性：</color><color=red>xxx</color>\n" +
                "<color=#3D1400>特点：</color><color=red>xxx</color>\n\n" +
                "<color=#3D1400>宝开鱼</color>");
        }
    }

    public class UltimateCornFume : MonoBehaviour
    {
        public static ID PlantID = 1966;
    }
}
