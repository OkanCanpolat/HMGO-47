using System.Collections.Generic;
using UnityEngine;

public class Animateur : MonoBehaviour
{
    private Transform root;

    public List<BaseAnimation> animations = new List<BaseAnimation>();

    public List<BaseAnimation> finishedAnimations = new List<BaseAnimation>();

    private void Start()
    {
        root = transform;
    }

    private void Update()
    {
        Vector3 zero = Vector3.zero;
        Quaternion identity = Quaternion.identity;

        foreach (BaseAnimation animation in animations)
        {
            animation.Update();
            zero += animation.DeltaPosition;
            identity *= animation.DeltaRotation;
        }

        if ((bool)root)
        {
            root.position += zero;
            root.rotation *= identity;
        }
        foreach (BaseAnimation finishedAnimation in finishedAnimations)
        {
            finishedAnimation.Stop();
            animations.Remove(finishedAnimation);
        }

        finishedAnimations.Clear();
    }

    public static void PushAnimation(GameObject gameObject, BaseAnimation animation)
    {
        if ((bool)gameObject)
        {
            Animateur animateur = gameObject.GetComponent<Animateur>();
            if (animateur == null)
            {
                animateur = gameObject.AddComponent<Animateur>();
            }
            animation.Animateur = animateur;
            animateur.animations.Add(animation);
            animation.Start();
        }
    }
}
