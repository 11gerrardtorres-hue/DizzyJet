using UnityEngine;

public class InfiniteGround : MonoBehaviour
{
    [Header("따라갈 대상")]
    public Transform target;          // Player

    [Header("격자무늬 설정")]
    public Color colorA = new Color(0.20f, 0.22f, 0.26f);
    public Color colorB = new Color(0.24f, 0.26f, 0.30f);
    public float tileWorldSize = 5f;  // 격자 한 칸의 실제 크기

    private Material mat;
    private float planeWorldSize;     // 바닥 한 변의 실제 길이
    private float tiling;

    void Start()
    {
        // 기본 Plane은 10x10 유닛 → Scale을 곱해 실제 크기 계산
        planeWorldSize = 10f * transform.localScale.x;
        tiling = planeWorldSize / tileWorldSize;

        // 체커보드 텍스처를 코드로 생성 (이미지 파일 불필요)
        Texture2D tex = new Texture2D(2, 2);
        tex.SetPixel(0, 0, colorA);
        tex.SetPixel(1, 1, colorA);
        tex.SetPixel(1, 0, colorB);
        tex.SetPixel(0, 1, colorB);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.Apply();

        // 바닥 머티리얼에 텍스처 적용
        mat = GetComponent<Renderer>().material;
        mat.mainTexture = tex;
        mat.mainTextureScale = new Vector2(tiling, tiling);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1) 바닥을 항상 플레이어 위치로 이동 (Y는 그대로)
        Vector3 pos = target.position;
        pos.y = transform.position.y;
        transform.position = pos;

        // 2) 격자무늬는 월드에 고정된 것처럼 흐르게 (무한 착시)
        Vector2 offset = -new Vector2(target.position.x, target.position.z) / planeWorldSize * tiling;
        mat.mainTextureOffset = offset;
    }
}