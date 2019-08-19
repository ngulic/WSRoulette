using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

namespace WSRouletteClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static int[] red_nums = { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
        static int[] black_nums = { 2, 4, 6, 8, 10, 11, 13, 15, 17, 20, 22, 24, 26, 28, 29, 31, 33, 35 };
        private Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        byte[] receivedBuf = new byte[1024];
        int credit = 1000;
        int stanje = 0;
        string link = "";
        int izbranCoin = 1;
        int stava = 0;
        int dobitek = 0;
        private DispatcherTimer Timer;
        int novaigra = 0;
        int timer = 30;
        int lastBet = 0;
        int lastWin = 0;
        int[] array = new int[49];//48 različnih polj
        /*
         0-36 števila
         37 1-2
         38 13-24
         39 25-36
         40 1-18    
         41 odd
         42 red 
         43 black
         44 even
         45 19-36
         46 2to1 3-36
         47 2to1 2-35
         48 2to1 1-34
             
             */
        Thread thr;

        public MainWindow()
        {
            InitializeComponent();


            // SendLoop();
            //lCredit.Content = credit;
            /*LoopConnect();
            _clientSocket.BeginReceive(receivedBuf, 0, receivedBuf.Length, SocketFlags.None, new AsyncCallback(ReceiveData), _clientSocket);
            byte[] buffer = Encoding.ASCII.GetBytes("povezan");
            _clientSocket.Send(buffer);*/

            btnCoin1.Background = Brushes.White;
            lCredit.Content = credit;

            

        }


        ObservableCollection<String> names = new ObservableCollection<String>();
        private void zgodovina()
        {

            // lwPrikazDobitnihStevil.ref
            string[] fileLines = File.ReadAllLines(@"ruletaStevila.txt", Encoding.UTF8);
            //lizpiszst.Content = fileLines[fileLines.Length - 1];
            for (int i = 0; i < fileLines.Length; i++)
            {
                ListViewItem l = new ListViewItem();

                l.Content = fileLines;
                lwPrikazDobitnihStevil.Items.Add(fileLines[fileLines.Length - 1 - i]);
                var lvitem = lwPrikazDobitnihStevil.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                int x = Int32.Parse(fileLines[fileLines.Length - 1 - i]);

                foreach (int num in red_nums)
                {

                    if (num == x)
                    {
                        lvitem.Background = Brushes.Red;
                        break;
                    }
                    else if (x == 0)
                    {
                        lvitem.Background = Brushes.Green;

                    }
                    else if (num != x)
                    {
                        lvitem.Background = Brushes.Black;
                    }

                }


            }





        }

        private void ReceiveData(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            int received = socket.EndReceive(ar);
            byte[] dataBuf = new byte[received];
            Array.Copy(receivedBuf, dataBuf, received);
            this.Dispatcher.Invoke(() =>
            {


                if (stanje != 0)
                {

                    string h = (Encoding.ASCII.GetString(dataBuf)); //txtServer.Text;

                    if (h == "END OF BETS")
                    {
                        txtText.Text = "NO MORE BETS";
                        pbStatus.Foreground = Brushes.Orange;
                        lWin.Content = "0";

                        disableBets();
                    }
                    else if (h == "START")
                    {
                        zgodovina();

                        txtText.Text = "PLACE YOUR BETS";
                        pbStatus.Foreground = Brushes.Yellow;
                        Progress_Bar(timer);
                        enableBets();
                    }
                    else
                    {

                        int i = Int32.Parse(h);
                        izpisCredita(i);//TEST
                        DisplayColor(i, red_nums);//TEST
                        //tbWinNumb.Text = (DisplayColor(3, red_nums)).to; //i.ToString();
                        //ZadnjaStevila();
                        resetGame();


                    }
                }
                stanje = 1;
            });
            _clientSocket.BeginReceive(receivedBuf, 0, receivedBuf.Length, SocketFlags.None, new AsyncCallback(ReceiveData), _clientSocket);
        }

        private void resetGame()
        {
            for (int num = 0; num < array.Length; num++)
            {
                array[num] = 0;
            }
            lastBet = stava;
            lLastBet.Content = lastBet;
            lastWin = dobitek;
            lLastWin.Content = lastWin;
            stava = 1;
            lStava.Content = 0;
            dobitek = 0;

            btn0.Content = "";
            l0.Content = 0;
            btn1.Content = "";
            l1.Content = 1;
            btn2.Content = "";
            l2.Content = 2;
            btn3.Content = "";
            l3.Content = 3;
            btn4.Content = "";
            l4.Content = 4;
            btn5.Content = "";
            l5.Content = 5;
            btn6.Content = "";
            l6.Content = 6;
            btn7.Content = "";
            l7.Content = 7;
            btn8.Content = "";
            l8.Content = 8;
            btn9.Content = "";
            l9.Content = 9;
            btn1_12.Content = "";
            l1_12.Content = "1-12";
            btn13_24.Content = "";
            l13_24.Content = "13-24";
            btn25_36.Content = "";
            l25_36.Content = "25-36";
            btn1_18.Content = "";
            l1_18.Content = "1-18";
            btnOdd.Content = "";
            lOdd.Content = "ODD";
            btnRed.Content = "";
            lRed.Content = "RED";
            btnBlack.Content = "";
            lBlack.Content = "BLACK";
            btnEven.Content = "";
            lEven.Content = "EVEN";
            btn19_36.Content = "";
            l19_36.Content = "19-36";
            btnTo1_1.Content = "";
            lTo1_1.Content = "2 to 1";
            btnTo1_2.Content = "";
            lTo1_2.Content = "2 to 1";
            btnTo1_3.Content = "";
            lTo1_3.Content = "2 to 1";
        }

        private void disableBets()
        {
            izbranCoin = 0;
            btnCoin1.IsEnabled = false;
            btnCoin2.IsEnabled = false;
            btnCoin3.IsEnabled = false;
            btnCoin4.IsEnabled = false;
            btnCoin5.IsEnabled = false;

            lCoin1.IsEnabled = false;
            lCoin2.IsEnabled = false;
            lCoin3.IsEnabled = false;
            lCoin4.IsEnabled = false;
            lCoin5.IsEnabled = false;
        }
        private void enableBets()
        {
            izbranCoin = 1;
            btnCoin1.IsEnabled = true;
            btnCoin2.IsEnabled = true;
            btnCoin3.IsEnabled = true;
            btnCoin4.IsEnabled = true;
            btnCoin5.IsEnabled = true;

            lCoin1.IsEnabled = true;
            lCoin2.IsEnabled = true;
            lCoin3.IsEnabled = true;
            lCoin4.IsEnabled = true;
            lCoin5.IsEnabled = true;
        }

        /*
private void SendLoop()
{
   while (true)
   {
       //Console.WriteLine("Enter a request: ");
       //string req = Console.ReadLine();
       //byte[] buffer = Encoding.ASCII.GetBytes(req);
       //_clientSocket.Send(buffer);

       byte[] receivedBuf = new byte[1024];
       int rev = _clientSocket.Receive(receivedBuf);
       if (rev != 0)
       {
           byte[] data = new byte[rev];
           Array.Copy(receivedBuf, data, rev);
           txtServer.Text = ("Received:  sendloop" + Encoding.ASCII.GetString(data));
           chat.AppendText("\nServer:  sendloop " + Encoding.ASCII.GetString(data));
       }
       else _clientSocket.Close();

   }
}*/

        private void LoopConnect()
        {
            int attempts = 0;
            while (!_clientSocket.Connected)
            {
                try
                {
                    attempts++;
                    _clientSocket.Connect(IPAddress.Loopback, 3333);
                }
                catch (SocketException)
                {
                    //Console.Clear();
                    txtText.Text = ("Connection attempts: LoopConect " + attempts.ToString());
                }
            }
            // txtServer.Text = ("Connected! LoopConect");
        }
        
        //PRIKAZ  ZMAGOVALNEGA ŠTEVILA
        private void DisplayColor(int randNumb, int[] array)
        {
            foreach (int num in array)
            {
                tbWinNumb.Text = randNumb.ToString();
                if (num == randNumb)
                {
                    tbWinNumb.Background = new SolidColorBrush(Colors.Red);
                    break;
                }
                else if (randNumb == 0)
                {
                    tbWinNumb.Background = new SolidColorBrush(Colors.Green);

                }
                else if (num != randNumb)
                {
                    tbWinNumb.Background = new SolidColorBrush(Colors.Black);
                }

            }
        }

        

        private void izpisCredita(int dobitnaSt)
        {
            //dobi st
            //če je barva izbrana preveri barvo
            //če je katero polje izbranov v spodnjih in stranskih vrsticah preveri
            if (dobitnaSt != 0)
            {
                preveriBarvo(dobitnaSt);
                preveriOddEven(dobitnaSt);
                preveriLowHigh(dobitnaSt);
                preveriStolpec(dobitnaSt);
                preveriVrstico(dobitnaSt);
            }

            dobitek = dobitek + (array[dobitnaSt] * 36);

            array[dobitnaSt] = 0;
            int odbitek = 0;

            for (int i = 0; i < array.Length; i++)
            {
                odbitek = odbitek - array[i];
            }
            //lIzpis.Content = odbitek;
            if (dobitek > 0)
            {
                credit = credit + dobitek + odbitek;
            }
            else
            {
                credit = credit + dobitek;
            }

            //credit = credit + dobitek + odbitek;
            odbitek = 0;
            lWin.Content = dobitek;
            lCredit.Content = credit;
        }



        private void preveriBarvo(int dobitnaSt)
        {
            int r = 0;
            for (int j = 0; j < red_nums.Length; j++)
            {
                if (dobitnaSt == red_nums[j])
                {
                    dobitek = dobitek + array[42] * 2;//RED
                    array[42] = 0;
                    r = 1;
                }
            }

            if (r == 0)
            {
                dobitek = dobitek + array[43] * 2;//BLACK
                array[43] = 0;
            }

        }
        private void preveriOddEven(int dobitnaSt)
        {
            if (dobitnaSt % 2 == 0)
            {
                dobitek = dobitek + array[44] * 2;//EVEN
                array[44] = 0;
            }
            else
            {
                dobitek = dobitek + array[41] * 2;//ODD
                array[41] = 0;
            }
        }
        private void preveriLowHigh(int dobitnaSt)
        {
            if (dobitnaSt <= 18)
            {
                dobitek = dobitek + array[40] * 2;//LOW
                array[40] = 0;
            }
            else
            {
                dobitek = dobitek + array[45] * 2;//HIGH
                array[45] = 0;
            }
        }

        private void preveriStolpec(int dobitnaSt)
        {
            if (dobitnaSt <= 12)
            {
                dobitek = dobitek + array[37] * 3;//1-12
                array[37] = 0;
            }
            else if (dobitnaSt > 12 && dobitnaSt <= 24)
            {
                dobitek = dobitek + array[38] * 3;//13-24
                array[38] = 0;
            }
            else
            {
                dobitek = dobitek + array[39] * 3;//25-36
                array[39] = 0;
            }
        }
        private void preveriVrstico(int dobitnaSt)
        {
            if (dobitnaSt % 3 == 0)
            {
                dobitek = dobitek + array[46] * 3;//3-36
                array[46] = 0;
            }
            else if (dobitnaSt % 3 == 2)
            {
                dobitek = dobitek + array[47] * 3;//2-35
                array[47] = 0;
            }
            else
            {
                dobitek = dobitek + array[48] * 3;//1-34
                array[48] = 0;
            }
        }

        private void preveriBarvostave(int visinaStave)
        {
            if (visinaStave < 5)
            {
                link = "images/chip1.png";
            }
            else if (visinaStave < 10)
            {
                link = "images/chip5.png";
            }
            else if (visinaStave < 50)
            {
                link = "images/chip10.png";
            }
            else if (visinaStave < 100)
            {
                link = "images/chip50.png";
            }
            else
            {
                link = "images/chip100.png";
            }
        }

        private void Btn1_12_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {

                if (credit >= izbranCoin)
                {
                    array[37] = array[37] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[37];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btn1_12.Content = img;
                    l1_12.Content = array[37];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }
        }

        private void Btn13_24_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {

                if (credit >= izbranCoin)
                {
                    array[38] = array[38] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[38];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btn13_24.Content = img;
                    l13_24.Content = array[38];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }
        }

        private void Btn25_36_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {
                lCredit.Content = credit;
                if (credit >= izbranCoin)
                {
                    array[39] = array[39] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[39];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btn25_36.Content = img;
                    l25_36.Content = array[39];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }
        }

        private void Btn1_18_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {
                lCredit.Content = credit;
                if (credit >= izbranCoin)
                {
                    array[40] = array[40] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[40];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btn1_18.Content = img;
                    l1_18.Content = array[40];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }
        }

        private void BtnOdd_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {
                lCredit.Content = credit;
                if (credit >= izbranCoin)
                {
                    array[41] = array[41] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[41];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btnOdd.Content = img;
                    lOdd.Content = array[41];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }
        }

        private void BtnRed_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {
                lCredit.Content = credit;
                if (credit >= izbranCoin)
                {
                    array[42] = array[42] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[42];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btnRed.Content = img;
                    lRed.Content = array[42];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }

        }

        private void BtnBlack_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {
                lCredit.Content = credit;
                if (credit >= izbranCoin)
                {
                    array[43] = array[43] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[43];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btnBlack.Content = img;
                    lBlack.Content = array[43];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }
        }

        private void BtnEven_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {
                lCredit.Content = credit;
                if (credit >= izbranCoin)
                {
                    array[44] = array[44] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[44];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btnEven.Content = img;
                    lEven.Content = array[44];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }
        }

        private void Btn19_36_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {
                lCredit.Content = credit;
                if (credit >= izbranCoin)
                {
                    array[45] = array[45] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[45];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btn19_36.Content = img;
                    l19_36.Content = array[45];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }
        }

        private void BtnTo1_1_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {
                lCredit.Content = credit;
                if (credit >= izbranCoin)
                {
                    array[46] = array[46] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[46];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btnTo1_1.Content = img;
                    lTo1_1.Content = array[46];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }
        }

        private void BtnTo1_2_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {
                lCredit.Content = credit;
                if (credit >= izbranCoin)
                {
                    array[47] = array[47] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[47];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btnTo1_2.Content = img;
                    lTo1_2.Content = array[47];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }

        }

        private void BtnTo1_3_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {
                lCredit.Content = credit;
                if (credit >= izbranCoin)
                {
                    array[48] = array[48] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[48];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btnTo1_3.Content = img;
                    lTo1_3.Content = array[48];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }
        }

        private void BtnCoin1_Click(object sender, RoutedEventArgs e)
        {
            izbranCoin = 1;
            btnCoin1.Background = Brushes.White;
            btnCoin2.Background = Brushes.Transparent;
            btnCoin3.Background = Brushes.Transparent;
            btnCoin4.Background = Brushes.Transparent;
            btnCoin5.Background = Brushes.Transparent;
        }

        private void BtnCoin2_Click(object sender, RoutedEventArgs e)
        {
            izbranCoin = 5;
            btnCoin1.Background = Brushes.Transparent;
            btnCoin2.Background = Brushes.White;
            btnCoin3.Background = Brushes.Transparent;
            btnCoin4.Background = Brushes.Transparent;
            btnCoin5.Background = Brushes.Transparent;
        }

        private void BtnCoin3_Click(object sender, RoutedEventArgs e)
        {
            izbranCoin = 10;
            btnCoin1.Background = Brushes.Transparent;
            btnCoin2.Background = Brushes.Transparent;
            btnCoin3.Background = Brushes.White;
            btnCoin4.Background = Brushes.Transparent;
            btnCoin5.Background = Brushes.Transparent;
        }

        private void BtnCoin4_Click(object sender, RoutedEventArgs e)
        {
            izbranCoin = 50;
            btnCoin1.Background = Brushes.Transparent;
            btnCoin2.Background = Brushes.Transparent;
            btnCoin3.Background = Brushes.Transparent;
            btnCoin4.Background = Brushes.White;
            btnCoin5.Background = Brushes.Transparent;
        }

        private void BtnCoin5_Click(object sender, RoutedEventArgs e)
        {
            izbranCoin = 100;
            btnCoin1.Background = Brushes.Transparent;
            btnCoin2.Background = Brushes.Transparent;
            btnCoin3.Background = Brushes.Transparent;
            btnCoin4.Background = Brushes.Transparent;
            btnCoin5.Background = Brushes.White;
        }

        private void Btn0_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {
                lCredit.Content = credit;
                if (credit >= izbranCoin)
                {
                    array[0] = array[0] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[0];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btn0.Content = img;
                    l0.Content = array[0];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }
        }

        private void Btn1_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {
                lCredit.Content = credit;
                if (credit >= izbranCoin)
                {
                    array[1] = array[1] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[1];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btn1.Content = img;
                    l1.Content = array[1];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }
        }

        private void Btn2_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {
                lCredit.Content = credit;
                if (credit >= izbranCoin)
                {
                    array[2] = array[2] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[2];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btn2.Content = img;
                    l2.Content = array[2];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }
        }

        private void Btn3_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {
                lCredit.Content = credit;
                if (credit >= izbranCoin)
                {
                    array[3] = array[3] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[3];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btn3.Content = img;
                    l3.Content = array[3];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }
        }

        private void Btn4_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {
                lCredit.Content = credit;
                if (credit >= izbranCoin)
                {
                    array[4] = array[4] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[4];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btn4.Content = img;
                    l4.Content = array[4];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }
        }

        private void Btn5_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {
                lCredit.Content = credit;
                if (credit >= izbranCoin)
                {
                    array[5] = array[5] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[5];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btn5.Content = img;
                    l5.Content = array[5];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }
        }

        private void Btn6_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {
                lCredit.Content = credit;
                if (credit >= izbranCoin)
                {
                    array[6] = array[6] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[6];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btn6.Content = img;
                    l6.Content = array[6];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }
        }

        private void Btn7_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {
                lCredit.Content = credit;
                if (credit >= izbranCoin)
                {
                    array[7] = array[7] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[7];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btn7.Content = img;
                    l7.Content = array[7];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }
        }

        private void Btn8_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {
                lCredit.Content = credit;
                if (credit >= izbranCoin)
                {
                    array[8] = array[8] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[8];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btn8.Content = img;
                    l8.Content = array[8];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
            }
        }

        private void Btn9_Click(object sender, RoutedEventArgs e)
        {
            if (izbranCoin > 0)
            {
                lCredit.Content = credit;
                if (credit >= izbranCoin)
                {
                    array[9] = array[9] + izbranCoin;
                    stava = stava + izbranCoin;
                    lStava.Content = stava;
                    credit = credit - izbranCoin;
                    lCredit.Content = credit;
                    Image img = new Image();
                    int x = array[9];
                    preveriBarvostave(x);

                    var uri = new Uri(@link, UriKind.Relative);
                    img.Source = new BitmapImage(uri);
                    btn9.Content = img;
                    l9.Content = array[9];

                }
                else
                {
                    lIzpis.Content = "Not enaught credits";
                }
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

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LoopConnect();
            _clientSocket.BeginReceive(receivedBuf, 0, receivedBuf.Length, SocketFlags.None, new AsyncCallback(ReceiveData), _clientSocket);
            byte[] buffer = Encoding.ASCII.GetBytes("povezan");
            _clientSocket.Send(buffer);
            client.IsEnabled = false;
            client.Foreground = Brushes.White;
        }
    }
}
