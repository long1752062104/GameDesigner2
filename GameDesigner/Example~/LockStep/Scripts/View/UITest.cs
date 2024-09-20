#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Net.Client;
using Net.Component;
using Net.Share;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UITest : SingleCase<UITest>
{
    public InputField cri;
    public Button cr, jr, er, rb, eb, playback, saveDataBtn;
    public Text text;

    // Start is called before the first frame update
    async void Start()
    {
        cr.onClick.AddListener(() =>
        {
            ClientBase.Instance.Call("CreateRoom", cri.text);
        });
        jr.onClick.AddListener(() =>
        {
            ClientBase.Instance.Call("JoinRoom", cri.text);
        });
        er.onClick.AddListener(() =>
        {
            ClientBase.Instance.Call("ExitRoom");
        });
        rb.onClick.AddListener(() =>
        {
            ClientBase.Instance.Call("StartBattle");
        });
        eb.onClick.AddListener(() =>
        {
            ClientBase.Instance.Call("ExitBattle");
        });
        playback.onClick.AddListener(() =>
        {
            ClientBase.Instance.DispatchRpc("Playback");
        });
        saveDataBtn.onClick.AddListener(() =>
        {
            LockStep.Client.GameWorld.I.SaveData();
        });
        while (ClientBase.Instance == null)
            await Task.Yield();
        while (!ClientBase.Instance.Connected)
            await Task.Yield();
        ClientBase.Instance.AddRpcAuto(this, this);
    }

    private void Update()
    {
        var i = LockStep.Client.GameWorld.I;
        text.text = $"网络帧:{i.frame}/秒 延迟:{i.delay}/秒";
    }

    [Rpc]
    void CreateRoomCallback(string str)
    {
        Debug.Log(str);
    }

    [Rpc]
    void JoinRoomCallback(string str)
    {
        Debug.Log(str);
    }

    [Rpc]
    void ExitRoomCallback(string str)
    {
        Debug.Log(str);
    }

    [Rpc]
    void StartGameSync()
    {
        Debug.Log("开始帧同步!");
    }

    private void OnDestroy()
    {
        ClientBase.Instance?.RemoveRpc(this);
    }
}
#endif