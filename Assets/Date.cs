using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using TriInspector;
using UnityEngine;

public class Date : MonoBehaviour
{
    public TextMeshProUGUI dateText;
    private int currentDay = -1;

    [Button("Update Date")]
    private void Update() {
        DateTime time = DateTime.Now;
        if (time.Day != currentDay)
        {
            Refresh();
        }
    }

    public void Refresh()
    {
        DateTime time = DateTime.Now;
        currentDay = time.Day;

        string weekFormat = TextController.GetTranslation(PREFS.IsAmericanDate.GetBool() ? "general.americandateformat" : "general.dateformat");
        if (!string.IsNullOrEmpty(weekFormat))
        {
            string dayOfWeek = TextController.GetTranslation("day." + time.DayOfWeek.ToString().ToLowerInvariant());
            string day = time.Day.ToString().PadLeft(2, '0');
            string month = time.Month.ToString().PadLeft(2, '0');

            string finalText = String.Format(weekFormat, dayOfWeek, day, month);
            dateText.text = finalText;
        }
    }

    public static void RefreshAllDates()
    {
        foreach (Date date in FindObjectsOfType(typeof(Date), true))
        {
            date.Refresh();
        }
    }
}
