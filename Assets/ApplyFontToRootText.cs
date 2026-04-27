using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ApplyFontToRootText : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset customTMPFont;
    [SerializeField] private Font customUIFont;

    void Start()
    {
        StartCoroutine(ApplyFontDelayed());
    }

    IEnumerator ApplyFontDelayed()
    {
        yield return null;
        yield return null;

        // TextMeshPro UI
        TextMeshProUGUI[] tmpUIs = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var text in tmpUIs)
        {
            if (customTMPFont != null)
                text.font = customTMPFont;
        }

        // TextMeshPro 3D
        TextMeshPro[] tmps = GetComponentsInChildren<TextMeshPro>(true);
        foreach (var text in tmps)
        {
            if (customTMPFont != null)
                text.font = customTMPFont;
        }

        // Unity ±âº» UI Text
        Text[] uiTexts = GetComponentsInChildren<Text>(true);
        foreach (var text in uiTexts)
        {
            if (customUIFont != null)
                text.font = customUIFont;
        }
    }
}