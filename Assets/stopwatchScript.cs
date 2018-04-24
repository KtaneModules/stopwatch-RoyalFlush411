using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using stopwatch;

public class stopwatchScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable startButton;
    public Renderer brassCentre;
    public TextMesh startTimeDisplay;

    //Hand movement
    Coroutine tickCoroutine;
    Coroutine tickSoundCoroutine;
    float totalElapsedTime = 0.0f;
    float tickElapsedTime = 0.0f;
    bool clockOn = false;
    string startTime;
    private int correctWaitTime;
    bool buttonLock = false;
    private int numberOfTicks = 0;
    bool resetComplete = true;

    //Text colors
    public Color green;
    public Color red;

    //Two numbers in serial
    int twoNumbersFirst;
    int twoNumbersSecond;

    //Modulos
    int firstModulo;
    int secondModulo;

    //Misc
    float bombStartTimeF = 0;
    int bombStartTime = 0;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        startButton.OnInteract += delegate () { OnstartButton(); return false; };
    }

    void Start()
    {
        bombStartTimeF = Bomb.GetTime();
        bombStartTime = Mathf.FloorToInt(bombStartTimeF);
        stopwatchLogic();
        if (bombStartTime <= 300 && bombStartTime > 60 && correctWaitTime > 30)
        {
            correctWaitTime = correctWaitTime / 10;
            Debug.LogFormat("[The Stopwatch #{0}] You have five minutes or less on your bomb clock. Your wait time has been divided by 10. The stopwatch must run for {1} seconds.", moduleId, correctWaitTime);
        }
        else if (bombStartTime <= 60 && correctWaitTime > 30)
        {
            correctWaitTime = correctWaitTime / 20;
            Debug.LogFormat("[The Stopwatch #{0}] You have one minute or less on your bomb clock. Your wait time has been divided by 20. The stopwatch must run for {1} seconds.", moduleId, correctWaitTime);
        }
    }

    void stopwatchLogic()
    {
        int[] serialNumbers = Bomb.GetSerialNumberNumbers().ToArray();

        if (serialNumbers.Length == 2)
        {
            int twoNumbersA = serialNumbers[0] * 10 + serialNumbers[1];
            int twoNumbersB = serialNumbers[1] * 10 + serialNumbers[0];
            int twoNumbersSubtraction = serialNumbers[0] * serialNumbers[1];
            int twoNumbersAFinal = twoNumbersA - twoNumbersSubtraction;
            int twoNumbersBFinal = twoNumbersB - twoNumbersSubtraction;

            if (twoNumbersAFinal <= twoNumbersBFinal)
            {
                twoNumbersFirst = twoNumbersAFinal;
                twoNumbersSecond = twoNumbersBFinal;
            }
            else
            {
                twoNumbersFirst = twoNumbersBFinal;
                twoNumbersSecond = twoNumbersAFinal;
            }
            Debug.LogFormat("[The Stopwatch #{0}] You have two digits in your serial number. The equations are {1} - {2} = {3} & {4} - {2} = {5}.", moduleId, twoNumbersA, twoNumbersSubtraction, twoNumbersAFinal, twoNumbersB, twoNumbersBFinal);
            Debug.LogFormat("[The Stopwatch #{0}] The first number is {1}. The second number is {2}.", moduleId, twoNumbersFirst, twoNumbersSecond);

            firstModulo = twoNumbersFirst % 3;
            secondModulo = twoNumbersSecond % 4;

            Debug.LogFormat("[The Stopwatch #{0}] The column reference is {1}. The row reference is {2}.", moduleId, firstModulo, secondModulo);

            tableOneCalculation();
        }

        else if (serialNumbers.Length == 3)
        {
            int batteries = Bomb.GetBatteryCount();
            int threeNumbersXY = serialNumbers[0] * serialNumbers[1];
            int threeNumbersZYmX = serialNumbers[2] * serialNumbers[1] - serialNumbers[0];
            Debug.LogFormat("[The Stopwatch #{0}] You have three digits in your serial number. The value of XY is {1}. The value of ZY-X is {2}.", moduleId, threeNumbersXY, threeNumbersZYmX);

            if (threeNumbersXY > threeNumbersZYmX)
            {
                serialNumbers[0] += batteries;
                serialNumbers[1] += batteries;
                serialNumbers[2] += batteries;
                Debug.LogFormat("[The Stopwatch #{0}] XY > ZY-X. The values of X, Y & Z have been increased by {1}.", moduleId, batteries);
            }

            int[] evenSerials = serialNumbers.Where((x) => x % 2 == 0).ToArray();
            int[] oddSerials = serialNumbers.Where((x) => x % 2 ==1).ToArray();

            if (evenSerials.Count() == 2)
            {
                firstModulo = evenSerials[0];
                secondModulo = evenSerials[1];
            }
            else if (oddSerials.Count() == 2)
            {
                int[] largestTwo = serialNumbers.OrderByDescending((x) => x).Take(2).ToArray();
                int[] largestTwoCorrect = largestTwo.OrderBy((x) => Array.IndexOf(serialNumbers, x)).ToArray();
                firstModulo = largestTwoCorrect[0];
                secondModulo = largestTwoCorrect[1];
            }
            else if (evenSerials.Count() == 3)
            {
                firstModulo = serialNumbers[1];
                secondModulo = serialNumbers[2];
            }
            else
            {
                serialNumbers[1] += 2;
                Debug.LogFormat("[The Stopwatch #{0}] There are three odd numbers. The value of Y has been increased by 2.", moduleId, batteries);
                int[] largestTwo = serialNumbers.OrderByDescending((x) => x).Take(2).ToArray();
                int[] largestTwoCorrect = largestTwo.OrderBy((x) => Array.IndexOf(serialNumbers, x)).ToArray();
                firstModulo = largestTwoCorrect[0];
                secondModulo = largestTwoCorrect[1];
            }
            Debug.LogFormat("[The Stopwatch #{0}] The value of X is {1}. The value of Y is {2}. The value of Z is {3}.", moduleId, serialNumbers[0], serialNumbers[1], serialNumbers[2]);
            Debug.LogFormat("[The Stopwatch #{0}] The first number is {1}. The second number is {2}.", moduleId, firstModulo, secondModulo);

            tableTwoCalculation();
        }

        else if (serialNumbers.Length == 4)
        {
            Debug.LogFormat("[The Stopwatch #{0}] You have four digits in your serial number.", moduleId);

            if (serialNumbers[1] == 0)
            {
                serialNumbers[1] = 1;
            }
            if (serialNumbers[3] == 0)
            {
                serialNumbers[3] = 1;
            }

            if (serialNumbers[0] % serialNumbers[1] == 0)
            {
                firstModulo = (serialNumbers[0] / serialNumbers[1]) % 4;
                Debug.LogFormat("[The Stopwatch #{0}] The first two digits will divide, giving the answer {1}.", moduleId, serialNumbers[0] / serialNumbers[1]);
            }
            else
            {
                firstModulo = (serialNumbers[0] + serialNumbers[1]) % 4;
                Debug.LogFormat("[The Stopwatch #{0}] The first two digits will not divide, therefore add to make {1}.", moduleId, serialNumbers[0] + serialNumbers[1]);
            }
            if (serialNumbers[2] % serialNumbers[3] == 0)
            {
                secondModulo = (serialNumbers[2] / serialNumbers[3]) % 4;
                Debug.LogFormat("[The Stopwatch #{0}] The second two digits will divide, giving the answer {1}.", moduleId, serialNumbers[2] / serialNumbers[3]);
            }
            else
            {
                secondModulo = (serialNumbers[2] + serialNumbers[3]) % 4;
                Debug.LogFormat("[The Stopwatch #{0}] The second two digits will not divide, therefore add to make {1}.", moduleId, serialNumbers[2] + serialNumbers[3]);
            }
            Debug.LogFormat("[The Stopwatch #{0}] The column reference is {1}. The row reference is {2}.", moduleId, firstModulo, secondModulo);

            tableOneCalculation();
        }
    }

    void tableOneCalculation()
    {
        if (firstModulo == 0)
        {
            if (secondModulo == 0)
            {
                correctWaitTime = 260;
            }
            else if (secondModulo == 1)
            {
                correctWaitTime = 73;
            }
            else if (secondModulo == 2)
            {
                correctWaitTime = 116;
            }
            else if (secondModulo == 3)
            {
                correctWaitTime = 269;
            }
        }
        else if (firstModulo == 1)
        {
            if (secondModulo == 0)
            {
                correctWaitTime = 66;
            }
            else if (secondModulo == 1)
            {
                correctWaitTime = 194;
            }
            else if (secondModulo == 2)
            {
                correctWaitTime = 158;
            }
            else if (secondModulo == 3)
            {
                correctWaitTime = 204;
            }
        }
        else if (firstModulo == 2)
        {
            if (secondModulo == 0)
            {
                correctWaitTime = 164;
            }
            else if (secondModulo == 1)
            {
                correctWaitTime = 99;
            }
            else if (secondModulo == 2)
            {
                correctWaitTime = 240;
            }
            else if (secondModulo == 3)
            {
                correctWaitTime = 121;
            }
        }
        else if (firstModulo == 3)
        {
            if (secondModulo == 0)
            {
                correctWaitTime = 152;
            }
            else if (secondModulo == 1)
            {
                correctWaitTime = 202;
            }
            else if (secondModulo == 2)
            {
                correctWaitTime = 195;
            }
            else if (secondModulo == 3)
            {
                correctWaitTime = 1;
            }
        }
        Debug.LogFormat("[The Stopwatch #{0}] The stopwatch must run for {1} seconds.", moduleId, correctWaitTime);
    }

    void tableTwoCalculation()
    {
        if (firstModulo % 2 == 0)
        {
            if (secondModulo % 2 == 0)
            {
                correctWaitTime = 220;
            }
            else
            {
                correctWaitTime = 252;
            }
        }
        else
        {
            if (secondModulo % 2 == 0)
            {
                correctWaitTime = 155;
            }
            else
            {
                correctWaitTime = 87;
            }
        }
        Debug.LogFormat("[The Stopwatch #{0}] The stopwatch must run for {1} seconds.", moduleId, correctWaitTime);
    }

    private IEnumerator Tick()
    {
        while(clockOn)
        {
            yield return null;
            totalElapsedTime += Time.deltaTime;
            tickElapsedTime += Time.deltaTime;
            while (tickElapsedTime >= 0.2f)
            {
                brassCentre.transform.Rotate(Vector3.up, 1.2f);
                tickElapsedTime -= 0.2f;
                numberOfTicks += 1;
            }
        }
    }

    private IEnumerator TickSound()
    {
        while(clockOn)
        {
            yield return new WaitForSeconds(0.2f);
            Audio.PlaySoundAtTransform("tick", transform);
        }
    }

    private IEnumerator ClockReset()
    {
        resetComplete = false;
        yield return new WaitForSeconds(0.5f);
        clockOn = true;
        while (numberOfTicks > 0)
        {
            yield return new WaitForSeconds(0.001f);
            brassCentre.transform.Rotate(Vector3.up, -12f);
            numberOfTicks -= 10;
            if (numberOfTicks < 10)
            {
                resetComplete = true;
                clockOn = false;
                Vector3 angle = brassCentre.transform.localEulerAngles;
                angle.y = 0;
                brassCentre.transform.localEulerAngles = angle;
            }
        }
        yield return new WaitUntil(() => resetComplete == true);
        numberOfTicks = 0;
        totalElapsedTime = 0.0f;
        tickElapsedTime = 0.0f;
        buttonLock = false;

    }

    public void OnstartButton()
    {
        Audio.PlaySoundAtTransform("click", transform);

        if (buttonLock == true)
        {
            return;
        }

        if (clockOn == false)
        {
            startTime = Bomb.GetFormattedTime();
            startTimeDisplay.text = startTime;
            startTimeDisplay.color = green;
            clockOn = true;
            tickCoroutine = StartCoroutine(Tick());
            tickSoundCoroutine = StartCoroutine(TickSound());
        }
        else if (clockOn)
        {
            startTime = Bomb.GetFormattedTime();
            startTimeDisplay.text = startTime;
            startTimeDisplay.color = red;
            clockOn = false;
            StopCoroutine(Tick());
            StopCoroutine(TickSound());
            Debug.LogFormat("[The Stopwatch #{0}] Total elapsed time: {1} seconds.", moduleId, Mathf.FloorToInt(totalElapsedTime));
            if (Mathf.FloorToInt(totalElapsedTime) == correctWaitTime)
            {
                GetComponent<KMBombModule>().HandlePass();
                Debug.LogFormat("[The Stopwatch #{0}] You have run the stopwatch for the correct amount of time ({1} seconds). Module disarmed.", moduleId, Mathf.FloorToInt(totalElapsedTime));
                buttonLock = true;
            }
            else
            {
                GetComponent<KMBombModule>().HandleStrike();
                Debug.LogFormat("[The Stopwatch #{0}] Strike! You have run the stopwatch for {1} seconds. It should have run for {2} seconds.", moduleId, Mathf.FloorToInt(totalElapsedTime), correctWaitTime);
                buttonLock = true;
                StartCoroutine(ClockReset());
            }
        }
    }

    #pragma warning disable 414
    private string TwitchHelpMessage = "Start the stopwatch using !{0} start. Get the current number of seconds using !{0} time. Stop at a specific number of seconds using !{0} stop at <seconds>.";
    #pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] split = command.ToLowerInvariant().Replace("stop at ", "stop ").Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

		if (split.Length == 1 && (split[0] == "go" || split[0] == "start") && !clockOn)
		{
			yield return null;
			startButton.OnInteract();
			yield return new WaitForSeconds(0.1f);
		}
		else if (clockOn)
		{
			if (split.Length == 1 && (split[0] == "time" || split[0] == "seconds"))
			{
				int seconds = Mathf.FloorToInt(totalElapsedTime) % 60;
				yield return string.Format("sendtochat There is currently {0} second{1} on the stopwatch.", seconds, seconds == 1 ? "" : "s");
			}
			else if (split.Length == 2 && split[0] == "stop")
			{
				int seconds;
				if (int.TryParse(split[1], out seconds))
				{
					yield return null;
					while (Mathf.FloorToInt(totalElapsedTime) % 60 != seconds) yield return "trycancel Stopwatch wasn't stopped due to request to cancel.";

					startButton.OnInteract();
					yield return new WaitForSeconds(0.1f);
				}
			}
		}
	}
}
