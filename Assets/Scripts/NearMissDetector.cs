using UnityEngine;

// 드리프트 중에 '나를 추적하는' 미사일이 가까이 스쳐 지나가면 니어미스 (미사일당 1회)
public class NearMissDetector : MonoBehaviour
{
    public float nearMissRadius = 2.0f;  // 이 반경 안으로 들어온 락온 미사일 = 니어미스

    private PlayerController player;

    void Start()
    {
        player = GetComponent<PlayerController>();
    }

    void Update()
    {
        // 드리프트 중일 때만 인정
        if (player == null || !player.IsDrifting) return;

        Collider[] cols = Physics.OverlapSphere(transform.position, nearMissRadius);
        foreach (var c in cols)
        {
            Missile m = c.GetComponent<Missile>();
            if (m != null && m.IsLockedOn && !m.nearMissCounted)
            {
                m.nearMissCounted = true;   // 이 미사일은 다시 카운트 안 됨
                if (GameManager.Instance != null) GameManager.Instance.RegisterNearMiss();
            }
        }
    }
}
