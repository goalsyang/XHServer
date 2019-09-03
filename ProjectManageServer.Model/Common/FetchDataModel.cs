namespace ProjectManageServer.Model
{
    public class FetchDataModel<T> where T : class
    { 
        public T ModelData { get; set; }


        public int Totals { get; set; }
    }
}
