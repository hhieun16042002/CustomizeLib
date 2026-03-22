using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace SubspeciesEntry.BepInEx
{
    public static class TypeInit
    {
        public static void Init()
        {
            ClassInjector.RegisterTypeInIl2Cpp<DataSave>();
        }
    }

    public class DataSave : MonoBehaviour
    {
        private Dictionary<string, object> SaveData { get; set; } = new();

        public T GetData<T>(string key)
        {
            if (SaveData.ContainsKey(key))
                return (T)SaveData[key];
            return default;
        }

        public void SetData(string key, object value)
        {
            if (!SaveData.ContainsKey(key))
                SaveData.Add(key, value);
            else
                SaveData[key] = value;
        }
    }

    public static class DataExtension
    {
        public static T GetData<T>(this Component comp, string key) => comp.gameObject.GetOrAddComponent<DataSave>().GetData<T>(key);
        public static T GetData<T>(this GameObject go, string key) => go.GetOrAddComponent<DataSave>().GetData<T>(key);
        public static void SetData(this Component comp, string key, object value) => comp.gameObject.GetOrAddComponent<DataSave>().SetData(key, value);
        public static void SetData(this GameObject go, string key, object value) => go.GetOrAddComponent<DataSave>().SetData(key, value);
    }
}
