using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

/// <summary>
/// Действие Unity Behavior (настроение CONFUSED): собака отстала от игрока,
/// останавливается и стоит <see cref="Duration"/> секунд, после чего граф
/// переводит её в состояние WORRIED.
/// </summary>
[System.Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dog Confused",
    description: "Собака останавливается на несколько секунд и не понимает, куда делся игрок.",
    story: "[Agent] стоит на месте [Duration] с и оглядывается",
    category: "Dog",
    id: "e5f8a7b6d9c08b2e3f4a5b6c7d8e9f01")]
public partial class DogConfusedAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> Duration = new BlackboardVariable<float>(2f);

    private DogMood m_Mood;
    private float m_Timer;

    protected override Status OnStart()
    {
        if (Agent == null || Agent.Value == null)
        {
            return Status.Failure;
        }

        m_Mood = Agent.Value.GetComponent<DogMood>();
        m_Timer = 0f;

        if (m_Mood != null)
        {
            m_Mood.SetMood(DogMoodState.Confused);
        }

        Debug.Log($"Собака: останавливаюсь на {Duration.Value:0.#} с и оглядываюсь.", Agent.Value);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (DogSenses.IsThreatened(Agent.Value))
        {
            return Status.Failure;
        }

        m_Timer += Time.deltaTime;
        return m_Timer >= Duration.Value ? Status.Success : Status.Running;
    }

    protected override void OnEnd()
    {
        m_Mood = null;
    }
}
