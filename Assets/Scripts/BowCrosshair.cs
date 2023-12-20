using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class BowCrosshair : MonoBehaviour
{
    public RectTransform outerCircle;
    public Image outerImage;
    public float transitionSpeed = 1f;
    public float scaleSpeed = 1f;
    public float initialWidth = 100f;
    public float exitScale = 2f;
    [Header("Runtime Values")]
    public float targetAlpha = 0f;
    public float targetOuterScale = 1.5f;
    [SerializeField, ReadOnly] float bowCharge;
    float outerScale = 1f;
    CanvasGroup group;
    // Start is called before the first frame update
    void Start()
    {
        group = this.GetComponent<CanvasGroup>();
    }

    public void Update()
    {
        if (!Application.isPlaying) return;
        targetAlpha = 0f;
        targetOuterScale = group.alpha < 1 ? exitScale : 1;
        if (PlayerActor.player == null || !PlayerActor.player.gameObject.activeInHierarchy)
        {
            targetAlpha = 0f;
            targetOuterScale = -1;
        }
        else if (PlayerActor.player.IsAiming() && PlayerActor.player.camState != PlayerActor.CameraState.Lock)
        {
            targetAlpha = 1f;
            if (PlayerActor.player.inventory.IsRangedDrawn() && PlayerActor.player.inventory.GetRangedWeapon() is RangedBow bow)
            {
                bowCharge = bow.GetBowCharge();
                targetOuterScale = Mathf.Lerp(1, 0, bowCharge);
            }
            else if (!PlayerActor.player.inventory.IsRangedEquipped())
            {
                targetOuterScale = -1;
            }
        }
    }
    private void OnGUI()
    {
        if (outerCircle == null || outerImage == null) return;
        if (group == null) group = this.GetComponent<CanvasGroup>();

        outerImage.enabled = targetOuterScale > 0;
        group.alpha = Mathf.MoveTowards(group.alpha, targetAlpha, transitionSpeed * Time.deltaTime);
        outerScale = Mathf.MoveTowards(outerScale, targetOuterScale, scaleSpeed * Time.deltaTime);
        outerCircle.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, initialWidth * outerScale);
        outerCircle.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, initialWidth * outerScale);
    }
}
