using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WSRouleteServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class SocketT2h
    {
        public Socket _Socket { get; set; }
        public string _Name { get; set; }
        public SocketT2h(Socket socket)
        {
            this._Socket = socket;
        }
    }
    public partial class MainWindow : Window
    {
        private int time = 30;//CELOTNA IGRA
        private int endBetTime = 10;

        private DispatcherTimer Timer;

        private byte[] _buffer = new byte[1024];

        public bool CheckForIllegalCrossThreadCalls { get; }
        public List<SocketT2h> __ClientSockets { get; set; }
        List<string> _names = new List<string>();
        private Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public MainWindow()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            __ClientSockets = new List<SocketT2h>();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            SetupServer();
        }
        private void SetupServer()
        {
            txtServer.Text = "Setting up server . . .";
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));
            _serverSocket.Listen(1);
            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }

        private void AppceptCallback(IAsyncResult ar)
        {
            Socket socket = _serverSocket.EndAccept(ar);
            __ClientSockets.Add(new SocketT2h(socket));

            this.Dispatcher.Invoke(() =>
            {
                list1.Items.Add(socket.RemoteEndPoint.ToString());
            });


            this.Dispatcher.Invoke(() =>
            {
                txtText.Text = "Stevilo clientov povezanih" + __ClientSockets.Count.ToString();
            });

            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }
        private void ReceiveCallback(IAsyncResult ar)
        {

            Socket socket = (Socket)ar.AsyncState;
            if (socket.Connected)
            {
                int received;
                try
                {
                    received = socket.EndReceive(ar);
                }
                catch (Exception)
                {
                    // odjavljeni clienti
                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (__ClientSockets[i]._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))
                        {
                            __ClientSockets.RemoveAt(i);
                            this.Dispatcher.Invoke(() =>
                            {
                                txtName.Text = "Stevilo clientov odjavljenih" + __ClientSockets.Count.ToString();
                            });

                        }
                    }
                    return;
                }
                if (received != 0)
                {
                    byte[] dataBuf = new byte[received];
                    Array.Copy(_buffer, dataBuf, received);
                    string text = Encoding.ASCII.GetString(dataBuf);
                    this.Dispatcher.Invoke(() =>
                    {
                        txtServer.Text = "Client " + text;
                    });


                    string reponse = string.Empty;


                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {

                        if (socket.RemoteEndPoint.ToString().Equals(__ClientSockets[i]._Socket.RemoteEndPoint.ToString()))
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                chat.AppendText("\n" + __ClientSockets[i]._Name + ": " + text);
                            });

                        }
                    }

                    if (text == "bye")
                    {
                        return;
                    }
                    reponse = "Server " + text;
                    Sendata(socket, reponse);
                }
                else
                {
                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (__ClientSockets[i]._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))
                        {
                            __ClientSockets.RemoveAt(i);
                            txtName.Text = "Stevilo clientov aaaaaaaaaaa: " + __ClientSockets.Count.ToString();
                        }
                    }
                }
            }
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }

        void Sendata(Socket socket, string noidung)
        {
            byte[] data = Encoding.ASCII.GetBytes(noidung);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }
        private void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            if (list1.SelectedItem != null)
            {
                Timer = new DispatcherTimer();
                Timer.Interval = new TimeSpan(0, 0, 1);
                Timer.Tick += Timer_tick;
                Timer.Start();
                Progress_Bar(time);

            }
            else
            {

                naslov.Text = "KLIKNI CLIENTA";

            }           

        }

        private void PosljiStevilo(int x)
        {



            Random rand = new Random();
            int randNumb = rand.Next(0, 36);
            //pisi txt
            StreamWriter txtFile = new StreamWriter(@"..\..\..\WSRouletteClient\bin\Debug\ruletaStevila.txt", true);
            txtFile.WriteLine(randNumb);
            txtFile.Close();

            for (int i = 0; i < list1.SelectedItems.Count; i++)
            {
                string t = list1.SelectedItems[i].ToString();
                for (int j = 0; j < __ClientSockets.Count; j++)
                {
                    //if (__ClientSockets[j]._Socket.Connected && __ClientSockets[j]._Name.Equals("@"+t))
                    {
                        Sendata(__ClientSockets[j]._Socket, randNumb.ToString());
                    }
                }
            }



            time = x;
            Timer.Start();
            Progress_Bar(time);

        }



        int checkTime = 30;

        void Timer_tick(object sender, EventArgs e)
        {

            if (time == checkTime)
            {
                for (int i = 0; i < list1.SelectedItems.Count; i++)
                {
                    string t = list1.SelectedItems[i].ToString();
                    for (int j = 0; j < __ClientSockets.Count; j++)
                    {
                        //if (__ClientSockets[j]._Socket.Connected && __ClientSockets[j]._Name.Equals("@"+t))
                        {
                            Sendata(__ClientSockets[j]._Socket, "START");
                        }
                    }
                }
            }

            if (time > 0)
            {

                if (time == endBetTime)
                {
                    pbStatus.Foreground = Brushes.Orange;

                    for (int i = 0; i < list1.SelectedItems.Count; i++)
                    {
                        string t = list1.SelectedItems[i].ToString();
                        for (int j = 0; j < __ClientSockets.Count; j++)
                        {
                            //if (__ClientSockets[j]._Socket.Connected && __ClientSockets[j]._Name.Equals("@"+t))
                            {
                                Sendata(__ClientSockets[j]._Socket, "END OF BETS");
                            }
                        }
                    }



                }
                time--;
                timerT.Text = string.Format("00:0{0}:{1}", time / 60, time % 60);
            }
            else
            {

                Timer.Stop();
                PosljiStevilo(30);//END OF BETS TIME
            }
        }



        //PROGRESS BAR
        private void Progress_Bar(int i)
        {
            pbStatus.Foreground = Brushes.Yellow;
            Duration duration = new Duration(TimeSpan.FromSeconds(i));
            DoubleAnimation doubleanimation = new DoubleAnimation(100, duration, FillBehavior.Stop);
            pbStatus.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);

        }
    }
}
