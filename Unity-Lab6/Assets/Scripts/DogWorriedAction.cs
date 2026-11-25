using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

/// <summary>
/// Действие Unity Behavior (настроение WORRIED): собака бегает туда-сюда в небольшом
/// радиусе и ищет игрока. Если игрок появился раньше — действие завершается неудачей,
/// и граф возвращает собаку в состояние HAPPY. Если нет — по истечении времени граф
/// переводит собаку в состояние DEPRESSION.
/// Маршрут бега лежит внутри границ платформы (см. <see cref="DogSettings"/>).
/// </summary>
[System.Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dog Worried",
    description: "Собака бегает туда-сюда в небольшом радиусе и ищет игрока.",
    story: "[Agent] бегает туда-сюда и ищет [Target] [Duration] с",
    category: "Dog",
    id: "f6a9b8c7e0d19c3f4a5b6c7d8e9f0123")]
public partial class DogWorriedAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Speed = new BlackboardVariable<float>(2.5f);
    [SerializeReference] public BlackboardVariable<float> Radius = new BlackboardVariable<float>(2f);
    [SerializeReference] public BlackboardVariable<float> Duration = new BlackboardVariable<float>(5f);
    [SerializeReference] public BlackboardVariable<float> DirectionChangeInterval = new BlackboardVariable<float>(1.5f);
    [SerializeReference] public BlackboardVariable<float> SightRange = new BlackboardVariable<float>(10f);
    [SerializeReference] public BlackboardVariable<float> FieldOfView = new BlackboardVariable<float>(140f);

    private const float k_ReachDistance = 0.2f;

    private Transform m_Dog;
    private DogMood m_Mood;
    private DogSettings m_Settings;
    private Vector3 m_PointA;
    private Vector3 m_PointB;
    private bool m_GoToPointB = true;
    private float m_Timer;
    private float m_PauseTimer;
    private bool m_IsPausing;

    private float MoveSpeed => m_Settings != null ? m_Settings.wanderSpeed : Speed.Value;
    private float SearchRadius => m_Settings != null ? m_Settings.wanderRadius : Radius.Value;
    private float SearchTime => m_Settings != null ? m_Settings.worriedDuration : Duration.Value;
    private float TurnDelay => m_Settings != null
        ? m_Settings.worriedDirectionChangeInterval
        : DirectionChangeInterval.Value;
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

        // Бегаем "туда-сюда" поперёк направления взгляда собаки, вокруг текущего места.
        Vector3 home = m_Dog.position;
        Vector3 axis = Vector3.Cross(Vector3.up, m_Dog.forward);
        if (axis.sqrMagnitude < 0.001f)
        {
            axis = m_Dog.right;
        }

        axis.Normalize();
        m_PointA = Clamp(home - axis * SearchRadius);
        m_PointB = Clamp(home + axis * SearchRadius);
        m_GoToPointB = true;
        m_Timer = 0f;
        m_PauseTimer = 0f;
        m_IsPausing = false;

        if (m_Mood != null)
        {
            m_Mood.SetMood(DogMoodState.Worried);
        }

        Debug.Log($"{Name}: ищу игрока — бегаю туда-сюда {SearchTime:0.#} с.", m_Dog.gameObject);
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
            return Status.Failure;
        }

        GameObject target = Target != null && Target.Value != null ? Target.Value : DogSenses.FindPlayer();
        if (DogSenses.CanSee(m_Dog.gameObject, target, Sight, Fov))
        {
            Debug.Log($"{Name}: нашла игрока!", m_Dog.gameObject);
            return Status.Failure;
        }

        m_Timer += Time.deltaTime;
        if (m_Timer >= SearchTime)
        {
            return Status.Success;
        }

        Patrol();
        return Status.Running;
    }

    protected override void OnEnd()
    {
        m_Dog = null;
        m_Mood = null;
        m_Settings = null;
    }

    private void Patrol()
    {
        // На развороте собака замирает на TurnDelay секунд.
        if (m_IsPausing)
        {
            m_PauseTimer += Time.deltaTime;
            if (m_PauseTimer < TurnDelay)
            {
                return;
            }

            m_IsPausing = false;
            m_PauseTimer = 0f;
            m_GoToPointB = !m_GoToPointB;
            return;
        }

        Vector3 destination = m_GoToPointB ? m_PointB : m_PointA;
        Vector3 toDestination = destination - m_Dog.position;
        toDestination.y = 0f;

        if (toDestination.magnitude <= k_ReachDistance)
        {
            m_IsPausing = true;
            m_PauseTimer = 0f;
            return;
        }

        Vector3 direction = toDestination.normalized;
        m_Dog.position = Clamp(m_Dog.position + direction * (MoveSpeed * Time.deltaTime));
        m_Dog.rotation = Quaternion.Slerp(m_Dog.rotation, Quaternion.LookRotation(direction),
            8f * Time.deltaTime);
    }

    /// <summary>Маршрут собаки никогда не выходит за границы платформы.</summary>
    private Vector3 Clamp(Vector3 position)
    {
        return m_Settings != null ? m_Settings.ClampToPlatform(position) : position;
    }

    private string Name => m_Mood != null ? m_Mood.dogName : "Собака";
    private float Calm => m_Settings != null ? m_Settings.calmDistance : DogSenses.CalmDistance;
    private float Close => m_Settings != null ? m_Settings.closeThreatRange : DogSenses.CloseThreatRange;
}
