using UnityEngine;
using TMPro;

/// <summary>Nguồn thời gian duy nhất cho đồng hồ, ánh sáng ngày đêm và skybox.</summary>
public class TimeController : MonoBehaviour
{
    public static TimeController Instance { get; private set; }

    [Header("Thời gian trong game")]
    [SerializeField, Min(1f)] private float dayDurationSeconds = 300f;
    [SerializeField, Range(0f, 24f)] private float startHour = 6f;
    [SerializeField] private TMP_Text timeText;

    [Header("Mốc bình minh và hoàng hôn")]
    [SerializeField, Range(0f, 24f)] private float sunriseHour = 6f;
    [SerializeField, Range(0f, 24f)] private float sunsetHour = 18f;

    [Header("4 Skybox theo thời điểm")]
    [SerializeField] private Material midnightSkybox;
    [SerializeField] private Material dawnSkybox;
    [SerializeField] private Material daySkybox;
    [SerializeField] private Material sunsetSkybox;
    [SerializeField, Range(0f, 24f)] private float dawnSkyStartHour = 5f;
    [SerializeField, Range(0f, 24f)] private float daySkyStartHour = 8f;
    [SerializeField, Range(0f, 24f)] private float sunsetSkyStartHour = 17f;
    [SerializeField, Range(0f, 24f)] private float midnightSkyStartHour = 21f;
    [SerializeField] private Color midnightSkyTint = new Color(.55f, .62f, .85f, 1f);
    [SerializeField] private Color dawnSkyTint = new Color(1f, .72f, .55f, 1f);
    [SerializeField] private Color daySkyTint = Color.white;
    [SerializeField] private Color sunsetSkyTint = new Color(1f, .62f, .42f, 1f);

    [Header("Ánh sáng")]
    [SerializeField] private Light sunLight;
    [SerializeField] private Light moonLight;
    [SerializeField] private DayNightController dayNightController;
    [SerializeField] private Color dayAmbientLight = new Color(.65f, .65f, .65f, 1f);
    [SerializeField] private Color nightAmbientLight = new Color(.08f, .10f, .2f, 1f);
    [SerializeField, Min(0f)] private float maxSunLightIntensity = 1f;
    [SerializeField, Min(0f)] private float maxMoonLightIntensity = .2f;

    public float CurrentHour { get; private set; }
    public float NormalizedDay => CurrentHour / 24f;
    public float SunRotationAngle { get; private set; }

    private Material activeSkyboxAsset;
    private Material activeSkyboxInstance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentHour = Mathf.Repeat(startHour, 24f);
        FindReferences();
        ConnectDayNightController();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (activeSkyboxInstance != null)
        {
            if (RenderSettings.skybox == activeSkyboxInstance)
                RenderSettings.skybox = activeSkyboxAsset;
            Destroy(activeSkyboxInstance);
        }
    }

    private void Start()
    {
        UpdateClockText();
        UpdateDayNight();
        UpdateSkybox();
    }

    private void Update()
    {
        if (dayDurationSeconds > 0f)
            CurrentHour = Mathf.Repeat(CurrentHour + Time.deltaTime * 24f / dayDurationSeconds, 24f);

        UpdateClockText();
        UpdateDayNight();
        UpdateSkybox();
    }

    private void FindReferences()
    {
        if (timeText == null)
        {
            foreach (TMP_Text text in FindObjectsOfType<TMP_Text>(true))
                if (string.Equals(text.gameObject.name, "TimeText", System.StringComparison.OrdinalIgnoreCase))
                {
                    timeText = text;
                    break;
                }
        }

        if (sunLight == null)
        {
            foreach (Light lightSource in FindObjectsOfType<Light>())
                if (lightSource.type == LightType.Directional)
                {
                    sunLight = lightSource;
                    break;
                }
        }
    }

    private void ConnectDayNightController()
    {
        if (dayNightController == null && sunLight != null)
            dayNightController = sunLight.GetComponent<DayNightController>();
        if (dayNightController == null)
            dayNightController = FindObjectOfType<DayNightController>();
        if (dayNightController == null && sunLight != null)
            dayNightController = sunLight.gameObject.AddComponent<DayNightController>();

        if (dayNightController != null)
            dayNightController.SetTimeController(this);
    }

    private void UpdateClockText()
    {
        if (timeText == null) return;
        int totalMinutes = Mathf.FloorToInt(CurrentHour * 60f);
        int hour = (totalMinutes / 60) % 24;
        int minute = totalMinutes % 60;
        timeText.text = hour.ToString("00") + ":" + minute.ToString("00");
    }

    private void UpdateDayNight()
    {
        float dayLength = Mathf.Max(.01f, sunsetHour - sunriseHour);
        float nightLength = Mathf.Max(.01f, 24f - dayLength);
        float daylight;

        if (CurrentHour >= sunriseHour && CurrentHour < sunsetHour)
        {
            float dayProgress = (CurrentHour - sunriseHour) / dayLength;
            daylight = Mathf.Sin(dayProgress * Mathf.PI);
            SunRotationAngle = Mathf.Lerp(0f, 180f, dayProgress);
        }
        else
        {
            float nightProgress = CurrentHour >= sunsetHour
                ? (CurrentHour - sunsetHour) / nightLength
                : (CurrentHour + 24f - sunsetHour) / nightLength;
            daylight = 0f;
            SunRotationAngle = Mathf.Lerp(180f, 360f, nightProgress);
        }

        daylight = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(daylight));
        if (sunLight != null) sunLight.intensity = maxSunLightIntensity * daylight;
        if (moonLight != null) moonLight.intensity = maxMoonLightIntensity * (1f - daylight);
        RenderSettings.ambientLight = Color.Lerp(nightAmbientLight, dayAmbientLight, daylight);
    }

    private void UpdateSkybox()
    {
        Material desiredSkybox;
        if (CurrentHour >= midnightSkyStartHour || CurrentHour < dawnSkyStartHour)
            desiredSkybox = midnightSkybox;
        else if (CurrentHour < daySkyStartHour)
            desiredSkybox = dawnSkybox;
        else if (CurrentHour < sunsetSkyStartHour)
            desiredSkybox = daySkybox;
        else if (CurrentHour < midnightSkyStartHour)
            desiredSkybox = sunsetSkybox;
        else
            desiredSkybox = midnightSkybox;

        if (desiredSkybox != null && desiredSkybox != activeSkyboxAsset)
        {
            if (activeSkyboxInstance != null) Destroy(activeSkyboxInstance);
            activeSkyboxAsset = desiredSkybox;
            activeSkyboxInstance = new Material(desiredSkybox);
            RenderSettings.skybox = activeSkyboxInstance;
            DynamicGI.UpdateEnvironment();
        }

        // Giữ nguyên asset gốc; chỉ đổi tint trên bản runtime nếu shader có hỗ trợ _Tint.
        if (activeSkyboxInstance != null && activeSkyboxInstance.HasProperty("_Tint"))
            activeSkyboxInstance.SetColor("_Tint", EvaluateSkyTint());
    }

    private Color EvaluateSkyTint()
    {
        if (CurrentHour >= midnightSkyStartHour || CurrentHour < dawnSkyStartHour)
        {
            float duration = Mathf.Max(.01f, 24f - midnightSkyStartHour + dawnSkyStartHour);
            float elapsed = CurrentHour >= midnightSkyStartHour
                ? CurrentHour - midnightSkyStartHour
                : CurrentHour + 24f - midnightSkyStartHour;
            return Color.Lerp(midnightSkyTint, dawnSkyTint, Mathf.SmoothStep(0f, 1f, elapsed / duration));
        }
        if (CurrentHour < daySkyStartHour)
        {
            float t = Mathf.InverseLerp(dawnSkyStartHour, daySkyStartHour, CurrentHour);
            return Color.Lerp(dawnSkyTint, daySkyTint, Mathf.SmoothStep(0f, 1f, t));
        }
        if (CurrentHour < sunsetSkyStartHour)
        {
            float t = Mathf.InverseLerp(daySkyStartHour, sunsetSkyStartHour, CurrentHour);
            return Color.Lerp(daySkyTint, sunsetSkyTint, Mathf.SmoothStep(0f, 1f, t));
        }

        float sunsetT = Mathf.InverseLerp(sunsetSkyStartHour, midnightSkyStartHour, CurrentHour);
        return Color.Lerp(sunsetSkyTint, midnightSkyTint, Mathf.SmoothStep(0f, 1f, sunsetT));
    }
}
