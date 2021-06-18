using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using BD0.BD;
using BD0.CP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.VisualBasic;


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
            ComPortWorking.Open(NumCom.Text, Int32.Parse(ParityBit.Text),
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
            ComPortWorking.Open(NumCom.Text, Int32.Parse(ParityBit.Text),
                Int32.Parse(BaudRate.Text), Int32.Parse(StopBits.Text), (bool)DTR.IsChecked);
        }

        void SendToCom(Button btn)
        {
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
        }

        async void Actions()
        {
            db.Add( new GetValues(await ComPortWorking.Write(Commands.GetCommand("Return set voltage")),
                await ComPortWorking.Write(Commands.GetCommand("Return set current")), DateAndTime.Now));
        }

        private void GetValue_Click(object sender, RoutedEventArgs e)
        {
            SendToCom(GetValue);
        }
    }

    public class GetValues
    {
        public int Id { get; set; }
        public string Voltage { get; set; }
        public string Ampere { get; set; }
        public DateTime DateTime { get; set; }

        public GetValues( string voltage, string ampere, DateTime dateTime)
        {
          
            Voltage = voltage;
            Ampere = ampere;
            DateTime = dateTime;
        }

        public GetValues()
        {
           
        }
    }

    public class ApplicationContext : DbContext
    {
        //представляет набор сущностей, хранящихся в базе данных
        public DbSet<GetValues> Users { get; set; }

        //Переопределение у класса контекста данных метода
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite(@"DataBaseSupply.db");//В этот метод передается объект DbContextOptionsBuilder,
        // который позволяет создать параметры подключения. Для их создания вызывается метод UseSqlServer, в который передается строка подключения.

    }
}
