using UnityEngine;

/// <summary>
/// Основные параметры поведения собаки. Компонент вешается на GameObject собаки,
/// поэтому все значения меняются прямо в инспекторе Unity.
/// Границы платформы тоже задаются здесь: маршрут собаки обрезается по этим границам
/// (никаких невидимых стен в сцене нет).
/// </summary>
[DisallowMultipleComponent]
public class DogSettings : MonoBehaviour
{
    [Header("Зрение (м и градусы)")]
    [Tooltip("Радиус, в котором собака замечает игрока и другую собаку.")]
    public float sightRange = 10f;

    [Tooltip("Полный угол поля зрения собаки.")]
    public float fieldOfView = 140f;

    [Tooltip("Ближе этой дистанции собака чует другую собаку, даже если та за спиной.")]
    public float closeThreatRange = 5f;

    [Tooltip("Если игрок ближе этой дистанции, собака успокаивается.")]
    public float calmDistance = 4f;

    [Header("Зрение злой собаки")]
    [Tooltip("Радиус видимости злой собаки — больше, чем у нашей.")]
    public float angrySightRange = 18f;

    public float angryFieldOfView = 160f;

    [Header("Скорости (м/с)")]
    [Tooltip("Скорость следования за игроком и скорость погони злой собаки — одинаковые.")]
    public float followSpeed = 3.5f;

    [Tooltip("Скорость бега туда-сюда в состоянии WORRIED.")]
    public float wanderSpeed = 2.5f;

    [Tooltip("Скорость бега в состоянии SCARED.")]
    public float scaredSpeed = 3.5f;

    [Header("Дистанции (м)")]
    [Tooltip("Собака держит такую дистанцию до игрока.")]
    public float stopDistance = 1.5f;

    [Tooltip("Дистанция, с которой злая собака кусает.")]
    public float attackDistance = 1.4f;

    [Tooltip("Радиус бега туда-сюда в состоянии WORRIED.")]
    public float wanderRadius = 2f;

    [Header("Длительности и интервалы (с)")]
    [Tooltip("Сколько собака стоит в состоянии CONFUSED.")]
    public float confusedDuration = 2f;

    [Tooltip("Сколько собака ищет игрока в состоянии WORRIED.")]
    public float worriedDuration = 5f;

    [Tooltip("Пауза на развороте в состоянии WORRIED (меньше, чем у SCARED).")]
    public float worriedDirectionChangeInterval = 1.5f;

    [Tooltip("Как часто собака меняет направление в SCARED (больше, чем у WORRIED).")]
    public float scaredDirectionChangeInterval = 3f;

    [Tooltip("Интервал всхлипов в состоянии DEPRESSION.")]
    public float sobInterval = 3f;

    [Tooltip("Скорость оглядывания злой собаки в состоянии BORED (град/с).")]
    public float lookAroundSpeed = 25f;

    [Tooltip("Интервал сообщений \"скучно\" у злой собаки.")]
    public float boredLogInterval = 5f;

    [Tooltip("Интервал сообщений об укусе.")]
    public float attackLogInterval = 2f;

    [Header("Границы маршрута (платформа)")]
    [Tooltip("Брать границы из объекта с тегом Ground автоматически.")]
    public bool useGroundBounds = true;

    [Tooltip("Центр платформы, если границы заданы вручную.")]
    public Vector3 platformCenter = Vector3.zero;

    [Tooltip("Размер платформы (X и Z), если границы заданы вручную.")]
    public Vector2 platformSize = new Vector2(100f, 100f);

    [Tooltip("Отступ от края платформы, чтобы собака не свисала с него.")]
    public float edgeMargin = 0.5f;

    private bool m_BoundsInitialized;
    private float m_MinX;
    private float m_MaxX;
    private float m_MinZ;
    private float m_MaxZ;

    /// <summary>Мировая позиция собаки, прижатая к границам платформы (по X и Z).</summary>
    public Vector3 ClampToPlatform(Vector3 position)
    {
        EnsureBounds();
        position.x = Mathf.Clamp(position.x, m_MinX, m_MaxX);
        position.z = Mathf.Clamp(position.z, m_MinZ, m_MaxZ);
        return position;
    }

    /// <summary>Выйдет ли собака за край платформы, если встанет в эту точку.</summary>
    public bool IsOutsidePlatform(Vector3 position)
    {
        EnsureBounds();
        return position.x < m_MinX || position.x > m_MaxX || position.z < m_MinZ || position.z > m_MaxZ;
    }

    /// <summary>Центр платформы в мировых координатах.</summary>
    public Vector3 PlatformCenter
    {
        get
        {
            EnsureBounds();
            return new Vector3((m_MinX + m_MaxX) * 0.5f, 0f, (m_MinZ + m_MaxZ) * 0.5f);
        }
    }

    private void Awake()
    {
        EnsureBounds();
    }

    private void EnsureBounds()
    {
        if (m_BoundsInitialized)
        {
            return;
        }

        m_BoundsInitialized = true;

        Vector3 center = platformCenter;
        Vector2 size = platformSize;

        if (useGroundBounds)
        {
            GameObject ground = null;
            try
            {
                ground = GameObject.FindGameObjectWithTag("Ground");
            }
            catch (UnityException)
            {
                // Тег Ground не задан в проекте — используем ручные границы.
            }

            if (ground != null)
            {
                float radiusX;
                float radiusZ;

                Collider groundCollider = ground.GetComponent<Collider>();
                if (groundCollider != null)
                {
                    Bounds bounds = groundCollider.bounds;
                    center = bounds.center;
                    radiusX = bounds.extents.x;
                    radiusZ = bounds.extents.z;
                }
                else
                {
                    Renderer groundRenderer = ground.GetComponent<Renderer>();
                    Bounds bounds = groundRenderer != null ? groundRenderer.bounds : new Bounds();
                    center = bounds.center;
                    radiusX = bounds.extents.x;
                    radiusZ = bounds.extents.z;
                }

                size = new Vector2(radiusX * 2f, radiusZ * 2f);
            }
        }

        float margin = Mathf.Max(0f, edgeMargin);
        m_MinX = center.x - size.x * 0.5f + margin;
        m_MaxX = center.x + size.x * 0.5f - margin;
        m_MinZ = center.z - size.y * 0.5f + margin;
        m_MaxZ = center.z + size.y * 0.5f - margin;
    }
}
