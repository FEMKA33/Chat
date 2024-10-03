using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class ChatForm : Form
    {
        private TcpClient client;
        private NetworkStream stream;

        public ChatForm()
        {
            InitializeComponent();
            ConnectToServer();
        }

        private async void ConnectToServer()
        {
            try
            {
                client = new TcpClient("127.0.0.1", 12345);
                stream = client.GetStream();
                await Task.Run(() => ListenForMessages());
                Console.WriteLine("Подключен к серверу.");
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"Не удалось подключиться к серверу: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ListenForMessages()
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Invoke((Action)(() => listBoxMessages.Items.Add(message)));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении сообщения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void buttonSend_Click(object sender, EventArgs e)
        {
            try
            {
                string message = textBoxMessage.Text;
                string username = textUsername.Text;
                if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(username))
                    return;

                byte[] data = Encoding.UTF8.GetBytes(username + ": " + message);
                await stream.WriteAsync(data, 0, data.Length);
                listBoxMessages.Items.Add("Я: " + message);
                textBoxMessage.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке сообщения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            stream?.Close();
            client?.Close();
            base.OnFormClosing(e);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
