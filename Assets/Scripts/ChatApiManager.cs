using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
namespace ChatApi {
    public class ChatApiManager  {
        string m_host;
        string m_apiKey;
        public static readonly HttpClient client = new HttpClient();

        


        string m_Model = "default-model"; // 代理服务器可能需要这个字段，可以随便填一个

        List<Message> currentMessage = new List<Message>();
        // Start is called before the first frame update
        public void Init(string host, string apiKey,string model) {
            m_host = host;
            m_apiKey = apiKey;
            m_Model = model;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", m_apiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }

        public async Task SendMessageToOpenAIAsync(string userMessage, System.Action<string> onResponse) {
            //Debug.Log("1111111111");
            currentMessage.Add(new Message { Role = "user", Content = userMessage });
            onResponse?.Invoke("user: " + userMessage);
            OpenAIRequest request = new OpenAIRequest {
                Model = m_Model,
                Messages = currentMessage
            };

            string jsonRequest = JsonConvert.SerializeObject(request);
            StringContent content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(m_host, content);

            if (response.IsSuccessStatusCode) {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                OpenAIResponse openAIResponse = JsonConvert.DeserializeObject<OpenAIResponse>(jsonResponse);

                if (openAIResponse != null && openAIResponse.Choices.Count > 0) {
                    string aiMessage = openAIResponse.Choices[0].Message.Content;
                    currentMessage.Add(new Message { Role = "assistant", Content = aiMessage });
                    //Debug.Log(aiMessage);

                    onResponse?.Invoke("assistant: <color=green>" + aiMessage + "</color>");
                }
                else {
                    onResponse?.Invoke("assistant: No response from AI.");
                }
            }
            else {
                onResponse?.Invoke("assistant:Error: " + response.ReasonPhrase);
            }

        }
        
        // Update is called once per frame
        private IEnumerator GetHeightCoroutine(RectTransform a, RectTransform b) {
            // --- 关键代码 ---
            // 等待到当前帧的末尾。此时，所有的UI布局和渲染计算都已经完成。
            yield return new WaitForEndOfFrame();

            // 现在，Content Size Fitter已经完成了它的工作
            b.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, a.rect.height * 0.3f);
            //Debug.Log("在协程中等待一帧后，获取到的最终高度是: " + a.rect.height);

            // 在这里可以使用获取到的正确高度来做其他事情
            // 例如：调整另一个元素的位置等
        }

    }
    public class Message {
        [JsonProperty("role")]
        public string Role { get; set; } = "";

        [JsonProperty("content")]
        public string Content { get; set; } = "";
    }

    public class OpenAIRequest {
        [JsonProperty("model")]
        public string Model { get; set; } = "default-model"; // 代理服务器可能需要这个字段，可以随便填一个gemini-2.5-flash

        [JsonProperty("messages")]
        public List<Message> Messages { get; set; } = new List<Message>();
    }


    public class OpenAIResponse {
        [JsonProperty("choices")]
        public List<Choice> Choices { get; set; } = new List<Choice>();
    }

    public class Choice {
        [JsonProperty("message")]
        public Message Message { get; set; }
    }
}