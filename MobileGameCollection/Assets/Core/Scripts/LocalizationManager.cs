using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;

    public enum Language { Turkish = 0, English = 1, German = 2, Chinese = 3 }

    public Language Current { get; private set; } = Language.Turkish;

    // [key] = { tr, en, de, zh }
    private static readonly Dictionary<string, string[]> Strings = new Dictionary<string, string[]>
    {
        { "app_title",   new[] { "Oyun Koleksiyonu", "Game Collection",  "Spielesammlung", "游戏合集" } },
        { "play",        new[] { "OYNA",   "PLAY",   "SPIELEN", "开始"  } },
        { "settings",    new[] { "Ayarlar","Settings","Einstellungen","设置" } },
        { "volume",      new[] { "Ses Seviyesi", "Volume",    "Lautstärke",  "音量" } },
        { "language",    new[] { "Dil",    "Language","Sprache",  "语言" } },
        { "close",       new[] { "Kapat",  "Close",   "Schließen","关闭" } },
    };

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Current = (Language)PlayerPrefs.GetInt("Language", 0);
    }

    public string Get(string key)
    {
        if (Strings.TryGetValue(key, out var arr))
            return arr[(int)Current];
        return key;
    }

    public static event System.Action OnLanguageChanged;

    public void SetLanguage(Language lang)
    {
        Current = lang;
        PlayerPrefs.SetInt("Language", (int)lang);
        PlayerPrefs.Save();
        OnLanguageChanged?.Invoke();
    }
}
