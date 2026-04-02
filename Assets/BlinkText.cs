using UnityEngine;
using TMPro;
using System.Collections;

public class BlinkTextGroup : MonoBehaviour
{
    public float blinkInterval = 0.8f;
    public float startDelay = 2.0f;

    private TextMeshProUGUI[] texts;
    private bool visible = true;

    void Start()
    {
        texts = GetComponentsInChildren<TextMeshProUGUI>();

        // 처음엔 텍스트 숨김
        foreach (var t in texts)
        {
            t.enabled = false;
        }

        StartCoroutine(StartBlinking());
    }

    IEnumerator StartBlinking()
    {
        //  4.5초 대기
        yield return new WaitForSeconds(startDelay);

        // 등장 (처음엔 보이게)
        foreach (var t in texts)
        {
            t.enabled = true;
        }

        // 이후 깜빡임 시작
        while (true)
        {
            yield return new WaitForSeconds(blinkInterval);

            visible = !visible;

            foreach (var t in texts)
            {
                t.enabled = visible;
            }
        }
    }
}