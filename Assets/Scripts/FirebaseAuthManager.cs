using UnityEngine;
using Firebase.Auth;
using TMPro;
using System.Threading.Tasks;
using Firebase;
using System.Net.Mail;
using UnityEditor;

// 1. �α���: �̸���, �н����� �Է½� ȸ������ ���ο� ���� �α���
// 2. ȸ������: �̸���, �н����� �Է� �� �̸��� ������ �ȴٸ�, ȸ������ �Ϸ�!
// 3. DB���� �ҷ�����: ���ѿ� ���� DB�� Ư�� ������ �����´�.
public class FirebaseAuthManager : MonoBehaviour
{
    public TMP_InputField signInEmailInput;
    public TMP_InputField signInPasswordInput;

    public TMP_InputField signUpEmailInput;
    public TMP_InputField signUpPasswordInput;
    public TMP_InputField signUpPasswordCheckInput;

    // class diagram
    // + Initialization(�ʱ�ȭ)
    // + SignIn(�α���)
    // + SignUp(ȸ������)
    // + SendVerificationEmail(�̸�������)
    FirebaseAuth auth;
    FirebaseUser user;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    private void OnDestroy()
    {
        auth.SignOut();
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null
                && auth.CurrentUser.IsValid();
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                Debug.Log(user.DisplayName ?? "");
                Debug.Log(user.Email ?? "");
                Debug.Log(user.IsEmailVerified);
                Debug.Log(user.PhotoUrl);
            }
        }
    }

    public void SignIn()
    {
        if(signInEmailInput.text == string.Empty)
        {
            print("Please enter your email.");
            return;
        }
        else if(signInPasswordInput.text == string.Empty)
        {
            print("Please enter your password.");
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(signInEmailInput.text, signInPasswordInput.text)
            .ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                Firebase.Auth.AuthResult result = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);
            });
    }

    public void SignUp()
    {
        if(signUpEmailInput.text == string.Empty || signUpPasswordInput.text == string.Empty || signUpPasswordCheckInput.text == string.Empty)
        {
            print("Email or password or password check is empty.");
            return;
        }

        if(signUpPasswordInput.text != signUpPasswordCheckInput.text)
        {
            print("Password is incorrect");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(signUpEmailInput.text, signUpPasswordInput.text).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Firebase user has been created.
            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

            user.SendEmailVerificationAsync().ContinueWith(task =>
            {
                if(task.IsCompleted)
                {
                    print("�̸����� Ȯ���Ͽ� ������ ������ �ּ���.");
                }
            });
        });
    }

}
