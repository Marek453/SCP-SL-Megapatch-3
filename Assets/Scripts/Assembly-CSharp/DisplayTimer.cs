using UnityEngine.UI;
using UnityEngine;
using System;

public class DisplayTimer : MonoBehaviour
{
    public Door door;
    string text = "emergency door unlock in ";
    void Update()
    {
        if(door.curCooldown < 0)
        {
            GetComponent<Text>().text = "<SIZE=250><COLOR=#44e364>DOOR UNLOCKED</COLOR></size>";
            return;
        }
        // THX CHAT GPT
        TimeSpan timeSinceRoundStart = TimeSpan.FromSeconds(door.curCooldown);
        string timeString = $"{(int)timeSinceRoundStart.TotalMinutes:00}:{timeSinceRoundStart.Seconds:00}";
        GetComponent<Text>().text =  text.ToUpper() + "<size=300>"+timeString+"</size>";
    }
}
