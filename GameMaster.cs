using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class GameMaster : MonoBehaviour
{
    [Header("UI Text")]
    public TMP_Text balanceText;
    public TMP_Text winningsText;
    public TMP_Text betText;
    private double balance, totalWinnings, bet;
    public GameObject gameOverScreen;

    [Header("Chest Arrays")]
    public TMP_Text[] chestText = new TMP_Text[9];
    public GameObject[] chests = new GameObject[9];
    private double[] winningsArray = new double[9];
    private double winnings;
    private int chestCounter;
    private bool playing;

    [Header("Buttons")]
    public GameObject playButton;
    public GameObject addBetButton;
    public GameObject minusBetButon;

    [Header("Audio")]
    private AudioSource audio;
    public AudioClip winPick, losePick;

    [Header("Tweening Animations")]
    public float chestSpeed;
    private RectTransform[] chestTransforms = new RectTransform[9];
    private Vector2[] startPositions = new Vector2[9];
    public Vector2[] endPositions = new Vector2[9];


    void Start()
    {
        //Start Balance at $10
        balance = 10.00;
        updateText(balance);

        //Start Winnings at $0
        winnings = 0.00;
        updateText(winnings, 'w');

        //Set denomination to $0.25
        bet = 0.25;
        betText.text = "$ 0.25";

        //Enable all chest objects, disable all chest text objects
        for(int i = 0; i < 9; i++)
        {
            chests[i].SetActive(true);
            chestText[i].text = "";
            chestTransforms[i] = chests[i].GetComponent<RectTransform>();
            startPositions[i] = new Vector2(chestTransforms[i].anchoredPosition.x, chestTransforms[i].anchoredPosition.y);
        }

        //Set playing to false;
        playing = false;

        //Get audiosource component
        audio = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!playing)
        {
            if (bet > balance) playButton.SetActive(false);
            else playButton.SetActive(true);

            if (bet == 0.25) minusBetButon.SetActive(false);
            else minusBetButon.SetActive(true);

            if (bet == 5.0) addBetButton.SetActive(false);
            else addBetButton.SetActive(true);

            if (balance < 0.25) gameOverScreen.SetActive(true);
            else gameOverScreen.SetActive(false);
        }
    }

    public void playGame()
    {
        chestCounter = 0;
        int percent = Random.Range(1, 100);
        int winMult, winningChests;

        balance -= bet;
        updateText(balance);

        totalWinnings = 0.0;
        updateText(totalWinnings, 'w');

        playButton.SetActive(false);
        addBetButton.SetActive(false);
        minusBetButon.SetActive(false);

        playing = true;
        chestCounter = 0;

        //Tween all chests back, disable all chest text objects
        for (int i = 0; i < 9; i++)
        {
            //chests[i].SetActive(true);
            chestTransforms[i].DOAnchorPos(startPositions[i], chestSpeed);
            chestText[i].text = "";
        }
        

        // Calculate the win multiplier
        if (percent <= 50)
        {
            winMult = 0;
        }
        else if (50 < percent && percent <= 80)
        {
            winMult = Random.Range(1, 10);
        }
        else if (80 < percent && percent <= 95)
        {
            int[] winOptions = new int[6] { 12, 16, 24, 32, 48, 64 };
            int winIndex = Random.Range(0, 5);
            winMult = winOptions[winIndex];
        }
        else
        {
            int[] winOptions = new int[5] { 100, 200, 300, 400, 500 };
            int winIndex = Random.Range(0, 4);
            winMult = winOptions[winIndex];
        }

        winnings = bet * winMult;
        winningChests = Random.Range(1, 8);
        print("Player should win $ " + string.Format("{0:F2}", winnings) + " this round");

        //Fill up winnings array (used in the pick chest function)
        for (int i = 0; i < 9; i++)
        {
            if (i < winningChests)
            {
                double win;
                //Ensure winning amount is never less than $0.05
                if (winnings < 0.10) win = winnings;
                else
                {
                    win = winnings / 2;
                    System.Math.Round(win, 2, System.MidpointRounding.AwayFromZero);
                }
                winningsArray[i] = win;
                winnings -= win;
            }
            else if (i == winningChests) winningsArray[i] = winnings;
            else winningsArray[i] = 0;
        }
        print("Play function done");
    }

    private void updateText(double newBalance, char type = 'b')
    {
        string balanceString = string.Format("{0:F2}", newBalance);
        if (type == 'b') balanceText.text = "$ " + balanceString;
        else winningsText.text = "$ " + balanceString;
    }

    public void addBet()
    {
        if (bet == 0.25)
        {
            bet = 0.50;
            betText.text = "$ 0.50";
        }
        else if(bet == .50)
        {
            bet = 1.0;
            betText.text = "$ 1.00";
        }
        else if (bet == 1.0)
        {
            bet = 5.0;
            betText.text = "$ 5.00";
        }
        
    }

    public void subBet()
    {
        if (bet == 0.50)
        {
            bet = 0.25;
            betText.text = "$ 0.25";
        }
        else if (bet == 1.0)
        {
            bet = 0.5;
            betText.text = "$ 0.50";
        }
        else if (bet == 5.0)
        {
            bet = 1.0;
            betText.text = "$ 1.00";
        }
    }

    public void chestClick(int index)
    {
        if (!playing) return;
        //chests[index].SetActive(false);
        chestTransforms[index].DOAnchorPos(endPositions[index], chestSpeed);
        if (winningsArray[chestCounter] != 0)
        {
            chestText[index].text = "$ " + string.Format("{0:F2}", winningsArray[chestCounter]);
            totalWinnings += winningsArray[chestCounter];
            updateText(totalWinnings, 'w');
            audio.PlayOneShot(winPick);
        }
        else
        {
            chestText[index].text = "Lose";
            playing = false;
            balance += totalWinnings;
            updateText(balance);
            playButton.SetActive(true);
            addBetButton.SetActive(true);
            minusBetButon.SetActive(true);
            audio.PlayOneShot(losePick);
        }
        chestCounter++;
    }

    public void addFunds()
    {
        balance = 10.00;
        updateText(balance);
    }

    public void quit()
    {
        Application.Quit();
    }
}
