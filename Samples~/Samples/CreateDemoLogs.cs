using UnityEngine;


public class CreateDemoLogs : MonoBehaviour
{
    public string IdentifyerText;
    
    
    private void Start()
    {
        Debug.LogError("This is an error log");
        
        Debug.LogFormat("The material gets switched on the gameObject {0}", IdentifyerText);
    }


    private void Update()
    {
        Debug.LogFormat(this, "This is a log with a context of the name of the GameObject where it came from: {0}", this.gameObject.name);
    }


    [ContextMenu(nameof(MakeWarningLog))]
    private void MakeWarningLog()
    {
        Debug.LogWarning("This is a warning log");
    }


    private void OnDisable()
    {
        Debug.Log("This is a log when the object is disabled");
    }
}