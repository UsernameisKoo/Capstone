using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject dialogueBox;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] GameObject nextIcon;
    [SerializeField] GameObject skipButton;

    [Header("Typing")]
    [SerializeField] float typingSpeed = 0.05f;

    [Header("Look")]
    [SerializeField] LookAtTarget daughterLookAt;

    [SerializeField] PlayerController playerController;

    string[] lines;
    int currentLine = 0;
    bool isDialogueActive = false;

    bool isTyping = false;
    string currentText;
    Coroutine typingCoroutine;

    void Update()
    {
        if (!isDialogueActive) return;

        if (IsNextInputPressed())
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;

                dialogueText.text = currentText;
                isTyping = false;
                nextIcon.SetActive(true);
            }
            else
            {
                NextLine();
            }
        }
    }

    bool IsNextInputPressed()
    {
        return Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.KeypadEnter)
            || Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.RightArrow)
            || Input.GetMouseButtonDown(0);
    }

    public void StartDialogue(string[] dialogueLines)
    {
        if (isDialogueActive) return;
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        lines = dialogueLines;
        currentLine = 0;
        isDialogueActive = true;

        dialogueBox.SetActive(true);

        if (skipButton != null)
            skipButton.SetActive(true);

        if (playerController != null)
        {
            playerController.canMove = false;
            playerController.canJump = false;
        }

        if (daughterLookAt != null)
            daughterLookAt.StartLooking();

        ShowLine();
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    void ShowLine()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        currentText = lines[currentLine];
        nextIcon.SetActive(false);

        typingCoroutine = StartCoroutine(TypeText(currentText));
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;
        nextIcon.SetActive(true);
    }

    void NextLine()
    {
        currentLine++;

        if (currentLine < lines.Length)
        {
            ShowLine();
        }
        else
        {
            CloseDialogue();
        }
    }

    public void SkipDialogue()
    {
        Debug.Log("스킵 버튼 눌림");
        if (!isDialogueActive) return;
        CloseDialogue();
    }

    public void CloseDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (daughterLookAt != null)
            daughterLookAt.StopLooking();

        if (playerController != null)
        {
            playerController.canMove = true;
            playerController.canJump = true;
        }

        isDialogueActive = false;
        isTyping = false;
        dialogueBox.SetActive(false);
        nextIcon.SetActive(false);

        if (skipButton != null)
            skipButton.SetActive(false);

        currentLine = 0;
        currentText = "";
    }

    public IEnumerator ShowAutoDialogue(string[] dialogueLines, float duration)
    {
        StartDialogue(dialogueLines);
        yield return new WaitForSeconds(duration);
        CloseDialogue();
    }

    public IEnumerator ShowAutoDialogueInstant(string line, float duration)
    {
        Debug.Log("ShowAutoDialogueInstant 호출: " + line);

        if (dialogueBox == null || dialogueText == null)
        {
            Debug.Log("dialogueBox 또는 dialogueText가 null");
            yield break;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isDialogueActive = true;
        isTyping = false;
        currentText = line;

        dialogueBox.SetActive(true);

        if (skipButton != null)
            skipButton.SetActive(true);

        dialogueText.text = line;
        nextIcon.SetActive(false);

        Debug.Log("대화창 온, 텍스트 표시 완료: " + line);

        if (daughterLookAt != null)
            daughterLookAt.StartLooking();

        if (playerController != null)
        {
            playerController.canMove = false;
            playerController.canJump = false;
        }

        yield return new WaitForSeconds(duration);
        dialogueText.text = "";
        CloseDialogue();
    }
}