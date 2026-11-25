using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

/// <summary>
/// Условие Unity Behavior: нашей собаке что-то угрожает — она видит злую собаку,
/// а игрок при этом не рядом. Параметры берутся из <see cref="DogSettings"/> (инспектор).
/// </summary>
[System.Serializable, GeneratePropertyBag]
[Condition(
    name: "Dog Threatened",
    description: "Проверяет, что собака видит злую собаку и игрока нет рядом.",
    story: "[Agent] видит злую собаку, а игрок дальше [CalmDistance] м",
    category: "Dog",
    id: "b2e5d4c3a6f7589b0d1e2f3a4b5c6d7e")]
public partial class DogThreatenedCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> ThreatRange = new BlackboardVariable<float>(10f);
    [SerializeReference] public BlackboardVariable<float> FieldOfView = new BlackboardVariable<float>(140f);
    [SerializeReference] public BlackboardVariable<float> CalmDistance = new BlackboardVariable<float>(4f);

    private DogSettings m_Settings;
    private GameObject m_SettingsOwner;

    public override bool IsTrue()
    {
        GameObject agent = Agent != null && Agent.Value != null ? Agent.Value : GameObject;
        if (agent == null)
        {
            return false;
        }

        if (m_SettingsOwner != agent)
        {
            m_SettingsOwner = agent;
            m_Settings = agent.GetComponent<DogSettings>();
        }

        return DogSenses.IsThreatened(agent,
            m_Settings != null ? m_Settings.sightRange : ThreatRange.Value,
            m_Settings != null ? m_Settings.fieldOfView : FieldOfView.Value,
            m_Settings != null ? m_Settings.calmDistance : CalmDistance.Value,
            m_Settings != null ? m_Settings.closeThreatRange : DogSenses.CloseThreatRange);
    }
}
