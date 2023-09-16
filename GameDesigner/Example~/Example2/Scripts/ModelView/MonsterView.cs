using UnityEngine;

public class MonsterView : MonoBehaviour
{
    public Monster Self;
    public Animation anim;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Self.Agent == null)
            return;
        Self.Agent.OnUpdate(Time.deltaTime);
        var agentTransform = Self.Agent.transform;
        transform.SetPositionAndRotation(agentTransform.Position, agentTransform.Rotation);
        if (Self.Agent.RemainingDistance < 0.1f)
            anim.Play("idle");
        else
            anim.Play("walk");
    }

    private void OnDrawGizmos()
    {
        if (Self.Agent == null)
            return;
        Self.Agent.OnDrawGizmos((begin, end) => Gizmos.DrawLine(begin, end));
    }
}
