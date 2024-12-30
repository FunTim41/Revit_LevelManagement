using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using 楼层管理器.Models;

namespace 楼层管理器.ViewModels
{
    public partial class ReNameViewModel : ObservableRecipient
    {
        List<WinLevel> reNameList;

        public ReNameViewModel()
        {
            IsActive = true;
            WeakReferenceMessenger.Default.Register<ValueChangedMessage<List<WinLevel>>, string>(
                this,
                "要重命名的集合",
                (r, m) =>
                {
                    reNameList = m.Value;
                }
            );
        }

        [ObservableProperty]
        string prifix;

        [ObservableProperty]
        int startNum;

        [ObservableProperty]
        string suffix;

        [RelayCommand]
        void CorrectEdit()
        {
            List<WinLevel> li;
            li = reNameList.OrderByDescending(item => item.Id).ToList();
            if (reNameList.All(i => i.Biao1gao1 <= 0))
            {
                li = reNameList.OrderBy(item => item.Id).ToList();
                ;
            }
            for (int i = 0; i < li.Count(); i++)
            {
                li[i].Name = Prifix + (StartNum + li.Count() - i - 1) + Suffix;
            }
            reNameList = li;
            WeakReferenceMessenger.Default.Send(
                new ValueChangedMessage<List<WinLevel>>(reNameList),
                "已重命名的集合"
            );
            WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(true), "关闭窗口");
        }
    }
}
