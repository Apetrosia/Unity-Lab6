using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

/// <summary>
/// Действие Unity Behavior (настроение HAPPY): собака следует за игроком.
/// Как только игрок пропадает из поля зрения, действие завершается неудачей —
/// граф переводит собаку в состояние CONFUSED.
/// </summary>
[System.Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dog Follow Player",
    description: "Собака идёт за игроком. Завершается неудачей, когда игрок пропал из поля зрения.",
    story: "[Agent] следует за [Target] со скоростью [Speed] м/с и держит дистанцию [StopDistance] м",
    category: "Dog",
    id: "d4e7f6a5c8b97a1d2e3f4a5b6c7d8e9f")]
public partial class DogFollowPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Speed = new BlackboardVariable<float>(3.5f);
    [SerializeReference] public BlackboardVariable<float> StopDistance = new BlackboardVariable<float>(1.5f);
    [SerializeReference] public BlackboardVariable<float> SightRange = new BlackboardVariable<float>(10f);
    [SerializeReference] public BlackboardVariable<float> FieldOfView = new BlackboardVariable<float>(140f);

    private Transform m_Dog;
    private DogMood m_Mood;

    protected override Status OnStart()
    {
        if (Agent == null || Agent.Value == null)
        {
            return Status.Failure;
        }

        m_Dog = Agent.Value.transform;
        m_Mood = Agent.Value.GetComponent<DogMood>();

        if (m_Mood != null)
        {
            m_Mood.SetMood(DogMoodState.Happy);
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (m_Dog == null)
        {
            return Status.Failure;
        }

        // Заметила злую собаку — бросает игрока и убегает (граф переключит её в SCARED).
        if (DogSenses.IsThreatened(m_Dog.gameObject, SightRange.Value, FieldOfView.Value))
        {
            return Status.Failure;
        }

        GameObject target = Target != null && Target.Value != null ? Target.Value : DogSenses.FindPlayer();
        if (!DogSenses.CanSee(m_Dog.gameObject, target, SightRange.Value, FieldOfView.Value))
        {
            // Игрок убежал — собака отстала.
            return Status.Failure;
        }

        Vector3 toTarget = target.transform.position - m_Dog.position;
        toTarget.y = 0f;

        Vector3 lookDirection = toTarget.sqrMagnitude > 0.0001f ? toTarget : m_Dog.forward;
        m_Dog.rotation = Quaternion.Slerp(m_Dog.rotation, Quaternion.LookRotation(lookDirection),
            10f * Time.deltaTime);

        if (toTarget.magnitude > StopDistance.Value)
        {
            m_Dog.position += toTarget.normalized * (Speed.Value * Time.deltaTime);
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        m_Dog = null;
        m_Mood = null;
    }
}
