using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NPC : MonoBehaviour
{
    public GameObject dialoguePanel;
    public Text dialogueText;
    public string[] dialogue;
    private int index;

    public GameObject contButton;
    public float wordSpeed;
    public bool playerIsClose;

    private static NPC instance;
    public AudioSource audioSource;
    public AudioClip continueSound;

    void Start()
    {
        /*UpdatePosition();
        SceneManager.sceneLoaded += OnSceneLoaded;*/
        FindSceneCanvas();
        // Initially, set the Dialogue Panel and its elements to inactive
        dialoguePanel.SetActive(false);
        dialogueText.text = "";

        // Optionally, you can also set the Continue Button inactive initially
        contButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Interact") && playerIsClose)
        {
            if (dialoguePanel.activeInHierarchy)
            {
                zeroText();
            }
            else
            {
                dialoguePanel.SetActive(true);
                contButton.SetActive(false);
                StopAllCoroutines();
                StartCoroutine(Typing());
            } 
        }

        if(dialogueText.text == dialogue[index])
        {
            contButton.SetActive(true);
        }
    }

    public void zeroText()
    {
        dialogueText.text = "";
        index = 0;
        dialoguePanel.SetActive(false);
        contButton.SetActive(false);
    }

    IEnumerator Typing()
    {
        foreach(char letter in dialogue[index].ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(wordSpeed);
        }
    }

    public void NextLine()
    {
        contButton.SetActive(false);

        if (audioSource != null && continueSound != null)
        {
            audioSource.PlayOneShot(continueSound);
        }

        if (index < dialogue.Length - 1)
        {
            index++;
            dialogueText.text = "";
            StopAllCoroutines();
            StartCoroutine(Typing());
        }
        else zeroText();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = false;
            zeroText();
        }
    }

    private void FindSceneCanvas()
    {
        // Attempt to find scene-specific canvas objects
        dialoguePanel = GameObject.Find("Dialogue Panel");
        if (dialoguePanel == null)
        {
            Debug.LogError("Dialogue Panel not found in the current scene.");
            return;
        }

        dialogueText = GameObject.Find("Dialogue Text")?.GetComponent<Text>();
        if (dialogueText == null)
        {
            Debug.LogError("Dialogue Text not found under Dialogue Panel.");
            return;
        }

        contButton = GameObject.Find("Continue Button")?.gameObject;
        if (contButton == null)
        {
            Debug.LogError("Continue Button not found under Dialogue Panel.");
            return;
        }
    }
}
