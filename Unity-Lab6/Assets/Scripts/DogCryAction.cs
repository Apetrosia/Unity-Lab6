using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

/// <summary>
/// Действие Unity Behavior (настроение DEPRESSION): собака стоит на месте и плачет,
/// пока игрок не вернётся в её поле зрения. Как только игрок появился, действие
/// завершается неудачей — граф заново запускает корень, и собака снова идёт за игроком.
/// </summary>
[System.Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dog Cry",
    description: "Собака стоит на месте и плачет, пока игрок не появится в поле зрения.",
    story: "[Agent] стоит и плачет, пока [Target] не появится в радиусе [SightRange] м",
    category: "Dog",
    id: "c3d6e5f4b7a8690c1d2e3f4a5b6c7d8e")]
public partial class DogCryAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> SightRange = new BlackboardVariable<float>(10f);
    [SerializeReference] public BlackboardVariable<float> FieldOfView = new BlackboardVariable<float>(140f);
    [SerializeReference] public BlackboardVariable<float> SobInterval = new BlackboardVariable<float>(3f);

    // Пока собака плачет, она медленно оглядывается по сторонам.
    private const float k_LookAroundDegreesPerSecond = 60f;

    private Transform m_Dog;
    private float m_SobTimer;

    protected override Status OnStart()
    {
        if (Agent == null || Agent.Value == null)
        {
            return Status.Failure;
        }

        m_Dog = Agent.Value.transform;
        m_SobTimer = 0f;

        // Игрок уже вернулся — сразу выходим, чтобы не переключать настроение зря.
        if (DogSenses.CanSee(m_Dog.gameObject, ResolveTarget(), SightRange.Value, FieldOfView.Value))
        {
            return Status.Failure;
        }

        DogMood mood = Agent.Value.GetComponent<DogMood>();
        if (mood != null)
        {
            mood.SetMood(DogMoodState.Depression);
        }

        Debug.Log("Собака: села на месте и плачет — игрок так и не появился.", m_Dog.gameObject);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (m_Dog == null)
        {
            return Status.Failure;
        }

        if (DogSenses.IsThreatened(m_Dog.gameObject, SightRange.Value, FieldOfView.Value))
        {
            Debug.Log("Собака: кто-то идёт! Вскакиваю.", m_Dog.gameObject);
            return Status.Failure;
        }

        if (DogSenses.CanSee(m_Dog.gameObject, ResolveTarget(), SightRange.Value, FieldOfView.Value))
        {
            Debug.Log("Собака: игрок вернулся! Вскакиваю и иду за ним.", m_Dog.gameObject);
            return Status.Failure;
        }

        m_SobTimer += Time.deltaTime;
        if (m_SobTimer >= SobInterval.Value)
        {
            m_SobTimer = 0f;
            Debug.Log("Собака: всхлип... *плачет*", m_Dog.gameObject);
        }

        // Оглядывается — так она может снова заметить игрока.
        m_Dog.Rotate(Vector3.up, k_LookAroundDegreesPerSecond * Time.deltaTime, Space.World);

        return Status.Running;
    }

    protected override void OnEnd()
    {
        m_Dog = null;
    }

    private GameObject ResolveTarget()
    {
        return Target != null && Target.Value != null ? Target.Value : DogSenses.FindPlayer();
    }
}
