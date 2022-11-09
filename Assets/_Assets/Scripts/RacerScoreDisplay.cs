using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class RacerScoreDisplay : MonoBehaviour
{
    [SerializeField] private Animation anim;
    [Space(5)]
    [SerializeField] private TextMeshProUGUI posNumText;
    [SerializeField] private TextMeshProUGUI posNumSuffixText;
    [Space(5)]
    [SerializeField] private Image playerImage;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [Space(5)]
    [SerializeField] private TextMeshProUGUI pointsToAddText;
    [SerializeField] private GameObject arrowObj;
    [SerializeField] private TextMeshProUGUI totalPointsText;
    [Space(5)]
    [SerializeField] private Image bgImage;
    [SerializeField] private Image iconBGImage;

    private float pointsAddedPerSec;

    private int lastScore;
    private int startingScore;
    private int newScore;
    private int pointsToAdd;

    static float saturationMultiplierBG = 0.5f;
    static float saturationMultiplierIconBG = 0.75f;
    static float valueMultiplierBG = 1.25f;
    static float valueMultiplierIconBG = 1f;

    private int prevPosNumber; //Overall placement player had before this race started, may not be unique
    private int newPosNumber; //Overall placement player had after this race ended, may not be unique
    private int newPosIndex; //0 to length-1, unique

    public static List<RacerScoreDisplay> scoreDisplays;

    public void SetData(bool showOnlyThisRacePoints, int _prevPosNum, int _newPosNum, int _newPosIndex, MechRacer racer, float _pointsToAddPerSec, int numTotalPlayers = 0)
    {
        startingScore = racer.LastScore;
        lastScore = racer.LastScore;
        newScore = racer.Score;
        pointsToAdd = racer.Score - racer.LastScore;

        prevPosNumber = _prevPosNum;
        newPosIndex = _newPosIndex;
        newPosNumber = _newPosNum;

        pointsAddedPerSec = _pointsToAddPerSec;


        //Stupid way of setting if this is the first race or not
        if (numTotalPlayers > 0)
        {
            //This is the first race, start all players in last
            SetVisualsForPosition(numTotalPlayers);
            _prevPosNum = numTotalPlayers;
        }
        else
            SetVisualsForPosition(_prevPosNum);


        playerImage.sprite = racer.MechStats.MechIcon;
        playerNameText.text = racer.name;
        playerNameText.color = racer.PlayerNameColor;


        if (showOnlyThisRacePoints)
        {
            pointsToAddText.text = "+"+Utils.AddLeadingZeroes(pointsToAdd, 0);
            arrowObj.SetActive(false);
            totalPointsText.text = "";
        }
        else
        {
            pointsToAddText.text = "+"+Utils.AddLeadingZeroes(pointsToAdd, 0);
            arrowObj.SetActive(true);
            totalPointsText.text = Utils.AddLeadingZeroes(lastScore, 0);
        }
    }


    private bool isLerping;

    private float lerpDuration;
    private static float startTime;

    private Vector3 startPos3D;
    private Vector3 endPos3D;

    private int lerpPosNum;

    public void StartLerp(float _duration, bool isPlayer = false)
    {
        isLerping = true;
        lerpDuration = _duration;

        startPos3D = transform.position;
        if (isPlayer)
            endPos3D = transform.position;
        else
            endPos3D = scoreDisplays[newPosIndex].transform.position;

        lerpPosNum = prevPosNumber;

        startTime = Time.time;
    }

    public void FlipShut()
    {
        anim.Play("RacerScoreFlipShut");
    }

    public void FlipOpen()
    {
        anim.Play("RacerScoreFlipOpen");
    }

    void Update()
    {
        if (isLerping == false)
            return;

        float elapsedTime = Time.time - startTime;
        float lerpPercent = elapsedTime / lerpDuration;

        if (lerpPercent >= 1)
        {
            //Lerp is done
            transform.position = endPos3D;
            SetVisualsForPosition(newPosNumber);

            pointsToAddText.text = Utils.AddLeadingZeroes(lastScore, 0);
            totalPointsText.text = Utils.AddLeadingZeroes(newScore, 0);

            isLerping = false;
            return;
        }

        float posFloat = Utils.RemapPercent(lerpPercent, prevPosNumber, newPosNumber);

        int currPosNum = Mathf.RoundToInt(posFloat);
        if (currPosNum != lerpPosNum)
        {
            SetVisualsForPosition(currPosNum);
            lerpPosNum = currPosNum;
        }

        pointsToAddText.text = "+"+Utils.AddLeadingZeroes(Mathf.RoundToInt(Utils.RemapPercent(lerpPercent, pointsToAdd, 0)), 0);
        totalPointsText.text = Utils.AddLeadingZeroes(Mathf.RoundToInt(Utils.RemapPercent(lerpPercent, lastScore, newScore)), 0);

        transform.position = Vector3.Lerp(startPos3D, endPos3D, lerpPercent);
    }

    /// <summary>
    /// Sets number of position (1st, 2nd, etc.) as well as color of number and BG color
    /// </summary>
    private void SetVisualsForPosition(int _posNum)
    {
        posNumText.text = _posNum.ToString();
        posNumSuffixText.text = RaceController.RacePosSuffix(_posNum);
        Color posColor = RaceController.Instance.GetColorForPos(_posNum);
        posNumText.color = posColor;
        posNumSuffixText.color = posColor;

        bgImage.color = Utils.MultiplySaturation(Utils.MultiplyValue(posColor, valueMultiplierBG, 0.9f), saturationMultiplierBG);
        iconBGImage.color = Utils.MultiplySaturation(Utils.MultiplyValue(posColor, valueMultiplierIconBG, 0.9f), saturationMultiplierIconBG);
    }
}
