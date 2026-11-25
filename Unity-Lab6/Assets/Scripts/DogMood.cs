using UnityEngine;
using UnityEngine.UI;

/// <summary>Настроение собаки.</summary>
public enum DogMoodState
{
    Happy,       // наша собака: игрок рядом — следует за ним
    Confused,    // наша собака: отстала от игрока — остановилась и не понимает, куда он делся
    Worried,     // наша собака: игрок не появился — бегает туда-сюда и ищет
    Depression,  // наша собака: игрок так и не вернулся — стоит и плачет
    Scared,      // наша собака: заметила злую собаку — убегает в случайных направлениях
    Bored,       // злая собака: рядом нет другой собаки — скучает
    Aggressive   // злая собака: другая собака не в состоянии HAPPY — нападает
}

/// <summary>
/// Передаёт настроение собаки через цвет шарика, надпись на канвасе и сообщения в Debug.Log.
/// </summary>
public class DogMood : MonoBehaviour
{
    [Header("Имя в сообщениях")]
    public string dogName = "Собака";

    [Header("Цвета настроения")]
    public Color happyColor = new Color(0.25f, 0.9f, 0.35f);
    public Color confusedColor = new Color(1f, 0.85f, 0.2f);
    public Color worriedColor = new Color(0.95f, 0.25f, 0.2f);
    public Color depressionColor = new Color(0.2f, 0.45f, 1f);
    public Color scaredColor = new Color(0.95f, 0.95f, 0.95f);
    public Color boredColor = new Color(0.6f, 0.6f, 0.6f);
    public Color aggressiveColor = new Color(0.7f, 0.05f, 0.05f);

    [Header("Канвас над собакой")]
    public Text moodLabel;

    [Header("Сообщения")]
    public bool logMessages = true;

    private Renderer m_Renderer;
    private Material m_Material;
    private DogMoodState? m_State;

    /// <summary>Текущее настроение (null, пока настроение не задано).</summary>
    public DogMoodState? State => m_State;

    /// <summary>Задаёт настроение: меняет цвет шарика, надпись и пишет сообщение в консоль.</summary>
    public void SetMood(DogMoodState state)
    {
        if (m_State == state)
        {
            return;
        }

        m_State = state;
        Color color = ColorFor(state);
        ApplyColor(color);

        if (moodLabel != null)
        {
            moodLabel.text = state.ToString().ToUpperInvariant();
            moodLabel.color = color;
        }

        if (logMessages)
        {
            Debug.Log($"{dogName}: {MessageFor(state)}", this);
        }
    }

    /// <summary>Цвет, соответствующий настроению.</summary>
    public Color ColorFor(DogMoodState state)
    {
        switch (state)
        {
            case DogMoodState.Happy: return happyColor;
            case DogMoodState.Confused: return confusedColor;
            case DogMoodState.Worried: return worriedColor;
            case DogMoodState.Depression: return depressionColor;
            case DogMoodState.Scared: return scaredColor;
            case DogMoodState.Bored: return boredColor;
            default: return aggressiveColor;
        }
    }

    private void ApplyColor(Color color)
    {
        if (m_Material == null)
        {
            if (m_Renderer == null)
            {
                m_Renderer = GetComponent<Renderer>();
            }

            if (m_Renderer != null)
            {
                m_Material = m_Renderer.material;
            }
        }

        if (m_Material != null)
        {
            m_Material.color = color;
        }
    }

    private static string MessageFor(DogMoodState state)
    {
        switch (state)
        {
            case DogMoodState.Happy:
                return "настроение HAPPY — игрок рядом, следую за игроком (зелёный шарик).";
            case DogMoodState.Confused:
                return "настроение CONFUSED — отстала от игрока, стою и не понимаю, куда он делся (жёлтый шарик).";
            case DogMoodState.Worried:
                return "настроение WORRIED — игрок не появился, бегаю туда-сюда и ищу (красный шарик).";
            case DogMoodState.Depression:
                return "настроение DEPRESSION — игрок так и не вернулся, стою и плачу (синий шарик).";
            case DogMoodState.Scared:
                return "настроение SCARED — заметила злую собаку, убегаю в случайных направлениях (белый шарик)!";
            case DogMoodState.Bored:
                return "настроение BORED — рядом нет другой собаки, скучаю (серый шарик).";
            default:
                return "настроение AGGRESSIVE — вижу другую собаку без хозяина, нападаю (тёмно-красный шарик)!";
        }
    }
}
