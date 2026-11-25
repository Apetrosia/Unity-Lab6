using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

/// <summary>
/// Действие Unity Behavior (настроение HAPPY): собака следует за игроком.
/// Как только игрок пропадает из поля зрения, действие завершается неудачей —
/// граф переводит собаку в состояние CONFUSED.
/// Параметры берутся из компонента <see cref="DogSettings"/> на собаке (инспектор),
/// значения из графа используются, только если компонента нет.
/// </summary>
[System.Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dog Follow Player",
    description: "Собака идёт за игроком. Завершается неудачей, когда игрок пропал из поля зрения.",
    story: "[Agent] следует за [Target] и держит дистанцию [StopDistance] м",
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
    private DogSettings m_Settings;

    private float MoveSpeed => m_Settings != null ? m_Settings.followSpeed : Speed.Value;
    private float Distance => m_Settings != null ? m_Settings.stopDistance : StopDistance.Value;
    private float Sight => m_Settings != null ? m_Settings.sightRange : SightRange.Value;
    private float Fov => m_Settings != null ? m_Settings.fieldOfView : FieldOfView.Value;

    protected override Status OnStart()
    {
        if (Agent == null || Agent.Value == null)
        {
            return Status.Failure;
        }

        m_Dog = Agent.Value.transform;
        m_Mood = Agent.Value.GetComponent<DogMood>();
        m_Settings = Agent.Value.GetComponent<DogSettings>();

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

        // Если хозяин в радиусе, угроза уже не считается (см. DogSenses.IsThreatened):
        // собака остаётся HAPPY и просто идёт к хозяину.
        if (DogSenses.IsThreatened(m_Dog.gameObject, Sight, Fov,
                m_Settings != null ? m_Settings.calmDistance : DogSenses.CalmDistance,
                m_Settings != null ? m_Settings.closeThreatRange : DogSenses.CloseThreatRange))
        {
            return Status.Failure;
        }

        GameObject target = Target != null && Target.Value != null ? Target.Value : DogSenses.FindPlayer();
        if (!DogSenses.CanSee(m_Dog.gameObject, target, Sight, Fov))
        {
            // Игрок убежал — собака отстала.
            return Status.Failure;
        }

        return Follow(target);
    }

    /// <summary>Идёт к хозяину и держит дистанцию, не выходя за край платформы.</summary>
    private Status Follow(GameObject target)
    {
        Vector3 toTarget = target.transform.position - m_Dog.position;
        toTarget.y = 0f;

        Vector3 lookDirection = toTarget.sqrMagnitude > 0.0001f ? toTarget : m_Dog.forward;
        m_Dog.rotation = Quaternion.Slerp(m_Dog.rotation, Quaternion.LookRotation(lookDirection),
            10f * Time.deltaTime);

        if (toTarget.magnitude > Distance)
        {
            Vector3 nextPosition = m_Dog.position + toTarget.normalized * (MoveSpeed * Time.deltaTime);
            m_Dog.position = m_Settings != null ? m_Settings.ClampToPlatform(nextPosition) : nextPosition;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        m_Dog = null;
        m_Mood = null;
        m_Settings = null;
    }
}
