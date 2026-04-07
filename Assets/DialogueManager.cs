using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject dialogueBox;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] GameObject nextIcon;

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
            || Input.GetMouseButtonDown(0); // 좌클릭
    }

    public void StartDialogue(string[] dialogueLines)
    {
        if (isDialogueActive) return;
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        lines = dialogueLines;
        currentLine = 0;
        isDialogueActive = true;

        dialogueBox.SetActive(true);

        // 대화 시작시 플레이어 움직임 정지
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

    public void CloseDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (daughterLookAt != null)
            daughterLookAt.StopLooking();

        // 대화 끝나면 플레이어 움직일 수 있음
        if (playerController != null)
            playerController.canMove = true;

        isDialogueActive = false;
        isTyping = false;
        dialogueBox.SetActive(false);
        nextIcon.SetActive(false);
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
        Debug.Log("ShowAutoDialogueInstant 호출됨: " + line);

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
        dialogueText.text = line;
        nextIcon.SetActive(false);

        Debug.Log("대화창 켬, 텍스트 설정 완료: " + line);

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