using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Revit.Async;
using 楼层管理器.Models;
using 楼层管理器.Views;

namespace 楼层管理器
{
    partial class MainViewModel : ObservableRecipient
    {
        private ExternalCommandData commandData;
        public ICollectionView ItemsView { get; set; }
        UIDocument uidoc;
        Document doc;

        public MainViewModel(ExternalCommandData commandData)
        {
            IsActive = true;
            this.commandData = commandData;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            LoadRevitLevel();
            RefreshCollection();
            WeakReferenceMessenger.Default.Register<ValueChangedMessage<List<WinLevel>>, string>(
                this,
                "已重命名的集合",
                (r, m) =>
                {
                    foreach (var item in m.Value)
                    { //更新名字
                        var match = WinLevelList.FirstOrDefault(it => it.Id == item.Id);
                        if (match != null)
                        {
                            match.Name = item.Name;
                        }
                    }
                    ItemsView.Refresh();
                }
            );

            WinLevelList.CollectionChanged += WinLevelList_CollectionChanged;

            foreach (var item in WinLevelList)
            {
                item.PropertyChanged += Lev_PropertyChanged;
            }
        }

        /// <summary>
        /// 加载Revit中的标高
        /// </summary>
        private void LoadRevitLevel()
        {
            var RevLev = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(i => i.Elevation)
                .ToList();
            WinLevelList = new ObservableCollection<WinLevel>();
            for (int i = 0; i < RevLev.Count(); i++)
            {
                WinLevel lv = new WinLevel();
                lv.Id = i + 1;
                lv.Name = RevLev[i].Name;
                if (i + 1 < RevLev.Count())
                {
                    lv.Ceng2gao1 = Math.Round(
                        UnitUtils.ConvertFromInternalUnits(
                            RevLev[i + 1].Elevation - RevLev[i].Elevation,
                            DisplayUnitType.DUT_METERS
                        ),
                        3
                    );
                }
                else
                {
                    lv.Ceng2gao1 = 0;
                }
                lv.Biao1gao1 = Math.Round(
                    UnitUtils.ConvertFromInternalUnits(
                        RevLev[i].Elevation,
                        DisplayUnitType.DUT_METERS
                    ),
                    3
                );
                WinLevelList.Add(lv);
            }
        }

        private void WinLevelList_CollectionChanged(
            object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e
        )
        {
            foreach (var item in WinLevelList)
            {
                item.PropertyChanged -= Lev_PropertyChanged;
            }
            foreach (var item in WinLevelList)
            {
                item.PropertyChanged += Lev_PropertyChanged;
            }
        }

        private void Lev_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                RefreshAllLev();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "123");
                return;
            }
        }

        void RefreshCollection()
        {
            ItemsView = CollectionViewSource.GetDefaultView(WinLevelList);
            ItemsView.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Descending));
        }

        private void RefreshGrid()
        { // 重新排序ID
            for (int i = 0; i < WinLevelList.Count; i++)
            {
                WinLevelList[i].Id = i + 1;
            }
            ItemsView.Refresh();
            //MessageBox.Show("表格已刷新");
            RefreshAllLev();
        }

        void RefreshLevel(int v)
        {
            try
            {
                if (WinLevelList.Count > 1)
                {
                    WinLevel Lv0 = WinLevelList.FirstOrDefault(i => i.Biao1gao1 == 0);
                    if (v == -1 && WinLevelList.Any(i => i.Biao1gao1 < 0))
                    {
                        for (int i = Lv0.Id - 1; i > 0; i--)
                        {
                            WinLevelList[i - 1].Biao1gao1 =
                                WinLevelList[i].Biao1gao1 - WinLevelList[i - 1].Ceng2gao1;
                        }
                    }
                    if (v == 1 && WinLevelList.Any(i => i.Biao1gao1 > 0))
                    {
                        WinLevelList[Lv0.Id].Biao1gao1 =
                            WinLevelList[Lv0.Id - 1].Biao1gao1 + WinLevelList[Lv0.Id - 1].Ceng2gao1;
                        for (int i = Lv0.Id; i < WinLevelList.Count - 1; i++)
                        {
                            WinLevelList[i + 1].Biao1gao1 =
                                WinLevelList[i].Biao1gao1 + WinLevelList[i].Ceng2gao1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "标高计算");
                return;
            }
        }

        void RefreshAllLev()
        {
            if (WinLevelList.All(i => i.Ceng2gao1 >= 0.0))
            {
                RefreshLevel(-1);
                RefreshLevel(+1);
            }
            if (WinLevelList.Last().Ceng2gao1 != 0)
            {
                NewLev0(0);
                WinLevelList.Last().Name = "屋顶";
            }
        }

        [ObservableProperty]
        ObservableCollection<WinLevel> winLevelList;

        [ObservableProperty]
        WinLevel selectedLev;

        [ObservableProperty]
        List<WinLevel> selectedLevList;

        [ObservableProperty]
        string prifix;

        [ObservableProperty]
        string suffix;

        [ObservableProperty]
        int startNum;

        [ObservableProperty]
        double height;

        [ObservableProperty]
        int heightNum;

        //获得多选List
        [RelayCommand]
        void SelectedChanged(IList<object> selectedItems)
        {
            SelectedLevList = new List<WinLevel>();
            try
            {
                foreach (var item in selectedItems)
                {
                    SelectedLevList.Add(item as WinLevel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Tip2");
                return;
            }
        }

        [RelayCommand]
        void AddUnderSel()
        {
            if (SelectedLev != null)
            {
                NewLevel(-1);
            }
            else
            {
                MessageBox.Show("请选择一个标高", "Tip");
                //NewLev0();
            }
        }

        [RelayCommand]
        void AddOnSel()
        {
            if (SelectedLev != null)
            {
                NewLevel(+1);
            }
            else
            {
                MessageBox.Show("请选择一个标高", "Tip");
                // NewLev0();
            }
        }

        //如果WinLevelList为0，新建一个标高
        void NewLev0(int v)
        {
            WinLevel newlev0 = new WinLevel();
            newlev0.Id = WinLevelList.Last().Id + 1;
            newlev0.Name = "New Level";
            newlev0.Ceng2gao1 = v;
            newlev0.Biao1gao1 = WinLevelList.Last().Biao1gao1 + WinLevelList.Last().Ceng2gao1;
            WinLevelList.Add(newlev0);
        }

        /// <summary>
        /// 新建Level
        /// </summary>
        /// <param name="i">层数量</param>
        /// <returns></returns>
        void NewLevel(int v)
        {
            if (SelectedLev.Ceng2gao1 == 0)
            {
                v = -1;
            }
            //MessageBox.Show(WinLevelList.Last().Name);
            for (int i = 0; i < HeightNum; i++)
            {
                WinLevel newlev = new WinLevel();
                if (Height <= 0)
                {
                    MessageBox.Show("层高必须大于0");
                    return;
                }
                newlev.Ceng2gao1 = Height;
                newlev.Biao1gao1 = v;
                if (v == -1)
                {
                    newlev.Name = Prifix + (StartNum + i) + Suffix;
                    WinLevelList.Insert(SelectedLev.Id - 1, newlev);
                }
                if (v == +1)
                {
                    newlev.Name = Prifix + (StartNum + HeightNum - 1 - i) + Suffix;
                    WinLevelList.Insert(SelectedLev.Id, newlev);
                }
            }

            RefreshGrid();
        }

        //删除
        [RelayCommand]
        void DeleteSel()
        {
            try
            {
                SelectedLevList.RemoveAll(i => i.Biao1gao1 == 0);

                foreach (var item in SelectedLevList)
                {
                    WinLevelList.Remove(item);
                }

                RefreshGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Tip1");
                return;
            }
        }

        //重命名
        [RelayCommand]
        void ReName()
        {
            try
            {
                if (SelectedLev != null || SelectedLevList != null)
                {
                    ReNameView reNameView = new ReNameView();
                    WeakReferenceMessenger.Default.Send(
                        new ValueChangedMessage<List<WinLevel>>(SelectedLevList),
                        "要重命名的集合"
                    );
                    reNameView.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Tip3");
                return;
            }
        }

        /// <summary>
        /// 在revit创建标高
        /// </summary>
        [RelayCommand]
        async Task CreateLevelFroRevit()
        {
            await RevitTask.RunAsync(uiapp =>
            {
                using (Transaction t = new Transaction(doc, "新建标高"))
                {
                    t.Start();
                    try
                    {
                        bool hasDuplicateNames = WinLevelList
                            .GroupBy(p => p.Name)
                            .Any(g => g.Count() > 1);
                        if (hasDuplicateNames)
                        {
                            TaskDialog.Show("Tip", "存在同名标高，请修改");
                            return;
                        }
                        if (WinLevelList.Count(i => i.Ceng2gao1 == 0) > 1)
                        {
                            TaskDialog.Show("Tip", "中间层高必须大于0，请修改");
                            return;
                        }
                        var RevLev = new FilteredElementCollector(doc)
                            .OfClass(typeof(Level))
                            .Cast<Level>()
                            .OrderBy(i => i.Elevation)
                            .ToList();
                        var lv0 = RevLev.Find(i => i.Elevation == 0);
                        RevLev.Remove(lv0);
                        foreach (var item in RevLev)
                        {
                            doc.Delete(item.Id);
                        }
                        var RevLevType = new FilteredElementCollector(doc)
                            .OfClass(typeof(LevelType))
                            .Cast<LevelType>()
                            .ToList();
                        foreach (var item in WinLevelList)
                        {
                            if (item.Biao1gao1 == 0)
                            {
                                lv0.Name = item.Name;
                                continue;
                            }
                            var newlevel = Level.Create(
                                doc,
                                UnitUtils.ConvertToInternalUnits(
                                    item.Biao1gao1,
                                    DisplayUnitType.DUT_METERS
                                )
                            );
                            newlevel.Name = item.Name;
                            if (item.Biao1gao1 < 0)
                            {
                                newlevel.ChangeTypeId(RevLevType.First(i => i.Name == "下标头").Id);
                            }
                        }

                        t.Commit();
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("Tip", ex.Message);
                        t.RollBack();
                    }
                }
            });
        }
    }
}
