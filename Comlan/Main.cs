using System.Net.Sockets;
using System.Text;

namespace Comlan
{
    public partial class Main : Form
    {
        /// <summary>                              
        /// Required designer variable.
        /// </summary>
        private static TcpClient? _client;
        private static NetworkStream? _stream;
        private static string? Username { get; set; }
        private static string AESkey = "cM95jd3wAI5ot7SJ76HisAKR3NuaAEhj";

        /// <summary>
        /// The main form of the application. It initializes the components, starts the connection to the server, and starts a thread to receive messages.
        /// </summary>
        public Main(string serverIP, int serverPort, string Key, string username = null)
        {
            InitializeComponent();

            if (Key != string.Empty)
                AESkey = Key;

            Username = username != null ? "@" + username : "@" + Environment.UserName;

            _client = new TcpClient();

            try
            {
                _client.Connect(serverIP, serverPort);

                if (_client.Connected)
                {
                    _stream = _client.GetStream();
                    AppendMessage("Server connexion : OK.");

                    Thread receiveThread = new(ReceiveMessages);
                    receiveThread.Start();
                }
                else
                {
                    MessageBox.Show("Connexion Error : Server not found.", "Comlan - Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connexion Error : " + ex.Message, "Comlan - Error");
            }
        }

        /// <summary>
        /// Method to receive messages from the server.
        /// </summary>
        private void ReceiveMessages()
        {
            byte[] buffer = new byte[4096];
            while (true)
            {
                try
                {
                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        if (!message.StartsWith('@'))
                        {
                            message = Aes256CbcEncrypt.Decrypt(message, AESkey);
                        }

                        AppendMessage(message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error when receiving message : " + ex.Message, "Comlan - Error");
                    break;
                }
            }
        }

        /// <summary>
        /// Method to append a message to the chat.
        /// </summary>
        /// <param name="message"></param>
        private void AppendMessage(string message)
        {
            if (richTextBoxChannel.InvokeRequired)
            {
                richTextBoxChannel.Invoke(new Action(() => richTextBoxChannel.AppendText(message + Environment.NewLine)));
            }
            else
            {
                richTextBoxChannel.AppendText(message + Environment.NewLine);
            }
        }

        /// <summary>
        /// Method to send a message to the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSend_Click(object sender, EventArgs e)
        {
            if (TextBoxWrite.Text.Trim() != string.Empty)
            {
                try
                {
                    string message = Username + ": " + TextBoxWrite.Text;
                    string EncryptedMessage = Aes256CbcEncrypt.Encrypt(message, AESkey);
                    byte[] data = Encoding.UTF8.GetBytes(EncryptedMessage);

                    if (_stream != null)
                    {
                        _stream.Write(data, 0, data.Length);
                        TextBoxWrite.Text = "";
                    }
                    else
                    {
                        throw new Exception("Network stream is null.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error when sending message : " + ex.Message, "Comlan - Error");
                }
            }
            else
            {
                MessageBox.Show("Please enter a message.", "Comlan - Error");
            }
        }

        /// <summary>
        /// Method to add a file if necessary.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonAddFile_Click(object sender, EventArgs e)
        {
            // Fonction pour ajouter un fichier si nécessaire
        }

        /// <summary>
        /// Method to close the connection and the application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClose_Click(object sender, EventArgs e)
        {
            if (_stream != null)
            {
                _stream.Close();
            }

            if (_client != null)
            {
                _client.Close();
            }

            this.Close();
        }

        /// <summary>
        /// Method to minimize the application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonMinimize_Click(object sender, EventArgs e)
        {
            Main.ActiveForm.WindowState = FormWindowState.Minimized;
        }
    }
}
