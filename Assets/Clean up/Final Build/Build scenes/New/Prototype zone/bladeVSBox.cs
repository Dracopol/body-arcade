using UnityEngine;

public class bladeVSBox : MonoBehaviour
{
    [SerializeField] private GameObject bladeRight;
    [SerializeField] private GameObject bladeLeft;
    [SerializeField] private GameObject boxLeft;
    [SerializeField] private GameObject boxRight;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
            switchToBox();

        if (Input.GetKeyDown(KeyCode.Alpha4))
            switchToBlade();
    }
    
    private void switchToBox()
    {
        bladeRight.SetActive(false);
        bladeLeft.SetActive(false);
        boxLeft.SetActive(true);
        boxRight.SetActive(true);
    }

    private void switchToBlade()
    {
        bladeRight.SetActive(true);
        bladeLeft.SetActive(true);
        boxLeft.SetActive(false);
        boxRight.SetActive(false);
    }
}
