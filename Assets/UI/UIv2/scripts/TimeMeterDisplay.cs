using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeMeterDisplay : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Animator animator;
    public Image meter;
    public Image cooldown;
    public Image spent;
    [Header("Values")]
    public float meterValue;
    public float meterMax;
    public float meterUpdateSpeed = 100f;
    public float cooldownValue;
    public float cooldownMax;
    public float fadeoutTime = 5f;
    public float fadeoutDelay = 2f;
    public float fadeinTime = 0.5f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float spentValue;
    public float spentUpdateSpeed = 1f;
    public float spentUpdateDelay = 2f;
    float spentClock;
    float lastMeter;
    float fadeT = 0f;
    float fadeClock;
    TimeTravelController timeController;
    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    public void Init()
    {
        timeController = TimeTravelController.time;
        if (timeController != null)
        {
            meterValue = timeController.meter.current;
            meterMax = timeController.meter.max;
            cooldownValue = timeController.timePowerClock;
            cooldownMax = timeController.timePowerCooldown;
            timeController.OnMeterFail.AddListener(MeterFail);
            timeController.OnCooldownFail.AddListener(CooldownFail);
            timeController.OnCooldownComplete.AddListener(CooldownComplete);
        }
    }
    // Update is called once per frame
    void Update()
    {
        bool cooldownMaxed = cooldownValue >= cooldownMax;
        if (timeController != null)
        {
            meterValue = Mathf.MoveTowards(meterValue, timeController.meter.current, meterUpdateSpeed * Time.deltaTime);
            cooldownValue = cooldownMax - timeController.timePowerClock;          
        }
        bool meterDecreasedLastFrame = meterValue < lastMeter;
        lastMeter = meterValue;
        if (!meterDecreasedLastFrame)
        {
            if (spentClock < spentUpdateDelay)
            {
                spentClock += Time.deltaTime;
            }
        }
        else
        {
            spentClock = 0f;
        }
        if (spentValue > meterValue)
        {
            if (!meterDecreasedLastFrame && spentClock >= spentUpdateDelay)
            {
                spentValue = Mathf.MoveTowards(spentValue, meterValue, spentUpdateSpeed * Time.deltaTime);
            }
            spent.gameObject.SetActive(true);
        }
        else
        {
            spentValue = meterValue;
            spent.gameObject.SetActive(false);
        }
        if (meterMax != 0f && cooldownMax != 0f)
        {
            meter.fillAmount = Mathf.Clamp01(meterValue / meterMax);
            cooldown.fillAmount = Mathf.Clamp01(cooldownValue / cooldownMax);
            spent.fillAmount = Mathf.Clamp01(spentValue / meterMax);
        }

        bool isMaxed = cooldownValue >= cooldownMax && meterValue >= meterMax;
        if (isMaxed)
        {
            if (fadeClock < 10f)
            {
                fadeClock += Time.deltaTime;
            }
            if (fadeClock >= fadeoutDelay)
            {
                fadeT = Mathf.MoveTowards(fadeT, 0f, Time.deltaTime / fadeoutTime);
            }
        }
        else
        {
            fadeClock = 0f;
            fadeT = Mathf.MoveTowards(fadeT, 1f, Time.deltaTime / fadeinTime);
        }

        canvasGroup.alpha = fadeCurve.Evaluate(fadeT);
    }

    public void CooldownComplete()
    {
        animator.SetTrigger("CooldownComplete");
    }
    public void CooldownFail()
    {
        animator.SetTrigger("CooldownFail");
    }
    public void MeterFail()
    {
        animator.SetTrigger("MeterFail");
    }
}
