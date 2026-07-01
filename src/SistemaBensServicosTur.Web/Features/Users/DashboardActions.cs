namespace SistemaBensServicosTur.Web.Features.Users;

public class DashboardActions
{
    private Func<Task>? _addHandler;
    private bool _pendingAddRequest;

    public event Func<Task>? AddRequested
    {
        add => _addHandler = value;
        remove { if (_addHandler == value) _addHandler = null; }
    }

    public async Task<bool> RequestAddAsync()
    {
        if (_addHandler is not null)
        {
            await _addHandler();
            return true;
        }

        _pendingAddRequest = true;
        return false;
    }

    public bool ConsumePendingAddRequest()
    {
        if (!_pendingAddRequest) return false;
        _pendingAddRequest = false;
        return true;
    }
}
