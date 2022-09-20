using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HunterAI : MonoBehaviour
{

    private enum AIState
    {
        Passive,
        Hostile,
        Dying
    }
    [SerializeField]
    private AIState _AIState;
    private NavMeshAgent _navMeshAgent;
    public float radius = 20f;
    public float angle = 90f;
    public bool _isChasingPlayer = false;
    public GameObject _player;
    [SerializeField]
    private bool _IAmWaiting;
    [SerializeField]
    private AudioSource _audioSource;
    [SerializeField]
    private AudioClip _roar;
    [SerializeField]
    private AudioClip _onHit;
    [SerializeField]
    private AudioClip _attack;
    [SerializeField]
    private AudioClip _onDeath;

    private Animator _anim;

    public LayerMask targetMask;
    public LayerMask ObstructionMask;
    public bool canSeePlayer;
    private bool isDying = false;
    [SerializeField]
    private bool haveIScreamed = false;
    [SerializeField]
    private GameObject[] _hitboxes;

    [SerializeField]
    private int _currentHp;

    [Range(0, 500)] public float walkRadius;
    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _anim = GetComponent<Animator>();
        _player = GameObject.Find("Player");
        StartCoroutine(CheckForPlayer());
        _currentHp = Random.Range(5,12);
    }

    void Update()
    {
        
        switch (_AIState)
        {
            case AIState.Passive:
                Wander();
                break;
            case AIState.Hostile:
                ChasePlayer();
                break;
            case AIState.Dying:
                Die();
                break;
        }

        if (canSeePlayer == true)
        {
            StartCoroutine(Roar());
            _AIState = AIState.Hostile;
        }
        if (_currentHp <= 0)
        {
            _AIState = AIState.Dying;
        }
        if (_navMeshAgent.speed == 0)
        {
            _anim.SetBool("isMoving", false);
        }
        else
        {
            _anim.SetBool("isMoving", true);

        }
    }

    private void Wander()
    {
        if (_navMeshAgent != null && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance && _IAmWaiting == false)
        {
            _navMeshAgent.SetDestination(RandomNavMeshLocation());
            _IAmWaiting = true;
            StartCoroutine(RandomWaitTimer());
        }
    }
    private IEnumerator Roar()
    {
        _navMeshAgent.speed = 0;
        
        if (_audioSource.isActiveAndEnabled == false)
        {
            _audioSource.enabled = true;
        }
        _anim.SetTrigger("hasSeenPlayer");
        yield return new WaitForSeconds(2f);
        _navMeshAgent.speed = 5;
        _anim.SetBool("haveIScreamed", true);

    }
    private void FieldOfViewCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, ObstructionMask))
                {
                    canSeePlayer = true;
                }
                else
                {
                    canSeePlayer = false;
                }
            }
            else
            {
                canSeePlayer = false;
            }
        }
        else if (canSeePlayer)
        {
            canSeePlayer = false;
        }
    }
    private IEnumerator CheckForPlayer()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }
    IEnumerator RandomWaitTimer()
    {
        int wait_time = Random.Range(3, 7);
        _navMeshAgent.speed = 0;
        yield return new WaitForSeconds(wait_time);
        _navMeshAgent.speed = 5;
        print("I waited for " + wait_time + "sec");
        _IAmWaiting = false;
    }

    public Vector3 RandomNavMeshLocation()
    {
        Vector3 finalPosition = Vector3.zero;
        Vector3 randomPosition = Random.insideUnitSphere * walkRadius;
        randomPosition += transform.position;
        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, walkRadius, 1))
        {
            finalPosition = hit.position;
        }
        return finalPosition;
    }
    private void ChasePlayer()
    {
        _isChasingPlayer = true;
        StartCoroutine(Roar());
        _navMeshAgent.destination = _player.transform.position;

        if (_navMeshAgent.remainingDistance < 1)
        {
            _anim.SetBool("inRangetoAttack", true);
            _hitboxes[0].SetActive(true);
            _hitboxes[1].SetActive(true);

        }
        else
        {
            _hitboxes[0].SetActive(false);
            _hitboxes[1].SetActive(false);
            _anim.SetBool("inRangetoAttack", false);

        }
    }
    private void Damage()
    {
        _currentHp--;
        StartCoroutine(DamageSlowDown());
        canSeePlayer = true;
    }

    private IEnumerator DamageSlowDown()
    {
        _navMeshAgent.speed = 4;
        yield return new WaitForSeconds(0.2f);
        _navMeshAgent.speed = 5;
    }

    private void Die()
    {
        if (_currentHp <= 0)
        {
            _anim.SetTrigger("isDying");
            _navMeshAgent.enabled = false;
            StartCoroutine(DespawnTimer());
        }
    }
    private IEnumerator DespawnTimer()
    {
        yield return new WaitForSeconds(4f);
        Destroy(this.gameObject);
    }
}
