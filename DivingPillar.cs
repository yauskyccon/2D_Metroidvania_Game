using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DivingPillar : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.CompareTag("Player") && !PlayerController.Instance.pState.invincible)
        {
            _other.GetComponent<PlayerController>().TakeDamage(TheHollowKnight.Instance.damage);
            if (PlayerController.Instance.pState.alive)
            {
                GameManager.Stop();
            }
        }
    }
}
