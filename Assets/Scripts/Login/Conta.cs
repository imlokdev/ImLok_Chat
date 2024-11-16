using System;

public class Conta
{
    public int ID {  get; private set; }
    public string User { get; private set; }
    public string Email { get; private set; }
    public bool IsBlocked { get; private set; }
    public bool IsAdmin { get; private set; }
    public bool IsBanned { get; private set; }
    public bool IsConfirmed { get; private set; }
    public DateTime Created_at { get; private set; }
    public DateTime Last_login { get; private set; }
    public string Token_acess { get; private set; }

    public Conta(int _id, string _user, string _email, bool _isBlocked, 
                 bool _isAdmin, bool _isBanned, bool _isConfirmed, 
                 DateTime _created_at, DateTime _last_login, string token_acess = null)
    {
        ID = _id;
        User = _user;
        Email = _email;
        IsBlocked = _isBlocked;
        IsAdmin = _isAdmin;
        IsBanned = _isBanned;
        IsConfirmed = _isConfirmed;
        Created_at = _created_at;
        Last_login = _last_login;
        Token_acess = token_acess;
    }
}