using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Persist : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        transform.parent = null;
        DontDestroyOnLoad(gameObject);
    }
}
