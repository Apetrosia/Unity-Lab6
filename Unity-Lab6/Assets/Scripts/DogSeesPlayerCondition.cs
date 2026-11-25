using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

/// <summary>
/// Условие Unity Behavior: видит ли собака игрока (радиус обзора + поле зрения).
/// Параметры берутся из <see cref="DogSettings"/> (инспектор).
/// </summary>
[System.Serializable, GeneratePropertyBag]
[Condition(
    name: "Dog Sees Player",
    description: "Проверяет, находится ли игрок в поле зрения собаки.",
    story: "[Agent] видит [Target] в радиусе [SightRange] м и поле зрения [FieldOfView]°",
    category: "Dog",
    id: "a1f4c3d2b5e6478a9c0d1e2f3a4b5c6d")]
public partial class DogSeesPlayerCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> SightRange = new BlackboardVariable<float>(10f);
    [SerializeReference] public BlackboardVariable<float> FieldOfView = new BlackboardVariable<float>(140f);

    private DogSettings m_Settings;
    private GameObject m_SettingsOwner;

    public override bool IsTrue()
    {
        GameObject agent = Agent != null && Agent.Value != null ? Agent.Value : GameObject;
        GameObject target = Target != null && Target.Value != null ? Target.Value : DogSenses.FindPlayer();

        if (m_SettingsOwner != agent)
        {
            m_SettingsOwner = agent;
            m_Settings = agent != null ? agent.GetComponent<DogSettings>() : null;
        }

        return DogSenses.CanSee(agent, target,
            m_Settings != null ? m_Settings.sightRange : SightRange.Value,
            m_Settings != null ? m_Settings.fieldOfView : FieldOfView.Value);
    }
}
