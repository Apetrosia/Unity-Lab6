using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

/// <summary>
/// Условие Unity Behavior: нашей собаке что-то угрожает — она видит злую собаку,
/// а игрок при этом не рядом (дальше <see cref="CalmDistance"/> метров).
/// </summary>
[System.Serializable, GeneratePropertyBag]
[Condition(
    name: "Dog Threatened",
    description: "Проверяет, что собака видит злую собаку и игрока нет рядом.",
    story: "[Agent] видит злую собаку в радиусе [ThreatRange] м, а игрок дальше [CalmDistance] м",
    category: "Dog",
    id: "b2e5d4c3a6f7589b0d1e2f3a4b5c6d7e")]
public partial class DogThreatenedCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> ThreatRange = new BlackboardVariable<float>(10f);
    [SerializeReference] public BlackboardVariable<float> FieldOfView = new BlackboardVariable<float>(140f);
    [SerializeReference] public BlackboardVariable<float> CalmDistance = new BlackboardVariable<float>(4f);

    public override bool IsTrue()
    {
        GameObject agent = Agent != null && Agent.Value != null ? Agent.Value : GameObject;
        if (agent == null)
        {
            return false;
        }

        return DogSenses.IsThreatened(agent, ThreatRange.Value, FieldOfView.Value, CalmDistance.Value);
    }
}
