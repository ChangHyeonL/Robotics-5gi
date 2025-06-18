using UnityEngine;
using ActUtlType64Lib;
using System;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class MxComponent : MonoBehaviour
{
    [SerializeField] TMP_Text logTxt;
    ActUtlType64 mxComponent;
    bool isConnected;
    const string inputStartDevice = "X0";
    const int inputBlockCnt = 2;
    const string outputStartDevice = "Y0";
    const int outBlockCnt = 1;

    // Y����̽��� �޴� ����� ����
    public List<Cylinder> cylinders;
    public Conveyor conveyor;
    public TowerManager towerManager;
    WaitForSeconds updateInterval = new WaitForSeconds(0.5f);


    private void Awake()
    {
        mxComponent = new ActUtlType64();

        mxComponent.ActLogicalStationNumber = 1;

        logTxt.text = "Please connect the PLC..";
        logTxt.color = Color.red;
    }

    // Ư�� �ð��� �ѹ��� �ݺ��Ͽ� PLC �����͸� �о�´�.
    IEnumerator UpdatePLCData()
    {
        while(isConnected)
        {
            ReadDeviceBlock(outputStartDevice, outBlockCnt);

            yield return updateInterval;
        }
    }

    private void OnDestroy()
    {
        if(isConnected)
            Close();   
    }

    public void Open()
    {
        int iRet = mxComponent.Open();

        if (iRet == 0)
        {
            isConnected = true;

            StartCoroutine(UpdatePLCData()); // ������ ��� �ҷ�����

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

    /*
    X����̽� ����(13��) -> X0 ���� 2�� ��� ���
    ������ư 1��(X0)
    ������ư 1��
    ���������ư 1��
    ���� LS 2��(X10)
    ���� LS 2��
    ���� LS 2��
    ���� LS 2��
    �������� 1��
    �ݼӼ��� 1��

    Y����̽� ����(13��) -> Y0 ���� 1�� ��� ���
    ���� Syl ����/���� 2��
    ���� Syl ����/���� 2��
    ���� Syl ����/���� 2��
    ���� Syl ����/���� 2��
    �����̾� CW/CCW 2��
    ���� 3��
    */

    public void ReadDeviceBlock(string startDevice, int blockCnt)
    {
        // { 336, 55 } -> 0001/1100/1110/0000
        int[] data = new int[blockCnt];
        int iRet = mxComponent.ReadDeviceBlock(startDevice, blockCnt, out data[0]);

        if(iRet == 0)
        {
            // { 0001110011100000, 0001110011100000 }
            string[] result = ConvertDecimalToBinary(data); // 336 -> 0001/1100/1110/0000


            // ���� ���� data�� ����
            // cylinders[0].isForward = data[0]
            // 1. Input X Device ���� ���


            // 2. output Y Device ���� ���: 1�� ��ϸ� ���
            if(startDevice.Contains("Y"))
            {
                string y = result[0]; // 001110011100000
                bool isActive = y[0] is '1' ? true : false;
                cylinders[0].isForward      = isActive;
                cylinders[0].isBackward     = y[1] is '1' ? true : false;
                cylinders[1].isForward      = y[2] is '1' ? true : false;
                cylinders[1].isBackward     = y[3] is '1' ? true : false;
                cylinders[2].isForward      = y[4] is '1' ? true : false;
                cylinders[2].isBackward     = y[5] is '1' ? true : false;
                cylinders[3].isForward      = y[6] is '1' ? true : false;
                cylinders[3].isBackward     = y[7] is '1' ? true : false;
                conveyor.isCW               = y[8] is '1' ? true : false;
                conveyor.isCCW              = y[9] is '1' ? true : false;
                towerManager.isRedLampOn    = y[10] is '1' ? true : false;
                towerManager.isYellowLampOn = y[11] is '1' ? true : false;
                towerManager.isGreenLampOn  = y[12] is '1' ? true : false;

                print(y);
            }

        }
        else
        {
            ShowError(iRet);
        }
    }

    // { 336, 55 } -> { 0001110011100000, 0001110011100000 }
    private string[] ConvertDecimalToBinary(int[] data)
    {
        string[] result = new string[data.Length];

        for(int i = 0; i < data.Length; i++)
        {
            // 1. 10���� 336 -> 2���� 101010000
            string binary = Convert.ToString(data[i], 2);

            // 2. ���ư� ������Ʈ �߰� 1/0101/0000 -> 0000/0010/1010/0000
            int upBitCnt = 16 - binary.Length;

            // 3. ������(������Ʈ �ε��� ���� ���) 1/0101/0000 -> 0000/1010/1
            string reversedBinary = new string(binary.Reverse().ToArray());

            // 4. ������Ʈ ���̱� 0000/1010/1 + 000/0000 = 0000/1010/1000/0000
            for(int j = 0; j < upBitCnt; j++)
            {
                reversedBinary += "0";
            }

            result[i] = reversedBinary;
        }

        return result;
    }
}
