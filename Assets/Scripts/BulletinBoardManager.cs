using UnityEngine;
using Firebase;
using Firebase.Database;
using TMPro;
using System;
using System.Collections;
using System.Threading.Tasks;
using Google.MiniJSON;
using System.Collections.Generic;

public class BulletinBoardManager : MonoBehaviour
{
    [Serializable]
    public class Post
    {
        public string userName;
        public string content;
        public long timestamp; // Unix Timestamp (ms)�� �ð� ����

        public string ToFormattedString()
        {
            // timestamp�� ����� ���� �� �ִ� �ð����� ��ȯ
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(timestamp).ToLocalTime(); // ���� �ð����� ��ȯ

            return $"[{userName}] ({dateTime:yyyy-MM-dd HH:mm})\n{content}\n" +
                   "--------------------------------\n";
        }
    }

    public string dbUrl;
    public TMP_InputField publicCommentInput;
    public TMP_Text publicContentTxt;
    public TMP_InputField privateCommentInput;
    public TMP_Text privateContentTxt;
    public TMP_InputField nameTxt;
    public TMP_InputField dateTimeTxt;
    DatabaseReference dbRef;
    public string tempPublicStr;
    public string tempPrivateStr;

    private void Awake()
    {
        FirebaseApp.DefaultInstance.Options.DatabaseUrl = new System.Uri(dbUrl);

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke("LoadData", 1);
    }

    void LoadData()
    {
        StartCoroutine(ReadPublicData());

        StartCoroutine(ReadPrivateData(FirebaseAuthManager.instance.user.UserId));
    }

    public IEnumerator ReadPublicData()
    {
        string content = "";

        Task task = dbRef.Child("PublicData").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapShot = task.Result;

                content = snapShot.GetRawJsonValue();

                print(content);

                // ������� �ʴ� �ڵ� -> �ٸ� ������ ������� ���뺯���� �ٷ� ���� ����.
                // publicTxt.text = content;
            }
        });

        yield return new WaitUntil(() => task.IsCompleted && content != "");

        publicContentTxt.text = content;
        tempPublicStr = content;

    }

    public IEnumerator ReadPrivateData(string uID)
    {
        string content = "";

        Task t = dbRef.Child("PrivateData").Child(uID).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapShot = task.Result;

                content = snapShot.GetRawJsonValue();

                print(content);
            }
        });

        yield return new WaitUntil(() => t.IsCompleted && content != "");

        privateContentTxt.text = content;
        tempPrivateStr = content;
    }

    public void OnPublicDataWriteBtnClkEvent()
    {
        StartCoroutine(UpdatePublicData());
    }

    IEnumerator UpdatePublicData()
    {
        yield return WritePublicData();

        yield return ReadPublicData();
    }

    IEnumerator WritePublicData()
    {
        string userId = FirebaseAuthManager.instance.user.UserId;
        string content = publicContentTxt.text;

        // 1. Post Ŭ���� ������ �´� Dictionary ��ü�� �����մϴ�.
        var postData = new Dictionary<string, object>
        {
            { "userName", userId },
            { "content", content },
            { "timestamp", DateTime.Now.ToString() } // Ŭ���̾�Ʈ �ð��� �ƴ� Firebase ���� �ð� ���
        };

        Task t = dbRef.Child("PublicData").Push().SetValueAsync(postData).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                print("PrivateData�� �Խñ� �ۼ� �Ϸ�.");

                tempPublicStr = content;
            }
        });

        yield return null;
    }

    public void OnPrivateDataWriteBtnClkEvent()
    {
        StartCoroutine(UpdatePrivateData());
    }

    IEnumerator UpdatePrivateData()
    {
        yield return WritePrivateData();

        yield return ReadPrivateData(FirebaseAuthManager.instance.user.UserId);
    }

    IEnumerator WritePrivateData()
    {
        string userId = FirebaseAuthManager.instance.user.UserId;
        string content = privateCommentInput.text; // ������ ������ �����ϰ� �ؽ�Ʈ��

        // ���� ó��
        if (string.IsNullOrEmpty(userId))
        {
            print("����: ������ �α��εǾ� ���� �ʽ��ϴ�.");
            yield break ;
        }
        if (string.IsNullOrEmpty(content))
        {
            print("������ �Է����ּ���.");
            yield break;
        }

        // 1. Post Ŭ���� ������ �´� Dictionary ��ü�� �����մϴ�.
        var postData = new Dictionary<string, object>
        {
            { "userName", userId },
            { "content", content },
            { "timestamp", DateTime.Now.ToString() } // Ŭ���̾�Ʈ �ð��� �ƴ� Firebase ���� �ð� ���
        };

        // UTC to MyTime
        DateTime date1 = new DateTime(2006, 3, 21, 2, 0, 0);
        TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        DateTime myTime = TimeZoneInfo.ConvertTimeFromUtc(date1, tz);

        // "PrivateData/{userId}" ��� �Ʒ��� Push()�� ���� Ű�� �����ϰ� �����͸� �߰��մϴ�.
        // �̰��� Firebase���� '�迭/����Ʈ'�� ����� ǥ�� ����Դϴ�.
        dbRef.Child("PrivateData").Child(userId).Push().SetValueAsync(postData).ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                print($"PrivateData/{userId}�� �� �Խñ��� �߰��߽��ϴ�.");
            }
            else
            {
                print($"������ ���� ����: {task.Exception}");
            }
        });

        // �Է�â ����
        privateCommentInput.text = "";

        // UI ������Ʈ (�����)
        nameTxt.text = userId;
        dateTimeTxt.text = DateTime.Now.ToString();
    }

}
