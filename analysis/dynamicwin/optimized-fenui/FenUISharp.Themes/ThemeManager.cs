using System;

namespace FenUISharp.Themes;

public class ThemeManager
{
	private Theme _currentTheme;

	public Theme CurrentTheme
	{
		get
		{
			return _currentTheme;
		}
		private set
		{
			_currentTheme = value;
			this.ThemeChanged?.Invoke();
		}
	}

	public event Action? ThemeChanged;

	public ThemeManager(Theme initialTheme)
	{
		_currentTheme = initialTheme;
	}

	public void SetTheme(Theme newTheme)
	{
		if (newTheme != CurrentTheme)
		{
			CurrentTheme = newTheme;
		}
	}
}
