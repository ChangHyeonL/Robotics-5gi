using ActUtlType64Lib;

namespace MxComp
{
    public partial class Form1 : Form
    {
        ActUtlType64 mxComponent;
        bool isConnected;

        public Form1()
        {
            InitializeComponent();

            label1.Text = "���α׷��� �����մϴ�.";

            mxComponent = new ActUtlType64();

            mxComponent.ActLogicalStationNumber = 1;
        }


        private void Open(object sender, EventArgs e)
        {
            int iRet = mxComponent.Open();

            if (iRet == 0)
            {
                isConnected = true;

                label1.Text = "�� ������ �Ǿ����ϴ�.";
            }
            else
            {
                // �����ڵ� ��ȯ(16����)
                label1.Text = Convert.ToString(iRet, 16);
            }
        }

        private void Close(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                label1.Text = "�̹� �������� �����Դϴ�.";

                return;
            }

            int iRet = mxComponent.Close();

            if (iRet == 0)
            {
                isConnected = false;

                label1.Text = "�� ������ �Ǿ����ϴ�.";
            }
            else
            {
                // �����ڵ� ��ȯ(16����)
                label1.Text = Convert.ToString(iRet, 16);
            }
        }

        private void GetDevice(object sender, EventArgs e)
        {
            if (textBox1.Text == string.Empty)
            {
                label1.Text = "����̽� �̸��� �Է��� �ּ���.";
                return;
            }

            int data = 0;
            int iRet = mxComponent.GetDevice(textBox1.Text, out data);

            if (iRet == 0)
            {
                label1.Text = $"{textBox1.Text}: {data}";
            }
            else
            {
                // �����ڵ� ��ȯ(16����)
                label1.Text = Convert.ToString(iRet, 16);
            }
        }

        private void SetDevice(object sender, EventArgs e)
        {
            if (textBox1.Text == string.Empty || textBox2.Text == string.Empty)
            {
                label1.Text = "����̽� �̸� �Ǵ� ���� �Է��� �ּ���.";
                return;
            }

            int value = 0;
            bool isOk = int.TryParse(textBox2.Text, out value);

            if(!isOk)
            {
                label1.Text = "���ڸ� �Է��� �ּ���.";
                return;
            }

            int iRet = mxComponent.SetDevice(textBox1.Text, value);

            if (iRet == 0)
            {
                label1.Text = $"{textBox1.Text}: {textBox2.Text}�� �� ����Ǿ����ϴ�.";
            }
            else
            {
                // �����ڵ� ��ȯ(16����)
                label1.Text = Convert.ToString(iRet, 16);
            }
        }
    }
}
