using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

/// <summary>
/// Действие Unity Behavior (настроение AGGRESSIVE): злая собака бежит к другой собаке
/// и нападает на неё. Скорость такая же, как у нашей собаки. Погоня прекращается,
/// когда другая собака становится HAPPY (хозяин подошёл) или уходит далеко.
/// Параметры берутся из <see cref="DogSettings"/> (инспектор); маршрут не выходит
/// за границы платформы.
/// </summary>
[System.Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Angry Dog Chase",
    description: "Злая собака бежит к другой собаке и нападает. Останавливается, если та стала HAPPY.",
    story: "[Agent] бежит к другой собаке и нападает",
    category: "Angry Dog",
    id: "b8c5d6e7f8091a2b3c4d5e6f70819203")]
public partial class AngryDogChaseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> Speed = new BlackboardVariable<float>(3.5f);
    [SerializeReference] public BlackboardVariable<float> SightRange = new BlackboardVariable<float>(18f);
    [SerializeReference] public BlackboardVariable<float> FieldOfView = new BlackboardVariable<float>(160f);
    [SerializeReference] public BlackboardVariable<float> AttackDistance = new BlackboardVariable<float>(1.4f);
    [SerializeReference] public BlackboardVariable<float> AttackLogInterval = new BlackboardVariable<float>(2f);

    private Transform m_Dog;
    private DogMood m_Mood;
    private DogSettings m_Settings;
    private float m_AttackLogTimer;

    private float MoveSpeed => m_Settings != null ? m_Settings.followSpeed : Speed.Value;
    private float Sight => m_Settings != null ? m_Settings.angrySightRange : SightRange.Value;
    private float Fov => m_Settings != null ? m_Settings.angryFieldOfView : FieldOfView.Value;
    private float AttackRange => m_Settings != null ? m_Settings.attackDistance : AttackDistance.Value;
    private float AttackLogRate => m_Settings != null ? m_Settings.attackLogInterval : AttackLogInterval.Value;

    protected override Status OnStart()
    {
        if (Agent == null || Agent.Value == null)
        {
            return Status.Failure;
        }

        m_Dog = Agent.Value.transform;
        m_Mood = Agent.Value.GetComponent<DogMood>();
        m_Settings = Agent.Value.GetComponent<DogSettings>();
        m_AttackLogTimer = float.MaxValue;

        if (m_Mood != null)
        {
            m_Mood.SetMood(DogMoodState.Aggressive);
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (m_Dog == null)
        {
            return Status.Failure;
        }

        DogMood otherDog = DogSenses.FindOtherDog(m_Dog.gameObject);

        // Хозяин подошёл к нашей собаке — она HAPPY, злая собака перестаёт бежать.
        if (otherDog != null && otherDog.State == DogMoodState.Happy)
        {
            Debug.Log($"{Name}: у другой собаки появился хозяин — прекращаю погоню.", m_Dog.gameObject);
            return Status.Failure;
        }

        if (!DogSenses.CanSee(m_Dog.gameObject, otherDog != null ? otherDog.gameObject : null, Sight, Fov))
        {
            Debug.Log($"{Name}: потеряла другую собаку из виду.", m_Dog.gameObject);
            return Status.Failure;
        }

        Vector3 toTarget = otherDog.transform.position - m_Dog.position;
        toTarget.y = 0f;

        // Всегда доворачивается на цель, иначе потеряет её из поля зрения.
        Vector3 direction = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : m_Dog.forward;
        m_Dog.rotation = Quaternion.Slerp(m_Dog.rotation, Quaternion.LookRotation(direction),
            10f * Time.deltaTime);

        if (toTarget.magnitude <= AttackRange)
        {
            m_AttackLogTimer += Time.deltaTime;
            if (m_AttackLogTimer >= AttackLogRate)
            {
                m_AttackLogTimer = 0f;
                Debug.Log($"{Name}: догнала и кусает другую собаку!", m_Dog.gameObject);
            }

            return Status.Running;
        }

        m_AttackLogTimer = 0f;
        Vector3 nextPosition = m_Dog.position + direction * (MoveSpeed * Time.deltaTime);
        m_Dog.position = m_Settings != null ? m_Settings.ClampToPlatform(nextPosition) : nextPosition;

        return Status.Running;
    }

    protected override void OnEnd()
    {
        m_Dog = null;
        m_Mood = null;
        m_Settings = null;
    }

    private string Name => m_Mood != null ? m_Mood.dogName : "Злая собака";
}
