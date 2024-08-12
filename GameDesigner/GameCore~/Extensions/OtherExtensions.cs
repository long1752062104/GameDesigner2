using GameCore;
using UnityEngine;

public static class OtherExtensions
{
    public static Sprite ToSprite(this string self)
    {
        return Global.Resources.LoadAsset<Sprite>(self);
    }
}
