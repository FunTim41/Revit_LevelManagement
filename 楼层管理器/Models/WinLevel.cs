using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace 楼层管理器.Models
{
    public partial class WinLevel : ObservableValidator
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [Range(0, 50, ErrorMessage = "请确保层高必须\n大于等于零，且单位为m")]
        [ObservableProperty]
        double ceng2gao1;

        partial void OnCeng2gao1Changed(double value)
        {
            ValidateAllProperties();
            if (HasErrors)
            {
                return;
            }
        }

        [ObservableProperty]
        double biao1gao1;
    }
}
