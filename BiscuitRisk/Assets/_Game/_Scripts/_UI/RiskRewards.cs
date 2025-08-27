using System;
using UnityEngine;

public class RiskRewards : MonoBehaviour
{
    public enum Risks
    { 
        None,
        LowHealth,
        EnemyFireRate,
        EnemyDamage,
    }

    public enum Rewards
    {
        None,
        MaxHealthInc,
        HealthRegenInc,
        MoveSpeedInc,
    }

    [SerializeField] private bool isRisk;
    [SerializeField] private Risks risk;
    [SerializeField] private Rewards reward;

    public void SetCard()
    {
        if (isRisk)
        {
            risk = HelperClass.GetRandomEnumValue<Risks>();
            return;
        }

        reward = HelperClass.GetRandomEnumValue<Rewards>();
    }

    public void Activate()
    {
        if (isRisk)
            ApplyRisk(risk);
        else
            ApplyReward(reward);
    }

    private void ApplyRisk(Risks risk)
    {
        switch (risk)
        {
            case Risks.LowHealth:
                Debug.Log("Apply Low Health debuff");
                break;
            default:
                break;
        }
    }

    private void ApplyReward(Rewards reward)
    {
        switch (reward)
        {
            case Rewards.MaxHealthInc:
                Debug.Log("Increase Max Health");
                break;
            default:
                break;
        }
    }
}
