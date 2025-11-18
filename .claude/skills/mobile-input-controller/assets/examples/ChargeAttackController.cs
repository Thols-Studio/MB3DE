using UnityEngine;

/// <summary>
/// Example: Charge attack that powers up while touch is held.
/// Visual feedback shows charge level in real-time (like Mega Man X or Dark Souls).
///
/// Integration Pattern: Polling-Based
/// Use this pattern when you need continuous state updates (charging, aiming, etc.).
/// </summary>
public class ChargeAttackController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MobileInputController inputController;
    [SerializeField] private ParticleSystem chargeEffect;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Charge Settings")]
    [SerializeField] private float minDamage = 10f;
    [SerializeField] private float maxDamage = 100f;
    [SerializeField] private float chargeThreshold1 = 0.33f; // First charge level
    [SerializeField] private float chargeThreshold2 = 0.66f; // Second charge level
    [SerializeField] private float chargeThreshold3 = 1.0f;  // Max charge level

    [Header("Visual Feedback")]
    [SerializeField] private Color chargeColor1 = Color.yellow;
    [SerializeField] private Color chargeColor2 = Color.orange;
    [SerializeField] private Color chargeColor3 = Color.red;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip chargeSound;
    [SerializeField] private AudioClip releaseSound;

    private int currentChargeLevel = 0;
    private bool wasCharging = false;

    private void Update()
    {
        // Poll current touch state every frame
        bool isCharging = inputController.IsTouchHeld;

        if (isCharging)
        {
            UpdateCharge();
        }
        else if (wasCharging)
        {
            // Touch just released - fire charged attack
            FireChargedAttack();
        }

        wasCharging = isCharging;
    }

    /// <summary>
    /// Update charge effect based on current hold time.
    /// Called continuously while touch is held.
    /// </summary>
    private void UpdateCharge()
    {
        // Get current charge level (0-1)
        float chargeLevel = inputController.GetCurrentNormalizedHoldTime();

        // Determine charge tier
        int newChargeLevel = GetChargeTier(chargeLevel);

        // Play sound and update effect when charge level increases
        if (newChargeLevel > currentChargeLevel)
        {
            OnChargeLevelIncreased(newChargeLevel);
        }

        currentChargeLevel = newChargeLevel;

        // Update visual effect intensity
        if (chargeEffect != null && !chargeEffect.isPlaying)
        {
            chargeEffect.Play();
            PlayChargeSound();
        }

        // Update particle color based on charge tier
        UpdateChargeVisuals(chargeLevel, currentChargeLevel);
    }

    /// <summary>
    /// Fire attack with power based on final charge level.
    /// </summary>
    private void FireChargedAttack()
    {
        // Stop charge effect
        if (chargeEffect != null && chargeEffect.isPlaying)
        {
            chargeEffect.Stop();
        }

        // Calculate damage based on charge level
        float damage = CalculateChargeDamage(currentChargeLevel);

        // Spawn and fire projectile
        if (projectilePrefab != null && firePoint != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

            // Set projectile damage
            var damageComponent = projectile.GetComponent<Projectile>();
            if (damageComponent != null)
            {
                damageComponent.SetDamage(damage);
            }

            // Apply force based on charge level
            var rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                float speed = Mathf.Lerp(10f, 30f, currentChargeLevel / 3f);
                rb.velocity = firePoint.forward * speed;
            }
        }

        // Play release sound
        if (audioSource != null && releaseSound != null)
        {
            audioSource.PlayOneShot(releaseSound);
        }

        // Reset charge state
        currentChargeLevel = 0;

        Debug.Log($"Charged attack fired! Damage: {damage}, Level: {currentChargeLevel}");
    }

    /// <summary>
    /// Determine charge tier (0-3) based on normalized time.
    /// </summary>
    private int GetChargeTier(float normalizedTime)
    {
        if (normalizedTime >= chargeThreshold3) return 3;
        if (normalizedTime >= chargeThreshold2) return 2;
        if (normalizedTime >= chargeThreshold1) return 1;
        return 0;
    }

    /// <summary>
    /// Calculate final damage based on charge tier.
    /// </summary>
    private float CalculateChargeDamage(int chargeTier)
    {
        float tierPercent = chargeTier / 3f;
        return Mathf.Lerp(minDamage, maxDamage, tierPercent);
    }

    /// <summary>
    /// Update visual feedback when charge level increases.
    /// </summary>
    private void OnChargeLevelIncreased(int newLevel)
    {
        Debug.Log($"Charge level increased to {newLevel}!");

        // Could add screen shake, camera zoom, or other effects here
        // Example: Camera shake intensity based on level
        // CameraShake.Instance?.Shake(0.1f + (newLevel * 0.05f));
    }

    /// <summary>
    /// Update charge particle effect visuals.
    /// </summary>
    private void UpdateChargeVisuals(float normalizedTime, int chargeTier)
    {
        if (chargeEffect == null) return;

        // Update emission rate based on charge
        var emission = chargeEffect.emission;
        emission.rateOverTime = Mathf.Lerp(10f, 100f, normalizedTime);

        // Update color based on charge tier
        var main = chargeEffect.main;
        switch (chargeTier)
        {
            case 1:
                main.startColor = chargeColor1;
                break;
            case 2:
                main.startColor = chargeColor2;
                break;
            case 3:
                main.startColor = chargeColor3;
                break;
            default:
                main.startColor = Color.white;
                break;
        }
    }

    /// <summary>
    /// Play charging sound effect.
    /// </summary>
    private void PlayChargeSound()
    {
        if (audioSource != null && chargeSound != null)
        {
            audioSource.clip = chargeSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}

/// <summary>
/// Simple projectile component for damage handling.
/// Attach to projectile prefab.
/// </summary>
public class Projectile : MonoBehaviour
{
    private float damage = 10f;

    public void SetDamage(float value)
    {
        damage = value;
    }

    public float GetDamage()
    {
        return damage;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Apply damage to target
        var health = collision.gameObject.GetComponent<HealthComponent>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        // Destroy projectile
        Destroy(gameObject);
    }
}

/// <summary>
/// Placeholder health component.
/// Replace with your actual health system.
/// </summary>
public class HealthComponent : MonoBehaviour
{
    public void TakeDamage(float amount)
    {
        Debug.Log($"{gameObject.name} took {amount} damage!");
    }
}
