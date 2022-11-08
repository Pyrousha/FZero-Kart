using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RacerScoreDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI posNumText;
    [SerializeField] private TextMeshProUGUI posNumSuffixText;
    [Space(5)]
    [SerializeField] private Image playerImage;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [Space(5)]
    [SerializeField] private TextMeshProUGUI pointsToAddText;
    [SerializeField] private TextMeshProUGUI totalPointsText;
    [Space(5)]
    [SerializeField] private Image bgImage;
    [SerializeField] private Image iconBGImage;

    private float pointsAddedPerSec;

    private int startingScore;
    private int currScore;
    private int pointsToAdd;

    static float saturationMultiplierBG = 0.5f;
    static float saturationMultiplierIconBG = 0.75f;
    static float valueMultiplierBG = 1.25f;
    static float valueMultiplierIconBG = 1f;

    public void SetData(int _posNum, MechRacer racer, float _pointsToAddPerSec, bool skipCounting = false)
    {
        startingScore = racer.LastScore;
        currScore = racer.LastScore;
        pointsToAdd = racer.Score -  racer.LastScore;


        posNumText.text = _posNum.ToString();
        posNumSuffixText.text = RaceController.RacePosSuffix(_posNum);
        Color posColor = RaceController.Instance.GetColorForPos(_posNum);
        posNumText.color = posColor;
        posNumSuffixText.color = posColor;

        bgImage.color = Utils.MultiplySaturation(Utils.MultiplyValue(posColor, valueMultiplierBG, 0.9f), saturationMultiplierBG);
        iconBGImage.color = Utils.MultiplySaturation(Utils.MultiplyValue(posColor, valueMultiplierIconBG, 0.9f), saturationMultiplierIconBG);


        playerImage.sprite = racer.MechStats.MechIcon;
        playerNameText.text = racer.name;
        playerNameText.color = racer.PlayerNameColor;

        pointsToAddText.text = Utils.AddLeadingZeroes(pointsToAdd, 0);
        totalPointsText.text = Utils.AddLeadingZeroes(currScore, 0);

        pointsAddedPerSec = _pointsToAddPerSec;

        if(skipCounting)
        {
            pointsToAddText.text = Utils.AddLeadingZeroes(racer.LastScore, 0);
            totalPointsText.text = Utils.AddLeadingZeroes(racer.Score, 0);
        }
    }

    public IEnumerator FillUpScore()
    {
        yield return new WaitForSeconds(0.5f);

        while (pointsToAdd > 0)
        {
            currScore++;
            pointsToAdd--;

            pointsToAddText.text = Utils.AddLeadingZeroes(pointsToAdd, 0);
            totalPointsText.text = Utils.AddLeadingZeroes(currScore, 0);
            yield return new WaitForSeconds(1 / pointsAddedPerSec);
        }

        // yield return new WaitForSeconds(0.5f);

        // pointsToAddText.text = Utils.AddLeadingZeroes(startingScore, 0);
    }
}
