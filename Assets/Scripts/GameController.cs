using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GameController : MonoBehaviour
{
    public TMP_InputField inputField;
    public GameObject messagePrefab;
    public GameObject receiverPrefab;
    public GameObject introduction;
    public GameObject canvas;
    public string name;
    public string startingText;
    
    private ChatGPTAPI chatGPT;
    private List<string> chatHistory;
    private List<GameObject> previousChats;

    private float totalHeight = 0;
    private bool isBlocked = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        chatGPT = FindObjectOfType<ChatGPTAPI>();

        if (chatGPT == null)
        {
            Debug.LogError("ChatGPTAPI component not found!");
        }
        else
        {
            // Subscribe to the response callback
            chatGPT.OnResponseReceived += HandleChatGPTResponse;
        }

        chatHistory = new List<string>();
        previousChats = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !isBlocked)
        {
            SendMessage(inputField.text);

            Debug.Log("message sent!");
            
            SendMessageToChatGPT(GetChatInput());

  
            inputField.text = "";

        }
    }

    public string GetChatInput() {
        string start = new string(startingText);
        for (int i = 0; i < chatHistory.Count; i++) {
            if (i % 2 == 0)
            {
                start += ("\n\nUser: " + chatHistory[i]);
            }
            else {
                start += ($"\n\n{name}: " + chatHistory[i]);
            }
        }
        start += $"\n\n{name}: ";
        return start;
    }

    public void SendMessage(string message) {
        GameObject instance = Instantiate(messagePrefab);

        // Set its parent
        instance.transform.SetParent(canvas.transform, false);

        Transform childTransform = instance.transform.Find("Text (TMP)");
        TextMeshProUGUI tmpText = childTransform.GetComponent<TextMeshProUGUI>();
        tmpText.text = message;

        Vector2 preferredValues = tmpText.GetPreferredValues();
        RectTransform rectTransform = instance.GetComponent<RectTransform>();
        Vector2 newScale = new Vector2(Mathf.Min((preferredValues.x + 100), 1000), preferredValues.y + 60);

        rectTransform.sizeDelta = newScale;

        // Access the Material of the Image
        Image materialInstance = instance.GetComponent<Image>();

        materialInstance.material = new Material(materialInstance.material);

        // Modify the shader properties
        materialInstance.material.SetFloat("_AspectRatio", newScale.x / newScale.y);

        MoveMessageRight(instance, (1000 - newScale.x) / 2);
        MoveMessageUp(instance, newScale.y / 2);

        chatHistory.Add(message);

        for (int i = 0; i < previousChats.Count; i++) {
            MoveMessageUp(previousChats[i], newScale.y + 35);
        }

        totalHeight += (newScale.y + 35);
        if (totalHeight > 700)
        {
            Destroy(introduction);
        }
        previousChats.Add(instance);
    }

    public void ReceiveMessage(string message)
    {
        if (message.Contains("[BLOCK]")) {
            isBlocked = true;
        }
        GameObject instance = Instantiate(receiverPrefab);

        // Set its parent
        instance.transform.SetParent(canvas.transform, false);

        Transform childTransform = instance.transform.Find("Text (TMP)");
        TextMeshProUGUI tmpText = childTransform.GetComponent<TextMeshProUGUI>();
        tmpText.text = message;

        Vector2 preferredValues = tmpText.GetPreferredValues();
        RectTransform rectTransform = instance.GetComponent<RectTransform>();
        Vector2 newScale = new Vector2(Mathf.Min((preferredValues.x + 100), 1000), preferredValues.y + 60);

        rectTransform.sizeDelta = newScale;

        // Access the Material of the Image
        Image materialInstance = instance.GetComponent<Image>();

        materialInstance.material = new Material(materialInstance.material);

        // Modify the shader properties
        materialInstance.material.SetFloat("_AspectRatio", newScale.x / newScale.y);

        MoveMessageRight(instance, -(1000 - newScale.x) / 2);
        MoveMessageUp(instance, newScale.y / 2);

        chatHistory.Add(message);
        for (int i = 0; i < previousChats.Count; i++)
        {
            MoveMessageUp(previousChats[i], newScale.y + 35);
        }
        totalHeight += (newScale.y + 35);

        if (totalHeight > 700)
        {
            Destroy(introduction);
        }
        previousChats.Add(instance);
    }

    public void MoveMessageUp(GameObject message, float amount) {
        RectTransform uiElement = message.GetComponent<RectTransform>();
        uiElement.anchoredPosition = new Vector2(uiElement.anchoredPosition.x, uiElement.anchoredPosition.y+amount);
    }

    public void MoveMessageRight(GameObject message, float amount)
    {
        RectTransform uiElement = message.GetComponent<RectTransform>();
        uiElement.anchoredPosition = new Vector2(uiElement.anchoredPosition.x + amount, uiElement.anchoredPosition.y);
    }

    public void SendMessageToChatGPT(string userInput)
    {
        if (chatGPT != null)
        {
            chatGPT.SendRequest(userInput);
        }
    }

    // Handle the response here
    private void HandleChatGPTResponse(string response)
    {
        Debug.Log("ChatGPT Response: " + response);
        ReceiveMessage(response);
        // You can now use the response however you want:
        // Update UI, trigger game events, etc.
    }
}
