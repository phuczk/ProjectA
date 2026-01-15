public interface ICommand
{
    // Trả về true = command đã hoàn thành
    bool Execute(float deltaTime);

    // Optional: gọi khi command bị hủy giữa chừng
    void OnCancel();

    // Optional: dùng để debug hoặc log
    string Name { get; }
}
