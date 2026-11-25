using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

/// <summary>
/// Действие Unity Behavior (настроение BORED): рядом нет собаки, за которой можно
/// увязаться (или у неё рядом хозяин), поэтому злая собака скучает на месте.
/// Завершается неудачей, как только появляется цель для нападения.
/// Параметры берутся из <see cref="DogSettings"/> (инспектор).
/// </summary>
[System.Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Angry Dog Bored",
    description: "Злая собака скучает на месте, пока не появится собака без хозяина.",
    story: "[Agent] скучает на месте, пока не увидит другую собаку",
    category: "Angry Dog",
    id: "c9d6e7f8091a2b3c4d5e6f7081920314")]
public partial class AngryDogBoredAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> SightRange = new BlackboardVariable<float>(18f);
    [SerializeReference] public BlackboardVariable<float> FieldOfView = new BlackboardVariable<float>(160f);
    [SerializeReference] public BlackboardVariable<float> LookAroundSpeed = new BlackboardVariable<float>(25f);
    [SerializeReference] public BlackboardVariable<float> BoredLogInterval = new BlackboardVariable<float>(5f);

    private Transform m_Dog;
    private DogMood m_Mood;
    private DogSettings m_Settings;
    private float m_BoredLogTimer;

    private float Sight => m_Settings != null ? m_Settings.angrySightRange : SightRange.Value;
    private float Fov => m_Settings != null ? m_Settings.angryFieldOfView : FieldOfView.Value;
    private float LookSpeed => m_Settings != null ? m_Settings.lookAroundSpeed : LookAroundSpeed.Value;
    private float LogRate => m_Settings != null ? m_Settings.boredLogInterval : BoredLogInterval.Value;

    protected override Status OnStart()
    {
        if (Agent == null || Agent.Value == null)
        {
            return Status.Failure;
        }

        m_Dog = Agent.Value.transform;
        m_Mood = Agent.Value.GetComponent<DogMood>();
        m_Settings = Agent.Value.GetComponent<DogSettings>();
        m_BoredLogTimer = 0f;

        if (m_Mood != null)
        {
            m_Mood.SetMood(DogMoodState.Bored);
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
        bool canChase = otherDog != null && otherDog.State != DogMoodState.Happy &&
                        DogSenses.CanSee(m_Dog.gameObject, otherDog.gameObject, Sight, Fov);

        if (canChase)
        {
            return Status.Failure;
        }

        // Скучает: лениво оглядывается по сторонам.
        m_Dog.Rotate(Vector3.up, LookSpeed * Time.deltaTime, Space.World);

        m_BoredLogTimer += Time.deltaTime;
        if (m_BoredLogTimer >= LogRate)
        {
            m_BoredLogTimer = 0f;
            Debug.Log($"{Name}: скучно... где все собаки?", m_Dog.gameObject);
        }

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
