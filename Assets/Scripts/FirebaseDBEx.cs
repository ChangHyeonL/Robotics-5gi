using UnityEngine;
using Firebase.Database;
using Firebase;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

// Firebase DB�� ����, �����͸� �а� ����.
public class FirebaseDBEx : MonoBehaviour
{
    [Serializable]
    public class Factory
    {
        public List<Robot> robots = new List<Robot>();
    }

    [Serializable]
    public class Robot
    {
        public string name;
        public int id;
        public string serialNum;
        public string managerName;
        public float cycleTime;

        // Json.Net ������ UnityEngine.Vector3 ���Ұ�
        //public List<Vector3> steps = new List<Vector3>();
        public List<Step> steps = new List<Step>();
    }

    [Serializable]
    public struct Step
    {
        public float posX;
        public float posY;
        public float posZ;
        public float rotX;
        public float rotY;
        public float rotZ;
        public float duration;
        public bool isGripperOn;
    }


    [SerializeField] string dbURL;
    public Factory factory;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Firebase URL ����
        FirebaseApp.DefaultInstance.Options.DatabaseUrl = new System.Uri(dbURL);

        // DB���� ���� ���� ����
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        // 1. DB�� ������ �б� ��û
        GetDBData(dbRef);

        // 2. DB�� ������ ���� ��û
        string json = @"
{
    ""address"":""���α�"",
    ""id"":12345
}";
        //dbRef.SetValueAsync(json);          // ���ڿ� ��ü�� �ֱ�
        //dbRef.SetRawJsonValueAsync(json).ContinueWith(task =>
        //{
        //    if (task.IsCanceled)
        //    {
        //        print("������ ���� ���");
        //    }
        //    else if (task.IsFaulted)
        //    {
        //        print("������ ���� ����");
        //    }
        //    else if (task.IsCompleted)
        //    {
        //        print("������ ���� �Ϸ�");
        //    }
        //});   // json ���� �������� �ֱ�

        dbRef.Child("Books").GetValueAsync().ContinueWith(task =>
        {

        });

        // ��ü(object) -> Json ���� ����
        SetDBRobotData(dbRef);

        GetDBRobotData(dbRef);
    }

    // Firebase DB RootReference�� �����͸� ������ȭ -> ��ü�� ��ȯ
    private void GetDBRobotData(DatabaseReference dbRef)
    {
        dbRef.GetValueAsync().ContinueWith(task =>
        {
            if(task.IsCanceled)
            {

            }
            else if(task.IsFaulted)
            {

            }
            else if(task.IsCompleted)
            {
                DataSnapshot snapShot = task.Result;

                string json = snapShot.GetRawJsonValue();

                factory = JsonConvert.DeserializeObject<Factory>(json);
            }
        });
    }

    private static void SetDBRobotData(DatabaseReference dbRef)
    {
        Robot robotA = new Robot
        {
            name = "Cobot1",
            id = 0,
            serialNum = "123456",
            managerName = "���¿�",
            cycleTime = 0
        };

        Step step0 = new Step()
        {
            posX = 1,
            posY = 2,
            posZ = 3,
            rotX = 50,
            rotY = 23,
            rotZ = 66,
            duration = 3,
            isGripperOn = true,
        };
        robotA.steps.Add(step0);
        robotA.steps.Add(step0);
        robotA.steps.Add(step0);

        Factory factory = new Factory();
        factory.robots.Add(robotA);
        factory.robots.Add(robotA);
        factory.robots.Add(robotA);

        // JsonUtility: 1���� ���̾��� ���������� ���� Ŭ���� ���� ��ȯ����
        // ����) Robot Ŭ���� json���� ��ȯ ����, Factory Ŭ������ ��ȯ �Ұ�


        // ��ü(object) -> Json ���� ����
        //string jsonRobot = JsonUtility.ToJson(factory);

        // json.Net Ŭ����(����ȭ: Object -> Json)
        string jsonRobot = JsonConvert.SerializeObject(factory);
        dbRef.SetRawJsonValueAsync(jsonRobot).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                print("Factory Data ���ε� �Ϸ�");
            }
        });
    }

    private static void GetDBData(DatabaseReference dbRef)
    {
        dbRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapShot = task.Result;

                print(snapShot.GetRawJsonValue());

                foreach (var item in snapShot.Children)
                {
                    string json = item.GetRawJsonValue();

                    //print($"{item.Key}: {json}");
                    print($"{item.Key}: {item.Value}");
                }
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
