using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ClockUI : MonoBehaviour
{
    private const float SecondsInADay = 5f;
    private Transform clockHandTransform;
    private float day;
    private void Awake()
    {
        clockHandTransform = transform.Find("ClockHand");
    }
    private void Update()
    {
        day += Time.deltaTime / SecondsInADay;
        float normalizedDay = day % 1f;
        float rotationAngle = 360f;
        clockHandTransform.eulerAngles = new Vector3(0f, 0f, -normalizedDay*rotationAngle);
    }
}
