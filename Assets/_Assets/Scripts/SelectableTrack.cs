using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectableTrack : MonoBehaviour
{
    [field: SerializeField] public RaceCourse Track { get; private set; }
    [Space(5)]
    [SerializeField] private Image trackImage;
    [SerializeField] private Image trackBG;
    [SerializeField] private TextMeshProUGUI trackName;

    // Start is called before the first frame update
    void Start()
    {
        LoadDataFromTrack();
    }

    // Update is called once per frame
    void LoadDataFromTrack()
    {
        trackName.text = Track.name;

        ColorBlock cb = trackBG.GetComponent<Button>().colors;
        cb.selectedColor = Track.SelectedColor;
        cb.normalColor = Track.BgColor;
        trackBG.GetComponent<Button>().colors = cb;

        trackImage.sprite = Track.RaceImage;
    }

    /// <summary>
    /// Called when the local player selects a track, sets that information on the SceneTransitioner
    /// </summary>
    public void SetTrack()
    {
        SceneTransitioner.Instance.SetLocalTrack(Track);
    }
}
