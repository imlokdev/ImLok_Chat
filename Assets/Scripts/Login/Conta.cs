public class Conta
{
    public int ID {  get; private set; }
    public string User { get; private set; }
    public string Email { get; private set; }
    public bool IsBlocked { get; private set; }
    public bool IsAdmin { get; private set; }
    public bool IsBanned { get; private set; }

    public Conta(int _id, string _user, string _email, bool _isBlocked, bool _isAdmin, bool _isBanned = false)
    {
        ID = _id;
        User = _user;
        Email = _email;
        IsBlocked = _isBlocked;
        IsAdmin = _isAdmin;
        IsBanned = _isBanned;
    }
}