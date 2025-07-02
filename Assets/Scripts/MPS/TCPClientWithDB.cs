#define UNITY_SLAVE // UNITY_MASTER or UNITY_SLAVE ��ó����: ���� �� �ڵ带 ������ �� �ִ� ���

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading; // CancellationToken�� ���� �߰�
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Firebase;
using Firebase.Database;
using System.Net;
using System.Collections;
using Newtonsoft.Json;

// TCPSever(�ܼ����α׷�)�� �Բ� ����ϴ� TCPClient + Firebase Realtime Database
// * ���ǻ���:  TCPSever�� ���� �� �Ŀ� ������ �ּ���.
public class TCPClientWithDB : MonoBehaviour
{
    public string dbUrl;
    DatabaseReference dbRef;

    public class DBInfo
    {
        public bool isConnected;
        public string plcData;
    }
    DBInfo dBInfo = new DBInfo();

    // ��Ʈ��ũ ����� ���� ������, Unity ������Ʈ ������ ���� �����忡�� �ϹǷ�
    // �� ������ ���� �����͸� �����ϰ� �ְ�ޱ� ���� ��ġ�� �ʿ�

    // ���� ������(Update)�� �����Ͽ� ��Ʈ��ũ ������� ���� ��û
    private string _requestToSend = "";
    // ��Ʈ��ũ �����尡 ���� ������ ���� ������� �����ϱ� ���� ����
    private string _lastReceivedResponse = null;
    // �� �����鿡 ���� �����ϴ� ���� ���� ���� ��� ��ü
    private readonly object _lock = new object();

    // ��� ���� ��ü��
    private TcpClient _client;
    private NetworkStream _stream;
    private CancellationTokenSource _cts;
    private Task _communicationTask;

    TcpClient client;
    NetworkStream stream;

    [SerializeField] TMP_Text logTxt;
    [SerializeField] bool isConnected;
    [SerializeField] bool isPowerOnCliked;
    [SerializeField] bool isStopCliked;
    [SerializeField] bool isEStopCliked;

    const string X_START_UNITY2PLC = "X0";
    const string X_START_PLC2UNITY = "X10";
    const string Y_START_PLC2UNITY = "Y0";
    const int X_BLOCKCNT_UNITY2PLC = 1;
    const int X_BLOCKCNT_PLC2UNITY = 1;
    const int Y_BLOCKCNT_PLC2UNITY = 1;

    [Header("Y����̽���")]
    public List<Cylinder> cylinders;
    public Conveyor conveyor;
    public TowerManager towerManager;

    [Header("X����̽���")]
    public Sensor ��������;
    public Sensor �ݼӼ���;

    private void Awake()
    {
        InitializeDB();
    }

    void InitializeDB()
    {
        FirebaseApp.DefaultInstance.Options.DatabaseUrl = new System.Uri(dbUrl);

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // ���� ���� 
    private void StartConnection()
    {
#if UNITY_MASTER
        // �̹� ���� �õ� ���̰ų� ����� ���¸� �ߺ� ���� ����
        if (_communicationTask != null && !_communicationTask.IsCompleted)
        {
            Debug.LogWarning("�̹� ���� ���μ����� ���� ���Դϴ�.");
            return;
        }

        // ���ο� CancellationTokenSource�� Task�� ����
        _cts = new CancellationTokenSource();
        // ������ ���ۿ� ������ ����
        _communicationTask = Task.Run(() => InitializeClient(_cts.Token));

#elif UNITY_SLAVE

#endif
        StartCoroutine(UpdateToDataBase());
    }

    // ���� ����
    private void StopConnection()
    {
        // �̹� ������ ����ų� ���� ���� ���̸� �ߺ� ���� ����
        if (!isConnected && (_communicationTask == null || _communicationTask.IsCompleted))
        {
            return;
        }

        // 1. isConnected �÷��׸� ���� false�� �����Ͽ� Update ������ ��û ������ �ߴ�
        isConnected = false;

        // 2. ������ "Disconnect" �޽����� �����޶�� ��û
        //    ��Ʈ��ũ �����尡 �� ��û�� ó���� ����
        lock (_lock)
        {
            _requestToSend = "Disconnect";
        }

        // 3. ��� ��ٸ� ��(�޽����� ���۵� �ð� Ȯ��), Task ���
        //    Task.Run�� ����Ͽ� ���� ������(���� ������)�� ���� ����
        Task.Run(async () =>
        {
            // 100ms ������ �޽����� �����⿡ ����� �ð�
            await Task.Delay(100);

            // 4. ���� ���� Task�� ������ ��� ��ȣ�� ����
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }
        });
    }

    private void Update()
    {
        // #define���� UNITY_MASTER�� ����� ����Ǵ� �κ�
        // DB�� �Ѱ������ ������: _requestToSend, responseToProcess
#if UNITY_MASTER
        // 1. (���� ������) ��Ʈ��ũ�� ���� ��û ���ڿ� ����
        if (isConnected)
        {
            // isPowerOnCliked ���� UI ���¸� �������� ��û ���ڿ��� ����
            RequestData(out string newRequest);
            // lock�� ����� ��Ʈ��ũ �����尡 ����� ������ �����ϰ� �Ҵ�
            lock (_lock)
            {
                _requestToSend = newRequest;
            }
        }

        // 2. (���� ������) ��Ʈ��ũ �����尡 �޾ƿ� ������ �ִ��� Ȯ���ϰ� ó��
        string responseToProcess = null;
        lock (_lock)
        {
            if (_lastReceivedResponse != null)
            {
                responseToProcess = _lastReceivedResponse;
                //_lastReceivedResponse = null; // ������ ���������� ����� (������ �� ó������ �ʵ���)
            }
        }

        // ó���� ������ �ִٸ�, ResponseData �Լ� ȣ��
        if (responseToProcess != null)
        {
            ResponseData(responseToProcess);
        }

        // #define���� UNITY_SLAVE�� ����� ����Ǵ� �κ�
        // DB���� �����;� �� ������: _requestToSend, _lastReceivedResponse
#elif UNITY_SLAVE
    // Firebase DB�� ���� �����͸� �����ͼ�, ���� ��������� ��.
    
#endif
    }

    /// <summary>
    /// Firebase DB�� _requestToSend, _lastReceivedResponse ������ ������Ʈ
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private IEnumerator UpdateToDataBase()
    {
#if UNITY_MASTER
        yield return new WaitUntil(() => isConnected);

        while(isConnected)
        {
            string json = $"{{" +
                $"\"isMasterConnected\":{isConnected}" + 
                $"\"plcData\":\"{_lastReceivedResponse}\"" +
                $"}}";

            Task t = dbRef.SetRawJsonValueAsync(json).ContinueWith(task =>
            {
                if(task.IsCompleted)
                {
                    print(json);
                }
            });

            yield return new WaitUntil(() => t.IsCompleted);
        }
#elif UNITY_SLAVE
        string json = "";
        dbRef.GetValueAsync().ContinueWith(task =>
        {
            if(task.IsCompleted)
            {
                DataSnapshot snapShot = task.Result;

                json = snapShot.GetRawJsonValue();

                dBInfo = JsonConvert.DeserializeObject<DBInfo>(json);
            }
        });

        yield return new WaitUntil(() => dBInfo.isConnected);

        while(dBInfo.isConnected)
        {
            Task t = dbRef.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapShot = task.Result;

                    json = snapShot.GetRawJsonValue();

                    dBInfo = JsonConvert.DeserializeObject<DBInfo>(json);
                }
            });

            yield return new WaitUntil(() => t.IsCompleted);

            ResponseData(dBInfo.plcData);
        }
#endif
    }

    private void ResponseData(string response)
    {
        if (string.IsNullOrEmpty(response)) return;

        // "Connected", "Disconnected" ���� ���� �޽��� ó��
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

            // xDevice ���� PLC -> UNITY
            cylinders[0].isFrontLimitSWON = binaryX[0] == '1';
            cylinders[0].isFrontLimitSWON = binaryX[0] == '1';
            cylinders[0].isBackLimitSWON = binaryX[1] == '1';
            cylinders[1].isFrontLimitSWON = binaryX[2] == '1';
            cylinders[1].isBackLimitSWON = binaryX[3] == '1';
            cylinders[2].isFrontLimitSWON = binaryX[4] == '1';
            cylinders[2].isBackLimitSWON = binaryX[5] == '1';
            cylinders[3].isFrontLimitSWON = binaryX[6] == '1';
            cylinders[3].isBackLimitSWON = binaryX[7] == '1';
            ��������.isActive = binaryX[8] == '1';
            �ݼӼ���.isActive = binaryX[9] == '1';

            // yDevice ���� PLC -> UNITY
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
        // �� �Լ��� ������ �ʿ� ���� (������ ����)
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
        // �� �Լ��� ������ �ʿ� ���� (������ ����)
        string readX = $"read,{X_START_PLC2UNITY},{X_BLOCKCNT_PLC2UNITY}";
        string readY = $"read,{Y_START_PLC2UNITY},{Y_BLOCKCNT_PLC2UNITY}";

        char power = (isPowerOnCliked ? '1' : '0');
        char stop = (isStopCliked ? '1' : '0');
        char eStop = (isEStopCliked ? '1' : '0');

        char cylAFrontLS = (cylinders[0].isFrontEnd ? '1' : '0');
        char cylABackLS = (cylinders[0].isFrontEnd ? '0' : '1');

        char cylBFrontLS = (cylinders[1].isFrontEnd ? '1' : '0');
        char cylBBackLS = (cylinders[1].isFrontEnd ? '0' : '1');

        char cylCFrontLS = (cylinders[2].isFrontEnd ? '1' : '0');
        char cylCBackLS = (cylinders[2].isFrontEnd ? '0' : '1');

        char cylDrontLS = (cylinders[3].isFrontEnd ? '1' : '0');
        char cylDBackLS = (cylinders[3].isFrontEnd ? '0' : '1');

        string binaryStr = $"{cylDBackLS}{cylDrontLS}{cylCBackLS}{cylCFrontLS}{cylBBackLS}{cylBFrontLS}{cylABackLS}{cylAFrontLS}" +
            $"{eStop}{stop}{power}";
        int decimalX = Convert.ToInt32(binaryStr, 2);
        string writeX = $"write,{X_START_UNITY2PLC},{X_BLOCKCNT_UNITY2PLC},{decimalX}";

        request = $"Request,{readX},{readY},{writeX}";
    }

    async Task InitializeClient(CancellationToken token)
    {
        try
        {
            // [�߿�] �׻� ���ο� TcpClient �ν��Ͻ��� ����
            _client = new TcpClient();
            await _client.ConnectAsync("127.0.0.1", 12345);
            _stream = _client.GetStream();

            Debug.Log("������ ����Ǿ����ϴ�.");
            isConnected = true; // ���� ���� �� �÷��� ����

            // --- Connect ��û�� �� �� ���� (������ ���� ��� �˸�) ---
            byte[] connectMsg = Encoding.UTF8.GetBytes("Connect");
            await _stream.WriteAsync(connectMsg, 0, connectMsg.Length, token);
            // �����κ��� "Connected" ������ ��ٸ��� ó���� �� ���� (������)

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
                        Debug.Log("������ ������ �����߽��ϴ�.");
                        break;
                    }

                    string receivedResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    lock (_lock) { _lastReceivedResponse = receivedResponse; }

                    if (receivedResponse.Contains("Disconnected"))
                    {
                        Debug.Log("�����κ��� ���� ���� ������ �޾ҽ��ϴ�.");
                        break;
                    }

                    lock (_lock)
                    {
                        if (_requestToSend == "Disconnect") _requestToSend = "";
                    }
                }

                // �۽� ������
                await Task.Delay(50, token);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("��� Task�� ���������� ��ҵǾ����ϴ�.");
        }
        catch (SocketException se)
        {
            Debug.LogError($"���� ���� ����: {se.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"��� �� ���� �߻�: {e.Message}");
        }
        finally
        {
            // finally ��Ͽ����� ���� ������ �ڿ� �������� ����
            isConnected = false;

            _stream?.Close();
            _client?.Close();
            _cts?.Dispose();

            // ������ null�� �����Ͽ� ������ �÷����� ���� ó���ϵ��� ��
            _cts = null;
            _stream = null;
            _client = null;
            _communicationTask = null; // Task�� ����

            Debug.Log("Ŭ���̾�Ʈ �ڿ��� ��� �����Ǿ����ϴ�.");
        }
    }

    public void OnConnectBtnClkEvent()
    {
        // ���� ���� �Լ� ȣ��
        StartConnection();
    }

    public void OnDisconnectBtnClkEvent()
    {
        // ���� ���� �Լ� ȣ��
        StopConnection();
    }

    public void OnPowerBtnToggle()
    {
        isPowerOnCliked = !isPowerOnCliked;
        // (���� ����) ��ư ���� ���� ������ ����ڿ��� ���¸� �ð������� �˷��ָ� �����ϴ�.
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