using Microsoft.AspNetCore.SignalR.Client;
using Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

using CAD;
using System.Windows.Media.TextFormatting;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UniformResourceLocator _Server = new UniformResourceLocator();

        private UniformResourceLocator _ChatHub = new UniformResourceLocator();

        private HubConnection _HubConnection;
        private bool IsConnected => _HubConnection?.State == HubConnectionState.Connected;

        private bool _IsConnected
        {
            get { return (bool)GetValue(_IsConnectedProperty); }
            set { SetValue(_IsConnectedProperty, value); }
        }

        public static readonly DependencyProperty _IsConnectedProperty = DependencyProperty.Register("_IsConnected", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        private Message _Message = new Message();

        private List<String> _Alias = new List<String>();

        private String _FontColor;

        public MainWindow()
        {
            InitializeComponent();

            RichTextBox.BorderThickness = new Thickness(0, 0, 0, 0);

            _Server.SetDomain("https://FLAME.Solutions/");

            _ChatHub.URL = _Server.Domain + "chathub";

            _Message.User = "Connect";

            TextBox_ChatHub.DataContext = _ChatHub;
            Button_Connect.DataContext = _Message;
            TextBox_Message.DataContext = _Message;
        }

        private void Button_Connect_Click(object _Sender, RoutedEventArgs _E)
        {
            InitializeSignalR(_ChatHub);
        }

        private void Button_Send_Click(object _Sender, RoutedEventArgs _E)
        {
            Send();
        }

        private async Task InitializeSignalR(UniformResourceLocator _ChatHub)
        {
            _HubConnection = new HubConnectionBuilder()
                .WithUrl(_ChatHub.URL)
                .WithAutomaticReconnect()
                .Build();

            _HubConnection.Closed += RefreshDisconnected;
            _HubConnection.Reconnecting += RefreshDisconnected;
            _HubConnection.Reconnected += RefreshConnected;

            _HubConnection.On<Message>("RX", (_Message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (_Message.Content.ToUpper().Trim() == "`")
                    {
                        CustomClientEvent(_Message);
                    }
                    else if (_Message.Content.Contains(Convert.ToChar(30).ToString()))
                    {
                        ParseTable(_Message);
                    }
                    else
                    {
                        AppendMessage(_Message);

                        Message Temp = _CAD.Command(_Message.Content);
                        if (Temp.Content != "")
                        {
                            _HubConnection.SendAsync("TX", Temp);
                            Temp.Content = "";
                        }
                    }
                });
            });

            await _HubConnection.StartAsync();

            if (IsConnected)
            {
                await RefreshConnected("");
            }
        }

        private async Task RefreshConnected(String _S)
        {
            this.Dispatcher.Invoke(() =>
            {
                _IsConnected = true;

                if (_Alias.Count() == 0)
                {
                    _Message.User = _HubConnection.ConnectionId.ToString().ToUpper().Substring(0, 5);
                    _Alias.Add(_Message.User);

                    _Message.Content = "mi nombre es aida";

                    ButtonAutomationPeer Button_Send_Automation = new ButtonAutomationPeer(Button_Send);
                    IInvokeProvider _InvokeProvider = Button_Send_Automation.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                    _InvokeProvider.Invoke();
                }
                else
                {
                    _Message.User = _Alias[_Alias.Count() - 1];
                    _Message.Content = "Connection ID " + _HubConnection.ConnectionId.ToString().ToUpper().Substring(0, 5) + " is me reconnected.";
                    Send();
                }
            });
        }

        private async Task RefreshDisconnected(Exception _E)
        {
            this.Dispatcher.Invoke(() =>
            {
                _IsConnected = false;
                _Message.User = "Connect";
            });
        }

        private async Task Send()
        {
            _Message.Time = DateTime.Now;

            if (_Message.Content != null)
            {
                if (_Message.Content.Trim() == "")
                {
                    return;
                }
                else
                {
                    String _NameCheck = _Message.Content.Trim().ToLower();

                    if (_NameCheck.StartsWith("mi nombre es"))
                    {
                        Char[] _Space = " ".ToCharArray();
                        String[] _NameCheckList = _NameCheck.Split(_Space);
                        if (_NameCheckList.Length == 4)
                        {
                            if (_HubConnection != null)
                            {
                                _Message.Content = "Call me " + _NameCheckList[3].ToUpper() + ".";
                                await _HubConnection.SendAsync("TX", _Message);
                                _Message.Content = "";
                            }

                            _Message.User = _NameCheckList[3].ToUpper().Trim();
                            _Alias.Add(_Message.User);

                            if (_Message.User == "AIDA")
                            {
                                _Message.Content = "I am an Artificial Intelligence Dispatch Agent (AIDA).";
                                await _HubConnection.SendAsync("TX", _Message);
                                _Message.Content = "";
                            }
                        }
                    }
                    else
                    {
                        if (_HubConnection != null)
                        {
                            await _HubConnection.SendAsync("TX", _Message);
                            _Message.Content = "";
                        }
                    }
                }
            }
        }

        private void CustomClientEvent(Message _Message)
        {
            _Message.Content = "<Custom Client Event>";
            AppendMessage(_Message);
        }

        private void AppendMessage(Message _Message)
        {
            _Message.Content = _Message.Content.Trim();

            if (_Message.Content == "")
            {
                return;
            }

            BrushConverter _BrushConverter = new BrushConverter();

            if (_Message.User == this._Message.User || _Alias.Contains(_Message.User))
            {
                _FontColor = "Green";
            }
            else if (_Message.Content == "\u2764")
            {
                _FontColor = "Red";
            }
            else
            {
                _FontColor = "Blue";
            }

            TextRange _TextRange_User = new TextRange(RichTextBox.Document.ContentEnd, RichTextBox.Document.ContentEnd);
            _TextRange_User.Text = _Message.Time.ToString("yyyy-MM-dd_HH:mm:ss.fff") + " " + _Message.User + ": ";
            try
            {
                _TextRange_User.ApplyPropertyValue(TextElement.ForegroundProperty, _BrushConverter.ConvertFromString("Black"));
            }
            catch (FormatException)
            {
            }

            TextRange _TextRange_Message = new TextRange(RichTextBox.Document.ContentEnd, RichTextBox.Document.ContentEnd);
            _TextRange_Message.Text = _Message.Content;
            try
            {
                _TextRange_Message.ApplyPropertyValue(TextElement.ForegroundProperty, _BrushConverter.ConvertFromString(_FontColor));
            }
            catch (FormatException)
            {
            }

            TextRange _TextRange_CR = new TextRange(RichTextBox.Document.ContentEnd, RichTextBox.Document.ContentEnd);
            _TextRange_CR.Text = "\r";
            try
            {
                _TextRange_CR.ApplyPropertyValue(TextElement.ForegroundProperty, _BrushConverter.ConvertFromString("Black"));
            }
            catch (FormatException)
            {
            }

            RichTextBox.ScrollToEnd();
        }

        private void ParseTable(Message _Message)
        {
            String[] _Rows = _Message.Content.Split(Convert.ToChar(30));
            foreach (String _Row in _Rows)
            {
                if (_Row != "")
                {
                    Message _Record = new Message();
                    _Record.Time = _Message.Time;
                    _Record.User = _Message.User;
                    _Record.Content = "";

                    String[] _Columns = _Row.Split(Convert.ToChar(31));
                    foreach (String _Column in _Columns)
                    {
                        if (_Column != "")
                        {
                            _Record.Content = _Record.Content + " " + _Column;
                        }
                    }
                    AppendMessage(_Record);
                }
            }
        }

        public CAD.CAD _CAD = new CAD.CAD();
    }

    public class BoolInverter : IValueConverter
    {
        public object Convert(object Value, Type TargetType, object Parameter, System.Globalization.CultureInfo Culture)
        {
            return !(bool)Value;
        }

        public object ConvertBack(object Value, Type TargetType, object Parameter, System.Globalization.CultureInfo Culture)
        {
            throw new NotImplementedException();
        }
    }
}
