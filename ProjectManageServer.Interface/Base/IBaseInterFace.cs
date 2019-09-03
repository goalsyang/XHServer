using ProjectManageServer.Model;
using System.Collections.ObjectModel;

namespace ProjectManageServer.Interface
{
    public interface IBaseInterFace<T> where T : class
    {

        ObservableCollection<T> LoadData();

        FetchDataModel<ObservableCollection<T>> FetchDataList(PagesModel model);

        string InsertData(T model);

        string UpdateData(T model);

        string DeleteData(T model);

    }
}
