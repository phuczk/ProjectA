public interface IBackHandler
{
    /// <returns>Trả về true nếu đã xử lý việc Back, false nếu không còn gì để đóng</returns>
    bool OnBack();
}