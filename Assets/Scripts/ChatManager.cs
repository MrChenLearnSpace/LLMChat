using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : GameManager
{
    public GameObject content_obj;
    public GameObject text_Prefabs;
    public InputField input_text;


    string m_Model = "default-model"; // 代理服务器可能需要这个字段，可以随便填一个

    // Start is called before the first frame update
    protected override void Start() {
        base.Start();
    }


    void CreateText(string content) {

        GameObject go = Instantiate(text_Prefabs, content_obj.transform);
        go.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = content;
        RectTransform a = go.transform.GetChild(0).GetComponent<RectTransform>();
        StartCoroutine(GetHeightCoroutine(a, go.GetComponent<RectTransform>()));
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
    public  void OnClickSend() {
        try {
            chatApiManager.SendMessageToOpenAIAsync(input_text.text, CreateText);
            input_text.text = "";
        }
        catch (Exception e) {
            CreateText(e.Message);
        }
    }
}
