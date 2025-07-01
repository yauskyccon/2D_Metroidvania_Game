using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Point : MonoBehaviour
{
    private int point = 0;
    public string Scene;
    [SerializeField] private Text pointText;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Reward"))
        {
            CollectReward(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("Exit"))
        {
            if (point == 20)
            {
                CollectExitItem(collision.gameObject);
            }
        }
    }

    private void CollectReward(GameObject reward)
    {
        Destroy(reward);
        point++;
        pointText.text = "Point: " + point;
    }

    private void CollectExitItem(GameObject exitItem)
    {
        Destroy(exitItem);
        Debug.Log("Loading next level: " + Scene);
        Invoke("LoadNextLevel", 1.0f);
    }

    private void LoadNextLevel()
    {
        SceneManager.LoadScene(Scene);
    }
}
