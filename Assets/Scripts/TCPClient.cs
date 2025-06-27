using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading; // CancellationToken을 위해 추가
using System.Threading.Tasks;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;

// TCPSever(콘솔프로그램)와 함께 사용하는 TCPClient
// * 주의사항:  TCPSever를 먼저 켠 후에 실행해 주세요.
public class TCPClient : MonoBehaviour
{
    // 네트워크 통신은 별도 스레드, Unity 오브젝트 조작은 메인 스레드에서 하므로
    // 두 스레드 간에 데이터를 안전하게 주고받기 위한 장치가 필요

    // 메인 스레드(Update)가 생성하여 네트워크 스레드로 보낼 요청
    private string _requestToSend = "";
    // 네트워크 스레드가 받은 응답을 메인 스레드로 전달하기 위한 변수
    private string _lastReceivedResponse = null;
    // 위 변수들에 동시 접근하는 것을 막기 위한 잠금 객체
    private readonly object _lock = new object();

    // 통신 관련 객체들
    private TcpClient _client;
    private NetworkStream _stream;
    private CancellationTokenSource _cts;
    private Task _communicationTask;

    TcpClient client;
    NetworkStream stream;

    [SerializeField] TMP_Text logTxt;
    bool isConnected;
    bool isPowerOnCliked;
    bool isStopCliked;
    bool isEStopCliked;

    const string X_START_UNITY2PLC = "X0";
    const string X_START_PLC2UNITY = "X10";
    const string Y_START_PLC2UNITY = "Y0";
    const int X_BLOCKCNT_UNITY2PLC = 1;
    const int X_BLOCKCNT_PLC2UNITY = 1;
    const int Y_BLOCKCNT_PLC2UNITY = 1;

    [Header("Y디바이스용")]
    public List<Cylinder> cylinders;
    public Conveyor conveyor;
    public TowerManager towerManager;

    [Header("X디바이스용")]
    public Sensor 근접센서;
    public Sensor 금속센서;


    void Start()
    {

    }

    // 연결 시작 
    private void StartConnection()
    {
        // 이미 연결 시도 중이거나 연결된 상태면 중복 실행 방지
        if (_communicationTask != null && !_communicationTask.IsCompleted)
        {
            Debug.LogWarning("이미 연결 프로세스가 진행 중입니다.");
            return;
        }

        // 새로운 CancellationTokenSource와 Task를 생성
        _cts = new CancellationTokenSource();
        _communicationTask = Task.Run(() => InitializeClient(_cts.Token));
    }

    // 연결 해제
    private void StopConnection()
    {
        // 이미 연결이 끊겼거나 해제 과정 중이면 중복 실행 방지
        if (!isConnected && (_communicationTask == null || _communicationTask.IsCompleted))
        {
            return;
        }

        // 1. isConnected 플래그를 먼저 false로 설정하여 Update 루프의 요청 생성을 중단
        isConnected = false;

        // 2. 서버에 "Disconnect" 메시지를 보내달라고 요청
        //    네트워크 스레드가 이 요청을 처리할 것임
        lock (_lock)
        {
            _requestToSend = "Disconnect";
        }

        // 3. 잠시 기다린 후(메시지가 전송될 시간 확보), Task 취소
        //    Task.Run을 사용하여 현재 스레드(메인 스레드)를 막지 않음
        Task.Run(async () =>
        {
            // 100ms 정도면 메시지를 보내기에 충분한 시간
            await Task.Delay(100);

            // 4. 실행 중인 Task가 있으면 취소 신호를 보냄
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }
        });
    }

    private void Update()
    {
        // 1. (메인 스레드) 네트워크로 보낼 요청 문자열 생성
        if (isConnected)
        {
            // isPowerOnCliked 같은 UI 상태를 바탕으로 요청 문자열을 만듦
            RequestData(out string newRequest);
            // lock을 사용해 네트워크 스레드가 사용할 변수에 안전하게 할당
            lock (_lock)
            {
                _requestToSend = newRequest;
            }
        }

        // 2. (메인 스레드) 네트워크 스레드가 받아온 응답이 있는지 확인하고 처리
        string responseToProcess = null;
        lock (_lock)
        {
            if (_lastReceivedResponse != null)
            {
                responseToProcess = _lastReceivedResponse;
                _lastReceivedResponse = null; // 응답을 가져왔으니 비워줌 (다음에 또 처리하지 않도록)
            }
        }

        // 처리할 응답이 있다면, ResponseData 함수 호출
        if (responseToProcess != null)
        {
            ResponseData(responseToProcess);
        }
    }

    private void ResponseData(string response)
    {
        if (string.IsNullOrEmpty(response)) return;

        // "Connected", "Disconnected" 같은 상태 메시지 처리
        if (response.Contains("Connected") || response.Contains("Disconnected"))
        {
            if (logTxt != null) logTxt.text = response;
            return;
        }

        if (logTxt != null) logTxt.text = $"Received: {response}";

        if (isConnected)
        {
            string[] splited = response.Split(',');
            string xData = "0";
            string yData = "0";

            for (int i = 0; i < splited.Length; i++)
            {
                if (splited[i].Equals("Read", StringComparison.OrdinalIgnoreCase))
                {
                    string address = splited[i + 1];
                    if (address.StartsWith("X")) xData = splited[i + 3].Trim();
                    else if (address.StartsWith("Y")) yData = splited[i + 3].Trim();
                }
            }

            if (!int.TryParse(xData, out int xInt)) return;
            if (!int.TryParse(yData, out int yInt)) return;

            string[] binaries = ConvertDecimalToBinary(new int[] { xInt, yInt });
            string binaryX = binaries[0];
            string binaryY = binaries[1];

            // xDevice 정보 PLC -> UNITY
            cylinders[0].isFrontLimitSWON = binaryX[0] == '1';
            cylinders[0].isBackLimitSWON = binaryX[1] == '1';
            cylinders[1].isFrontLimitSWON = binaryX[2] == '1';
            cylinders[1].isBackLimitSWON = binaryX[3] == '1';
            cylinders[2].isFrontLimitSWON = binaryX[4] == '1';
            cylinders[2].isBackLimitSWON = binaryX[5] == '1';
            cylinders[3].isFrontLimitSWON = binaryX[6] == '1';
            cylinders[3].isBackLimitSWON = binaryX[7] == '1';
            근접센서.isActive = binaryX[8] == '1';
            금속센서.isActive = binaryX[9] == '1';

            // yDevice 정보 PLC -> UNITY
            cylinders[0].isForward = binaryY[0] == '1';
            cylinders[0].isBackward = binaryY[1] == '1';
            cylinders[1].isForward = binaryY[2] == '1';
            cylinders[1].isBackward = binaryY[3] == '1';
            cylinders[2].isForward = binaryY[4] == '1';
            cylinders[2].isBackward = binaryY[5] == '1';
            cylinders[3].isForward = binaryY[6] == '1';
            cylinders[3].isBackward = binaryY[7] == '1';
            conveyor.isCW = binaryY[8] == '1';
            conveyor.isCCW = binaryY[9] == '1';
            towerManager.isRedLampOn = binaryY[10] == '1';
            towerManager.isYellowLampOn = binaryY[11] == '1';
            towerManager.isGreenLampOn = binaryY[12] == '1';
            UIController.instance.isRobotOn = binaryY[13] == '1';
        }
    }

    private string[] ConvertDecimalToBinary(int[] data)
    {
        // 이 함수는 수정할 필요 없음 (기존과 동일)
        string[] result = new string[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            string binary = Convert.ToString(data[i], 2);
            string reversedBinary = new string(binary.Reverse().ToArray());
            reversedBinary = reversedBinary.PadRight(16, '0');
            result[i] = reversedBinary;
        }
        return result;
    }

    private void RequestData(out string request)
    {
        // 이 함수는 수정할 필요 없음 (기존과 동일)
        string readX = $"read,{X_START_PLC2UNITY},{X_BLOCKCNT_PLC2UNITY}";
        string readY = $"read,{Y_START_PLC2UNITY},{Y_BLOCKCNT_PLC2UNITY}";

        char power = (isPowerOnCliked ? '1' : '0');
        char stop = (isStopCliked ? '1' : '0');
        char eStop = (isEStopCliked ? '1' : '0');
        
        
        char cylAFrontLS = (cylinders[0].isFrontEnd ? '1' : '0');
        char cylABackLS = (cylinders[0].isFrontEnd ? '1' : '0');

        char cylBFrontLS = (cylinders[0].isFrontEnd ? '1' : '0');
        char cylBBackLS = (cylinders[0].isFrontEnd ? '1' : '0');

        char cylCFrontLS = (cylinders[0].isFrontEnd ? '1' : '0');
        char cylCBackLS = (cylinders[0].isFrontEnd ? '1' : '0');

        char cylDFrontLS = (cylinders[0].isFrontEnd ? '1' : '0');
        char cylDBackLS = (cylinders[0].isFrontEnd ? '1' : '0');

        string binaryStr = $"{cylDBackLS}{cylDFrontLS}{cylCBackLS}{cylCFrontLS}{cylBBackLS}{cylBFrontLS}{cylABackLS}{cylAFrontLS}" + $"{eStop}{stop}{power}";
        int decimalX = Convert.ToInt32(binaryStr, 2);
        string writeX = $"write,{X_START_UNITY2PLC},{X_BLOCKCNT_UNITY2PLC},{decimalX}";

        request = $"Request,{readX},{readY},{writeX}";
    }


    async Task InitializeClient(CancellationToken token)
    {
        try
        {
            // [중요] 항상 새로운 TcpClient 인스턴스를 생성
            _client = new TcpClient();
            await _client.ConnectAsync("127.0.0.1", 12345);
            _stream = _client.GetStream();

            Debug.Log("서버에 연결되었습니다.");
            isConnected = true; // 연결 성공 시 플래그 설정

            // --- Connect 요청을 한 번 보냄 (서버에 연결 사실 알림) ---
            byte[] connectMsg = Encoding.UTF8.GetBytes("Connect");
            await _stream.WriteAsync(connectMsg, 0, connectMsg.Length, token);
            // 서버로부터 "Connected" 응답을 기다리고 처리할 수 있음 (선택적)

            while (!token.IsCancellationRequested)
            {
                string currentRequest = null;
                lock (_lock) { currentRequest = _requestToSend; }

                if (!string.IsNullOrEmpty(currentRequest))
                {
                    byte[] data = Encoding.UTF8.GetBytes(currentRequest);
                    await _stream.WriteAsync(data, 0, data.Length, token);

                    byte[] buffer = new byte[1024];
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, token);

                    if (bytesRead == 0)
                    {
                        Debug.Log("서버가 연결을 종료했습니다.");
                        break;
                    }

                    string receivedResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    lock (_lock) { _lastReceivedResponse = receivedResponse; }

                    if (receivedResponse.Contains("Disconnected"))
                    {
                        Debug.Log("서버로부터 연결 해제 응답을 받았습니다.");
                        break;
                    }

                    lock (_lock)
                    {
                        if (_requestToSend == "Disconnect") _requestToSend = "";
                    }
                }
                await Task.Delay(50, token);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("통신 Task가 정상적으로 취소되었습니다.");
        }
        catch (SocketException se)
        {
            Debug.LogError($"소켓 연결 실패: {se.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"통신 중 에러 발생: {e.Message}");
        }
        finally
        {
            // finally 블록에서는 상태 정리와 자원 해제에만 집중
            isConnected = false;

            _stream?.Close();
            _client?.Close();
            _cts?.Dispose();

            // 참조를 null로 설정하여 가비지 컬렉션이 쉽게 처리하도록 함
            _cts = null;
            _stream = null;
            _client = null;
            _communicationTask = null; // Task도 정리

            Debug.Log("클라이언트 자원이 모두 해제되었습니다.");
        }
    }

    public void OnConnectBtnClkEvent()
    {
        // 연결 시작 함수 호출
        StartConnection();
    }

    public void OnDisconnectBtnClkEvent()
    {
        // 연결 해제 함수 호출
        StopConnection();
    }

    public void OnPowerBtnToggle()
    {
        isPowerOnCliked = !isPowerOnCliked;
        // (선택 사항) 버튼 색상 변경 등으로 사용자에게 상태를 시각적으로 알려주면 좋습니다.
        Debug.Log($"Power Button Toggled: {isPowerOnCliked}");
    }

    public void OnStopBtnToggle()
    {
        isStopCliked = !isStopCliked;
        Debug.Log($"Stop Button Toggled: {isStopCliked}");
    }

    public void OnEStopBtnToggle()
    {
        isEStopCliked = !isEStopCliked;
        Debug.Log($"E-Stop Button Toggled: {isEStopCliked}");
    }

    private void OnDestroy()
    {
        StopConnection();
    }
}