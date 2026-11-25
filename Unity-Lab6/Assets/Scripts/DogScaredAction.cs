using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

/// <summary>
/// Действие Unity Behavior (настроение SCARED): собака заметила злую собаку и убегает
/// в случайных направлениях, меняя направление не чаще раза в
/// <see cref="DirectionChangeInterval"/> секунд (дольше, чем в состоянии WORRIED).
/// Завершается неудачей, когда угроза пропала: злая собака вне поля зрения или игрок рядом.
/// </summary>
[System.Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dog Scared",
    description: "Собака убегает от злой собаки в случайных направлениях. Завершается, когда угроза пропала.",
    story: "[Agent] убегает от злой собаки со скоростью [Speed] м/с, меняя направление каждые [DirectionChangeInterval] с",
    category: "Dog",
    id: "a7b4c5d6e7f8091a2b3c4d5e6f708192")]
public partial class DogScaredAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> Speed = new BlackboardVariable<float>(3.5f);
    [SerializeReference] public BlackboardVariable<float> DirectionChangeInterval = new BlackboardVariable<float>(3f);
    [SerializeReference] public BlackboardVariable<float> SightRange = new BlackboardVariable<float>(10f);
    [SerializeReference] public BlackboardVariable<float> FieldOfView = new BlackboardVariable<float>(140f);
    [SerializeReference] public BlackboardVariable<float> CalmDistance = new BlackboardVariable<float>(4f);

    // Разброс случайного направления относительно направления "прочь от злой собаки".
    private const float k_MaxDeviationDegrees = 80f;

    private Transform m_Dog;
    private DogMood m_Mood;
    private Vector3 m_Direction;
    private float m_DirectionTimer;
    private float m_RunTime;

    protected override Status OnStart()
    {
        if (Agent == null || Agent.Value == null)
        {
            return Status.Failure;
        }

        m_Dog = Agent.Value.transform;
        m_Mood = Agent.Value.GetComponent<DogMood>();
        m_RunTime = 0f;
        PickDirection();

        if (m_Mood != null)
        {
            m_Mood.SetMood(DogMoodState.Scared);
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (m_Dog == null)
        {
            return Status.Failure;
        }

        if (!IsThreatened())
        {
            Debug.Log($"{m_Mood?.dogName ?? "Собака"}: угроза пропала, перестаю убегать.", m_Dog.gameObject);
            return Status.Failure;
        }

        m_RunTime += Time.deltaTime;
        m_DirectionTimer += Time.deltaTime;

        // Направление меняется не чаще, чем раз в DirectionChangeInterval секунд.
        if (m_DirectionTimer >= DirectionChangeInterval.Value)
        {
            PickDirection();
        }

        m_Dog.position += m_Direction * (Speed.Value * Time.deltaTime);
        m_Dog.rotation = Quaternion.Slerp(m_Dog.rotation, Quaternion.LookRotation(m_Direction),
            10f * Time.deltaTime);

        return Status.Running;
    }

    protected override void OnEnd()
    {
        m_Dog = null;
        m_Mood = null;
    }

    private bool IsThreatened()
    {
        if (DogSenses.IsPlayerClose(m_Dog.gameObject, CalmDistance.Value))
        {
            return false;
        }

        // Раз собака уже увидела злую, она не теряет её из виду каждый раз, когда убегает
        // спиной — угроза держится, пока злая собака ближе SightRange метров.
        DogMood otherDog = DogSenses.FindOtherDog(m_Dog.gameObject);
        if (otherDog == null)
        {
            return false;
        }

        Vector3 toOtherDog = otherDog.transform.position - m_Dog.position;
        toOtherDog.y = 0f;
        return toOtherDog.sqrMagnitude <= SightRange.Value * SightRange.Value;
    }

    private void PickDirection()
    {
        m_DirectionTimer = 0f;

        DogMood otherDog = DogSenses.FindOtherDog(m_Dog.gameObject);
        Vector3 away = otherDog != null ? m_Dog.position - otherDog.transform.position : -m_Dog.forward;
        away.y = 0f;

        if (away.sqrMagnitude < 0.001f)
        {
            away = -m_Dog.forward;
        }

        // Случайное направление, но с уклоном прочь от злой собаки.
        Vector3 direction = Quaternion.Euler(0f, Random.Range(-k_MaxDeviationDegrees, k_MaxDeviationDegrees), 0f)
                            * away.normalized;
        direction.y = 0f;
        m_Direction = direction.normalized;

        if (m_RunTime > 0f)
        {
            Debug.Log($"{m_Mood?.dogName ?? "Собака"}: меняю направление бега через {DirectionChangeInterval.Value:0.#} с.", m_Dog.gameObject);
        }
    }
}
