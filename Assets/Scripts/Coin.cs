using UnityEngine;

// 빙글 돌며 떠 있다가, 플레이어가 닿으면 점수 주고 사라짐
public class Coin : MonoBehaviour
{
    public int value = 25;           // 획득 점수
    public float collectRadius = 1.8f; // 이 거리 안이면 획득
    public float spinSpeed = 180f;   // 회전 연출

    [System.NonSerialized] public Transform player; // 스포너가 넣어줌

    void Update()
    {
        // 반짝이게 회전 (납작한 코인이 뒤집히며 반짝)
        transform.Rotate(Vector3.right, spinSpeed * Time.deltaTime, Space.World);

        if (player == null) return;

        // 평면(XZ) 거리로 획득 판정 (높이 차이 무시)
        Vector3 d = transform.position - player.position;
        d.y = 0f;
        if (d.sqrMagnitude < collectRadius * collectRadius)
        {
            if (GameManager.Instance != null) GameManager.Instance.CollectCoin(); // 콤보 적립
            Destroy(gameObject);
        }
    }
}
