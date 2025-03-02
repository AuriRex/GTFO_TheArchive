using Player;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Localization;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace TheArchive.Features.Hud;

public class Watermark : Feature
{
    public override string Name => "Watermark Tweaks";

    public override FeatureGroup Group => FeatureGroups.Hud;

    [IgnoreLocalization]
    public override string Description => Localization.Format(1, ColorHex, ArchiveMod.MOD_NAME, ArchiveMod.VERSION_STRING);

    public override bool SkipInitialOnEnable => true;

    public const string ColorHex = "FBF3FF";

    public static new ILocalizationService Localization { get; set; }

    public new static IArchiveLogger FeatureLogger { get; set; }

    [FeatureConfig]
    public static WatermarkSettings Settings { get; set; }

    public class WatermarkSettings
    {
        [FSDisplayName("Watermark Mode")]
        [FSDescription($"{nameof(WatermarkMode.Mod)}: Shows currently installed mod version\n{nameof(WatermarkMode.Positional)}: Shows your current position / XYZ coordinates")]
        public WatermarkMode Mode { get; set; } = WatermarkMode.Positional;

        [FSSlider(0f, 1f)]
        [FSDisplayName("XYZ Saturation")]
        [FSDescription("How much color to apply to the XYZ coords.")]
        public float XYZSaturation { get; set; } = 0.5f;

        [FSDisplayName("XYZ Decimals")]
        [FSDescription("Decimal precision to show on the XYZ coords.")]
        public DecimalPrecision Precision { get; set; } = DecimalPrecision.One;

        [Localized]
        public enum DecimalPrecision
        {
            None,
            One,
            Two,
        }

        [Localized]
        public enum WatermarkMode
        {
            Mod,
            Positional,
            Timer,
        }
    }

#if MONO
		private static MethodAccessor<PUI_Watermark> A_PUI_Watermark_UpdateWatermark;
#endif

    public override void Init()
    {
        Setup();
#if MONO
			A_PUI_Watermark_UpdateWatermark = MethodAccessor<PUI_Watermark>.GetAccessor("UpdateWatermark");
#endif
    }


    public override void OnEnable()
    {
        CallUpdateWatermark();
        WatermarkTopLine?.gameObject?.SetActive(true);
    }

    public override void OnDisable()
    {
        CallUpdateWatermark();
        WatermarkTopLine?.gameObject?.SetActive(false);
    }

    private static void CallUpdateWatermark()
    {
#if IL2CPP
        GuiManager.WatermarkLayer?.m_watermark?.UpdateWatermark();
#else
			var watermark = GuiManager.WatermarkLayer?.m_watermark;
			if (watermark != null)
				A_PUI_Watermark_UpdateWatermark.Invoke(watermark);
#endif
    }

    public override void OnFeatureSettingChanged(FeatureSetting setting)
    {
        Setup();
        CallUpdateWatermark();
    }

    public static TMPro.TextMeshPro WatermarkTopLine { get; private set; }

    private static string _format = "0.00";

    private Color _cRed = Color.red;
    private Color _cGreen = Color.green;
    private Color _cBlue = Color.blue;

    private string _cRedString;
    private string _cGreenString;
    private string _cBlueString;

    public void Setup()
    {
        SetupColors(Settings.XYZSaturation);

        switch (Settings.Precision)
        {
            case WatermarkSettings.DecimalPrecision.None:
                _format = "0";
                break;
            case WatermarkSettings.DecimalPrecision.One:
                _format = "0.0";
                break;
            case WatermarkSettings.DecimalPrecision.Two:
                _format = "0.00";
                break;
        }
    }

    public Color SaturateColor(Color color, float saturation)
    {
        Color.RGBToHSV(color, out var H, out _, out var V);
        return Color.HSVToRGB(H, saturation, V);
    }

    public void SetupColors(float saturation)
    {
        _cRedString = SaturateColor(_cRed, saturation).ToHexString();
        _cGreenString = SaturateColor(_cGreen, saturation).ToHexString();
        _cBlueString = SaturateColor(_cBlue, saturation).ToHexString();
    }

    private static readonly eGameStateName _eGameStateName_InLevel = Utils.GetEnumFromName<eGameStateName>(nameof(eGameStateName.InLevel));

    private bool _hideElement = true;

    public override void Update()
    {
        if (WatermarkTopLine == null)
            return;

        _hideElement = true;
        switch (Settings.Mode)
        {
            default:
                return;
            case WatermarkSettings.WatermarkMode.Positional:
                if (PlayerManager.TryGetLocalPlayerAgent(out var localPlayer))
                {
                    Vector3 pos = localPlayer.transform.position;

                    WatermarkTopLine.text = $"<{_cRedString}>X:{pos.x.ToString(_format)}</color> <{_cGreenString}>Y:{pos.y.ToString(_format)}</color> <{_cBlueString}>Z:{pos.z.ToString(_format)}</color>";
                    WatermarkTopLine.Rebuild(CanvasUpdate.PreRender);

                    _hideElement = false;
                }
                break;
            case WatermarkSettings.WatermarkMode.Timer:
                if (((eGameStateName)CurrentGameState) != _eGameStateName_InLevel)
                    break;

                WatermarkTopLine.text = EnhancedExpeditionTimer.TotalElapsedMissionTimeFormatted;
                WatermarkTopLine.Rebuild(CanvasUpdate.PreRender);

                _hideElement = false;
                break;
        }

        if (_hideElement && WatermarkTopLine.text != string.Empty)
        {
            WatermarkTopLine.text = string.Empty;
            WatermarkTopLine.Rebuild(CanvasUpdate.PreRender);
        }
    }

    public const string TOPLINE_GO_NAME = $"{ArchiveMod.MOD_NAME}_WatermarkTopLine";

    [ArchivePatch(typeof(PUI_Watermark), "UpdateWatermark")]
    internal static class PUI_Watermark_UpdateWatermarkPatch
    {
#if IL2CPP
        public static void Postfix(PUI_Watermark __instance)
        {
            var rundownKey = __instance.m_rundownKey;
            var revision = __instance.m_revision;
            var ogText = __instance.m_watermark;
#else
			public static void Postfix(PUI_Watermark __instance, string ___m_rundownKey, int ___m_revision, ref string ___m_watermark, TMPro.TextMeshPro ___m_watermarkText)
			{
				var rundownKey = ___m_rundownKey;
				var revision = ___m_revision;
				var ogText = ___m_watermark;
#endif
            try
            {
                var go = __instance.transform.GetChildWithExactName(TOPLINE_GO_NAME)?.gameObject;

                if (go == null)
                {
                    go = UnityEngine.Object.Instantiate(__instance.m_watermarkText.gameObject);
                    go.name = TOPLINE_GO_NAME;
                    go.transform.parent = __instance.transform;
                    go.transform.position = __instance.m_watermarkText.transform.position + new Vector3(0, 18, 0);

                    WatermarkTopLine = go.GetComponent<TMPro.TextMeshPro>();
                    WatermarkTopLine.text = string.Empty;

                    var rectTrans = go.GetComponent<RectTransform>();
                    rectTrans.sizeDelta = Vector2.zero;
                }

                if(Settings.Mode == WatermarkSettings.WatermarkMode.Mod)
                {
                    WatermarkTopLine.text = $"<#{ColorHex}>{ArchiveMod.MOD_NAME} v{ArchiveMod.VERSION_STRING}</color>";
                    WatermarkTopLine.Rebuild(CanvasUpdate.PreRender);
                }

                if (Is.R6OrLater)
                    return;

                string secondLine = ogText.Split(new string[] { "\n" }, 2, StringSplitOptions.None)[1];

#if IL2CPP
                __instance.m_watermark = secondLine;
                __instance.m_watermarkText.text = secondLine;
#else
					___m_watermark = secondLine;
					___m_watermarkText.text = secondLine;
#endif
            }
            catch (Exception ex)
            {
                FeatureLogger.Error($"Watermark broke! Please fix~ {ex}: {ex.Message}");
                FeatureLogger.Exception(ex);
            }
        }
    }
}