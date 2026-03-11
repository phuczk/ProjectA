using UnityEngine;
using GlobalEnums;

[System.Serializable]
public class PhantomDashEffect : Effect
{
    public float duration;
    public float cooldown = 1f;
    public override CursedObjectType EffectType => CursedObjectType.Ability;

    private static float _lastDashTime = -999f;

    public override void OnDash(PlayerController player, Vector2 dir)
    {
        if (Time.time - _lastDashTime < cooldown)
        {
            return;
        }
        
        _lastDashTime = Time.time;
        
        player.StartCoroutine(PhantomDashRoutine(player, duration));
    }
    
    private System.Collections.IEnumerator PhantomDashRoutine(PlayerController player, float dashDuration)
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        
        float originalInvincibleTimer = 0f;
        
        if (health != null)
        {
            var invincibleField = typeof(PlayerHealth).GetField("_invincibleTimer", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (invincibleField != null)
            {
                originalInvincibleTimer = (float)invincibleField.GetValue(health);
                invincibleField.SetValue(health, float.MaxValue);
            }
            
            var dashCounterField = typeof(PlayerHealth).GetField("_phantomDashCounter", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dashCounterField != null)
            {
                int counter = (int)dashCounterField.GetValue(health);
                dashCounterField.SetValue(health, counter + 1);
            }
            
            health.isNoTakeDamageTime = true;
        }
        
        yield return new WaitForSeconds(dashDuration);
        
        if (health != null)
        {
            var invincibleField = typeof(PlayerHealth).GetField("_invincibleTimer", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (invincibleField != null)
            {
                invincibleField.SetValue(health, originalInvincibleTimer);
            }
            
            var dashCounterField = typeof(PlayerHealth).GetField("_phantomDashCounter", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dashCounterField != null)
            {
                int counter = (int)dashCounterField.GetValue(health);
                counter--;
                dashCounterField.SetValue(health, counter);
                
                if (counter <= 0)
                {
                    health.isNoTakeDamageTime = false;
                    dashCounterField.SetValue(health, 0);
                }
            }
        }
    }
}
