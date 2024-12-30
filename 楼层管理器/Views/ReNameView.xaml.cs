using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Windows;
using 楼层管理器.ViewModels;

namespace 楼层管理器.Views
{
    /// <summary>
    /// ReNameView.xaml 的交互逻辑
    /// </summary>
    public partial class ReNameView : Window
    {
        public ReNameView()
        {
            InitializeComponent();
            this.DataContext = new ReNameViewModel();
            WeakReferenceMessenger.Default.Register<ValueChangedMessage<bool>, string>(
               this,
               "关闭窗口",
               (r, m) =>
               {
                   if (m.Value == true)
                   {
                       this.Close();
                   }
               });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

       
    }
}
