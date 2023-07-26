using UnityEngine;
using UnityEngine.Events;

public abstract class BaseVariableContainer<T> : ScriptableObject
{
    public UnityEvent<T> OnValueChangedArgumented;
    public UnityEvent OnValueChanged;
    public T Value { get; protected set; }

    public void SetValue(T newValue)
    {
        Value = newValue;
        OnValueChanged?.Invoke();
        OnValueChangedArgumented?.Invoke(Value);
    }
}
