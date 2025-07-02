using UnityEngine;
using Firebase.Auth;
using TMPro;
using System.Threading.Tasks;
using Firebase;
using System.Collections;

namespace MPS
{
    // 1. 로그인: 이메일, 패스워드 입력시 회원가입 여부에 따라 로그인
    // 2. 회원가입: 이메일, 패스워드 입력 후 이메일 인증이 된다면, 회원가입 완료!
    // 3. DB정보 불러오기: 권한에 따라 DB의 특정 정보를 가져온다.
    public class FirebaseAuthManager : MonoBehaviour
    {
        public static FirebaseAuthManager instance;

        public GameObject signInPanel;
        public GameObject signUpPanel;
        public GameObject verificationPanel;

        public TMP_InputField signInEmailInput;
        public TMP_InputField signInPasswordInput;

        public TMP_InputField signUpEmailInput;
        public TMP_InputField signUpPasswordInput;
        public TMP_InputField signUpPasswordCheckInput;

        // class diagram
        // + Initialization(초기화)
        // + SignIn(로그인)
        // + SignUp(회원가입)
        // + SendVerificationEmail(이메일인증)
        FirebaseAuth auth;
        public FirebaseUser user;

        private void Awake()
        {
            if (instance == null)
                instance = this;
        }
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
            if (signInEmailInput.text == string.Empty)
            {
                print("Please enter your email.");
                return;
            }
            else if (signInPasswordInput.text == string.Empty)
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
            if (signUpEmailInput.text == string.Empty || signUpPasswordInput.text == string.Empty || signUpPasswordCheckInput.text == string.Empty)
            {
                print("Email or password or password check is empty.");
                return;
            }

            if (signUpPasswordInput.text != signUpPasswordCheckInput.text)
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
                    print(task.Exception);

                    if (task.IsCompleted)
                    {
                        print("이메일을 확인하여 인증을 끝마쳐 주세요.");
                    }
                });
            });
        }

        public void OnSignUpBtnClkEvent()
        {
            signInPanel.SetActive(false);
            signUpPanel.SetActive(true);
        }

        public void OnSignUpOKBtnClkEvent()
        {
            StartCoroutine(SignUp(signUpEmailInput.text, signUpPasswordInput.text,
                signUpPasswordCheckInput.text));
        }

        IEnumerator SignUp(string email, string password, string passwordCheck)
        {
            if (email == "" || password == "" || passwordCheck == "")
            {
                print("이메일 또는 패스워드를 입력해 주세요.");

                yield break;
            }

            if (password != passwordCheck)
            {
                print("비밀번호와 확인비밀번호가 같지 않습니다. 다시 확인 후 진행해 주세요.");

                yield break;
            }

            Task task = auth.CreateUserWithEmailAndPasswordAsync(email, password);

            yield return new WaitUntil(() => task.IsCompleted == true);

            if (task.Exception != null)
            {
                FirebaseException e = task.Exception.GetBaseException() as FirebaseException;
                AuthError authError = (AuthError)e.ErrorCode;

                switch (authError)
                {
                    case AuthError.InvalidEmail:
                        print("유효하지 않은 이메일 입니다.");
                        yield break;
                    case AuthError.WeakPassword:
                        print("비밀번호가 취약합니다.");
                        yield break;
                    case AuthError.EmailAlreadyInUse:
                        print("이미 사용중인 이메일 입니다.");
                        yield break;
                }
            }

            StartCoroutine(SendVerificationEmail(email));
        }

        IEnumerator SendVerificationEmail(string email)
        {
            user = auth.CurrentUser;
            print(user.UserId);

            if (user != null)
            {
                Task task = user.SendEmailVerificationAsync();

                yield return new WaitUntil(() => task.IsCompleted == true);

                if (task.Exception != null)
                {
                    FirebaseException e = task.Exception.GetBaseException() as FirebaseException;
                    AuthError authError = (AuthError)e.ErrorCode;
                    print(authError);
                }

                verificationPanel.SetActive(true);
                print($"인증메일을 {email}로 보냈습니다. 메일을 확인해 주세요.");

                yield return new WaitForSeconds(3);

                signInPanel.SetActive(true);
                signUpPanel.SetActive(false);
                verificationPanel.SetActive(false);
            }
        }

        public void OnSignInOKBtnClkEvent()
        {
            StartCoroutine(SignIn(signInEmailInput.text, signInPasswordInput.text));
        }

        IEnumerator SignIn(string email, string password)
        {
            if (email == "" || password == "")
            {
                print("이메일 또는 패스워드를 입력해 주세요.");
            }

            Task task = auth.SignInWithEmailAndPasswordAsync(email, password);

            yield return new WaitUntil(() => task.IsCompleted == true);

            user = auth.CurrentUser;

            if (user.IsEmailVerified)
            {
                print("로그인이 완료되었습니다.");
                signInPanel.SetActive(false);
            }
            else
            {
                print("이메일이 인증되지 않았습니다. 이메일 인증 후 다시 로그인 해주세요.");
            }
        }

    }
}
