using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace WindowsFormsApp1
{
    public partial class chatFrom : Form
    {

        delegate void addMessage(string message);

        const int PORT = 54545;
        const string broadcastAddress = "255.255.255.255";
        string ip = "";

        UdpClient receivingClient;
        UdpClient sendingClient;

        Thread receivingThread;

        public string getIp()
        {
            string strHostName = "";
            strHostName = System.Net.Dns.GetHostName();

            IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);

            IPAddress[] addr = ipEntry.AddressList;

            return addr[addr.Length - 1].ToString();
        }

        public chatFrom()
        {
            InitializeComponent();
        }

        void chatFrom_Load(object sender, EventArgs e)
        {
            this.AcceptButton = sendButton;
            messageTextBox.Focus();
            ip = getIp();
            InitializeSender();
            InitializeReceiver();
        }

        private void InitializeSender()
        {
            sendingClient = new UdpClient(broadcastAddress, PORT);
            sendingClient.EnableBroadcast = true;
        }

        private void InitializeReceiver()
        {
            receivingClient = new UdpClient(PORT);

            ThreadStart start = new ThreadStart(Receiver);
            receivingThread = new Thread(start);
            receivingThread.IsBackground = true;
            receivingThread.Start();
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            messageTextBox.Text = messageTextBox.Text.TrimEnd();

            if (!string.IsNullOrEmpty(messageTextBox.Text))
            {
                string toSend = ip + " :\n" + messageTextBox.Text;
                byte[] data = Encoding.ASCII.GetBytes(toSend);
                sendingClient.Send(data, data.Length);
                messageTextBox.Text = "";
            }

            messageTextBox.Focus();
            messageRichTextBox.ScrollToCaret();
        }

        private void Receiver()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, PORT);
            addMessage messageDelegate = MessageReceived;

            while (true)
            {
                byte[] data = receivingClient.Receive(ref endPoint);
                string message = Encoding.ASCII.GetString(data);
                Invoke(messageDelegate, message);
                
            }
        }

        private void MessageReceived(string message)
        {
            messageRichTextBox.Text += message + "\n";
            messageRichTextBox.SelectionStart = messageRichTextBox.Text.Length;
            messageRichTextBox.ScrollToCaret();
        }
    }
}
