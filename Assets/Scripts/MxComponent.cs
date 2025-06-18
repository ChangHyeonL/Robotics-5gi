using UnityEngine;
using ActUtlType64Lib;
using System;
using TMPro;

public class MxComponent : MonoBehaviour
{
    [SerializeField] TMP_Text logTxt;
    ActUtlType64 mxComponent;
    bool isConnected;

    private void Awake()
    {
        mxComponent = new ActUtlType64();

        mxComponent.ActLogicalStationNumber = 1;

        logTxt.text = "Please connect the PLC..";
        logTxt.color = Color.red;
    }

    public void Open()
    {
        int iRet = mxComponent.Open();

        if (iRet == 0)
        {
            isConnected = true;

            logTxt.text = "PLC is connected!";
            logTxt.color = Color.green;
            print("�� ������ �Ǿ����ϴ�.");
        }
        else
        {
            // �����ڵ� ��ȯ(16����)
            ShowError(iRet);
            print(Convert.ToString(iRet, 16));
        }
    }

    private void ShowError(int iRet)
    {
        logTxt.text = "Error: " + Convert.ToString(iRet, 16);
        logTxt.color = Color.red;
    }

    public void Close()
    {
        if (!isConnected)
        {
            logTxt.text = "PLC is already disconnected.";
            logTxt.color = Color.red;
            print("�̹� �������� �����Դϴ�.");

            return;
        }

        int iRet = mxComponent.Close();

        if (iRet == 0)
        {
            isConnected = false;

            logTxt.text = "PLC is disconnected completely.";
            logTxt.color = Color.red;
            print("�� ������ �Ǿ����ϴ�.");
        }
        else
        {
            // �����ڵ� ��ȯ(16����)
            ShowError(iRet);
            print(Convert.ToString(iRet, 16));
        }
    }
}
