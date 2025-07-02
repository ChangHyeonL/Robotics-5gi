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
        public long timestamp; // Unix Timestamp (ms)로 시간 저장

        public string ToFormattedString()
        {
            // timestamp를 사람이 읽을 수 있는 시간으로 변환
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(timestamp).ToLocalTime(); // 로컬 시간으로 변환

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

                // 실행되지 않는 코드 -> 다른 스레드 사용으로 내용변경이 바로 되지 않음.
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

        // 1. Post 클래스 구조에 맞는 Dictionary 객체를 생성합니다.
        var postData = new Dictionary<string, object>
        {
            { "userName", userId },
            { "content", content },
            { "timestamp", DateTime.Now.ToString() } // 클라이언트 시간이 아닌 Firebase 서버 시간 사용
        };

        Task t = dbRef.Child("PublicData").Push().SetValueAsync(postData).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                print("PrivateData에 게시글 작성 완료.");

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
        string content = privateCommentInput.text; // 저장할 내용은 순수하게 텍스트만

        // 예외 처리
        if (string.IsNullOrEmpty(userId))
        {
            print("에러: 유저가 로그인되어 있지 않습니다.");
            yield break ;
        }
        if (string.IsNullOrEmpty(content))
        {
            print("내용을 입력해주세요.");
            yield break;
        }

        // 1. Post 클래스 구조에 맞는 Dictionary 객체를 생성합니다.
        var postData = new Dictionary<string, object>
        {
            { "userName", userId },
            { "content", content },
            { "timestamp", DateTime.Now.ToString() } // 클라이언트 시간이 아닌 Firebase 서버 시간 사용
        };

        // UTC to MyTime
        DateTime date1 = new DateTime(2006, 3, 21, 2, 0, 0);
        TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        DateTime myTime = TimeZoneInfo.ConvertTimeFromUtc(date1, tz);

        // "PrivateData/{userId}" 경로 아래에 Push()로 고유 키를 생성하고 데이터를 추가합니다.
        // 이것이 Firebase에서 '배열/리스트'를 만드는 표준 방식입니다.
        dbRef.Child("PrivateData").Child(userId).Push().SetValueAsync(postData).ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                print($"PrivateData/{userId}에 새 게시글을 추가했습니다.");
            }
            else
            {
                print($"데이터 쓰기 실패: {task.Exception}");
            }
        });

        // 입력창 비우기
        privateCommentInput.text = "";

        // UI 업데이트 (참고용)
        nameTxt.text = userId;
        dateTimeTxt.text = DateTime.Now.ToString();
    }

}
