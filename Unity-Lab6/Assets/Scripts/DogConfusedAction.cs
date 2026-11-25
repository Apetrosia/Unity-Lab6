using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

/// <summary>
/// Действие Unity Behavior (настроение CONFUSED): собака отстала от игрока,
/// останавливается и стоит несколько секунд, после чего граф переводит её в состояние WORRIED.
/// Параметры берутся из <see cref="DogSettings"/> (инспектор).
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

    private GameObject m_Dog;
    private DogMood m_Mood;
    private DogSettings m_Settings;
    private float m_Timer;

    private float StandTime => m_Settings != null ? m_Settings.confusedDuration : Duration.Value;

    protected override Status OnStart()
    {
        if (Agent == null || Agent.Value == null)
        {
            return Status.Failure;
        }

        m_Dog = Agent.Value;
        m_Mood = m_Dog.GetComponent<DogMood>();
        m_Settings = m_Dog.GetComponent<DogSettings>();
        m_Timer = 0f;

        if (m_Mood != null)
        {
            m_Mood.SetMood(DogMoodState.Confused);
        }

        Debug.Log($"{Name}: останавливаюсь на {StandTime:0.#} с и оглядываюсь.", m_Dog);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (m_Dog == null)
        {
            return Status.Failure;
        }

        // Хозяин вошёл в радиус — сразу радуемся, без промежуточных состояний.
        if (DogSenses.IsPlayerClose(m_Dog, Calm))
        {
            Debug.Log($"{Name}: хозяин вернулся! Сразу перехожу в HAPPY.", m_Dog);
            return Status.Failure;
        }

        if (DogSenses.IsThreatened(m_Dog, Sight, Fov, Calm, Close))
        {
            return Status.Failure;
        }

        m_Timer += Time.deltaTime;
        return m_Timer >= StandTime ? Status.Success : Status.Running;
    }

    protected override void OnEnd()
    {
        m_Dog = null;
        m_Mood = null;
        m_Settings = null;
    }

    private string Name => m_Mood != null ? m_Mood.dogName : "Собака";
    private float Sight => m_Settings != null ? m_Settings.sightRange : DogSenses.ThreatRange;
    private float Fov => m_Settings != null ? m_Settings.fieldOfView : DogSenses.FieldOfView;
    private float Calm => m_Settings != null ? m_Settings.calmDistance : DogSenses.CalmDistance;
    private float Close => m_Settings != null ? m_Settings.closeThreatRange : DogSenses.CloseThreatRange;
}
