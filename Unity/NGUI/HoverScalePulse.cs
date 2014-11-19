using UnityEngine;
using System.Collections;

public class HoverScalePulse : MonoBehaviour
{
    public Transform target = null;

    public float pulseRate = 1f;
    public Vector3 upperScale = new Vector3(1.1f,1.1f,1.1f);
    public bool ignoreTimescale = true;

    private Vector3 normalScale;

    private bool pulse = false;
    private float current = 1f;


    void Start()
    {
        if (target == null) target = this.transform;
        normalScale = target.localScale;
    }
    void OnEnable()
    {
        pulse = false;
        current = 1f;
    }


    void OnHover(bool over)
    { pulse = over; }


    void Update()
    {
        //switch side if disabled
        if (!pulse && current < .5f)
            current = 1f - current;

        if (pulse || current < 1f)
        {
            if (current >= 1f)
                current -= (float)((int)current);

            current += pulseRate *
                (ignoreTimescale ? RealTime.deltaTime : Time.deltaTime)
                * (pulse ? 1f : 2f);

            float val = (1f - Mathf.Cos(current * Mathf.PI * 2f)) / 2f;
            target.localScale = Vector3.Lerp(normalScale, upperScale, val);
        }
    }

}
