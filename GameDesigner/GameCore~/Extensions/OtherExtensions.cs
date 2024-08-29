using Cysharp.Threading.Tasks;
using GameCore;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public static class OtherExtensions
{
    public static Sprite ToSprite(this string self)
    {
        return Global.Resources.LoadAsset<Sprite>(self);
    }

    public static void LoadSprite(this Image self, string imgPath)
    {
        self.sprite = ToSprite(imgPath);
    }

    public static void LoadSpriteUrl(this Image self, string imgUrl)
    {
        _ = LoadSpriteUrlInternal(self, imgUrl);
    }

    private static async UniTaskVoid LoadSpriteUrlInternal(Image self, string imgUrl)
    {
        if (string.IsNullOrEmpty(imgUrl))
            return;
        if (!imgUrl.StartsWith("http"))
            if (!File.Exists(imgUrl))
                return;
        var request = UnityWebRequestTexture.GetTexture(imgUrl);
        await request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            var texture = DownloadHandlerTexture.GetContent(request);
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            self.sprite = sprite;
        }
    }
}
