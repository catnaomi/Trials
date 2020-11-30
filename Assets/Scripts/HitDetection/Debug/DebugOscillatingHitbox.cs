using UnityEngine;
using System.Collections;

public class DebugOscillatingHitbox : MonoBehaviour
{

    private HitboxGroup hitboxGroup;
    public DamageKnockback damageKnockback;
    public float mult1;
    public float mult2;
    public float radius = 1f;
    public float toggleDuration = 1f;
    public float length = 10f;
    private float clock;
    private bool act;
    // Use this for initialization
    void Start()
    {
        hitboxGroup = Hitbox.CreateHitboxLine(this.transform.position, this.transform.up, length, radius, this.transform, damageKnockback, this.gameObject);
        clock = 0f;
        act = false;
    }

    // Update is called once per frame
    void Update()
    {
        hitboxGroup.root.transform.position = this.transform.position + Vector3.up + (this.transform.forward * (mult1 * Mathf.Sin(Time.time * mult2)));
        clock += Time.deltaTime;
        if (clock > toggleDuration)
        {
            clock = 0f;
            act = !act;

            hitboxGroup.SetActive(act);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(this.transform.position + Vector3.up, this.transform.up * length);
    }
}
