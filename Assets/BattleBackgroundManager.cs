using UnityEngine;
using UnityEngine.UI;

public class BattleBackgroundManager : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image backgroundBottom;

    public void Apply()
    {
        var battleInfo = SceneInfo.GetBattleInfo();

        if (battleInfo == null) return;

        if (!battleInfo.IsTrainerBattle)
        {
            Debug.Log("[BattleBackground] Trainer battle 아님. 기본 배경 유지");
            return;
        }

        if (battleInfo.BattleBackground != null)
            background.sprite = battleInfo.BattleBackground;

        if (battleInfo.BattleBackgroundBottom != null)
            backgroundBottom.sprite = battleInfo.BattleBackgroundBottom;
    }
}