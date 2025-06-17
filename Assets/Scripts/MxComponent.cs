using UnityEngine;
using ActUtlType64Lib;
using System;

public class MxComponent : MonoBehaviour
{
    ActUtlType64 mxComponent;
    bool isConnected;

    private void Awake()
    {
        mxComponent = new ActUtlType64();

        mxComponent.ActLogicalStationNumber = 1;
    }

    public void Open()
    {
        int iRet = mxComponent.Open();

        if (iRet == 0)
        {
            isConnected = true;

            print("�� ������ �Ǿ����ϴ�.");
        }
        else
        {
            // �����ڵ� ��ȯ(16����)
            print(Convert.ToString(iRet, 16));
        }
    }

    public void Close()
    {
        if (!isConnected)
        {
            print("�̹� �������� �����Դϴ�.");

            return;
        }

        int iRet = mxComponent.Close();

        if (iRet == 0)
        {
            isConnected = false;

            print("�� ������ �Ǿ����ϴ�.");
        }
        else
        {
            // �����ڵ� ��ȯ(16����)
            print(Convert.ToString(iRet, 16));
        }
    }
}
