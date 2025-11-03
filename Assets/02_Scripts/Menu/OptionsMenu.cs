using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class OptionsMenu : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer mixer;
    public string musicParam = "MusicVolume";
    public Slider musicSlider;
    public TextMeshProUGUI musicValueText;

    [Header("Pantalla")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    [Header("Navegación")]
    public GameObject mainPanel;      // (opcional) Panel_Main para re-mostrar al cerrar
    public float fadeTime = 0.2f;

    // Internos
    CanvasGroup _cg;
    List<Resolution> _resList;
    bool _visible;

    const string KEY_VOL = "opt_music_vol_db";
    const string KEY_W = "opt_res_w";
    const string KEY_H = "opt_res_h";
    const string KEY_FS = "opt_fullscreen";

    void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        HideInstant();
    }

    void Start()
    {
        InitAudio();
        InitDisplay();
    }

    void Update()
    {
        if (_visible && Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    // ---------- PUBLIC: abrir/cerrar ----------
    public void Open()
    {
        _visible = true;
        if (mainPanel) mainPanel.SetActive(false);   // ⬅️ APAGA EL MAIN
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(Fade(0f, 1f));                 // fade del CanvasGroup
    }


    public void Close()
    {
        _visible = false;
        StopAllCoroutines();
        StartCoroutine(Fade(1f, 0f, () => {
            gameObject.SetActive(false);
            if (mainPanel) mainPanel.SetActive(true); // ⬅️ ENCIENDE EL MAIN
        }));
    }


    public void Toggle()
    {
        if (_visible) Close(); else Open();
    }

    void HideInstant()
    {
        _cg.alpha = 0f;
        _cg.blocksRaycasts = false;
        _cg.interactable = false;
        gameObject.SetActive(false);
    }

    System.Collections.IEnumerator Fade(float from, float to, System.Action onEnd = null)
    {
        _cg.blocksRaycasts = true;
        _cg.interactable = true;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.001f, fadeTime);
            _cg.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
        _cg.alpha = to;
        if (to == 0f)
        {
            _cg.blocksRaycasts = false;
            _cg.interactable = false;
        }
        onEnd?.Invoke();
    }

    // ---------- AUDIO ----------
    void InitAudio()
    {
        float db = PlayerPrefs.HasKey(KEY_VOL) ? PlayerPrefs.GetFloat(KEY_VOL, -12f) : -12f;
        mixer.SetFloat(musicParam, db);
        float lin = DbToLinear(db);
        if (musicSlider)
        {
            musicSlider.SetValueWithoutNotify(lin);
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }
        UpdateMusicText(lin);
    }

    public void OnMusicChanged(float linear01)
    {
        float db = LinearToDb(linear01);
        mixer.SetFloat(musicParam, db);
        PlayerPrefs.SetFloat(KEY_VOL, db);
        UpdateMusicText(linear01);
    }

    void UpdateMusicText(float linear01)
    {
        if (musicValueText) musicValueText.text = Mathf.RoundToInt(linear01 * 100f) + "%";
    }

    // ---------- DISPLAY ----------
    void InitDisplay()
    {
        var distinct = new Dictionary<(int, int), Resolution>();
        foreach (var r in Screen.resolutions)
        {
            var key = (r.width, r.height);
            if (!distinct.ContainsKey(key) || r.refreshRateRatio.value > distinct[key].refreshRateRatio.value)
                distinct[key] = r;
        }
        _resList = distinct.Values.OrderBy(r => r.width).ThenBy(r => r.height).ToList();

        var options = _resList.Select(r => $"{r.width} x {r.height}").ToList();
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);

        int curW = PlayerPrefs.GetInt(KEY_W, Screen.width);
        int curH = PlayerPrefs.GetInt(KEY_H, Screen.height);
        bool fs = PlayerPrefs.GetInt(KEY_FS, Screen.fullScreen ? 1 : 0) == 1;

        int idx = _resList.FindIndex(r => r.width == curW && r.height == curH);
        if (idx < 0) idx = _resList.FindIndex(r => r.width == Screen.width && r.height == Screen.height);
        if (idx < 0) idx = Mathf.Clamp(_resList.Count - 1, 0, int.MaxValue);

        resolutionDropdown.SetValueWithoutNotify(idx);
        resolutionDropdown.RefreshShownValue();
        fullscreenToggle.SetIsOnWithoutNotify(fs);
    }

    public void ApplyAndSave()
    {
        if (_resList == null || _resList.Count == 0) return;

        int idx = Mathf.Clamp(resolutionDropdown.value, 0, _resList.Count - 1);
        var r = _resList[idx];
        bool fs = fullscreenToggle.isOn;

        Screen.fullScreenMode = fs ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.SetResolution(r.width, r.height, fs);

        PlayerPrefs.SetInt(KEY_W, r.width);
        PlayerPrefs.SetInt(KEY_H, r.height);
        PlayerPrefs.SetInt(KEY_FS, fs ? 1 : 0);
        PlayerPrefs.Save();
    }

    // ---------- Utils ----------
    static float LinearToDb(float linear) => Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
    static float DbToLinear(float db) => Mathf.Pow(10f, db / 20f);
}
