using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class DropdownUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string selectorTag = "Tag";
    [SerializeField] private bool saveValue;

    [Header("Dropdown")]
    [SerializeField] private TMP_Dropdown dropdown;

    [Header("Items")]
    [SerializeField] private List<ItemSelector> itemSelectors = new();

    private string prefsKey;
    private int index = 0;

    // Predefined common resolutions (giới hạn tối đa 1920x1080)
    private readonly Vector2Int[] commonResolutions = new Vector2Int[]
    {
        new Vector2Int(1920, 1080),
        new Vector2Int(1680, 1050),
        new Vector2Int(1600, 900),
        new Vector2Int(1440, 900),
        new Vector2Int(1366, 768),
        new Vector2Int(1280, 1024),
        new Vector2Int(1280, 800),
        new Vector2Int(1280, 720),
        new Vector2Int(1024, 768),
        new Vector2Int(800, 600)
    };

    private void Start()
    {
        prefsKey = selectorTag + "DropdownSelector";
        itemSelectors.Clear();

        if (selectorTag == "Resolutions")
        {
            CreateResolutionDropdown();
        }
        else if (selectorTag == "RefreshRate")
        {
            CreateRefreshRateDropdown();
        }
        else if (selectorTag == "FPS")
        {
            CreateFPSDropdown();
        }
        else if (selectorTag == "WindowMode")
        {
            CreateWindowModeDropdown();
        }
        else if (selectorTag == "Quality")
        {
            CreateQualityDropdown();
        }
        else if (selectorTag == "Shadows")
        {
            CreateShadowsDropdown();
        }
        else if (selectorTag == "AntiAliasing")
        {
            CreateAntiAliasingDropdown();
        }
        else if (selectorTag == "TextureQuality")
        {
            CreateTextureQualityDropdown();
        }
        else if (selectorTag == "VSync")
        {
            CreateVSyncDropdown();
        }
        else if (selectorTag == "AnisotropicFiltering")
        {
            CreateAnisotropicFilteringDropdown();
        }
        else if (selectorTag == "ShadowDistance")
        {
            CreateShadowDistanceDropdown();
        }
        else if (selectorTag == "ShadowResolution")
        {
            CreateShadowResolutionDropdown();
        }
        else if (selectorTag == "RealtimeReflectionProbes")
        {
            CreateRealtimeReflectionProbesDropdown();
        }
        else if (selectorTag == "ParticleRaycastBudget")
        {
            CreateParticleRaycastBudgetDropdown();
        }
        else
        {
            index = PlayerPrefs.GetInt(prefsKey, 0);
        }

        if (saveValue)
            index = PlayerPrefs.GetInt(prefsKey, index);

        CreateDropdown();

        if (itemSelectors.Count > 0 && index < itemSelectors.Count)
        {
            dropdown.value = index;
            itemSelectors[index].OnValueChange?.Invoke();
            if (saveValue)
                PlayerPrefs.SetInt(prefsKey, index);
        }

        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void CreateResolutionDropdown()
    {
        // Lấy tất cả resolutions từ system
        Resolution[] systemResolutions = Screen.resolutions;
        HashSet<Vector2Int> uniqueResolutions = new HashSet<Vector2Int>();

        // Thêm các resolution phổ biến trước (nếu system hỗ trợ)
        foreach (var commonRes in commonResolutions)
        {
            bool isSupported = systemResolutions.Any(res => 
                res.width == commonRes.x && res.height == commonRes.y);
            
            if (isSupported && commonRes.x <= 1920 && commonRes.y <= 1080)
            {
                uniqueResolutions.Add(commonRes);
            }
        }

        // Thêm các resolution khác từ system (giới hạn 1920x1080)
        foreach (var res in systemResolutions)
        {
            if (res.width <= 1920 && res.height <= 1080)
            {
                uniqueResolutions.Add(new Vector2Int(res.width, res.height));
            }
        }

        // Sắp xếp theo thứ tự giảm dần
        var sortedResolutions = uniqueResolutions.OrderByDescending(r => r.x * r.y).ToList();

        foreach (var res in sortedResolutions)
        {
            string title = $"{res.x}x{res.y}";
            Vector2Int currentRes = res;

            var selector = new ItemSelector { title = title };
            selector.OnValueChange.AddListener(() =>
            {
                // Tìm refresh rate phù hợp
                var matchingRes = systemResolutions.FirstOrDefault(r => 
                    r.width == currentRes.x && r.height == currentRes.y);
                
                if (matchingRes.width > 0)
                {
                    Screen.SetResolution(currentRes.x, currentRes.y, Screen.fullScreen, matchingRes.refreshRate);
                }
            });

            itemSelectors.Add(selector);
        }

        // Tìm resolution hiện tại
        Resolution current = Screen.currentResolution;
        for (int i = 0; i < sortedResolutions.Count; i++)
        {
            if (sortedResolutions[i].x == current.width && sortedResolutions[i].y == current.height)
            {
                index = i;
                break;
            }
        }
    }

    private void CreateRefreshRateDropdown()
    {
        Resolution[] resolutions = Screen.resolutions;
        int width = Screen.currentResolution.width;
        int height = Screen.currentResolution.height;

        HashSet<int> refreshRates = new HashSet<int>();

        foreach (var res in resolutions)
        {
            if (res.width == width && res.height == height)
            {
                refreshRates.Add(res.refreshRate);
            }
        }

        var sortedRates = refreshRates.OrderByDescending(r => r).ToList();

        foreach (int rate in sortedRates)
        {
            string title = $"{rate} Hz";
            int currentRate = rate;

            var selector = new ItemSelector { title = title };
            selector.OnValueChange.AddListener(() =>
            {
                Screen.SetResolution(width, height, Screen.fullScreen, currentRate);
            });

            itemSelectors.Add(selector);
        }

        int currentRefreshRate = Screen.currentResolution.refreshRate;
        index = sortedRates.IndexOf(currentRefreshRate);
        if (index < 0) index = 0;
    }

    private void CreateFPSDropdown()
    {
        int[] fpsOptions = { -1, 240, 144, 120, 60, 30 }; // -1 = Unlimited
        foreach (int fps in fpsOptions)
        {
            string title = (fps == -1) ? "Unlimited" : $"{fps} FPS";
            int cap = fps;

            var selector = new ItemSelector { title = title };
            selector.OnValueChange.AddListener(() =>
            {
                Application.targetFrameRate = cap;
            });

            itemSelectors.Add(selector);
        }

        index = Array.IndexOf(fpsOptions, Application.targetFrameRate);
        if (index < 0) index = 0;
    }

    private void CreateWindowModeDropdown()
    {
        string[] modes = { "Fullscreen", "Borderless", "Windowed" };
        FullScreenMode[] fullScreenModes = { FullScreenMode.ExclusiveFullScreen, FullScreenMode.FullScreenWindow, FullScreenMode.Windowed };

        for (int i = 0; i < modes.Length; i++)
        {
            int modeIndex = i;
            var selector = new ItemSelector { title = modes[i] };
            selector.OnValueChange.AddListener(() =>
            {
                Screen.fullScreenMode = fullScreenModes[modeIndex];
            });

            itemSelectors.Add(selector);
        }

        index = Array.IndexOf(fullScreenModes, Screen.fullScreenMode);
        if (index < 0) index = 0;
    }

    private void CreateQualityDropdown()
    {
        string[] qualityNames = QualitySettings.names;
        for (int i = 0; i < qualityNames.Length; i++)
        {
            int qualityIndex = i;
            var selector = new ItemSelector { title = qualityNames[i] };
            selector.OnValueChange.AddListener(() =>
            {
                QualitySettings.SetQualityLevel(qualityIndex);
            });

            itemSelectors.Add(selector);
        }

        index = QualitySettings.GetQualityLevel();
    }

    private void CreateShadowsDropdown()
    {
        string[] shadowOptions = { "Disable", "Hard Only", "All" };
        for (int i = 0; i < shadowOptions.Length; i++)
        {
            int shadowIndex = i;
            var selector = new ItemSelector { title = shadowOptions[i] };
            selector.OnValueChange.AddListener(() =>
            {
                QualitySettings.shadows = (ShadowQuality)shadowIndex;
            });

            itemSelectors.Add(selector);
        }

        index = (int)QualitySettings.shadows;
    }

    private void CreateAntiAliasingDropdown()
    {
        string[] aaOptions = { "Disabled", "2x Multi Sampling", "4x Multi Sampling", "8x Multi Sampling" };
        int[] aaValues = { 0, 2, 4, 8 };

        for (int i = 0; i < aaOptions.Length; i++)
        {
            int aaValue = aaValues[i];
            var selector = new ItemSelector { title = aaOptions[i] };
            selector.OnValueChange.AddListener(() =>
            {
                QualitySettings.antiAliasing = aaValue;
            });

            itemSelectors.Add(selector);
        }

        index = Array.IndexOf(aaValues, QualitySettings.antiAliasing);
        if (index < 0) index = 0;
    }

    private void CreateTextureQualityDropdown()
    {
        string[] textureOptions = { "Full Res", "Half Res", "Quarter Res", "Eighth Res" };
        for (int i = 0; i < textureOptions.Length; i++)
        {
            int textureLevel = i;
            var selector = new ItemSelector { title = textureOptions[i] };
            selector.OnValueChange.AddListener(() =>
            {
                QualitySettings.globalTextureMipmapLimit = textureLevel;
            });

            itemSelectors.Add(selector);
        }

        index = QualitySettings.globalTextureMipmapLimit;
    }

    private void CreateVSyncDropdown()
    {
        string[] vsyncOptions = { "Don't Sync", "Every V Blank", "Every Second V Blank" };
        for (int i = 0; i < vsyncOptions.Length; i++)
        {
            int vsyncCount = i;
            var selector = new ItemSelector { title = vsyncOptions[i] };
            selector.OnValueChange.AddListener(() =>
            {
                QualitySettings.vSyncCount = vsyncCount;
            });

            itemSelectors.Add(selector);
        }

        index = QualitySettings.vSyncCount;
    }

    private void CreateAnisotropicFilteringDropdown()
    {
        string[] anisoOptions = { "Disabled", "Enable", "ForceEnable" };
        for (int i = 0; i < anisoOptions.Length; i++)
        {
            int anisoIndex = i;
            var selector = new ItemSelector { title = anisoOptions[i] };
            selector.OnValueChange.AddListener(() =>
            {
                QualitySettings.anisotropicFiltering = (AnisotropicFiltering)anisoIndex;
            });

            itemSelectors.Add(selector);
        }

        index = (int)QualitySettings.anisotropicFiltering;
    }

    private void CreateShadowDistanceDropdown()
    {
        float[] distances = { 50f, 100f, 150f, 200f, 300f, 500f };
        foreach (float distance in distances)
        {
            float currentDistance = distance;
            var selector = new ItemSelector { title = $"{distance}m" };
            selector.OnValueChange.AddListener(() =>
            {
                QualitySettings.shadowDistance = currentDistance;
            });

            itemSelectors.Add(selector);
        }

        // Tìm giá trị gần nhất
        float currentShadowDistance = QualitySettings.shadowDistance;
        index = 0;
        float minDiff = Mathf.Abs(distances[0] - currentShadowDistance);
        for (int i = 1; i < distances.Length; i++)
        {
            float diff = Mathf.Abs(distances[i] - currentShadowDistance);
            if (diff < minDiff)
            {
                minDiff = diff;
                index = i;
            }
        }
    }

    private void CreateShadowResolutionDropdown()
    {
        string[] resOptions = { "Low", "Medium", "High", "Very High" };
        ShadowResolution[] resValues = { ShadowResolution.Low, ShadowResolution.Medium, ShadowResolution.High, ShadowResolution.VeryHigh };

        for (int i = 0; i < resOptions.Length; i++)
        {
            int resIndex = i;
            var selector = new ItemSelector { title = resOptions[i] };
            selector.OnValueChange.AddListener(() =>
            {
                QualitySettings.shadowResolution = resValues[resIndex];
            });

            itemSelectors.Add(selector);
        }

        index = Array.IndexOf(resValues, QualitySettings.shadowResolution);
        if (index < 0) index = 0;
    }

    private void CreateRealtimeReflectionProbesDropdown()
    {
        string[] options = { "Disabled", "Enabled" };
        bool[] values = { false, true };

        for (int i = 0; i < options.Length; i++)
        {
            bool value = values[i];
            var selector = new ItemSelector { title = options[i] };
            selector.OnValueChange.AddListener(() =>
            {
                QualitySettings.realtimeReflectionProbes = value;
            });

            itemSelectors.Add(selector);
        }

        index = QualitySettings.realtimeReflectionProbes ? 1 : 0;
    }

    private void CreateParticleRaycastBudgetDropdown()
    {
        int[] budgets = { 16, 32, 64, 128, 256, 512 };
        foreach (int budget in budgets)
        {
            int currentBudget = budget;
            var selector = new ItemSelector { title = budget.ToString() };
            selector.OnValueChange.AddListener(() =>
            {
                QualitySettings.particleRaycastBudget = currentBudget;
            });

            itemSelectors.Add(selector);
        }

        index = Array.IndexOf(budgets, QualitySettings.particleRaycastBudget);
        if (index < 0) index = 0;
    }

    private void CreateDropdown()
    {
        dropdown.ClearOptions();

        List<string> optionTitles = new();
        foreach (var item in itemSelectors)
        {
            optionTitles.Add(item.title);
        }

        dropdown.AddOptions(optionTitles);
    }

    private void OnDropdownValueChanged(int selectedIndex)
    {
        index = selectedIndex;

        if (index >= 0 && index < itemSelectors.Count)
        {
            itemSelectors[index].OnValueChange?.Invoke();
            if (saveValue)
                PlayerPrefs.SetInt(prefsKey, index);
        }
    }
}
