using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class ChatGPTAPI : MonoBehaviour
{
    [SerializeField] private string apiKey = "";
    private string apiUrl = "https://api.openai.com/v1/chat/completions";

    [Serializable]
    public class ChatGPTRequest
    {
        public string model = "gpt-4o-mini";
        public List<Message> messages = new List<Message>();
    }

    [Serializable]
    public class Message
    {
        public string role;
        public string content;

        public Message(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }

    [Serializable]
    public class ChatGPTResponse
    {
        public List<Choice> choices;
    }

    [Serializable]
    public class Choice
    {
        public Message message;
    }

    // Action to pass back the response
    public Action<string> OnResponseReceived;

    public void SendRequest(string userInput)
    {
        ChatGPTRequest request = new ChatGPTRequest();
        request.messages.Add(new Message("system", "You are a helpful assistant."));
        request.messages.Add(new Message("user", userInput));

        StartCoroutine(PostRequest(apiUrl, JsonConvert.SerializeObject(request)));
    }

    private IEnumerator PostRequest(string url, string jsonData)
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                ChatGPTResponse response = JsonConvert.DeserializeObject<ChatGPTResponse>(webRequest.downloadHandler.text);
                string reply = response.choices[0].message.content;
                Debug.Log("Assistant: " + reply);

                // Invoke the callback
                OnResponseReceived?.Invoke(reply);
            }
            else
            {
                Debug.LogError("Error: " + webRequest.error);
                OnResponseReceived?.Invoke("Error: " + webRequest.error);
            }
        }
    }
}
