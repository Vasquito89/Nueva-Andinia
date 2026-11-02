using UnityEngine;
using UnityEngine.AI;

public class Npc : MonoBehaviour
{
    public NavMeshAgent AI;
    [SerializeField] private float velocity;
    [SerializeField] private Transform[] objectives;
    [SerializeField] private float distance;
    private Transform objective;

    [Header("Animaciones")]
    [SerializeField] private Animation anim;
    [SerializeField] private string walkinAnim;

    private void Start()
    {
        objective = objectives[Random.Range(0, objectives.Length)];
        anim.Play(walkinAnim);
    }
    private void Update()
    {
        distance = Vector3.Distance(transform.position, objective.position);

        if(distance < 2)
        {
            objective = objectives[Random.Range(0, objectives.Length)];
        }
        AI.destination = objective.position;
        AI.speed = velocity;
    }
}
