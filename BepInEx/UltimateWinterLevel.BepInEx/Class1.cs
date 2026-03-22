using BepInEx;
using CustomizeLib.BepInEx;
using HarmonyLib;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace UltimateWinterLevel.BepInEx
{
    [BepInPlugin("salmon.ultimatewinterlevel", "UltimateWinterLevel", "1.0")]
    public class Core : CorePlugin
    {
        public static int levelID = -1;
        public override void OnGameInit()
        {
            CustomLevelData customLevelData = new CustomLevelData();
            var boardTag = new Board.BoardTag();
            customLevelData.BoardTag = boardTag;
            customLevelData.Name = () => "我是僵尸·陨冬雪影";
            customLevelData.SceneType = SceneType.Snow;
            customLevelData.RowCount = 5;
            customLevelData.WaveCount = () => 40;
            customLevelData.BgmType = MusicType.Snow_boss;
            customLevelData.Logo = GameAPP.resourcesManager.zombieSprites[ZombieType.UltimateSnowZombie];
            customLevelData.ZombieList = () => new List<ZombieType>()
            {
                ZombieType.UltimateSnowZombie,
                ZombieType.IceZombie,
                ZombieType.SnowDrownZombie,
                ZombieType.LevatationZombie,
                ZombieType.SnowShieldZombie,
                ZombieType.SnowGunZombie,
                ZombieType.SnowMonsterZombie,
                ZombieType.SuperSnowMonsterZombie,
                ZombieType.TallIceNutZombie,
                ZombieType.MiniSnowMonster
            };
            customLevelData.NeedSelectCard = false;
            levelID = CustomCore.RegisterCustomLevel(customLevelData);
        }
    }

    public class UltimateSnowZombieLevel : MonoBehaviour
    {
        public UltimateSnowZombie zombie => gameObject.GetComponent<UltimateSnowZombie>();
        public static float ColumnX => Mouse.Instance.GetBoxXFromColumn(1) - Mouse.Instance.GetBoxXFromColumn(0);
        public static float RowY => Mouse.Instance.GetBoxYFromRow(1) - Mouse.Instance.GetBoxYFromRow(0);

        public float speed = 6f;
        public Vector3 position;
        public Board? board;
        public int damage = 100;
        public float attackCountDown = 0.02f;
        public bool entering = true;

        public void Awake()
        {
            position = transform.position;
            board = Board.Instance;
            if (board == null)
            {
                Destroy(this);
                return;
            }
            this.StartCoroutine(Enter());
        }

        public IEnumerator Enter()
        {
            var start = transform.position;
            var target = new Vector3(-5f, start.y);
            float elapsedTime = 0f;

            while (elapsedTime < 1.5f)
            {
                float t = elapsedTime / 1.5f;

                // 线性插值
                transform.position = Vector3.Lerp(start, target, t);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = target;
            entering = false;
            yield break;
        }

        public void Update()
        {
            if (board == null) return;
            if (GameAPP.theGameStatus == GameStatus.InGame && !entering)
            {
                if (Input.GetKey(KeyCode.W))
                    position += new Vector3(0f, speed * Time.deltaTime);
                if (Input.GetKey(KeyCode.S))
                    position += new Vector3(0f, -speed * Time.deltaTime);
                if (Input.GetKey(KeyCode.A))
                    position += new Vector3(-speed * Time.deltaTime, 0f);
                if (Input.GetKey(KeyCode.D))
                    position += new Vector3(speed * Time.deltaTime, 0f);
                var x = position.x;
                x = Mathf.Clamp(x, Mouse.Instance.GetBoxXFromColumn(0), Mouse.Instance.GetBoxXFromColumn(board.columnNum - 1) + ColumnX);
                var y = position.y;
                y = Mathf.Clamp(y, board.boardMinY, board.boardMaxY);
                position = new Vector3(x, y);

                attackCountDown -= Time.deltaTime;

                zombie.SetPosition(position);
            }
            if (zombie != null && (zombie.beforeDying || zombie.theHealth <= 0))
                foreach (var component in CreateZombie.Instance.SetZombie(2, ZombieType.TrainingDummy, -9.9f).GetComponentsInChildren<SpriteRenderer>())
                    component.enabled = false;
        }

        public void OnTriggerStay2D(Collider2D collision)
        {
            if (collision != null && collision.gameObject != null && !collision.IsDestroyed() && !collision.gameObject.IsDestroyed())
            {
                if (collision.TryGetComponent<Zombie>(out var zombie) && zombie != null && !zombie.isMindControlled)
                {
                    if (attackCountDown <= 0f)
                    {
                        zombie.TakeDamage(DmgType.NormalAll, damage);
                        if (zombie.theZombieType is ZombieType.UltimateSnowZombie or ZombieType.SnowMonsterZombie or ZombieType.SuperSnowMonsterZombie)
                            this.zombie.TakeDamage(DmgType.Normal, 20);
                        attackCountDown = 0.02f;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(CheckAdv), nameof(CheckAdv.Start))]
    public static class CheckAdvPatch
    {
        [HarmonyPostfix]
        public static void Postfix(CheckAdv __instance)
        {
            if (__instance.theLevel == Core.levelID && __instance.transform.FindChild("Window/Name").GetComponent<TextMeshProUGUI>().text == "我是僵尸·陨冬雪影")
            {
                __instance.transform.FindChild("Window/Name").GetComponent<TextMeshProUGUI>().text = "陨冬雪影";
            }
        }
    }

    [HarmonyPatch(typeof(InGameUI), nameof(InGameUI.Update))]
    public class ShowTextPatch
    {
        [HarmonyPostfix]
        public static void Postfix(InGameUI __instance)
        {
            if (GameAPP.theBoardLevel == Core.levelID && GameAPP.theBoardType == Utils.CustomLevelType && GameAPP.theGameStatus == GameStatus.InGame &&
                !__instance.GetData<bool>("UltimateWinterLevel_LevelInit"))
            {
                CreateZombie.Instance.SetZombieWithMindControl(2, ZombieType.UltimateSnowZombie, -7.5f).GetComponent<UltimateSnowZombie>().
                    SetLevelBoss().AddComponent<UltimateSnowZombieLevel>();
                InGameText.Instance.ShowText("WASD控制移动，JKL使用技能，注意别让操控的究极雪原女皇死亡", 5f);
                __instance.SetData("UltimateWinterLevel_LevelInit", true);
            }
        }
    }

    [HarmonyPatch(typeof(UltimateSnowZombie))]
    public static class UltimateSnowZombiePatch
    {
        [HarmonyPatch(nameof(UltimateSnowZombie.MoveUpdate))]
        [HarmonyPrefix]
        public static bool PreMoveUpdate(UltimateSnowZombie __instance)
        {
            if (__instance.IsLevelBoss())
            {
                __instance.theSpeed = 0f;
                
                return false;
            }
            return true;
        }
    }

    public static class UltimateWinterLevelExt
    {
        public static bool IsLevelBoss(this UltimateSnowZombie zombie) => zombie.GetData<bool>("UltimateWinterLevel_IsLevel");
        public static UltimateSnowZombie SetLevelBoss(this UltimateSnowZombie zombie)
        {
            zombie.SetData("UltimateWinterLevel_IsLevel", true);
            return zombie;
        }
    }
}
