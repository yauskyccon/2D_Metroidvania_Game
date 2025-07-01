using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class THKEvents : MonoBehaviour
{
    public void SlashDamagePlayer()
    {
        Vector2 playerPosition = PlayerController.Instance.transform.position;
        Vector2 bossPosition = transform.position;

        float deltaX = playerPosition.x - bossPosition.x;
        float deltaY = playerPosition.y - bossPosition.y;

        if (Mathf.Abs(deltaY) > Mathf.Abs(deltaX)) // Vertical attack
        {
            if (deltaY > 0) // Player is above
            {
                Hit(TheHollowKnight.Instance.UpAttackTransform, TheHollowKnight.Instance.UpAttackArea);
            }
            else // Player is below
            {
                Hit(TheHollowKnight.Instance.DownAttackTransform, TheHollowKnight.Instance.DownAttackArea);
            }
        }
        else // Horizontal attack
        {
            if (deltaX > 0) // Player is to the right
            {
                Hit(TheHollowKnight.Instance.SideAttackTransform, TheHollowKnight.Instance.SideAttackArea);
            }
            else // Player is to the left
            {
                Hit(TheHollowKnight.Instance.SideAttackTransform, TheHollowKnight.Instance.SideAttackArea);
            }
        }
        /*if (PlayerController.Instance.transform.position.x - transform.position.x != 0)
        {
            Hit(TheHollowKnight.Instance.SideAttackTransform, TheHollowKnight.Instance.SideAttackArea);
        }
        else if (PlayerController.Instance.transform.position.y > transform.position.y)
        {
            Hit(TheHollowKnight.Instance.UpAttackTransform, TheHollowKnight.Instance.UpAttackArea);
        }
        else if (PlayerController.Instance.transform.position.y < transform.position.y)
        {
            Hit(TheHollowKnight.Instance.DownAttackTransform, TheHollowKnight.Instance.DownAttackArea);
        }*/
    }
    void Hit(Transform _attackTransform, Vector2 _attackArea)
    {
        Collider2D[] _objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0);

        for (int i = 0; i < _objectsToHit.Length; i++)
        {
            if (_objectsToHit[i].GetComponent<PlayerController>() != null && !PlayerController.Instance.pState.invincible)
            {
                _objectsToHit[i].GetComponent<PlayerController>().TakeDamage(TheHollowKnight.Instance.damage);
                if (PlayerController.Instance.pState.alive)
                {
                    GameManager.Stop();
                }
            }
        }
    }

    public void Parrying()
    {
        TheHollowKnight.Instance.parrying = true;
    }

    void BendDownCheck()
    {
        if (TheHollowKnight.Instance.barrageAttack)
        {
            StartCoroutine(BarrageAttackTransition());
        }
        if (TheHollowKnight.Instance.outbreakAttack)
        {
            StartCoroutine(OutbreakAttackTransition());
        }
        if (TheHollowKnight.Instance.bounceAttack)
        {
            TheHollowKnight.Instance.anim.SetTrigger("Bounce1");
        }

    }

    void BarrageOrOutbreak()
    {
        if (TheHollowKnight.Instance.barrageAttack)
        {
            TheHollowKnight.Instance.StartCoroutine(TheHollowKnight.Instance.Barrage());
        }
        if (TheHollowKnight.Instance.outbreakAttack)
        {
            TheHollowKnight.Instance.StartCoroutine(TheHollowKnight.Instance.Outbreak());
        }
    }

    IEnumerator OutbreakAttackTransition()
    {
        yield return new WaitForSeconds(1f);
        TheHollowKnight.Instance.anim.SetBool("Cast", true);
    }

    IEnumerator BarrageAttackTransition()
    {
        yield return new WaitForSeconds(1f);
        TheHollowKnight.Instance.anim.SetBool("Cast", true);
    }

    void DestroyAfterDeath()
    {
        SpawnBoss.Instance.IsNotTrigger();
        TheHollowKnight.Instance.DestroyAfterDeath();
        GameManager.Instance.THKDefeated = true;
        SaveData.Instance.SaveBossData();
        SaveData.Instance.SavePlayerData();
    }
}
