using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;

    public string host;
    public string apiKey;
    public ChatApi.ChatApiManager chatApiManager = new ChatApi.ChatApiManager();

    string m_Model = "default-model"; // 代理服务器可能需要这个字段，可以随便填一个

    // Start is called before the first frame update
    protected virtual void Start()
    {
       if(gameManager == null)
        {
            gameManager = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        chatApiManager.Init(host, apiKey, m_Model);
    }
    
    
    
}
