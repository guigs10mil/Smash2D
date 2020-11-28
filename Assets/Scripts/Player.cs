using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using Cinemachine;

public class Player : MonoBehaviour
{
    private Vector3 startPosition;
    private GameObject attackCollider;
    private PlayerMovement pm;
    private AudioManager audioManager;
    private GameManager gameManager;
    public InputUser user;
    public GameObject explosion;

    private HealthDisplay healthDisplay;

    public GameObject specialAttack;

    // private int maxHealth = 3;
    private float health = 0;
    private bool imune = false;
    private bool canSpecial = true;
    private bool canAttack = true;

    private void Start()
    {
        startPosition = transform.position;
        attackCollider = transform.GetChild(1).gameObject;
        pm = GetComponent<PlayerMovement>();
        audioManager = FindObjectOfType<AudioManager>();
        gameManager = FindObjectOfType<GameManager>();
    }

    // private void OnDestroy()
    // {
    //     print("Destroyed: " + user.index);
    //     user.UnpairDevices();
    // }

    public void Die()
    {
        StartCoroutine("Death");
    }

    public void SpecialAttack()
    {
        if (canSpecial)
        {
            StartCoroutine("SpecialAttackCooldown");
            StartCoroutine("SpecialAttackDuration");
        }
    }

    public void Attack()
    {
        if (canAttack)
        {
            StartCoroutine("AttackCooldown");
            StartCoroutine("AttackDuration");
        }
    }

    public void TakeDamage(GameObject source, float amount, float hitMultiplier)
    {
        pm.stuned = false;
        if (!imune)
        {
            float hitForce = Mathf.Lerp(10f, 40f, health);
            hitForce *= hitMultiplier;
            pm.DamageKnokback(source.transform.position, hitForce);

            health += amount;
            if (health > 1)
                health = 1;

            gameManager.UpdatePercentage(user.index, (int)(health * 100));

            if (health > 0.7f)
                audioManager.Play("Punch L");
            else if (health > 0.3f)
                audioManager.Play("Punch M");
            else
                audioManager.Play("Punch S");

            float secondsImune = 1f;

            if (hitMultiplier < 0.5f)
                secondsImune = 0.5f * health;

            StartCoroutine(ImunityTimer(secondsImune));
        }

    }

    public void TakeDamageFromIceBall(float amount)
    {
        if (!pm.stuned)
        {
            imune = false;
            health += amount;
            if (health > 1)
                health = 1;

            audioManager.Play("Freeze");

            StartCoroutine(pm.StunTimer(2f));

            gameManager.UpdatePercentage(user.index, (int)(health * 100));
        }
    }

    public void TakeDamageFromHazard()
    {
        // health--;
        // UpdateHealthDisplay();
        // if (health <= 0)
        //     Die();
        // else {
        pm.DamageKnokbackFromHazard();
        StartCoroutine("ImunityTimer");
        // }
    }

    // public void RegenerateHealth(int amount) {
    //     health += amount;
    //     if (health > maxHealth) {
    //         health = maxHealth;
    //     }
    //     // UpdateHealthDisplay();
    // }

    // void UpdateHealthDisplay() {
    //     if (healthDisplay != null)
    //         PlayerPrefs.SetInt("Health", health);
    //         healthDisplay.UpdateHealthDisplay();
    // }

    IEnumerator SpecialAttackDuration()
    {
        GameObject sa = Instantiate(specialAttack, new Vector3(transform.position.x + (transform.localScale.x * 0.7f), transform.position.y + 0.1f, transform.position.z), transform.rotation);
        sa.GetComponent<IceBall>().dir = transform.localScale.x;
        sa.GetComponent<IceBall>().parentIndex = user.index;
        audioManager.Play("Swing");
        pm.specialAttacking = true;
        yield return new WaitForSeconds(0.1f);
        pm.specialAttacking = false;
    }

    IEnumerator SpecialAttackCooldown()
    {
        canSpecial = false;
        yield return new WaitForSeconds(1f);
        canSpecial = true;
    }

    IEnumerator AttackDuration()
    {
        audioManager.PlayVariation("Swing");
        attackCollider.SetActive(true);
        pm.attacking = true;
        yield return new WaitForSeconds(0.1f);
        attackCollider.SetActive(false);
        pm.attacking = false;
    }

    IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(0.2f);
        canAttack = true;
    }

    IEnumerator ImunityTimer(float seconds)
    {
        imune = true;
        GetComponentInChildren<SpriteRenderer>().color = Color.black;
        yield return new WaitForSeconds(0.1f);
        for (float i = 0; i < seconds; i += 0.2f)
        {
            GetComponentInChildren<SpriteRenderer>().color = Color.white;
            yield return new WaitForSeconds(0.1f);
            GetComponentInChildren<SpriteRenderer>().color = Color.black;
            yield return new WaitForSeconds(0.1f);
        }
        GetComponentInChildren<SpriteRenderer>().color = Color.white;
        imune = false;
    }

    IEnumerator Death()
    {
        audioManager.Play("Fall");
        GetComponent<CinemachineImpulseSource>().GenerateImpulse();
        Instantiate(explosion, transform.position, transform.rotation);
        gameManager.OnPlayersDeath(user.index);
        Destroy(gameObject);
        yield return new WaitForSecondsRealtime(1f);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        print(other.gameObject.name);
    }
}
