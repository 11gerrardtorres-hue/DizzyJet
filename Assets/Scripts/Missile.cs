using UnityEngine;

public class Missile : MonoBehaviour
{
    [Header("미사일 설정")]
    public float speed = 18f;       // 전진 속도 (플레이어 15보다 빠름)
    public float turnSpeed = 130f;  // 선회 속도 (제한적 → 급선회로 회피 가능)
    public float lifetime = 5f;     // 수명 (초)
    public float hitRadius = 1f;    // 플레이어와 이 거리 안이면 명중

    private Transform target;

    void Start()
    {
        // 씬에서 플레이어를 자동으로 찾음
                PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            target = player.transform;
            Debug.Log("미사일: 플레이어 찾음!");
        }
        else
        {
            Debug.Log("미사일: 플레이어 못 찾음!");
        }

        // 수명이 끝나면 자동 소멸
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (target != null)
        {
            // 플레이어 방향으로 조금씩 회전 (즉시 못 꺾음)
            Vector3 dir = target.position - transform.position;
            dir.y = 0f;
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, turnSpeed * Time.deltaTime);

            // 명중 판정
            float dist = Vector3.Distance(transform.position, target.position);
            if (dist < hitRadius)
            {
                Debug.Log("게임오버! 미사일에 맞음");
                Destroy(gameObject);
                return;
            }
        }

        // 바라보는 방향으로 전진
        transform.position += transform.forward * speed * Time.deltaTime;
    }
}