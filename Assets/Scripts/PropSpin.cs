using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//THIS SCRIPT WAS DONE IN CLASS, CODE TAKEN DIRECTLY FROM WHAT THE PROFESSOR SHARED
public class PropSpin : MonoBehaviour
{
    [SerializeField] private GameObject propellor;
    [SerializeField] private Quaternion TargetRotation;
    [SerializeField] private float TargetAngle;
    [SerializeField, Range(0, 100)]
    private float RotationRate;

    // Start is called before the first frame update
    void Start()
    {
        TargetAngle = 0f;
        TargetRotation = Quaternion.AngleAxis(TargetAngle, Vector3.forward);
        propellor.transform.localRotation = TargetRotation;
    }

    // Update is called once per frame
    void Update()
    {
        TargetAngle += RotationRate * Time.fixedDeltaTime;
        TargetRotation = Quaternion.AngleAxis(TargetAngle, Vector3.forward);
        propellor.transform.localRotation = TargetRotation;
    }
}
