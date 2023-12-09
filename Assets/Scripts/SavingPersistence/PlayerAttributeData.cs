using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerAttributeData
{
    public AttributeValue health;
    public AttributeValue timeCharges;
    public int lives;
    public int maxLives;
    public PlayerAttributeData()
    {
        health = new AttributeValue();
        timeCharges = new AttributeValue();
        lives = 0;
        maxLives = 0;
    }

    public PlayerAttributeData(PlayerAttributeData data)
    {
        health = new AttributeValue()
        {
            current = data.health.current,
            max = data.health.max,
            baseValue = data.health.baseValue,
        };

        timeCharges = new AttributeValue()
        {
            current = data.timeCharges.current,
            max = data.timeCharges.max,
            baseValue = data.timeCharges.baseValue,
        };

        lives = data.lives;
        maxLives = data.maxLives;
    }
    public void GetAttributeData(TimeTravelController time, ActorAttributes playerAttributes)
    {
        health.Copy(playerAttributes.health);
        lives = playerAttributes.lives;
        maxLives = playerAttributes.maxLives;
        timeCharges.Copy(time.charges);
    }


    public string ToJSON()
    {
        return JsonUtility.ToJson(this);
    }

    public static PlayerAttributeData FromJSON(string json)
    {
        return JsonUtility.FromJson<PlayerAttributeData>(json);
    }

    public void LoadDataToAttributes(ActorAttributes attributes)
    {
        attributes.health.Copy(this.health);
        attributes.lives = this.lives;
        attributes.maxLives = this.maxLives;
    }

    public void LoadDataToTimeController(TimeTravelController time)
    {
        time.charges.Copy(this.timeCharges);
    }

    public static PlayerAttributeData GetDefault()
    {
        PlayerAttributeData data = new PlayerAttributeData();
        data.health.Set(9);
        data.timeCharges.Set(1);
        data.lives = 1;
        data.maxLives = 1;
        return data;

    }
}
