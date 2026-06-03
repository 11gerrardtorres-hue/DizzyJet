using UnityEngine;

// 판이 끝나도 유지되는 영구 코인 지갑 (나중에 상점에서 사용)
public static class Wallet
{
    const string KEY = "Coins";

    // 현재 보유 코인
    public static int Coins => PlayerPrefs.GetInt(KEY, 0);

    // 코인 적립
    public static void Add(int amount)
    {
        PlayerPrefs.SetInt(KEY, Coins + amount);
        PlayerPrefs.Save();
    }

    // 충분하면 차감하고 true, 아니면 false (상점 구매용)
    public static bool TrySpend(int amount)
    {
        if (Coins < amount) return false;
        PlayerPrefs.SetInt(KEY, Coins - amount);
        PlayerPrefs.Save();
        return true;
    }
}
