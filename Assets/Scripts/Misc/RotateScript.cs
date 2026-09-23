using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RotateScript : MonoBehaviour
{
    private bool rotating = true;
    public float speed = 1f;
    public GameObject shadow;

    private void Update()
    {
        bool shouldRotate = ShopController._instance.rotateCircle;
        if (rotating || shouldRotate)
        {
            var objectTransform = transform;
            Vector3 localRot = objectTransform.localEulerAngles;

            if (!shouldRotate)
            {
                // we reset it
                localRot.z = 0;
            }
            else
            {
                // we keep going
                localRot.z = (localRot.z + Time.deltaTime * speed) % 360;
            }
            objectTransform.localEulerAngles = localRot;
            // and we set the shadow
            if (shadow is not null)
            {
                shadow.transform.localEulerAngles = localRot;
            }
        }
    }
}
