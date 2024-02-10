using UnityEngine;

public class NetworkServer : MonoBehaviour
{
    private GameService Service;

    // Start is called before the first frame update
    void Start()
    {
        Service = new GameService();
        Service.Start();
    }

    // Update is called once per frame
    void Update()
    {
        Service.SceneUpdate();
    }

    private void OnDestroy()
    {
        Service?.Close();
    }
}
