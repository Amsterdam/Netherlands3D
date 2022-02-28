using UnityEngine;

public class KeyFrameWeightExample : MonoBehaviour
{
    public AnimationCurve animCurve = null;

    void Start()
    {
        Keyframe[] ks = new Keyframe[3];

        ks[0] = new Keyframe(0, 0);
        ks[0].weightedMode = WeightedMode.In;
        ks[0].inWeight = 0.5f;

        ks[1] = new Keyframe(4, 0);
        ks[1].weightedMode = WeightedMode.In;
        ks[1].inWeight = 0f;    // Zero weight.  The segment will be linear if previous keyframe outWeight is also zero.

        ks[2] = new Keyframe(6, 0);
        ks[2].weightedMode = WeightedMode.In;
        ks[2].inWeight = 1f / 3f;    // 1/3 is the default weight in WeightedMode.None weightedMode.

        animCurve = new AnimationCurve(ks);
    }

    void Update()
    {
        if(animCurve != null)
            transform.position = new Vector3(Time.time, animCurve.Evaluate(Time.time), 0);
    }
}