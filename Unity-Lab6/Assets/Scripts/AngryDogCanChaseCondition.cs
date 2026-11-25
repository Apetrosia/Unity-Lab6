using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

/// <summary>
/// Условие Unity Behavior: злая собака может нападать — другая собака в её радиусе
/// обзора (он больше, чем у нашей собаки) и эта собака НЕ в состоянии HAPPY.
/// </summary>
[System.Serializable, GeneratePropertyBag]
[Condition(
    name: "Angry Dog Can Chase",
    description: "Проверяет, что другая собака в радиусе видимости и не в настроении HAPPY.",
    story: "[Agent] видит другую собаку в радиусе [SightRange] м, и та не в состоянии HAPPY",
    category: "Angry Dog",
    id: "c4a7e6f5d2b8093c1d2e3f4a5b6c7d8e")]
public partial class AngryDogCanChaseCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> SightRange = new BlackboardVariable<float>(18f);
    [SerializeReference] public BlackboardVariable<float> FieldOfView = new BlackboardVariable<float>(160f);

    public override bool IsTrue()
    {
        GameObject agent = Agent != null && Agent.Value != null ? Agent.Value : GameObject;
        if (agent == null)
        {
            return false;
        }

        DogMood otherDog = DogSenses.FindOtherDog(agent);
        if (otherDog == null)
        {
            return false;
        }

        // Хозяин рядом с другой собакой (она HAPPY) — злая собака не нападает.
        if (otherDog.State == DogMoodState.Happy)
        {
            return false;
        }

        return DogSenses.CanSee(agent, otherDog.gameObject, SightRange.Value, FieldOfView.Value);
    }
}
