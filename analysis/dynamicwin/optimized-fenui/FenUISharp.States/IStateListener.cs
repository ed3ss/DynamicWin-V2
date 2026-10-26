namespace FenUISharp.States;

public interface IStateListener
{
	void OnInternalStateChanged<T>(T value);
}
