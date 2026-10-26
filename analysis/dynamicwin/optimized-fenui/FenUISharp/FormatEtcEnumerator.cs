using System.Runtime.InteropServices.ComTypes;

namespace FenUISharp;

public class FormatEtcEnumerator : IEnumFORMATETC
{
	private readonly FORMATETC[] _formats;

	private int _index;

	public FormatEtcEnumerator(FORMATETC[] formats)
	{
		_formats = formats;
	}

	public int Next(int celt, FORMATETC[] rgelt, int[] pceltFetched)
	{
		int num = 0;
		while (_index < _formats.Length && num < celt)
		{
			rgelt[num] = _formats[_index];
			num++;
			_index++;
		}
		if (pceltFetched != null && pceltFetched.Length != 0)
		{
			pceltFetched[0] = num;
		}
		return (num != celt) ? 1 : 0;
	}

	public int Skip(int celt)
	{
		_index += celt;
		return (_index > _formats.Length) ? 1 : 0;
	}

	public void Reset()
	{
		_index = 0;
	}

	public void Clone(out IEnumFORMATETC newEnum)
	{
		newEnum = new FormatEtcEnumerator(_formats)
		{
			_index = _index
		};
	}

	int IEnumFORMATETC.Reset()
	{
		Reset();
		return 0;
	}
}
