using BepInEx;
using CustomizeLib.BepInEx;
using HarmonyLib;
using Unity.VisualScripting;
using UnityEngine;

namespace UltimateDoomSqualour.BepInEx
{
    [BepInPlugin("salmon.ultimatedoomsqualour", "UltimateDoomSqualour", "1.0")]
    public class Core : CorePlugin
    {
        public override void OnStart()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(Tools.Assembly, "ultimatedoomsqualour");
            CustomCore.RegisterCustomPlant<Squalour, UltimateDoomSqualour>(
                (int)UltimateDoomSqualour.PlantID,
                ab.GetAsset<GameObject>("UltimateDoomSqualourPrefab"),
                ab.GetAsset<GameObject>("UltimateDoomSqualourPreview"),
                new List<(int, int)>
                {
                    ((int)PlantType.NuclearDoomCherry, (int)PlantType.Squalour)
                },
                0f, 0f, 1800, 300, 0f, 250
            );
            CustomCore.AddPlantAlmanacStrings((int)UltimateDoomSqualour.PlantID,
                $"聚爆猫瓜({(int)UltimateDoomSqualour.PlantID})",
                "蕴含着核能力量的红温猫瓜，可千万不要招惹她…\n" +
                "<color=#0000FF>核爆樱桃同人亚种</color>\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>转化配方：</color><color=red>樱桃炸弹←→猫瓜</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>3600</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①每碾压1个僵尸，聚爆核爆的基础威力增加1200，最多7200\n" +
                "②碾压火红莲造成7200的毁灭菇爆炸，不留坑，并返还卡片\n" +
                "③碾压僵尸火红莲造成等同于核爆樱桃的爆炸，不留坑，并返还卡片</color>\n\n" +
                "<color=#3D1400>窝红温辣</color>"
            );
            CustomCore.AddFusion((int)PlantType.NuclearDoomCherry, (int)UltimateDoomSqualour.PlantID, (int)PlantType.CherryBomb);
            CustomCore.AddFusion((int)PlantType.NuclearDoomCherry, (int)PlantType.CherryBomb, (int)UltimateDoomSqualour.PlantID);
        }
    }

    public class UltimateDoomSqualour : MonoBehaviour
    {
        public static PlantType PlantID = (PlantType)1952;

        public void Awake()
        {
            gameObject.AddComponent<EndoFlameSaver>().endoFlame = null;
        }

        public Squalour plant => gameObject.GetComponent<Squalour>();
    }

    public class EndoFlameSaver : MonoBehaviour { public Plant? endoFlame = null; }

    [HarmonyPatch(typeof(Squalour))]
    public class SqualourPatch
    {
        [HarmonyPatch(nameof(Squalour.LourDie))]
        [HarmonyPrefix]
        public static bool Prefix(Squalour __instance)
        {
            if (__instance.thePlantType == UltimateDoomSqualour.PlantID)
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(Squalour.ActionOnZombie))]
        [HarmonyPrefix]
        public static bool Prefix(Squalour __instance, ref Zombie zombie)
        {
            if (__instance.thePlantType == UltimateDoomSqualour.PlantID)
            {
                if (zombie == null) return false;
                zombie.TakeDamage(DmgType.Squash, 1800);
                zombie.TakeDamage(DmgType.Carred, 3600);
                __instance.squashCount++;
                __instance.squashed = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Squash))]
    public class SquashPatch
    {
        [HarmonyPatch(nameof(Squash.AttackZombie))]
        [HarmonyPrefix]
        public static bool Prefix(Squash __instance)
        {
            if (__instance.thePlantType == UltimateDoomSqualour.PlantID)
            {
                {
                    Vector3 position = __instance.axis.position;
                    Vector2 centerPos = new Vector2(position.x, position.y);

                    // 检测区域内的僵尸
                    int zombieLayer = LayerMask.GetMask("Zombie");
                    Collider2D[] colliders = Physics2D.OverlapBoxAll(
                        centerPos,
                        new Vector2(1f, 3f),
                        0f,
                        zombieLayer
                    );

                    if (colliders != null)
                    {
                        foreach (var collider in colliders)
                        {
                            Zombie zombie;
                            if (collider.TryGetComponent<Zombie>(out zombie))
                            {
                                // 尝试攻击僵尸
                                bool attacked = __instance.AttackLandZombie(zombie);

                                // 检查是否符合特殊攻击条件
                                if (attacked && (zombie.theZombieRow == __instance.thePlantRow || zombie.theZombieType == ZombieType.DancePolZombie2))
                                {
                                    __instance.ActionOnZombie(zombie);
                                }
                            }
                        }
                    }

                    // 检查当前位置的格子类型
                    int col = Lawnf.GetColumnFromX(position.x);
                    int row = __instance.thePlantRow;

                    if (__instance.board.GetBoxType(col, row) != BoxType.Water)
                    {
                        GameAPP.PlaySound(74, 0.5f, 1f);
                        ScreenShake.shakeDuration = 0.05f;
                    }
                    else // 正常格子
                    {
                        // 创建特效
                        GameObject effectPrefab = Resources.Load<GameObject>("Particle/Anim/Water/WaterSplashPrefab");
                        Vector3 spawnPos = new Vector3(position.x, position.y - 1.75f, 0f);
                        Transform boardTransform = __instance.board.transform;
                        GameObject.Instantiate(effectPrefab, spawnPos, Quaternion.identity, boardTransform);

                        GameAPP.PlaySound(75, 0.5f, 1f);
                        __instance.Die();
                    }
                }

                if (__instance.TryGetComponent<EndoFlameSaver>(out var saver) && saver != null && !saver.IsDestroyed() && saver.endoFlame != null &&
                    saver.endoFlame.thePlantType == PlantType.EndoFlame)
                {
                    __instance.board.SetDoom(__instance.thePlantColumn, __instance.thePlantRow, false, false, __instance.axis.transform.position, 7200);
                    Lawnf.SetDroppedCard(__instance.axis.transform.position, UltimateDoomSqualour.PlantID, 0);
                    __instance.Die();
                }
                else
                {
                    int damage = 3600 + 1200 * Mathf.Min(__instance.GetComponent<Squalour>().squashCount, 3);
                    var position = __instance.axis.transform.position;
                    position.y -= 0.5f;
                    __instance.board.SetDoom(__instance.thePlantColumn, __instance.thePlantRow, false, false, position, damage);

                    Doom.SetDoom(__instance.board, __instance.axis.transform.position, DoomType.Nuclear);

                    var radiation = UnityEngine.Object.Instantiate(
                        Resources.Load<GameObject>("plants/cherrybomb/nucleardoomcherry/Radiation"),
                        __instance.axis.transform.position,
                        Quaternion.identity, __instance.board.transform).GetComponent<Radiation>();
                    radiation.damage = damage;
                    // 调用死亡方法
                    __instance.Die();
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Squash))]
    public class SqualourUpdatePatch
    {
        [HarmonyPatch(nameof(Squash.SquashUpdate))]
        [HarmonyPrefix]
        public static bool Prefix(Squash __instance)
        {
            if (__instance.thePlantType == UltimateDoomSqualour.PlantID)
            {
                List<Plant> nearPlant = Lawnf.Get1x1Plants(__instance.thePlantColumn + 1, __instance.thePlantRow).ToArray().ToList();

                foreach (Plant plant in nearPlant)
                {
                    if (plant != null && plant.thePlantType == PlantType.EndoFlame || plant.thePlantType == PlantType.ZombieEndoFlame)
                    {
                        Plant endoFlame = plant;
                        bool success = __instance.TryGetComponent<EndoFlameSaver>(out var component);
                        if (success)
                        {
                            component.endoFlame = plant;

                            // 设置攻击状态
                            __instance.isJump = true;

                            // 确定攻击方向（左/右）
                            __instance.anim.SetTrigger("lookleft"); // 向右看

                            // 设置父级对象（跟随棋盘）
                            __instance.transform.SetParent(__instance.board.transform);

                            // 记录攻击开始时间
                            __instance.startTime = Time.time;

                            // 保存目标位置
                            __instance.endPos = plant.axis.transform.position;
                            __instance.targetZombie = null;

                            // 执行攻击动作

                            // 鼠标选择特殊处理
                            __instance.invincible = true;

                            // 禁用碰撞器
                            foreach (var boxCol in __instance.boxCols)
                            {
                                boxCol.enabled = false;
                            }
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        [HarmonyPatch(nameof(Squash.AnimMove))]
        [HarmonyPrefix]
        public static bool Prefix_(Squash __instance)
        {
            if ((__instance.thePlantType == PlantType.Squalour) || ((int)__instance.thePlantType == 1913) || ((int)__instance.thePlantType == 1914))
            {
                bool findEndoFlame = false;
                Plant endoFlame = null;
                foreach (Plant plant in Lawnf.Get1x1Plants(__instance.thePlantColumn + 1, __instance.thePlantRow).ToArray().ToList())
                    if (plant.thePlantType == PlantType.EndoFlame || plant.thePlantType == PlantType.ZombieEndoFlame)
                    {
                        findEndoFlame = true;
                        endoFlame = plant;
                        bool success = __instance.TryGetComponent<EndoFlameSaver>(out var component);
                        if (success)
                            component.endoFlame = plant;
                        break;
                    }

                if (findEndoFlame)
                {
                    SpriteRenderer renderer = __instance.axis.GetComponent<SpriteRenderer>();
                    UnityEngine.Object.Destroy(renderer);

                    __instance.freeMoving = false;
                    __instance.RemoveFromList();  // 从植物列表中移除自身


                    Vector2 targetPoint = new Vector2(endoFlame.axis.transform.position.x, endoFlame.axis.transform.position.y + 1.75f);
                    __instance.startPos = __instance.axis.transform.position;
                    __instance.endPos = targetPoint;

                    // 启动跳跃协程
                    __instance.StartCoroutine(__instance.MoveToZombie(targetPoint, 5f));
                }
                return !findEndoFlame;
            }
            return true;
        }
    }
}
