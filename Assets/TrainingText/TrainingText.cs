using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using KModkit;

public class TrainingText : MonoBehaviour {
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;

    public KMSelectable SubmitButton;
    public KMSelectable[] TimeButtons;
    
    public Text FlavorText;
    public TextMesh ClockText;
    public TextMesh AnswerText;

    public TextMesh[] ClockNumbers;
    public Renderer[] ClockTickmarks;
    public Color[] ClockColors;
    public Material[] ClockMaterials;

    // Logging info
    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved = false;

    // Solving info
    private int lastSerialDigit;
    private int batteryCount;
    private bool hasSerialPort = false;
    private bool hasEmptyPlate = false;

    private FlavorTextList flavorTexts;
    private FlavorTextObject module = new FlavorTextObject();

    private bool displayingModuleName = false;

    private int strikes;
    private double bombTime;
    private int roundedBombTime;

    private int correctHour;
    private int correctMinute;
    private string correctState;

    private int currentHour;
    private int currentMinute;
    private string currentState;

    private int realHour;
    private int realMinute;
    private int realSecond;

    private int finishingHour;
    private int finishingMinute;
    private int finishingSecond;

    private int actualCorrectTime;
    private int actualCurrentTime;

    // Ran as bomb loads
    private void Awake() {
        moduleId = moduleIdCounter++;

		SubmitButton.OnInteract += delegate () { SubmitButtonPressed(); return false; };

        for (int i = 0; i < TimeButtons.Length; i++) {
            int j = i;
            TimeButtons[i].OnInteract += delegate () { TimeButtonPressed(j); return false; };
        }
	}

    // Gets information
    private void Start() {
        // Gets edgework and day of the week
        lastSerialDigit = Bomb.GetSerialNumberNumbers().Last();
        batteryCount = Bomb.GetBatteryCount();

        if (Bomb.GetPortCount(Port.Serial) > 0)
            hasSerialPort = true;

        foreach (object[] plate in Bomb.GetPortPlates()) {
            if (plate.Length == 0) {
                hasEmptyPlate = true;
                break;
            }
        }


        // Gets the list of flavor texts and chooses a random text
        flavorTexts = AllFlavorTexts.AddFlavorTexts();
        module = flavorTexts.getRandomFlavorText();

        // Formats the flavor text for logging
        string modifiedFlavorText = module.getFlavorText();
        modifiedFlavorText = modifiedFlavorText.Replace('\n', ' ');

        Debug.LogFormat("[Training Text #{0}] The module selected was {1}.", moduleId, module.getModuleName());
        Debug.LogFormat("[Training Text #{0}] The flavor text is: {1}", moduleId, modifiedFlavorText);
        Debug.LogFormat("[Training Text #{0}] The module was released on {1}/{2}/{3}.", moduleId, module.getMonth(), module.getDay(), module.getYear());


        // Sets a random time on the clock
        currentHour = UnityEngine.Random.Range(1, 13);
        currentMinute = UnityEngine.Random.Range(0, 60);

        int rand = UnityEngine.Random.Range(0, 2);
        if (rand == 0)
            currentState = "AM";

        else
            currentState = "PM";

        CalculateCorrectTime();
        DisplayCurrentTime();

        FlavorText.text = module.getFlavorText();
    }


    // Calculates correct time
    private void CalculateCorrectTime() {
        correctHour = module.getMonth() % 12;
        correctMinute = module.getDay();

        if (lastSerialDigit % 2 == 0)
            correctState = "PM";

        else
            correctState = "AM";

        correctHour = correctHour % 12;
        Debug.LogFormat("[Training Text #{0}] The unmodified time is {1}:{2} {3}", moduleId, correctHour, FormatMinutes(correctMinute), correctState);
        if (correctHour == 12)
            correctHour = 0;


        // Modifies the time
        bool rulesApplied = false;

        if (module.getYear() < 2017) {
            correctMinute += 45;
            rulesApplied = true;
            Debug.LogFormat("[Training Text #{0}] The module was released before 2017. (+45 minutes)", moduleId);
        }

        if (module.getHasQuotes() == true) {
            correctMinute += 20;
            rulesApplied = true;
            Debug.LogFormat("[Training Text #{0}] The module's flavor text has quotation marks. (+20 minutes)", moduleId);
        }

        if (module.getStartsDP() == true) {
            correctMinute -= 30;
            rulesApplied = true;
            Debug.LogFormat("[Training Text #{0}] The module's name starts with a letter between D and P. (-30 minutes)", moduleId);
        }

        if (module.getIsMonday() == true) {
            correctHour -= 5;
            rulesApplied = true;
            Debug.LogFormat("[Training Text #{0}] The module was released on a Monday. (-5 hours)", moduleId);
        }

        if (Bomb.GetSolvableModuleNames().Count(x => x.Contains("Training Text")) > 1) {
            correctHour++;
            rulesApplied = true;
            Debug.LogFormat("[Training Text #{0}] There is another Training Text module on the bomb. (+1 hour)", moduleId);
        }

        if (hasSerialPort == true) {
            correctMinute += 5;
            rulesApplied = true;
            Debug.LogFormat("[Training Text #{0}] The bomb has a serial port. (+5 minutes)", moduleId);
        }

        if (hasEmptyPlate == true) {
            correctMinute -= 90;
            rulesApplied = true;
            Debug.LogFormat("[Training Text #{0}] The bomb has an empty port plate. (-90 minutes)", moduleId);
        }

        if (batteryCount == 0) {
            correctMinute -= 10;
            rulesApplied = true;
            Debug.LogFormat("[Training Text #{0}] The bomb has no batteries. (-10 minutes)", moduleId);
        }

        if (rulesApplied == false)
            Debug.LogFormat("[Training Text #{0}] No rules from Step 3 applied.", moduleId);


        // Catches rollovers in the time
        while (correctMinute >= 60) {
            correctMinute -= 60;
            correctHour++;
        }

        while (correctMinute < 0) {
            correctMinute += 60;
            correctHour--;
        }

        while (correctHour >= 12) {
            correctHour -= 12;
            if (correctState == "PM")
                correctState = "AM";

            else
                correctState = "PM";
        }

        while (correctHour < 0) {
            correctHour += 12;
            if (correctState == "PM")
                correctState = "AM";

            else
                correctState = "PM";
        }

        if (correctHour == 0)
            correctHour = 12;


        // If the specified module is on the bomb
        if (Bomb.GetSolvableModuleNames().Count(x => x.Contains(module.getModuleName())) > 0) {
            Debug.LogFormat("[Training Text #{0}] The time after Step 3 is {1}:{2} {3}", moduleId, correctHour, FormatMinutes(correctMinute), correctState);
            Debug.LogFormat("[Training Text #{0}] The module selected is present on the bomb.", moduleId);

            correctHour = (correctHour + 6) % 12;
            correctMinute = (correctHour + 30) % 60;

            if (correctState == "PM")
                correctState = "AM";

            else
                correctState = "PM";

            if (correctHour == 0)
                correctHour = 12;
        }

        Debug.LogFormat("[Training Text #{0}] The correct time to submit is {1}:{2} {3}.", moduleId, correctHour, FormatMinutes(correctMinute), correctState);
    }


    // Displays time and highlights tickmarks
    private void DisplayCurrentTime() {
        ClockText.text = currentHour.ToString() + ":" + FormatMinutes(currentMinute) + " " + currentState;

        ClockNumbers[currentHour % 12].color = ClockColors[1];

        if (currentHour == 12) {
            ClockNumbers[11].color = ClockColors[0];
            ClockNumbers[1].color = ClockColors[0];
        }

        else {
            ClockNumbers[(currentHour - 1) % 12].color = ClockColors[0];
            ClockNumbers[(currentHour + 1) % 12].color = ClockColors[0];
        }

        ClockTickmarks[currentMinute].material = ClockMaterials[1];

        if (currentMinute == 0) {
            ClockTickmarks[59].material = ClockMaterials[0];
            ClockTickmarks[1].material = ClockMaterials[0];
        }

        else {
            ClockTickmarks[(currentMinute - 1) % 60].material = ClockMaterials[0];
            ClockTickmarks[(currentMinute + 1) % 60].material = ClockMaterials[0];
        }
    }

    // Formats the minutes on the clock
    private string FormatMinutes(int mins) {
        if (mins >= 10)
            return mins.ToString();

        else
            return "0" + mins.ToString();
    }


    // Submit button pressed
    private void SubmitButtonPressed() {
        SubmitButton.AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
        bool correct = false;

        if (moduleSolved == false) {
            Debug.LogFormat("[Training Text #{0}] You submitted {1}:{2} {3}.", moduleId, currentHour, FormatMinutes(currentMinute), currentState);

            // If the correct time can be reached within the bomb's remaining time
            if (DateTime.Now.DayOfWeek.ToString() != "Friday" && ZenModeActive == false) {
                // Gets the real time left on the bomb's timer
                bombTime = Math.Floor(Bomb.GetTime());
                strikes = Bomb.GetStrikes();

                // Modifies the bomb time with strikes
                switch (strikes) {
                case 0: break;
                case 1: bombTime /= 1.25; break;
                case 2: bombTime /= 1.5; break;
                case 3: bombTime /= 1.75; break;
                default: bombTime /= 2; break;
                }

                roundedBombTime = (int) Math.Floor(bombTime);

                // Gets the current real time
                realHour = DateTime.Now.Hour;
                realMinute = DateTime.Now.Minute;
                realSecond = DateTime.Now.Second;

                if (realHour == 0)
                    Debug.LogFormat("[Training Text #{0}] The current local time when pressing the button was 12:{1} AM.", moduleId, FormatMinutes(realMinute));

                else if (realHour == 12)
                    Debug.LogFormat("[Training Text #{0}] The current local time when pressing the button was 12:{1} PM.", moduleId, FormatMinutes(realMinute));

                else if (realHour > 12)
                    Debug.LogFormat("[Training Text #{0}] The current local time when pressing the button was {1}:{2} PM.", moduleId, realHour - 12, FormatMinutes(realMinute));
                
                else
                    Debug.LogFormat("[Training Text #{0}] The current local time when pressing the button was {1}:{2} AM.", moduleId, realHour, FormatMinutes(realMinute));

                // Adds the bomb's timer to the real time to get the finishing time
                finishingHour = realHour;
                finishingMinute = realMinute;
                finishingSecond = realSecond;

                finishingSecond += roundedBombTime;

                while (finishingSecond >= 60) {
                    finishingSecond -= 60;
                    finishingMinute++;
                }

                while (finishingMinute >= 60) {
                    finishingMinute -= 60;
                    finishingHour++;
                }

                while (finishingHour >= 24)
                    finishingHour -= 24;

                // Determines if the answer time can be reached within the bomb's time
                bool timeReached = false;

                int modifiedCorrectHour = correctHour % 12;
                int modifiedFinishingHour = finishingHour;

                if (correctState == "PM")
                    modifiedCorrectHour += 12;

                if (modifiedCorrectHour < realHour)
                    modifiedCorrectHour += 24;

                if (modifiedFinishingHour < realHour)
                    modifiedFinishingHour += 24;


                if (modifiedFinishingHour % 24 == 0)
                    Debug.LogFormat("[Training Text #{0}] The bomb will finish at 12:{1} AM.", moduleId, FormatMinutes(finishingMinute));

                else if (modifiedFinishingHour % 24 == 12)
                    Debug.LogFormat("[Training Text #{0}] The bomb will finish at 12:{1} PM.", moduleId, FormatMinutes(finishingMinute), FormatMinutes(finishingSecond));

                else if (modifiedFinishingHour % 24 > 12)
                    Debug.LogFormat("[Training Text #{0}] The bomb will finish at {1}:{2} PM.", moduleId, (modifiedFinishingHour - 12) % 24, FormatMinutes(finishingMinute));

                else
                    Debug.LogFormat("[Training Text #{0}] The bomb will finish at {1}:{2} AM.", moduleId, modifiedFinishingHour % 24, FormatMinutes(finishingMinute));


                if (realHour < modifiedCorrectHour && modifiedCorrectHour < modifiedFinishingHour)
                    timeReached = true;

                else if (realHour == modifiedFinishingHour && realMinute < correctMinute && correctMinute <= finishingMinute)
                    timeReached = true;

                else if (modifiedCorrectHour == modifiedFinishingHour && realHour != modifiedFinishingHour && correctMinute <= finishingMinute)
                    timeReached = true;

                else if (modifiedCorrectHour == realHour && realHour != modifiedFinishingHour && correctMinute > realMinute)
                    timeReached = true;


                // Time is not reached
                if (timeReached == false) {
                    if (currentHour == correctHour && currentMinute == correctMinute && currentState == correctState)
                        correct = true;
                }
                
                // Time is reached
                else {
                    Debug.LogFormat("[Training Text #{0}] Note that the correct answer time could be reached in the bomb's remaining time when the button was pressed.", moduleId);
                    actualCorrectTime = finishingMinute + finishingHour * 60;
                    actualCurrentTime = currentMinute + currentHour * 60;

                    if (currentHour == 12 && currentState == "AM")
                        actualCurrentTime -= (12 * 60);

                    if (currentHour != 12 && currentState == "PM")
                        actualCurrentTime += (12 * 60);

                    if (actualCorrectTime < 23 * 60 + 55 && actualCurrentTime > actualCorrectTime && actualCurrentTime <= actualCorrectTime + 5)
                        correct = true;

                    else if (actualCurrentTime < 5 && actualCurrentTime + (24 * 60) > actualCorrectTime && actualCurrentTime + (24 * 60) <= actualCorrectTime + 5)
                        correct = true;

                    else if (actualCurrentTime > actualCorrectTime && actualCurrentTime <= actualCorrectTime + 5)
                        correct = true;
                }
            }


            else if (currentHour == correctHour && currentMinute == correctMinute && currentState == correctState)
                correct = true;

            if (DateTime.Now.DayOfWeek.ToString() == "Friday") {
                Debug.LogFormat("[Training Text #{0}] Note that it was Friday when the button was pressed.", moduleId);
            }


            // Correct answer
            if (correct == true) {
                Debug.LogFormat("[Training Text #{0}] Module solved!", moduleId);
                GetComponent<KMBombModule>().HandlePass();
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, gameObject.transform);
                moduleSolved = true;
            }

            // Incorrect answer
            else {
                Debug.LogFormat("[Training Text #{0}] Strike!", moduleId);
                GetComponent<KMBombModule>().HandleStrike();
            }
        }

        // Displays the module name on the top screen
        if (displayingModuleName == false) {
            displayingModuleName = true;

            if (correct == true)
                AnswerText.text = module.getModuleName();

            else
                StartCoroutine(ModuleNameFlash());
        }
    }

    // Displays the module name upon striking
    private IEnumerator ModuleNameFlash() {
        yield return new WaitForSeconds(0.6f);
        Audio.PlaySoundAtTransform("TrainingText_TextShow", transform);
        yield return new WaitForSeconds(0.04f);
        AnswerText.text = module.getModuleName();
        yield return new WaitForSeconds(0.25f);
        AnswerText.text = "";
        yield return new WaitForSeconds(0.15f);
        AnswerText.text = module.getModuleName();
        yield return new WaitForSeconds(0.25f);
        AnswerText.text = "";
        yield return new WaitForSeconds(0.15f);
        AnswerText.text = module.getModuleName();
    }

    // Time button pressed
    private void TimeButtonPressed(int i) {
        TimeButtons[i].AddInteractionPunch(0.25f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, gameObject.transform);
        if (moduleSolved == false) {
            /* 0 = Hour Up
             * 1 = Hour Down
             * 2 = Minute Up
             * 3 = Minute Down
             */

            currentHour = currentHour % 12;

            switch (i) {
            case 0: currentHour++; break;
            case 1: currentHour--; break;
            case 2: currentMinute++; break;
            case 3: currentMinute--; break;
            }

            while (currentMinute >= 60) {
                currentMinute -= 60;
                currentHour++;
            }

            while (currentMinute < 0) {
                currentMinute += 60;
                currentHour--;
            }

            while (currentHour >= 12) {
                currentHour -= 12;
                if (currentState == "PM")
                    currentState = "AM";

                else
                    currentState = "PM";
            }

            while (currentHour < 0) {
                currentHour += 12;
                if (currentState == "PM")
                    currentState = "AM";

                else
                    currentState = "PM";
            }

            if (currentHour == 0)
                currentHour = 12;

            DisplayCurrentTime();
        }
    }

#pragma warning disable 414
    private bool ZenModeActive;
#pragma warning restore 414
}