using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Windows;

namespace 楼层管理器
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        public MainView(Autodesk.Revit.UI.ExternalCommandData commandData)
        {
            InitializeComponent();
            this.DataContext = new MainViewModel(commandData);
            this.Closing += MainView_Closing;
        }

        private void MainView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(true), "关闭窗口");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
