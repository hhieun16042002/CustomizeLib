using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace CustomizeLib.BepInEx
{
    public static class PlantExtensions
    {
        public static TextMeshProUGUI RegisterText(this Plant plant, Color color, Func<string> func, Vector2? size = null)
        {
            if (func == null) return null;
            if (plant == null || plant.healthSlider == null) return null;
            var healthText = plant.healthSlider.healthTextContainer.gameObject.AddComponent<CustomHealthText>();
            var text = UnityEngine.Object.Instantiate(plant.healthSlider.healthText, plant.healthSlider.healthTextContainer).GetComponent<TextMeshProUGUI>();
            text.color = color;
            text.gameObject.SetActive(true);
            text.text = func.Invoke();
            healthText.registedTexts.Add(text, func);
            if (size != null)
                text.GetComponent<RectTransform>().sizeDelta = size.Value;
            return text;
        }

        public static void ClearAllText(this Plant plant)
        {
            if (plant.healthSlider == null) return;
            foreach (var kvp in plant.healthSlider.registedTexts)
                UnityEngine.Object.Destroy(kvp.Key.gameObject);
            plant.healthSlider.registedTexts.Clear();
        }
    }

    public class CustomHealthText : MonoBehaviour
    {
        public Dictionary<TextMeshProUGUI, Func<string>> registedTexts = new();

        public void Update()
        {
            foreach (var (key, value) in registedTexts)
                key.text = value.Invoke();
        }
    }
}
