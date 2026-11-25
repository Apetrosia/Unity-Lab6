using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

/// <summary>
/// Действие Unity Behavior (настроение DEPRESSION): собака стоит на месте и плачет,
/// пока игрок не вернётся в её поле зрения. Как только игрок появился, действие
/// завершается неудачей — граф заново запускает корень, и собака снова идёт за игроком.
/// Параметры берутся из <see cref="DogSettings"/> (инспектор).
/// </summary>
[System.Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dog Cry",
    description: "Собака стоит на месте и плачет, пока игрок не появится в поле зрения.",
    story: "[Agent] стоит и плачет, пока [Target] не появится рядом",
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
    private DogMood m_Mood;
    private DogSettings m_Settings;
    private float m_SobTimer;

    private float Sight => m_Settings != null ? m_Settings.sightRange : SightRange.Value;
    private float Fov => m_Settings != null ? m_Settings.fieldOfView : FieldOfView.Value;
    private float Sob => m_Settings != null ? m_Settings.sobInterval : SobInterval.Value;

    protected override Status OnStart()
    {
        if (Agent == null || Agent.Value == null)
        {
            return Status.Failure;
        }

        m_Dog = Agent.Value.transform;
        m_Mood = Agent.Value.GetComponent<DogMood>();
        m_Settings = Agent.Value.GetComponent<DogSettings>();
        m_SobTimer = 0f;

        // Игрок уже вернулся — сразу выходим, чтобы не переключать настроение зря.
        if (DogSenses.CanSee(m_Dog.gameObject, ResolveTarget(), Sight, Fov))
        {
            return Status.Failure;
        }

        if (m_Mood != null)
        {
            m_Mood.SetMood(DogMoodState.Depression);
        }

        Debug.Log($"{Name}: села на месте и плачет — игрок так и не появился.", m_Dog.gameObject);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (m_Dog == null)
        {
            return Status.Failure;
        }

        // Хозяин вошёл в радиус — сразу HAPPY, без промежуточных состояний.
        if (DogSenses.IsPlayerClose(m_Dog.gameObject, Calm))
        {
            Debug.Log($"{Name}: хозяин вернулся! Сразу перехожу в HAPPY.", m_Dog.gameObject);
            return Status.Failure;
        }

        if (DogSenses.IsThreatened(m_Dog.gameObject, Sight, Fov, Calm, Close))
        {
            Debug.Log($"{Name}: кто-то идёт! Вскакиваю.", m_Dog.gameObject);
            return Status.Failure;
        }

        if (DogSenses.CanSee(m_Dog.gameObject, ResolveTarget(), Sight, Fov))
        {
            Debug.Log($"{Name}: игрок вернулся! Вскакиваю и иду за ним.", m_Dog.gameObject);
            return Status.Failure;
        }

        m_SobTimer += Time.deltaTime;
        if (m_SobTimer >= Sob)
        {
            m_SobTimer = 0f;
            Debug.Log($"{Name}: всхлип... *плачет*", m_Dog.gameObject);
        }

        // Оглядывается — так она может снова заметить игрока.
        m_Dog.Rotate(Vector3.up, k_LookAroundDegreesPerSecond * Time.deltaTime, Space.World);

        return Status.Running;
    }

    protected override void OnEnd()
    {
        m_Dog = null;
        m_Mood = null;
        m_Settings = null;
    }

    private GameObject ResolveTarget()
    {
        return Target != null && Target.Value != null ? Target.Value : DogSenses.FindPlayer();
    }

    private string Name => m_Mood != null ? m_Mood.dogName : "Собака";
    private float Calm => m_Settings != null ? m_Settings.calmDistance : DogSenses.CalmDistance;
    private float Close => m_Settings != null ? m_Settings.closeThreatRange : DogSenses.CloseThreatRange;
}
