using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;

namespace Chat {

    public partial class MainWindow {

        Client client;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void startServer_Click(object sender, RoutedEventArgs e)
        {
            triggerAllOnConnection();

            try
            {
                var server = new ServerPingable();
                server.Start();

                client = new ClientPingable(resetState, ReceivedMessageCallback);

                if (!await client.Connect("127.0.0.1"))
                {
                    ReceivedMessageCallback("Client", "could not connect to a server");
                    resetState();
                    return;
                }

                client.ListenForMessages(ErrorMessageCallback);

                ReceivedMessageCallback("Connected", "server ready");
                enableFunctionalityOnConnection();

            }
            catch (SocketException)
            {
                ReceivedMessageCallback("Server", "already running");
                resetState();
            }
        }

        private async void startClient_Click(object sender, RoutedEventArgs e)
        {
            triggerAllOnConnection();

            client = new ClientPingable(resetState, ReceivedMessageCallback);

            if (!await client.Connect(connectAddress.Text))
            {
                ReceivedMessageCallback("Client", "could not connect to a server");
                resetState();
                return;
            }

            client.ListenForMessages(ErrorMessageCallback);

            ReceivedMessageCallback("Client", "connected");
            enableFunctionalityOnConnection();
        }

        private void sendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (messageToSend.Text.Contains('\n'))
            {
                ReceivedMessageCallback("Error", "Messages cannot contain new lines.");
                return;
            }

            if (username.Text.Contains(' '))
            {
                ReceivedMessageCallback("Error", "Username cannot contain spaces.");
                return;
            }

            client.SendMessage(messageToSend.Text, username.Text);
            messageToSend.Clear();
        }

        private void ErrorMessageCallback()
        {
            resetState();
        }

        private void resetState()
        {
            triggerAllOnConnection(true);
            enableFunctionalityOnConnection(false);
        }

        private void triggerAllOnConnection(bool to = false)
        {
            startClient.IsEnabled = to;
            startServer.IsEnabled = to;
            connectAddress.IsEnabled = to;
        }

        private void enableFunctionalityOnConnection(bool to = true)
        {
            sendMessage.IsEnabled = to;
        }

        private void ReceivedMessageCallback(string username, string message)
        {
            //texts.AppendText(String.Format("({0}) {1}: {2}", DateTime.Now, username, message) + Environment.NewLine);

            var i = ((Paragraph)texts.Document.Blocks.FirstBlock).Inlines;

            i.Add(new Italic(new Run("(" + DateTime.Now + ")")));
            i.Add(new Bold(new Run(" " + username + ": ")));
            i.Add(message);
            i.Add(Environment.NewLine);

            texts.ScrollToEnd();
        }

        private void username_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (username.Text.Contains(' '))
                username.BorderBrush = Brushes.Red;
            else username.ClearValue(TextBox.BorderBrushProperty);
        }

        private void messageToSend_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (messageToSend.Text.Contains('\n'))
                username.BorderBrush = Brushes.Red;
            else username.ClearValue(TextBox.BorderBrushProperty);
        }

        private void messageToSend_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                sendMessage.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            }
        }
    }
}
