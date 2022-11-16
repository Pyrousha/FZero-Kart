using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectableCup : MonoBehaviour
{
    [field: SerializeField] public RaceCup RaceCup { get; private set; }
    [Space(5)]
    [SerializeField] private Image trophyImage;
    [SerializeField] private Image cupImage;
    [SerializeField] private Image cupBG;
    [SerializeField] private TextMeshProUGUI cupName;

    // Start is called before the first frame update
    void Start()
    {
        LoadDataFromCup();
    }

    // Update is called once per frame
    void LoadDataFromCup()
    {
        trophyImage.sprite = RaceCup.GetTrophySprite();
        cupName.text = RaceCup.name;
        cupBG.color = RaceCup.BgColor;

        cupImage.sprite = RaceCup.CupSprite;
    }

    public void SetCup()
    {
        SceneTransitioner.Instance.SetCup(RaceCup);
    }
}
