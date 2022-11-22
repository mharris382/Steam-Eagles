using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UINarrativeText : MonoBehaviour
{
    public string[] lines;
    public Events events;
    public float timePerChar = 0.1f;
    [Serializable]
    public class Events
    {
        public UnityEvent onTextStart;
        public UnityEvent<string> onTextChanged;
        public UnityEvent onTextEnd;
    }

    public KeyCode nextLineKey = KeyCode.Tab;

    bool running = false;
    
    public IEnumerator RunText()
    {
        running = true;
        events.onTextStart.Invoke();
        string text = "";
        foreach (string line in lines)
        {
            text = "";
            for (int i = 0; i < line.Length; i++)
            {
                text+= line[i];
                events.onTextChanged.Invoke(text);
                yield return new WaitForSeconds(timePerChar );
            }
            yield return new WaitUntil(() => Input.GetKeyDown(nextLineKey));
        }
        events.onTextEnd.Invoke();
        running = false;
    }

    private void OnEnable()
    {
        StartCoroutine(RunText());
    }

    private void OnDisable()
    {
        
    }
}
