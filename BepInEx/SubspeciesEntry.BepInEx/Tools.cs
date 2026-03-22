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

        public static void AdjustLightLevel(Vector2 position, float radius, int level)
        {
            foreach (var collider in Physics2D.OverlapCircleAll(position, radius, LayerMask.GetMask("Plant")))
            {
                if (!collider.IsObjExist() || !collider.TryGetComponent<Plant>(out var plant) || !plant.IsObjExist()) continue;
                plant.currentLightLevel += level;
            }
        }
    }

    public static class ReflectionTools
    {
        public static MethodInfo? GetMethod(Type type, BindingFlags flags, string name) => type.GetMethod(name, flags);

        // 你说得对，但是这代码是ai写的，窝看不懂
        /// <summary>
        /// 获取MethodInfo(Action形式)
        /// </summary>
        /// <param name="method">原Method</param>
        /// <returns>Action，参数：(实例, 方法参数)</returns>
        public static Action<object, object[]> GetInvoker(MethodInfo method)
        {
            var dynamicMethod = new DynamicMethod(
                "GetInvoker",
                typeof(void),
                new Type[] { typeof(object), typeof(object[]) },
                method.DeclaringType.Module
            );

            var il = dynamicMethod.GetILGenerator();
            var parameters = method.GetParameters();

            if (!method.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                if (method.DeclaringType.IsValueType)
                {
                    il.Emit(OpCodes.Unbox, method.DeclaringType);
                }
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldelem_Ref);

                var paramType = parameters[i].ParameterType;
                if (paramType.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, paramType);
                }
                else if (paramType != typeof(object))
                {
                    il.Emit(OpCodes.Castclass, paramType);
                }
            }

            il.Emit(method.IsStatic ? OpCodes.Call : OpCodes.Call, method);
            il.Emit(OpCodes.Ret);

            return (Action<object, object[]>)dynamicMethod.CreateDelegate(
                typeof(Action<object, object[]>));
        }
    }

    public class Light
    {
        public Vector2 position;
        public int level = 0;
        public float radius = 0f;
        public List<(int, int)> AddBox = new();

        public Light(Vector2 position, int level, float radius)
        {
            this.position = position;
            this.level = level;
            this.radius = radius;
            foreach (var collider in Physics2D.OverlapCircleAll(position, radius, LayerMask.GetMask("Plant")))
            {
                if (!collider.IsObjExist() || !collider.TryGetComponent<Plant>(out var plant) || !plant.IsObjExist()) continue;
                if (AddBox.Contains((plant.thePlantColumn, plant.thePlantRow))) continue;
                CreatePlant.Instance.AdjustLightLevel(plant.thePlantColumn, plant.thePlantRow, level, 0);
                AddBox.Add((plant.thePlantColumn, plant.thePlantRow));
            }
        }

        public void MoveTo(Vector2 position)
        {
            // 清除已有植物的增益
            foreach (var (column, row) in AddBox)
            {
                CreatePlant.Instance.AdjustLightLevel(column, row, -level, 0);
            }
            AddBox.Clear();

            // 添加新的位置
            foreach (var collider in Physics2D.OverlapCircleAll(position, radius, LayerMask.GetMask("Plant")))
            {
                if (!collider.IsObjExist() || !collider.TryGetComponent<Plant>(out var plant) || !plant.IsObjExist()) continue;
                if (AddBox.Contains((plant.thePlantColumn, plant.thePlantRow))) continue;
                CreatePlant.Instance.AdjustLightLevel(plant.thePlantColumn, plant.thePlantRow, level, 0);
                AddBox.Add((plant.thePlantColumn, plant.thePlantRow));
            }
        }

        public void Die()
        {
            foreach (var (column, row) in AddBox)
            {
                CreatePlant.Instance.AdjustLightLevel(column, row, -level, 0);
            }

            AddBox.Clear();
        }
    }
}
