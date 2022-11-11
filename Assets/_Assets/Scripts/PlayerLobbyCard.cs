using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLobbyCard : MonoBehaviour
{
    [SerializeField] private Animator cardAnim;
    [SerializeField] private Animation checkAnim;
    [Space(5)]
    [SerializeField] private TextMeshProUGUI posNumText;
    [SerializeField] private TextMeshProUGUI posNumSuffixText;
    [Space(5)]
    [SerializeField] private Image playerImage;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [Space(5)]
    [SerializeField] private TextMeshProUGUI totalPointsText;
    [Space(5)]
    [SerializeField] private Image bgImage;
    [SerializeField] private Image iconBGImage;

    public bool isReady { get; private set; }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Called when the player this card is linked to readies up.
    /// Plays the check turnign green animation and sets ready to true.
    /// </summary>
    public void ReadyUp()
    {
        checkAnim.enabled = true;
        isReady = true;
    }

    /// <summary>
    /// Called right after this object has been created, sets visuals. 
    /// </summary>
    /// <param name="racer"> Which racer is this card linked to </param>
    /// <param name="currPos"> Overall position rank (set to 0 if no races have been played yet) </param>
    public void SetRacerStats(MechRacer racer, int currPos = 0)
    {
        if (currPos > 0)
            SetVisualsForPosition(currPos);
        else
        {
            posNumText.text = "";
            posNumSuffixText.text = "";
        }

        playerImage.sprite = racer.MechStats.MechIcon;
        playerNameText.text = racer.name;
        playerNameText.color = racer.PlayerNameColor;

        totalPointsText.text = racer.Score.ToString();
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

        bgImage.color = Utils.MultiplySaturation(Utils.MultiplyValue(posColor, RacerScoreDisplay.valueMultiplierBG, 0.9f), RacerScoreDisplay.saturationMultiplierBG);
        iconBGImage.color = Utils.MultiplySaturation(Utils.MultiplyValue(posColor, RacerScoreDisplay.valueMultiplierIconBG, 0.9f), RacerScoreDisplay.saturationMultiplierIconBG);
    }

    /// <summary>
    /// Called when the player disconnects, start the "disconnect" animation
    /// </summary>
    public void OnDisconnect()
    {
        cardAnim.SetTrigger("Disconnect");
    }

    /// <summary>
    /// Called by the animator when it finishes the "disconnect" animation
    /// </summary>
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
