using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using CustomizeLib.BepInEx;
using Unity.VisualScripting;
using System.Collections;

namespace ElectricSuperGatling_BepInEx
{
    [BepInPlugin("salmon.electricsupergatling", "ElectricSuperGatling", "1.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<Bullet_electricSuperGatlingPea>();
            ClassInjector.RegisterTypeInIl2Cpp<ElectricSuperGatling>();
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "electricsupergatling");
            CustomCore.RegisterCustomBullet<Bullet_pea, Bullet_electricSuperGatlingPea>((BulletType)Bullet_electricSuperGatlingPea.BulletID, ab.GetAsset<GameObject>("ElectricSuperGatlingBullet"));
            CustomCore.RegisterCustomPlant<SuperGatling, ElectricSuperGatling>(
                ElectricSuperGatling.PlantID,
                ab.GetAsset<GameObject>("ElectricSuperGatlingPrefab"),
                ab.GetAsset<GameObject>("ElectricSuperGatlingPreview"),
                new List<(int, int)>
                {
                    ((int)PlantType.SuperGatling, (int)PlantType.ElectricOnion)
                },
                1.5f, 0f, 20, 300, 7.5f, 825
            );
            CustomCore.AddUltimatePlant((PlantType)ElectricSuperGatling.PlantID);
            CustomCore.AddPlantAlmanacStrings(ElectricSuperGatling.PlantID,
                $"超级电能机枪射手({ElectricSuperGatling.PlantID})",
                "一次发射六颗电能豌豆，有概率一次性发射大量电能豌豆\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>使用条件：</color><color=red>旅行模式</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>20x6/1.5秒</color>\n" +
                "<color=#3D1400>子弹伤害：</color><color=red>20/0.15秒</color>\n" +
                "<color=#3D1400>特点：</color><color=red>每次攻击有2%概率出发大招，5秒内，每0.02秒散射3发电能豌豆</color>\n" +
                "<color=#3D1400>电能豌豆：</color><color=red>①无限穿透，子弹会向3x3范围持续造成伤害\n" +
                "②子弹前三次直击目标时，索敌半径3.7格的非直击目标造成一次半径0.5格的电击伤害，造成10倍攻击力的灰烬伤害\n" +
                "③子弹伤害的目标有0.1%概率造成0.5秒的定身效果</color>\n" +
                "<color=#3D1400>融合配方：</color><color=red>超级机枪射手+闪电洋葱</color>\n\n" +
                "<color=#3D1400>“你是电，你是光，你是唯一的神话，我只爱你，You are my super star!”超级电能机枪豌豆总是把这句歌词挂在嘴边，电光火石间，僵尸们就因触碰114514伏的高压电流而死。</color>"
            );
        }
    }

    public class Bullet_electricSuperGatlingPea : MonoBehaviour
    {
        public static int BulletID = 1904;

        public float attackCountDown = 0f;
        public int hitTimes = 0;

        public void Start()
        {
            hitTimes = 0;
        }

        public void OnHitZombie(Zombie z)
        {
            hitTimes++;
            if (hitTimes <= 3)
            {
                foreach (var collider in Physics2D.OverlapCircleAll(transform.position, 3.7f, LayerMask.GetMask("Zombie")))
                {
                    if (collider == null || collider.IsDestroyed() || collider.gameObject == null || collider.gameObject.IsDestroyed() ||
                        !collider.TryGetComponent<Zombie>(out var zombie) || zombie == null || zombie.IsDestroyed() || zombie.gameObject == null ||
                        zombie.gameObject.IsDestroyed() || zombie == z) continue;
                    foreach (var coli in Physics2D.OverlapCircleAll(transform.position, 0.5f, LayerMask.GetMask("Zombie")))
                    {
                        if (coli == null || coli.IsDestroyed() || coli.gameObject == null || coli.gameObject.IsDestroyed() ||
                            !coli.TryGetComponent<Zombie>(out var zi) || zi == null || zi.IsDestroyed() || zi.gameObject == null ||
                            zi.gameObject.IsDestroyed() || zi == z) continue;
                        zi.TakeDamage(DmgType.Carred, bullet.Damage * 10, bullet.fromType);
                        if (UnityEngine.Random.Range(1, 1001) <= 1 && !zi.GetData<bool>("ElectricSuperGatling_Stopping"))
                        {
                            zi.SetData("ElectricSuperGatling_Stopping", true);
                            zi.timers.Add((ZombieTimer)BulletID, 0.5f);
                            zi.SetData("ElectricSuperGatling_ZombieData", (zi.theSpeed, zi.anim.speed));
                            zi.theSpeed = 0f;
                            zi.anim.speed = 0f;
                            zombie.StartCoroutine(CheckTimer(zombie));
                        }
                    }
                    Debug.Log("found & hit");
                    break;
                }
            }
        }

        public void FixedUpdate()
        {
            if (GameAPP.theGameStatus is (int)GameStatus.InGame)
            {
                attackCountDown -= Time.deltaTime;
                if (attackCountDown <= 0f)
                {
                    bool hit = false;
                    foreach (var collider in Physics2D.OverlapCircleAll(transform.position, 1.5f, LayerMask.GetMask("Zombie")))
                    {
                        if (collider == null || collider.IsDestroyed() || collider.gameObject == null || collider.gameObject.IsDestroyed() ||
                            !collider.TryGetComponent<Zombie>(out var zombie) || zombie == null || zombie.IsDestroyed() || zombie.gameObject == null ||
                            zombie.gameObject.IsDestroyed()) continue;
                        zombie.TakeDamage(DmgType.NormalAll, bullet.Damage, bullet.fromType);
                        if (UnityEngine.Random.Range(1, 1001) <= 1 && !zombie.GetData<bool>("ElectricSuperGatling_Stopping"))
                        {
                            zombie.SetData("ElectricSuperGatling_Stopping", true);
                            zombie.timers.Add((ZombieTimer)BulletID, 0.5f);
                            zombie.SetData("ElectricSuperGatling_ZombieData", (zombie.theSpeed, zombie.anim.speed));
                            zombie.theSpeed = 0f;
                            zombie.anim.speed = 0f;
                            zombie.StartCoroutine(CheckTimer(zombie));
                        }
                        hit = true;
                    }
                    attackCountDown = 0.15f;
                    if (hit)
                        GameAPP.PlaySound(UnityEngine.Random.RandomRange(0, 3));
                }
            }
        }

        public static IEnumerator CheckTimer(Zombie zombie)
        {
            if (zombie == null || zombie.IsDestroyed() || zombie.gameObject == null || zombie.gameObject.IsDestroyed()) yield break;
            if (!zombie.timers.TryGetValue((ZombieTimer)BulletID, out var _)) yield break;
            while (zombie.timers[(ZombieTimer)BulletID] > 0f)
            {
                if (zombie == null || zombie.IsDestroyed() || zombie.gameObject == null || zombie.gameObject.IsDestroyed()) yield break;
                zombie.theSpeed = 0f;
                zombie.anim.speed = 0f;
                yield return null;
            }
            if (zombie == null || zombie.IsDestroyed() || zombie.gameObject == null || zombie.gameObject.IsDestroyed()) yield break;
            var (speed, animSpeed) = zombie.GetData<(float, float)>("ElectricSuperGatling_ZombieData");
            zombie.theSpeed = speed;
            zombie.anim.speed = animSpeed;
            yield break;
        }

        public void OnEnable()
        {
            hitTimes = 0;
        }

        public Bullet_pea bullet => gameObject.GetComponent<Bullet_pea>();
    }

    public class ElectricSuperGatling : MonoBehaviour
    {
        public static int PlantID = 1906;

        public SuperGatling plant => gameObject.GetComponent<SuperGatling>();

        public void Awake()
        {
            plant.shoot = plant.gameObject.transform.GetChild(0).FindChild("Shoot");
        }
    }

    [HarmonyPatch(typeof(SuperGatling), nameof(SuperGatling.GetBulletType))]
    public class SuperGatling_GetBulletType
    {
        public static bool Prefix(SuperGatling __instance, ref BulletType __result)
        {
            if ((int)__instance.thePlantType == ElectricSuperGatling.PlantID)
            {
                __result = (BulletType)Bullet_electricSuperGatlingPea.BulletID;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Bullet_pea), nameof(Bullet_pea.HitZombie))]
    public static class Bullet_peaPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Bullet_pea __instance, ref Zombie zombie)
        {
            if ((int)__instance.theBulletType == Bullet_electricSuperGatlingPea.BulletID)
            {
                __instance.GetComponent<Bullet_electricSuperGatlingPea>().OnHitZombie(zombie);
                return false;
            }
            return true;
        }
    }
}