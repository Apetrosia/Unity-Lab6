using UnityEngine;

/// <summary>
/// Разворачивает канвас так, чтобы он всегда смотрел в камеру.
/// </summary>
public class CanvasBillboard : MonoBehaviour
{
    private Camera m_Camera;

    private void LateUpdate()
    {
        if (m_Camera == null)
        {
            m_Camera = Camera.main;
            if (m_Camera == null)
            {
                return;
            }
        }

        transform.rotation = m_Camera.transform.rotation;
    }
}
