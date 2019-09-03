using System.ComponentModel;

namespace ProjectManageServer.Model
{
    public class PagesModel : INotifyPropertyChanged
    {

        private int _Currentpage = 1;

        public int Currentpage { get => _Currentpage; set { _Currentpage = value; OnPropertyChanged("Currentpage"); } } //当前页

        private int _Pagesize = 15;

        public int Pagesize { get => _Pagesize; set { _Pagesize = value; OnPropertyChanged("Pagesize"); } } //当前页的数据

        private int _Totals = 0;

        public int Totals { get => _Totals; set { _Totals = value; OnPropertyChanged("Totals"); } } //总数据

        private string _WhereSql;

        public string WhereSql { get => _WhereSql; set { _WhereSql = value; OnPropertyChanged("WhereSql"); } }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
