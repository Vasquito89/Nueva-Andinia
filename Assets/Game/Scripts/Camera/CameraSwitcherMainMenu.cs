using System.Collections;
using UnityEngine;

public class CameraSwitcherMainMenu : MonoBehaviour
{
    [Header("Points")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private Transform pointC;

    [Header("Movement")]
    [SerializeField] private float moveDuration = 0.4f;

    private bool isMoving = false;

    public void GoToOptions()
    {
        if (isMoving) return;

        StartCoroutine(MoveTo(pointB));
    }

    public void ReturnToMain()
    {
        if (isMoving) return;

        StartCoroutine(MoveTo(pointA));
    }
    public void GoToPlay()
    {
        if (isMoving) return;

        StartCoroutine(MoveTo(pointC));
    }

    private IEnumerator MoveTo(Transform target)
    {
        if (target == null) yield break;

        isMoving = true;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / moveDuration);

            transform.position = Vector3.Lerp(startPos, target.position, t);
            transform.rotation = Quaternion.Slerp(startRot, target.rotation, t);

            yield return null;
        }

        transform.position = target.position;
        transform.rotation = target.rotation;

        isMoving = false;
    }
}