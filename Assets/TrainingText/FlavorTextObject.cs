using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using KModkit;

public class FlavorTextObject {
    private string moduleName;
    private int year;
    private int month;
    private int day;
    private string flavorText;

    private bool hasQuotes = false;
    private bool isMonday = false;
    private bool startsDP = false;

    private readonly char[] validLetters = { 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P' };

    public FlavorTextObject() {
        moduleName = "Wires";
        year = 2015;
        month = 10;
        day = 8;
        flavorText = "Cut all the things. No, wait, cut only what is needed.";
    }

    public FlavorTextObject(string moduleName, int year, int month, int day, string flavorText) {
        this.moduleName = moduleName;
        this.year = year;
        this.month = month;
        this.day = day;
        this.flavorText = flavorText;

        if (flavorText.Contains("\""))
            hasQuotes = true;

        isMonday = checkMonday(this.year, this.month, this.day);

        startsDP = checkStartsDP(this.moduleName);
    }


    public string getModuleName() {
        return moduleName;
    }

    public int getYear() {
        return year;
    }

    public int getMonth() {
        return month;
    }

    public int getDay() {
        return day;
    }

    public string getFlavorText() {
        return flavorText;
    }

    public bool getHasQuotes() {
        return hasQuotes;
    }

    public bool getIsMonday() {
        return isMonday;
    }

    public bool getStartsDP() {
        return startsDP;
    }


    // Determines if the module name starts with D-p
    public bool checkStartsDP(string txt) {
        // Removes the word "needy"
        if (txt.Length > 6 && txt.Substring(0, 6) == "Needy ")
            txt = txt.Substring(6, txt.Length - 6);

        for (int i = 0; i < validLetters.Length; i++) {
            if (txt.First() == validLetters[i])
                return true;
        }

        return false;
    }

    // Determines if the module was released on a Monday
    public bool checkMonday(int y, int m, int d) {
        int totalDays = 0;
        int yearsAhead = y - 2000;

        totalDays = yearsAhead * 365 + (int) Math.Floor((yearsAhead - 1) * 0.25 + 1);

        switch(m) {
        case 1: totalDays += 0; break;
        case 2: totalDays += 31; break;
        case 3: totalDays += 59; break;
        case 4: totalDays += 90; break;
        case 5: totalDays += 120; break;
        case 6: totalDays += 151; break;
        case 7: totalDays += 181; break;
        case 8: totalDays += 212; break;
        case 9: totalDays += 243; break;
        case 10: totalDays += 273; break;
        case 11: totalDays += 304; break;
        case 12: totalDays += 334; break;
        default: totalDays += 0; break;
        }

        if (m > 2 && yearsAhead % 4 == 0)
            totalDays++;

        totalDays += d;

        if ((totalDays + 5) % 7 == 1)
            return true;

        return false;
    }
}