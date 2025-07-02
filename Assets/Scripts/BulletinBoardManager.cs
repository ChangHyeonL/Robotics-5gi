using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using TMPro;

public class BulletinBoardManager : MonoBehaviour
{
    // 버튼
    public Button privateWriteButton;   // 개인 게시글 작성 Button
    public Button publicWriteButton;    // 공용 게시글 작성 Button

    // InputField (TMP)
    public TMP_InputField nameInputField;           // Name InputField (TMP)
    public TMP_InputField dateTimeInputField;       // DateTime InputField (TMP)
    public TMP_InputField privateContentInputField; // Private Content InputField (TMP)
    public TMP_InputField publicContentInputField;  // Public Content InputField (TMP)

    // 게시글 표시용 Text (TMP)
    public TMP_Text privateText;        // Private Text (TMP)
    public TMP_Text publicText;         // Public Text (TMP)

    // ★★★ 추가: authManager 선언 ★★★
    private FirebaseAuthManager authManager;



    void Start()
    {
        authManager = FindObjectOfType<FirebaseAuthManager>();

        // 버튼 이벤트 연결 (필요시)
        privateWriteButton.onClick.AddListener(SavePrivateBoard);
        publicWriteButton.onClick.AddListener(SavePublicBoard);
    }

    public void SavePrivateBoard()
    {
        var user = authManager.user;
        if (user == null)
        {
            Debug.LogError("로그인 필요");
            return;
        }

        string userName = user.DisplayName ?? user.Email;
        string dateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string content = privateContentInputField.text;

        string value = $"{userName},{dateTime},{content}";
        string path = $"PrivateBoard/{user.UserId}";

        FirebaseDatabase.DefaultInstance.RootReference.Child(path).SetValueAsync(value).ContinueWith(task =>
        {
            if (task.IsCompleted)
                Debug.Log("개인 게시판 저장 성공");
            else
                Debug.LogError("개인 게시판 저장 실패: " + task.Exception);
        });
    }

    public void SavePublicBoard()
    {
        var user = authManager.user;
        if (user == null)
        {
            Debug.LogError("로그인 필요");
            return;
        }

        string userName = user.DisplayName ?? user.Email;
        string dateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string content = publicContentInputField.text;

        string value = $"{userName},{dateTime},{content}";
        string path = "PublicBoard";

        FirebaseDatabase.DefaultInstance.RootReference.Child(path).Push().SetValueAsync(value).ContinueWith(task =>
        {
            if (task.IsCompleted)
                Debug.Log("공용 게시판 저장 성공");
            else
                Debug.LogError("공용 게시판 저장 실패: " + task.Exception);
        });
    }
}