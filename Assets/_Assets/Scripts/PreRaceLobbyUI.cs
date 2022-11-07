using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreRaceLobbyUI : MonoBehaviour
{
    public void StartCup(int cupIndex)
    {
        SceneTransitioner.Instance.StartCup(cupIndex);
    }
}
