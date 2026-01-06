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
        bool isRAG;//todo:考虑将对话连入知识库的功能
        int maxContextLength = 5; // 最大对话长度，单位为次

        private const string summaryPrompt = "请简要总结上述对话的关键信息，以便我们在接下来的对话中继续引用。";
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

            // --- 新增逻辑开始：检查是否需要总结 ---
            // 假设 maxHistoryCount 是你设定的最大消息数量（例如 10 条）
            if (currentMessage.Count >= maxContextLength) {
                //onResponse?.Invoke("system: 正在总结历史记忆...");

                // 1. 构建总结请求的消息列表（复制当前历史 + 总结提示词）
                List<Message> summaryContext = new List<Message>(currentMessage);
                summaryContext.Add(new Message { Role = "user", Content = summaryPrompt });

                OpenAIRequest summaryRequest = new OpenAIRequest {
                    Model = m_Model,
                    Messages = summaryContext
                };

                string jsonSummaryRequest = JsonConvert.SerializeObject(summaryRequest);
                StringContent summaryContent = new StringContent(jsonSummaryRequest, System.Text.Encoding.UTF8, "application/json");

                try {
                    // 2. 发送总结请求
                    HttpResponseMessage summaryResponseMsg = await client.PostAsync(m_host, summaryContent);

                    if (summaryResponseMsg.IsSuccessStatusCode) {
                        string jsonSummaryResponse = await summaryResponseMsg.Content.ReadAsStringAsync();
                        OpenAIResponse openAISummaryResponse = JsonConvert.DeserializeObject<OpenAIResponse>(jsonSummaryResponse);

                        if (openAISummaryResponse != null && openAISummaryResponse.Choices.Count > 0) {
                            string summaryText = openAISummaryResponse.Choices[0].Message.Content;

                            // 3. 清空历史记录
                            currentMessage.RemoveRange(0,maxContextLength);

                            // 4. 将总结作为 System 消息或第一条 User 消息插入
                            // 建议使用 "system" 角色，这样AI知道这是背景信息
                            currentMessage.Add(new Message { Role = "system", Content = "以下是之前的对话总结: " + summaryText });

                           // Debug.Log("历史对话已总结: " + summaryText);
                        }
                    }
                }
                catch (System.Exception e) {
                    Debug.LogError("总结对话失败: " + e.Message);
                    // 如果总结失败，可以选择删除一半旧消息，防止死循环
                    if (currentMessage.Count > 2) {
                        currentMessage.RemoveRange(0, currentMessage.Count / 2);
                    }
                }
            }
            // --- 新增逻辑结束 ---

            // --- 以下是原有的正常对话逻辑 ---

            //Debug.Log("1111111111");
            // 将当前用户的消息加入（此时 list 已经被清理过，或者包含总结信息）
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