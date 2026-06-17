using System.Windows.Forms;

namespace WebDesk;

/// <summary>Registers global hotkeys via a hidden message window.</summary>
internal sealed class HotkeyService : NativeWindow, IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private readonly Dictionary<int, Action> _actions = new();
    private int _nextId = 1;

    public HotkeyService() => CreateHandle(new CreateParams());

    public void Register(uint modifiers, uint key, Action action)
    {
        var id = _nextId++;
        if (NativeMethods.RegisterHotKey(Handle, id, modifiers, key))
            _actions[id] = action;
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY && _actions.TryGetValue((int)m.WParam, out var action))
            action();
        base.WndProc(ref m);
    }

    public void Dispose()
    {
        foreach (var id in _actions.Keys) NativeMethods.UnregisterHotKey(Handle, id);
        _actions.Clear();
        DestroyHandle();
    }
}
