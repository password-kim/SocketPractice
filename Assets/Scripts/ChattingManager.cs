using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ChattingManager : MonoBehaviour
{
    public static ChattingManager Instance;

    public InputField SendInput;
    public RectTransform ChatContent;
    public Text ChatText;
    public ScrollRect ChatScrollRect;

    public void ShowMessage(string data)
    {
        ChatText.text += ChatText.text == "" ? data : "\n" + data;
        
        Fit(ChatText.GetComponent<RectTransform>());
        Fit(ChatContent);
        Invoke("ScrollDelay", 0.03f);
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShowMessage("gdgd" + Random.Range(0, 100));
        }
    }

    private void Fit(RectTransform rect) => LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

    void ScrollDelay() => ChatScrollRect.verticalScrollbar.value = 0;
}
