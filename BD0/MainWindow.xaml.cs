using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using BD0.CP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.VisualBasic;
using static BD0.BD.Commands;
using static BD0.CP.ComPortWorking;


namespace BD0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///


    public partial class MainWindow : Window
    {
        private static DispatcherTimer MyTimer;

        private ApplicationContext db;//перменная для ипсолзования баз данных

        public MainWindow()
        {
            InitializeComponent();
            SettingsSerializer.InitSettings();//создаем файл с настройками
            //по умолчанию если такого файла еше нет
            db = new ApplicationContext();

            var users = db.Users.OrderBy(b => b.Id);//выберем первый элемент из списка базы данных
            foreach (var user in users)
            {
                if (users.Last() != user)
                {
                    Debug.WriteLine($"{user.Id} {user.Voltage} {user.Ampere} {user.DateTime}");//выведем на коносль элемент из списка базы данных
                    DataSupply.Items.Add(user);
                }

            }

        }

        private void NumCom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void BaudRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void ParityBit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void StopBits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void FlowControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            //при запуске берется из файла индексы конфигуратора и записываются в индексы комобоксов
            NumCom.SelectedIndex = SettingsSerializer.Deserialize().ChannelNum;
            ParityBit.SelectedIndex = SettingsSerializer.Deserialize().ParityBit;
            BaudRate.SelectedIndex = SettingsSerializer.Deserialize().BaudRate;
            StopBits.SelectedIndex = SettingsSerializer.Deserialize().StopBits;
            DTR.IsChecked = SettingsSerializer.Deserialize().DTR;
            //запуск компортра с выписаными параметрами из комобоксов
            StartSettings();
        }


        void StartSettings()
        {
            //записывааем в компорт занчения из комобоксов и открываем его (компорт)
            //string num, int parity, int baud,  int stop, bool dtr
            Open(NumCom.Text, Int32.Parse(ParityBit.Text),
                Int32.Parse(BaudRate.Text), Int32.Parse(StopBits.Text), (bool)DTR.IsChecked);
        }
        //считвываем значения индексов из комобоксов и отправляем их в класс конфига
        public ComConfig SettingsSendOrShange()
        {
            var com = NumCom.SelectedIndex;
            var parity = ParityBit.SelectedIndex;
            var baud = BaudRate.SelectedIndex;
            var stops = StopBits.SelectedIndex;
            //TextBlock flow = (TextBlock)NumCom.SelectedValue;
            var dtr = (bool)DTR.IsChecked;

            //присваиваем индексы из комобоксов в класс конфига
            return new ComConfig
            {
                ChannelNum = com,
                BaudRate = baud,
                ParityBit = parity,
                StopBits = stops,
                DTR = dtr
            };

        }
        //кнопка принять 
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            //записываем новые значения в файл конфига
            SettingsSerializer.Serialize(SettingsSendOrShange());
            //если порт открыть закрывем его
            ComPortWorking.Close();
            //записываем значения из комобоксов в компорт и открываем его
            Open(NumCom.Text, Int32.Parse(ParityBit.Text),
                Int32.Parse(BaudRate.Text), Int32.Parse(StopBits.Text), (bool)DTR.IsChecked);
        }

        void SendToCom(Button btn)
        {
            //var users = db.Users.OrderBy(b => b.Id);//выберем первый элемент из списка базы данных
            //foreach (var user in users)
            //{
            //    if (users.Last() != user)
            //    {
            //        Debug.WriteLine($"{user.Id} {user.Voltage} {user.Ampere} {user.DateTime}");//выведем на коносль элемент из списка базы данных
                   
            //    }

            //}
            SetMyTimer(new TimeSpan(0, 0, 0, 1));//утанвалвиваем таймер
        }

        void SetMyTimer(TimeSpan time)
        {
            MyTimer = new DispatcherTimer();
            MyTimer.Interval = time;
            MyTimer.Start();
            MyTimer.Tick += new EventHandler(timer_Tick);
        }

        private void timer_Tick(object? sender, EventArgs e)
        {
            Actions();
            SendToTable();
        }

        private GetValues GetVal;

        async void Actions()
        {
            GetVal = new GetValues(await Write(GetCommand("Return voltage"), 200),
                await Write(GetCommand("Return current"), 200));

            db.Add(GetVal);
            db.SaveChanges();

        }

        private void GetValue_Click(object sender, RoutedEventArgs e)
        {
            SendToCom(GetValue);

        }

        public async void Out_Click(object sender, RoutedEventArgs e)
        {
            var timerEnabl = true;
            if (MyTimer != null && MyTimer.IsEnabled)
            {
                MyTimer.Stop();
                timerEnabl = false;
            }
            var getAnswer = await Write(GetCommand("Get Output"), 200);
            if (getAnswer == "0")
            {

                await Write(":outp:stat 1", 200);
                StatusButtonOn(Ind, true);
            }
            else
            {
                await Write(":outp:stat 0", 200);
                StatusButtonOn(Ind, false);
            }

            if (!timerEnabl)
            {
                MyTimer.Start();
            }
        }

        void StatusButtonOn(Button name, bool activate)
        {
            if (activate)
            {
                name.Background = Brushes.Green;
            }

            if (!activate)
            {
                name.Background = Brushes.Red;
            }

        }

        void SendToTable()
        {
            var users = db.Users.OrderBy(b => b.Id).Last();//выберем первый элемент из списка базы данных

            Debug.WriteLine($"{users.Id} {users.Voltage} {users.Ampere} {users.DateTime}");//выведем на коносль элемент из списка базы данных
            DataSupply.Items.Add(users);
            DataSupply.ScrollIntoView(users);
        }

      
    }

    public class GetValues
    {
        public int Id { get; set; }
        public string Voltage { get; set; }
        public string Ampere { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;

        public GetValues(string voltage, string ampere) //,DateTime dateTime)
        {

            Voltage = voltage;
            Ampere = ampere;
            //DateTime = dateTime;
        }

        public GetValues()
        {

        }
    }

    public class ApplicationContext : DbContext
    {
        //представляет набор сущностей, хранящихся в базе данных
        public DbSet<GetValues> Users { get; set; }
        private string DBPath = "DataBaseSupply.db";

        public ApplicationContext()
        {
            if (!File.Exists(DBPath))
            {
                Database.EnsureCreated();
            }

        }

        //Переопределение у класса контекста данных метода
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source = {DBPath}"); //В этот метод передается объект DbContextOptionsBuilder,

        // который позволяет создать параметры подключения. Для их создания вызывается метод UseSqlServer, в который передается строка подключения.
    }
}

