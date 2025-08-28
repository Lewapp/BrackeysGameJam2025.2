using System;
using TMPro;
using UnityEngine;

public class RiskRewards : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textInfo;
    [SerializeField] private RiskRewardData[] effects;
    [SerializeField] private RiskRewardData chosenEffect;

    public void PickEffect()
    {
        if (effects.Length <= 0)
            return;

        int _effectIndex = UnityEngine.Random.Range(0, effects.Length);
        chosenEffect = effects[_effectIndex];
        if (textInfo)
            textInfo.text = effects[_effectIndex].description;
    }

    public void ApplyEffect()
    {
        // Apply Effect
    }




}
