using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ValueSelection : UIWidgetContainer
{
    public GameObject tweenIncrement;
    public GameObject tweenDecrement;

    public UITexture targetTexture;
    public List<Texture> textures;

    public UILabel targetLabel;
    public List<string> words;


    public List<EventDelegate> onValueChange = new List<EventDelegate>();

    int v;
    /// <summary>
    /// value, clamped between 0 and the max index of either textures or words
    /// </summary>
    public int value
    {
        get { return v; }
        set
        {
            v = Mathf.Max(0,
                Mathf.Min(
                Mathf.Max(textures != null ? textures.Count - 1 : 0, words != null ? words.Count - 1 : 0),
                value));
            SetObjects();
            EventDelegate.Execute(onValueChange);
        }
    }

    void SetObjects()
    {
        if (targetTexture) if(v < textures.Count) targetTexture.mainTexture = textures[v];
        if (targetLabel) if (v < words.Count) targetLabel.text = words[v];
    }


    public virtual void OnIncrement()
    {
        ++value;
        if (tweenIncrement) tweenIncrement.SendMessage("PlayForward");
    }

    public virtual void OnDecrement()
    {
        --value;
        if (tweenDecrement) tweenDecrement.SendMessage("PlayForward");
    }
}
