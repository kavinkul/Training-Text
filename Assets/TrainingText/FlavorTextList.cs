using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using KModkit;

public class FlavorTextList {
    private FlavorTextObject[] flavorTexts;
    private int counter;

    public readonly int SIZE = 1;

    public FlavorTextList() {
        flavorTexts = new FlavorTextObject[SIZE];
        counter = 0;
    }

    public void add(FlavorTextObject text) {
        if (counter == flavorTexts.Length)
            ensureCapacity(counter * 2 + 1);

        flavorTexts[counter] = text;
        counter++;
    }

    private void ensureCapacity(int minSize) {
        FlavorTextObject[] biggerList;

        if (flavorTexts.Length < minSize) {
            biggerList = new FlavorTextObject[minSize];
            Array.Copy(flavorTexts, 0, biggerList, 0, counter);
            flavorTexts = biggerList;
        }
    }

    public FlavorTextObject getRandomFlavorText() {
        int rand = UnityEngine.Random.Range(0, counter);
        return flavorTexts[rand];
    }
}