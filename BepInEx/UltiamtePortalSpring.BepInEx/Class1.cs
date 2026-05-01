using BepInEx;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using CustomizeLib.BepInEx;
using HarmonyLib;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace UltimatePortalSpring.BepInEx
{
    [BepInPlugin("salmon.ultimateportalspring", "UltimatePortalSpring", "1.0")]
    public class Core : CorePlugin
    {
        public override void OnStart()
        {
            var ab = CustomCore.GetAssetBundle(Tools.GetAssembly(), "ultimateportalspring");
            CustomCore.RegisterCustomPlant<UltimateSpring, UltimatePortalSpring>(UltimatePortalSpring.PlantID, ab.GetAsset<GameObject>("UltimatePortalSpringPrefab"),
                ab.GetAsset<GameObject>("UltimatePortalSpringPreview"), [], 0f, 0f, 300, 3000, 7.5f, 500);
            CustomCore.AddPlantAlmanacStrings(UltimatePortalSpring.PlantID, $"究极超时空弹弹菇",
                "投射蕴含时空力量的黑洞，能够持续吸引拉扯僵尸，并赋予传送状态。\n" +
                "<color=#0000FF>究极火神弹弹菇同人亚种</color>\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>转换配方：</color><color=red>橄榄帽←→超时空碎片</color>\n" +
                "<color=#3D1400>韧性：</color><color=red>3000</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>300</color>\n" +
                "<color=#3D1400>索敌范围：</color><color=red>前方4.5格</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①可手动点击发射，伤害x2\n" +
                "②蓄力时，使究极黑洞的持续时间提升（1+持续时间x5%）倍，蓄力上限30秒\n" +
                "③发射会休息1.5秒，投射究极黑洞种子，命中后生成究极黑洞</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①持续2.5秒，能吸引半径1.5格的僵尸减速25%-80%，并持续使僵尸移动到黑洞中心，僵尸与黑洞中心的距离越远幅度越高\n" +
                "②每存在0.5秒，黑洞的大小增加15%，至多200%\n" +
                "③每1秒对范围内的僵尸施加0.5秒的传送状态\n" +
                "④持续时间结束或存在7.5秒时坍缩，对半径1.5格范围造成7200的灰烬伤害，若已存在5秒则伤害和范围半径x5</color>\n" +
                "<color=#3D1400>词条1:</color><color=red>气定神闲：蓄力速度和蓄力上限x3</color>\n" +
                "<color=#3D1400>词条2:</color><color=red>僵尸试图在火海中游泳：黑洞的吸引效果和存在时间翻倍</color>\n\n" +
                "<color=#3D1400>宝开鱼</color>"); // 存在时间翻倍：7.5f->15f
            CustomCore.RegisterCustomBullet<Bullet_springMelon>(UltimatePortalSpring.BulletID, ab.GetAsset<GameObject>("Bullet_portalSpringMelon"));
            CustomCore.RegisterCustomParticle(UltimatePortalSpring.ParticleID, ab.GetAsset<GameObject>("PortalBombCloud"));
            CustomCore.RegisterCustomUseItemOnPlantEvent(PlantType.UltimateSpring, BucketType.PortalHeart, UltimatePortalSpring.PlantID);
            CustomCore.RegisterCustomUseItemOnPlantEvent(UltimatePortalSpring.PlantID, BucketType.Helmet, PlantType.UltimateSpring);
            UltimatePortalSpring.target = ab.GetAsset<GameObject>("target_portal");
            UltimatePortalSpring.hole = ab.GetAsset<GameObject>("BlackHole_portal");
            UltimatePortalSpring.hole.AddComponent<PortalHole>();
        }

        public static void SetLayer(Transform transform, string layer)
        {
            var parsprite = transform.GetComponent<SpriteRenderer>();
            if (parsprite != null) parsprite.sortingLayerName = layer;
            var pargroup = transform.GetComponent<SortingGroup>();
            if (pargroup != null) pargroup.sortingLayerName = layer;
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                var sprite = child.GetComponent<SpriteRenderer>();
                if (sprite != null) sprite.sortingLayerName = layer;
                var group = child.GetComponent<SortingGroup>();
                if (group != null) group.sortingLayerName = layer;
                SetLayer(child, layer);
            }
        }
    }

    public class UltimatePortalSpring : MonoBehaviour
    {
        public static ID PlantID = 1969;
        public static ID BulletID = 1969;
        public static ID ParticleID = 1969;
        public static GameObject target = null;
        public static GameObject hole = null;

        public void Awake()
        {
            plant.shoot = transform.FindChild("Shoot");
            Core.SetLayer(target.transform, "particle10");
            Core.SetLayer(hole.transform, "particle10");
        }

        public void Update()
        {
            if (plant == null || plant.energyText == null || plant.energyTextShadow == null) return;
            var timer = plant.timer;
            if (CoreTools.TravelUltimate("气定神闲"))
                timer *= 3;
            plant.energyText.text = $"{Math.Round(2.5f * (1 + 0.05f * timer), 2, MidpointRounding.AwayFromZero)}";
            plant.energyTextShadow.text = plant.energyText.text;
            if (plant.ThrowerSearchZombie() == null) plant.anim.ResetTrigger("shoot");
        }

        public void PortalShoot()
        {
            var tuple = plant.FindUmbrella(plant.shoot.position);
            var parameters = tuple.Item1;
            var umbrellaPlant = tuple.Item2;

            float x = plant.shoot.position.x;
            float y = plant.shoot.position.y;

            if (umbrellaPlant == null)
            {
                if (plant.targetZombie != null && plant.targetZombie.col != null)
                {
                    Bounds bounds = plant.targetZombie.col.bounds;

                    float targetX = bounds.center.x;

                    parameters = Lawnf.CalculateProjectileParameters(
                        new Vector2(x, y),
                        plant.firstTime,
                        plant.firstPostion,
                        Time.time,
                        plant.GetZombiePosition(plant.targetZombie),
                        plant.flightTime
                    );
                    
                    if (Mathf.Abs(targetX - bounds.center.z) < 1f)
                    {
                        x = targetX - bounds.center.z;
                        y = (bounds.center.y + bounds.extents.y) - 0.1f;
                    }
                }
                else
                {
                    parameters = Lawnf.CalculateProjectileParameters(
                        new Vector2(x, y),
                        plant.firstTime,
                        plant.firstPostion,
                        Time.time,
                        plant.firstPostion,
                        plant.flightTime
                    );
                }
            }

            var bullet = CreateBullet.Instance.SetBullet(x, y, plant.thePlantRow, BulletID, BulletMoveWay.Throw);
            if (parameters != null && parameters.Length > 1)
            {
                bullet.Vx = parameters[1];
                if (parameters.Length > 2)
                {
                    bullet.Vy = parameters[2];
                    if (parameters.Length > 3)
                    {
                        bullet.detaVy = -parameters[3];
                    }
                }
            }

            bullet.targetPlant = umbrellaPlant;

            bullet.Damage = plant.attackDamage;
            bullet.fromType = plant.thePlantType;

            if (plant.melonSputter)
                bullet.melonSputter = true;

            if (plant.PumpkinType == PlantType.MelonPumpkin)
                plant.MelonShoot();

            var timer = plant.timer;
            if (CoreTools.TravelUltimate("气定神闲"))
                timer *= 3;
            bullet.SetData("UltimatePortalSpring_HoleTime", 2.5f * (1 + 0.05f * timer));
            plant.timer = 0f;

            plant.targetZombie = null;
            Extension.StartCoroutine(this, ResetCharge());

            GameAPP.PlaySound(UnityEngine.Random.Range(3, 5), 0.5f, 1.0f);
        }

        public IEnumerator ResetCharge()
        {
            yield return new WaitForSeconds(1.5f);
            if (plant != null) plant.anim.SetTrigger("charge");
            yield break;
        }

        public void PortalShootByMouse()
        {
            var bullet = CreateBullet.Instance.SetBullet(plant.shoot.position.x, plant.shoot.position.y, Mouse.Instance.GetRowFromY(plant.cannonTarget.x, plant.cannonTarget.y),
                BulletID, BulletMoveWay.Throw);
            if (bullet == null)
                return;

            Lawnf.SetBulletTarget(
                bullet,
                plant.shoot.position,
                Vector2.zero,
                plant.cannonTarget,
                1.5f
            );

            bullet.melonSputter = plant.melonSputter;
            bullet.fromType = plant.thePlantType;

            var timer = plant.timer;
            if (CoreTools.TravelUltimate("气定神闲"))
                timer *= 3;
            bullet.SetData("UltimatePortalSpring_HoleTime", 2.5f * (1 + 0.05f * timer));
            plant.timer = 0f;

            bullet.Damage *= 2;
            Extension.StartCoroutine(this, ResetCharge());
        }

        public void SetShootTarget(Vector2 position)
        {
            if (plant.theStatus == PlantStatus.Melonfume_charge)
            {
                plant.anim.SetTrigger("shoot2");

                plant.cannonTarget = position;
            }
        }

        public UltimateSpring plant => gameObject.GetComponent<UltimateSpring>();
    }

    public class PortalHole : MonoBehaviour
    {
        // 静态字段：全局记录所有正在被影响的僵尸及其原始速度
        private static Dictionary<Zombie, float> affectedZombies = new Dictionary<Zombie, float>();
        // 存储被吸引的子弹信息
        private class AttractedBullet
        {
            public Bullet bullet;
            public float angle;           // 当前角度
            public float radius;          // 当前轨道半径
            public float speed;           // 角速度
            public float attractSpeed;    // 向中心靠近的速度
            public float startAngle;      // 开始旋转的起始角度
        }
        private List<AttractedBullet> attractedBullets = new List<AttractedBullet>();

        public float timer = 2.5f;
        public float live = 0f;
        public float attackCountDown = 1f;
        public Board board;
        public Dictionary<Zombie, float> myAffectedZombies = new();
        private bool isDying = false;
        private bool isShrinking = false;
        private float shrinkDuration = 1f;
        private float shrinkElapsed = 0f;
        private Vector3 originalScale;

        private int absorbedBulletCount = 0;
        private Vector3 baseScale;
        private float maxGrowScale = 2f;
        private float autoGrowTargetScale = 1f; // 自主增长的目标大小

        // 吸引中心点（向下偏移0.5f）
        private Vector3 AttractCenter => transform.position - new Vector3(0f, 0.5f, 0f);

        public static bool IsPortalHoleInRange(Vector3 center, float radius)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(center, radius);
            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent<PortalHole>(out _))
                {
                    return true;
                }
            }
            return false;
        }

        public void Start()
        {
            timer = Mathf.Min(CoreTools.TravelUltimate("僵尸试图在火海中游泳") ? 15f : 7.5f, timer);
            board = Board.Instance;
            if (board == null)
                Destroy(gameObject);

            baseScale = transform.localScale;
            originalScale = baseScale;
            autoGrowTargetScale = baseScale.x * maxGrowScale;
        }

        public void Update()
        {
            if (isDying) return;

            if (isShrinking)
            {
                shrinkElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(shrinkElapsed / shrinkDuration);
                float smoothT = Mathf.SmoothStep(1f, 0f, t);
                float targetScale = Mathf.Lerp(originalScale.x, 0.2f, 1f - smoothT);
                transform.localScale = new Vector3(targetScale, targetScale, originalScale.z);

                if (t >= 1f)
                {
                    Die();
                }
                return;
            }

            // 检查并恢复离开作用范围的僵尸
            CheckAndRestoreZombies();

            attackCountDown -= Time.deltaTime;
            var attack = false;
            if (attackCountDown <= 0f)
            {
                attackCountDown = 1f;
                attack = true;
            }

            // 黑洞自动变大 - 只增不减，且只在未达到目标大小时执行
            if (transform.localScale.x < autoGrowTargetScale)
            {
                float growthFactor = 1f + (0.5f * (1f - 1f / (1f + live * 2f)));
                float targetScaleX = Mathf.Min(baseScale.x * growthFactor, autoGrowTargetScale);

                if (targetScaleX > transform.localScale.x)
                {
                    transform.localScale = new Vector3(targetScaleX, targetScaleX, transform.localScale.z);
                }
            }

            // 更新被吸引的子弹
            UpdateAttractedBullets();

            // 吸收新子弹
            AttractBullets();

            // 减速僵尸和吸引效果（使用偏移后的中心点）
            float maxRadius = CoreTools.ColumnX * 1.5f * transform.localScale.x;
            Vector3 attractCenter = AttractCenter;

            foreach (var collider in Physics2D.OverlapCircleAll(attractCenter, maxRadius, LayerMask.GetMask("Zombie")))
            {
                if (collider.IsObjExist() && collider.TryGetComponent<Zombie>(out var zombie) && zombie.IsObjExist())
                {
                    if (affectedZombies.ContainsKey(zombie) && !myAffectedZombies.ContainsKey(zombie))
                        continue;

                    float distance = Vector2.Distance(attractCenter, zombie.axis.transform.position);

                    float normalizedDistance = Mathf.Max(distance / maxRadius, 0.1f);
                    float effectStrength = 1f / normalizedDistance;
                    effectStrength = Mathf.Min(effectStrength, 5f);

                    float speedMultiplier = Mathf.Lerp(0.3f, 1f, 1f - Mathf.Clamp01(effectStrength / 5f));
                    float attractionForce = Mathf.Lerp(0.5f, 5f, effectStrength / 5f);

                    if (attack)
                    {
                        if (zombie.GetAttrTimers().portaledTimer > 0f)
                            zombie.GetAttrTimers().portaledTimer += 0.5f;
                        else
                            zombie.SetPortaled(0.5f);
                    }

                    if (!myAffectedZombies.ContainsKey(zombie))
                    {
                        if (!affectedZombies.ContainsKey(zombie))
                        {
                            affectedZombies.Add(zombie, zombie.theOriginSpeed);
                        }
                        myAffectedZombies.Add(zombie, zombie.theOriginSpeed);
                    }

                    float originalSpeed = myAffectedZombies[zombie];
                    zombie.theOriginSpeed = originalSpeed * speedMultiplier;

                    float moveDistance = attractionForce * Time.deltaTime;
                    zombie.SetPosition(Vector3.MoveTowards(zombie.axis.position, attractCenter, moveDistance));
                    zombie.theZombieRow = Mouse.Instance.GetRowFromY(zombie.axis.position.x, zombie.axis.position.y);
                }
            }

            live += Time.deltaTime;
            if (live >= timer && !isShrinking)
            {
                isShrinking = true;
                shrinkElapsed = 0f;
                originalScale = transform.localScale;
            }
        }

        private void CheckAndRestoreZombies()
        {
            float maxRadius = CoreTools.ColumnX * 1.5f * transform.localScale.x;
            Vector3 attractCenter = AttractCenter;
            List<Zombie> zombiesToRestore = new List<Zombie>();

            foreach (var kvp in myAffectedZombies)
            {
                Zombie zombie = kvp.Key;
                if (zombie == null || !zombie.IsObjExist())
                {
                    zombiesToRestore.Add(zombie);
                    continue;
                }

                float distance = Vector2.Distance(attractCenter, zombie.axis.transform.position);
                if (distance > maxRadius)
                {
                    zombiesToRestore.Add(zombie);
                }
            }

            foreach (var zombie in zombiesToRestore)
            {
                if (zombie != null && zombie.IsObjExist() && myAffectedZombies.ContainsKey(zombie))
                {
                    float originalSpeed = myAffectedZombies[zombie];
                    zombie.theOriginSpeed = originalSpeed;

                    if (zombie.GetAttrTimers().portaledTimer > 0f)
                    {
                        zombie.GetAttrTimers().portaledTimer = 0f;
                    }

                    myAffectedZombies.Remove(zombie);

                    if (affectedZombies.ContainsKey(zombie))
                    {
                        bool isAffectedByOther = false;
                        var allPortals = FindObjectsOfType<PortalHole>();
                        foreach (var portal in allPortals)
                        {
                            if (portal != this && portal.myAffectedZombies.ContainsKey(zombie))
                            {
                                isAffectedByOther = true;
                                break;
                            }
                        }

                        if (!isAffectedByOther)
                        {
                            affectedZombies.Remove(zombie);
                        }
                    }
                }
            }
        }

        private void AttractBullets()
        {
            float attractRadius = CoreTools.ColumnX * 1.7f * transform.localScale.x;

            foreach (var collider in Physics2D.OverlapCircleAll(transform.position, attractRadius, LayerMask.GetMask("Bullet")))
            {
                if (collider.IsObjExist() && collider.TryGetComponent<Bullet>(out var bullet))
                {
                    if (attractedBullets.Any(ab => ab.bullet == bullet))
                        continue;

                    if (bullet.theBulletType == UltimatePortalSpring.BulletID)
                    {
                        Vector2 direction = bullet.transform.position - transform.position;
                        float distance = direction.magnitude;
                        float initialAngle = Mathf.Atan2(direction.y, direction.x);

                        float angularSpeed = 5f / Mathf.Max(distance, 0.5f);
                        float inwardSpeed = 3f / Mathf.Max(distance, 0.5f);

                        bullet.SetData("UltimatePortalSpring_BulletDieByHole", true);
                        foreach (var col in bullet.GetComponents<BoxCollider2D>())
                            col.enabled = false;

                        AttractedBullet attractedBullet = new AttractedBullet
                        {
                            bullet = bullet,
                            angle = initialAngle,
                            radius = distance,
                            speed = angularSpeed,
                            attractSpeed = inwardSpeed,
                            startAngle = initialAngle
                        };

                        attractedBullets.Add(attractedBullet);
                    }
                }
            }
        }

        private void UpdateAttractedBullets()
        {
            for (int i = attractedBullets.Count - 1; i >= 0; i--)
            {
                AttractedBullet attracted = attractedBullets[i];

                if (attracted.bullet == null || !attracted.bullet.IsObjExist())
                {
                    attractedBullets.RemoveAt(i);
                    continue;
                }

                float angleDelta = attracted.speed * Time.deltaTime;
                attracted.angle += angleDelta;

                float rotatedAngle = Mathf.Abs(attracted.angle - attracted.startAngle);
                float currentRotationCount = rotatedAngle / (2f * Mathf.PI);

                if (currentRotationCount >= 2f)
                {
                    AbsorbBullet(attracted.bullet);
                    attractedBullets.RemoveAt(i);
                    continue;
                }

                attracted.radius -= attracted.attractSpeed * Time.deltaTime;
                float minRadius = CoreTools.ColumnX * 0.3f * transform.localScale.x;
                if (attracted.radius < minRadius)
                {
                    attracted.radius = minRadius;
                }

                float currentDistance = Mathf.Max(attracted.radius, 0.3f);
                attracted.speed = 8f / currentDistance;
                attracted.attractSpeed = 5f / currentDistance;

                float x = transform.position.x + Mathf.Cos(attracted.angle) * attracted.radius;
                float y = transform.position.y + Mathf.Sin(attracted.angle) * attracted.radius;
                attracted.bullet.transform.position = new Vector3(x, y, attracted.bullet.transform.position.z);
            }
        }

        private void AbsorbBullet(Bullet bullet)
        {
            if (bullet == null || !bullet.IsObjExist())
                return;

            absorbedBulletCount++;

            // 直接增加scale，不做任何其他修改
            float newScale = transform.localScale.x + 0.1f;
            transform.localScale = new Vector3(newScale, newScale, transform.localScale.z);

            bullet.Die();
        }

        public void Die()
        {
            if (isDying) return;
            isDying = true;

            // 1. 销毁所有正在圆周运动的子弹（还未被吸收的）
            foreach (var attracted in attractedBullets)
            {
                if (attracted.bullet != null && attracted.bullet.IsObjExist())
                {
                    // 恢复子弹的碰撞器（如果需要）
                    // 直接销毁子弹
                    attracted.bullet.Die();
                }
            }
            attractedBullets.Clear();

            // 2. 恢复所有受当前黑洞影响的僵尸速度
            foreach (var kvp in myAffectedZombies.ToList())
            {
                Zombie zombie = kvp.Key;
                if (zombie != null && zombie.IsObjExist())
                {
                    // 恢复原始速度
                    zombie.theOriginSpeed = kvp.Value;

                    // 重置传送门效果标记
                    if (zombie.GetAttrTimers().portaledTimer > 0f)
                    {
                        zombie.GetAttrTimers().portaledTimer = 0f;
                    }
                }

                // 从全局字典中移除
                if (affectedZombies.ContainsKey(zombie))
                {
                    bool isAffectedByOther = false;
                    var allPortals = FindObjectsOfType<PortalHole>();
                    foreach (var portal in allPortals)
                    {
                        if (portal != this && portal.myAffectedZombies.ContainsKey(zombie))
                        {
                            isAffectedByOther = true;
                            break;
                        }
                    }

                    if (!isAffectedByOther)
                    {
                        affectedZombies.Remove(zombie);
                    }
                }
            }
            myAffectedZombies.Clear();

            // 3. 造成爆炸伤害（使用原始位置，因为爆炸应该以黑洞为中心）
            var buff = live >= 5f ? 5 : 1;
            var damage = 7200 * buff;
            float explosionRadius = CoreTools.ColumnX * 1.5f * transform.localScale.x * buff;
            foreach (var collider in Physics2D.OverlapCircleAll(transform.position, explosionRadius, LayerMask.GetMask("Zombie")))
            {
                if (collider.IsObjExist() && collider.TryGetComponent<Zombie>(out var zombie) && zombie.IsObjExist())
                {
                    zombie.TakeDamage(DmgType.Carred, damage, UltimatePortalSpring.PlantID);
                }
            }

            // 4. 播放死亡特效和音效
            CreateParticle.SetParticle(UltimatePortalSpring.ParticleID, transform.position, 11);
            GameAPP.PlaySound(40, 0.5f, 1.0f);

            // 5. 销毁对象
            Destroy(gameObject);
        }

        public void OnDestroy()
        {
            if (!isDying)
            {
                Die();
            }
        }

        public void OnApplicationQuit()
        {
            // 恢复所有僵尸的原始速度（全局清理）
            foreach (var kvp in affectedZombies)
            {
                if (kvp.Key != null && kvp.Key.IsObjExist())
                {
                    kvp.Key.theOriginSpeed = kvp.Value;
                    if (kvp.Key.GetAttrTimers().portaledTimer > 0f)
                    {
                        kvp.Key.GetAttrTimers().portaledTimer = 0f;
                    }
                }
            }
            affectedZombies.Clear();

            // 清理所有被吸引的子弹
            foreach (var portal in FindObjectsOfType<PortalHole>())
            {
                foreach (var attracted in portal.attractedBullets)
                {
                    if (attracted.bullet != null && attracted.bullet.IsObjExist())
                    {
                        attracted.bullet.Die();
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Bullet_springMelon))]
    public static class Bullet_springMelonPatch
    {
        [HarmonyPatch(nameof(Bullet_springMelon.HitZombie))]
        [HarmonyPrefix]
        public static bool PreHitZombie(Bullet_springMelon __instance, ref Zombie zombie)
        {
            if (__instance.theBulletType == UltimatePortalSpring.BulletID)
            {
                if (!__instance.GetData<bool>("UltimatePortalSpring_BulletDieByHole") && !PortalHole.IsPortalHoleInRange(__instance.transform.position, 1f))
                {
                    zombie.TakeDamage(DmgType.Normal, __instance.Damage, __instance.fromType);
                    zombie.SetJalaed();
                    var effectPos = zombie.axis.position + new Vector3(0f, 0.5f, 0f);

                    var hole = UnityEngine.Object.Instantiate(UltimatePortalSpring.hole, effectPos, Quaternion.identity, __instance.board.transform)?.GetComponent<PortalHole>();
                    if (hole != null) hole.timer = __instance.GetData<float>("UltimatePortalSpring_HoleTime");

                    CreateParticle.SetParticle(UltimatePortalSpring.ParticleID, effectPos, 11);
                    __instance.AttackOtherZombie(zombie, MelonSputterType.RealJala);

                    GameAPP.PlaySound(40, 0.5f, 1.0f);
                    __instance.Die();
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(Bullet_springMelon.HitLand))]
        [HarmonyPrefix]
        public static bool PreHitLand(Bullet_springMelon __instance)
        {
            if (__instance.theBulletType == UltimatePortalSpring.BulletID)
            {
                if (!__instance.GetData<bool>("UltimatePortalSpring_BulletDieByHole") && !PortalHole.IsPortalHoleInRange(__instance.transform.position, 1f))
                {
                    var effectPos = __instance.transform.position;

                    var hole = UnityEngine.Object.Instantiate(UltimatePortalSpring.hole, effectPos, Quaternion.identity, __instance.board.transform)?.GetComponent<PortalHole>();
                    if (hole != null) hole.timer = __instance.GetData<float>("UltimatePortalSpring_HoleTime");

                    CreateParticle.SetParticle(UltimatePortalSpring.ParticleID, effectPos, 11);

                    GameAPP.PlaySound(40, 0.5f, 1.0f);
                    __instance.Die();
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Bullet))]
    public static class BulletPatch
    {
        [HarmonyPatch(nameof(Bullet.Die))]
        [HarmonyPostfix]
        public static void PostDie(Bullet __instance)
        {
            if (__instance.theBulletType == UltimatePortalSpring.BulletID)
            {
                foreach (var col in __instance.GetComponents<BoxCollider2D>())
                    col.enabled = true;
                __instance.SetData("UltimatePortalSpring_BulletDieByHole", false);
            }
        }
    }

    [HarmonyPatch(typeof(Mouse))]
    public static class MousePatch
    {
        [HarmonyPatch(nameof(Mouse.LeftClickWithNothing))]
        [HarmonyPostfix]
        public static void PostLeftClickWithNothing(Mouse __instance)
        {
            if (__instance.theItemOnMouse == null)
            {
                var hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                foreach (var plant in __instance.GetPlantsOnMouse(hits))
                {
                    if (!plant.IsObjExist()) continue;
                    if (plant.thePlantType == UltimatePortalSpring.PlantID)
                    {
                        if (plant.theStatus == PlantStatus.Melonfume_charge)
                        {
                            __instance.cannonPlant = plant;
                            __instance.theItemOnMouse = UnityEngine.Object.Instantiate(UltimatePortalSpring.target, Camera.main.ScreenToWorldPoint(Input.mousePosition),
                                Quaternion.identity, __instance.transform);
                            __instance.theItemOnMouse.name = "target_portal";
                        }
                        break;
                    }
                }
            }
        }

        [HarmonyPatch(nameof(Mouse.LeftClickWithSomeThing))]
        [HarmonyPostfix]
        public static void PostLeftClickWithSomeThing(Mouse __instance)
        {
            if (__instance.theItemOnMouse != null && __instance.cannonPlant != null && __instance.cannonPlant.thePlantType == UltimatePortalSpring.PlantID &&
                __instance.theItemOnMouse.name == "target_portal")
            {
                __instance.cannonPlant.GetComponent<UltimatePortalSpring>().SetShootTarget(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                __instance.ClearItemOnMouse(true);
            }
        }
    }
}
