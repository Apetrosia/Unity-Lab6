using UnityEngine;

/// <summary>
/// Зрение собаки: радиус обзора и угол поля зрения.
/// </summary>
public static class DogSenses
{
    public const string PlayerTag = "Player";

    /// <summary>Радиус, в котором наша собака замечает злую собаку.</summary>
    public const float ThreatRange = 10f;

    /// <summary>Поле зрения собаки в градусах.</summary>
    public const float FieldOfView = 140f;

    /// <summary>Дистанция до игрока, при которой собака успокаивается.</summary>
    public const float CalmDistance = 4f;

    /// <summary>Вблизи собака чует другую собаку, даже если та не в поле зрения.</summary>
    public const float CloseThreatRange = 5f;

    private static GameObject s_CachedPlayer;

    /// <summary>Находит игрока: по тегу, а если тег не задан — по компоненту PlayerController.</summary>
    public static GameObject FindPlayer()
    {
        if (s_CachedPlayer != null)
        {
            return s_CachedPlayer;
        }

        s_CachedPlayer = GameObject.FindGameObjectWithTag(PlayerTag);
        if (s_CachedPlayer == null)
        {
            PlayerController playerController = Object.FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                s_CachedPlayer = playerController.gameObject;
            }
        }

        return s_CachedPlayer;
    }

    /// <summary>Ближайшая другая собака (не <paramref name="self"/>).</summary>
    public static DogMood FindOtherDog(GameObject self)
    {
        DogMood[] dogs = Object.FindObjectsByType<DogMood>(FindObjectsSortMode.None);
        DogMood nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (DogMood dog in dogs)
        {
            if (dog == null || dog.gameObject == self)
            {
                continue;
            }

            float distance = (dog.transform.position - self.transform.position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = dog;
            }
        }

        return nearest;
    }

    /// <summary>Находится ли собака в настроении HAPPY (рядом с ней игрок).</summary>
    public static bool IsHappy(GameObject dog)
    {
        if (dog == null)
        {
            return false;
        }

        DogMood mood = dog.GetComponent<DogMood>();
        return mood != null && mood.State == DogMoodState.Happy;
    }

    /// <summary>Ближе ли игрок, чем <paramref name="distance"/> метров.</summary>
    public static bool IsPlayerClose(GameObject dog, float distance)
    {
        GameObject player = FindPlayer();
        if (dog == null || player == null)
        {
            return false;
        }

        Vector3 toPlayer = player.transform.position - dog.transform.position;
        toPlayer.y = 0f;
        return toPlayer.sqrMagnitude <= distance * distance;
    }

    /// <summary>
    /// Угрожает ли собаке другая собака: она в поле зрения, а игрока (хозяина) рядом нет.
    /// </summary>
    public static bool IsThreatened(GameObject dog, float threatRange = ThreatRange,
        float fieldOfView = FieldOfView, float calmDistance = CalmDistance,
        float closeThreatRange = CloseThreatRange)
    {
        if (dog == null)
        {
            return false;
        }

        if (IsPlayerClose(dog, calmDistance))
        {
            return false;
        }

        DogMood otherDog = FindOtherDog(dog);
        if (otherDog == null)
        {
            return false;
        }

        if (CanSee(dog, otherDog.gameObject, threatRange, fieldOfView))
        {
            return true;
        }

        // Совсем рядом собака чувствует другую собаку, даже если та за спиной —
        // иначе убегающая собака мгновенно "теряла" бы угрозу и останавливалась.
        Vector3 toOtherDog = otherDog.transform.position - dog.transform.position;
        toOtherDog.y = 0f;
        return toOtherDog.sqrMagnitude <= closeThreatRange * closeThreatRange;
    }

    /// <summary>
    /// Видит ли собака цель: цель внутри радиуса обзора и внутри угла поля зрения.
    /// </summary>
    public static bool CanSee(GameObject dog, GameObject target, float sightRange, float fieldOfView)
    {
        if (dog == null || target == null)
        {
            return false;
        }

        Vector3 toTarget = target.transform.position - dog.transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude > sightRange * sightRange)
        {
            return false;
        }

        if (toTarget.sqrMagnitude < 0.0001f)
        {
            return true;
        }

        float angle = Vector3.Angle(dog.transform.forward, toTarget.normalized);
        return angle <= fieldOfView * 0.5f;
    }
}
