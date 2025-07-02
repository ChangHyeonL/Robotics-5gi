using UnityEngine;

// Lamp Red, Yellow, Green의 신호를 받아오고, 적용한다.
// 속성: 각 램프의 signal들, 각 램프의 Renderer
public class TowerManager : MonoBehaviour
{
    public bool isRedLampOn = false;       // PLC의 신호
    public bool isYellowLampOn = false;     
    public bool isGreenLampOn = false;
    public Renderer lampRed;
    public Renderer lampYellow;
    public Renderer lampGreen;

    private void Start()
    {
        lampRed.material.SetColor("_BaseColor", Color.black);
        lampYellow.material.SetColor("_BaseColor", Color.black);
        lampGreen.material.SetColor("_BaseColor", Color.black);
    }

    private void Update()
    {
        if(isRedLampOn)
        {
            lampRed.material.SetColor("_BaseColor", Color.red);
        }
        else if(!isRedLampOn)
        {
            lampRed.material.SetColor("_BaseColor", Color.black);
        }

        if (isYellowLampOn)
        {
            lampYellow.material.SetColor("_BaseColor", Color.yellow);
        }
        else if(!isYellowLampOn)
        {
            lampYellow.material.SetColor("_BaseColor", Color.black);
        }

        if (isGreenLampOn)
        {
            lampGreen.material.SetColor("_BaseColor", Color.green);
        }
        else if (!isGreenLampOn)
        {
            lampGreen.material.SetColor("_BaseColor", Color.black);
        }
    }



    public void TurnOnRedLamp()
    {
        isRedLampOn = !isRedLampOn;

        if(isRedLampOn)
        {
            lampRed.material.SetColor("_BaseColor", Color.red);
        }
        else
        {
            lampRed.material.SetColor("_BaseColor", Color.black);
        }
    }

    public void TurnOnYellowLamp()
    {
        isYellowLampOn = !isYellowLampOn;

        if (isYellowLampOn)
        {
            lampYellow.material.SetColor("_BaseColor", Color.yellow);
        }
        else
        {
            lampYellow.material.SetColor("_BaseColor", Color.black);
        }
    }

    public void TurnOnGreenLamp()
    {
        isGreenLampOn = !isGreenLampOn;

        if (isGreenLampOn)
        {
            lampGreen.material.SetColor("_BaseColor", Color.green);
        }
        else
        {
            lampGreen.material.SetColor("_BaseColor", Color.black);
        }
    }
}
