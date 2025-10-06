using UnityEngine;

public class EditorOnlyCanvas : MonoBehaviour
{
    private void Awake()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            //destroy canvas on Android platforms
            Destroy(gameObject);
        }
    }
}