using System.Net;
using System.Net.Sockets;
using System.Text;
using ActUtlType64Lib;

namespace TCPServer
{
    class Program
    {
        enum State
        {
            CONNECTED,
            DISCONNECTED
        }
        static ActUtlType64 mxComponent;
        static State state;

        // 1:N 비동기통신
        static async Task Main()
        {
            // 콘솔 프로그램 종료 이벤트 등록
            //Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);

            // MxComponent 초기설정
            mxComponent = new ActUtlType64();
            mxComponent.ActLogicalStationNumber = 1;
            state = State.DISCONNECTED;

            //SyncUpdate();
            // 1. 서버 객체 선언
            IPAddress addr = IPAddress.Parse("127.0.0.1");
            TcpListener server = new TcpListener(addr, 12345);

            // 2. 서버 실행
            server.Start();
            Console.WriteLine("서버가 시작되었습니다.");
            Console.WriteLine("클라이언트의 연결을 대기합니다...");

            while (true)
            {
                // 3. 클라이언트의 접속을 비동기적으로 기다림
                TcpClient client = await server.AcceptTcpClientAsync();
                Console.WriteLine("클라이언트가 연결되었습니다.");

                // 4. 데이터 통신 -> 다른 스레드 사용
                Task.Run(() => HandleClientAync(client));
            }
        }

        static async Task HandleClientAync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            Byte[] buffer = new byte[1024];

            int byteRead;

            // 클라이언트가 보낸 데이터를 계속 읽음
            // <Unity 클라이언트가 보낸 데이터 형식>
            // 1. PLC 연결: "Connect" -> MxComponent.Open()
            // 2. PLC 연결해지: "Disconnect" -> MxComponent.Close()
            // 3. 데이터 요청: "Request,read,X0,2" -> MxComponent.ReadDeviceBlock(X0, 2, out data[0]);
            //                 "Request,write,X10,1,240" -> MxComponent.ReadDeviceBlock(X10, 1, out data[0]);
            while ((byteRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                // "Connect", "DisConnect", "Request,read,X0,2"
                string dataStr = Encoding.UTF8.GetString(buffer, 0, byteRead);
                Console.WriteLine($"수신: {dataStr}");

                // FSM(finite state machine): 유한상태머신
                string result = FSM(dataStr); // result -> Unity

                byte[] responseData = Encoding.UTF8.GetBytes(result);

                await stream.WriteAsync(responseData, 0, responseData.Length);
                Console.WriteLine($"송신: {result}");
            }
        }

        // "Connect", "DisConnect", "Request,read,X0,2"
        private static string FSM(string dataStr)
        {
            if (dataStr.Contains("Connect"))
            {
                return Connect();
            }
            else if (dataStr.Contains("Disconnect"))
            {
                return Disconnect();
            }
            else if (dataStr.Contains("Request,read")) // "Request,read,X10,1,write,X0,1,7"
            {
                return ReadDeviceBlock(dataStr);
            }
            else if (dataStr.Contains("Request,write"))
            {
                return WriteDeviceBlock(dataStr);
            }
            else
            {
                return "Fail";
            }
        }

        // 콘솔프로그램 종료시 이벤트
        static void ExitHandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("프로그램 종료 중...");

            Disconnect();
        }

        private static string WriteDeviceBlock(string dataStr)
        {
            // 문자열 파싱: "Request,write,X10,2,355,366" -> { Request, read, X10, 2, 355, 366 }
            // 시작버튼, 정지버튼, 긴급정지버튼 -> 7(111)
            string[] data = dataStr.Split(",");
            string address = data[2];
            int blockCnt;
            bool isInt = int.TryParse(data[3], out blockCnt);

            if (!isInt)
                return "Request Error: BlockCnt 문자열 오류";

            int value;
            isInt = int.TryParse(data[4], out value);

            if (!isInt)
                return "Request Error: Device Value 문자열 오류";

            int[] values = new int[blockCnt];
            values[0] = value;

            int iRet = mxComponent.WriteDeviceBlock(address, blockCnt, ref values[0]);
            if (iRet == 0)
            {
                return $"Write,{address},{blockCnt},{value}";
            }
            else
            {
                return "0x" + Convert.ToString(iRet, 16);
            }
        }

        private static string ReadDeviceBlock(string dataStr)
        {
            // 문자열 파싱: "Request,read,X0,1" -> { Request, read, X0, 1 }  -> { Request, read, X10, 1, write, X0, 1, 7}
            string[] data = dataStr.Split(",");
            string address = data[2];
            int blockCnt;
            bool isInt = int.TryParse(data[3], out blockCnt);

            if (!isInt)
                return "Request Error: BlockCnt 문자열 오류";

            int[] newData = new int[blockCnt];
            int iRet = mxComponent.ReadDeviceBlock(address, blockCnt, out newData[0]);
            if (iRet == 0)
            {
                string str = "";
                for (int i = 0; i < newData.Length; i++)
                {
                    str += newData[i] + ", ";
                }

                return $"Read,{address},{blockCnt},{str}"; // "Read,X0,1,35" ex) 센서정보들
            }
            else
            {
                return "0x" + Convert.ToString(iRet, 16);
            }
        }

        private static string Disconnect()
        {
            int iRet = mxComponent.Close();
            if (iRet == 0)
            {
                state = State.DISCONNECTED;
                return "Disconnected";
            }
            else
            {
                return "0x" + Convert.ToString(iRet, 16);
            }
        }

        private static string Connect()
        {
            int iRet = mxComponent.Open();
            if (iRet == 0)
            {
                state = State.CONNECTED;
                return "Connected";
            }
            else
            {
                return "0x" + Convert.ToString(iRet, 16);
            }
        }

        // 1:1 동기통신용
        private static void SyncUpdate()
        {
            // 서버 소켓 생성 + 바인딩(특정 아이피와 포트할당)
            TcpListener server = new TcpListener(IPAddress.Any, 12345);

            server.Start();

            Console.WriteLine("서버 시작");

            // 1. 클라이언트의 연결을 대기
            TcpClient client = server.AcceptTcpClient();

            Console.WriteLine("클라이언트 연결됨.\n");

            // 2. 네트워크의 스트림 가져오기
            NetworkStream stream = client.GetStream();

            while (true)
            {
                // 3. 클라이언트로 부터 데이터 읽기
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                // 4. 버퍼에 저장한 데이터를 인코딩(UTF8)
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("클라이언트: " + data);

                // 5. 답장 쓰기(데이터 전송하기)
                string response = "서버: 데이터 받음\n";
                byte[] responseData = Encoding.UTF8.GetBytes(response);
                stream.Write(responseData, 0, responseData.Length);
            }
        }
    }
}