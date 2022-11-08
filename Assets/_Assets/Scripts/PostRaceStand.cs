using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PostRaceStand : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI posNumberText;
    [SerializeField] private TextMeshProUGUI posSuffixText;

    public void SetRacerAndRank(MechRacer racer, int currPos, int numTotalRacers)
    {
        //Set racer position
        racer.transform.position = transform.position + new Vector3(0,10,0);
        racer.transform.forward = -transform.forward;

        //Enable racer abovehead nameplate if player
        racer.EnableNameplate();

        //Update number and color on pedastol
        string posSuffix = RaceController.RacePosSuffix(currPos);

        posNumberText.text = currPos.ToString();
        posSuffixText.text = "\n" + posSuffix;

        Color posColor = RaceController.Static_GetColorForPos(currPos, numTotalRacers);
        posNumberText.color = posColor;
        posSuffixText.color = posColor;
    }
}
