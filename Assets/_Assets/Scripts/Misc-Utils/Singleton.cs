using UnityEngine;
public class Singleton<T> : MonoBehaviour where T : Component
{
    void Start()
    {
        if ((Instance != null) && (Instance != this))
        {
            Debug.Log("Destroyed script type" + typeof(T) + " on gameObject" + gameObject.name);
            Destroy(gameObject);
        }
    }
    private static T instance = null;

    public static T Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<T>();
            return instance;
        }
    }

}