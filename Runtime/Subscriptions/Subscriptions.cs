using System;

    // <------------------------------- Functions ------------------------------->

    // Value => To SET or GET the variable's value.

    // Bool() => Check if the requested variable currently matches to specified value.

    // OnMatch() => With match, 2 subscribed functions - Invokes only when the value actually changes.
    // OnMatchAlways() => With match, 2 subscribed functions - Invokes every time the variable is set.
    // OnKey() => Without match, 1 subscribed function - Invokes only when the value actually changes.
    // OnKeyAlways() => Without match, 1 subscribed function - Invokes every time the variable is set.

    // Dispose() => Unsubscribe.

    // <------------------------------------------------------------------------->

namespace SubLynx
{

public class Subscription<T>
{
    
    T _value;
    event Action<T, T> _onChange;
    event Action<T> _onSet;

    public Subscription(T initialValue)
    {
        _value = initialValue;
    }

    public T Value
    {
        get => _value;
        set
        {
            T oldValue = _value;
            _value = value;

            _onSet?.Invoke(value);

            if (!AreEqual(oldValue, value))
                _onChange?.Invoke(oldValue, value);
        }
    }

    public bool Bool(T matchValue) => AreEqual(_value, matchValue);

    public static implicit operator T(Subscription<T> variable) => variable.Value;

    public IDisposable OnMatch(T matchValue, Action onMatch, Action onMismatch, bool invokeImmediately = true)
    {
        void Handler(T oldVal, T newVal)
        {
            if (AreEqual(newVal, matchValue))
                onMatch?.Invoke();
            else
                onMismatch?.Invoke();
        }

        _onChange += Handler;
        if (invokeImmediately)
            Handler(_value, _value);
        return new UnsubscribeHandle(() => _onChange -= Handler);
    }

    public IDisposable OnMatchAlways(T matchValue, Action onMatch, Action onMismatch, bool invokeImmediately = true)
    {
        void Handler(T val)
        {
            if (AreEqual(val, matchValue))
                onMatch?.Invoke();
            else
                onMismatch?.Invoke();
        }

        _onSet += Handler;
        if (invokeImmediately)
            Handler(_value);
        return new UnsubscribeHandle(() => _onSet -= Handler);
    }

    public IDisposable OnKey(Action callback, bool invokeImmediately = true)
    {
        Action<T, T> handler = (oldVal, newVal) => callback?.Invoke();
        _onChange += handler;
        if (invokeImmediately)
            callback?.Invoke();
        return new UnsubscribeHandle(() => _onChange -= handler);
    }

    public IDisposable OnKeyAlways(Action callback, bool invokeImmediately = true)
    {
        Action<T> handler = val => callback?.Invoke();
        _onSet += handler;
        if (invokeImmediately)
            callback?.Invoke();
        return new UnsubscribeHandle(() => _onSet -= handler);
    }

    bool AreEqual(T a, T b)
    {
        if (typeof(T) == typeof(string))
            return string.Equals(a as string, b as string, StringComparison.OrdinalIgnoreCase);
            
        return Equals(a, b);
    }

    public override string ToString() => _value?.ToString() ?? "null";

    sealed class UnsubscribeHandle : IDisposable
    {
        Action _onDispose;

        public UnsubscribeHandle(Action onDispose) => _onDispose = onDispose;

        public void Dispose()
        {
            _onDispose?.Invoke();
            _onDispose = null;
        }
    }

}

}