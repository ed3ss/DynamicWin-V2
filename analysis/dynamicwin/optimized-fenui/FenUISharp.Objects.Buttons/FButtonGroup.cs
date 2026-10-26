using System;
using System.Collections.Generic;
using System.Linq;

namespace FenUISharp.Objects.Buttons;

public class FButtonGroup
{
	private bool _allowMultiSelect;

	private bool _alwaysMustSelectOne;

	public List<SelectableButton> Buttons { get; } = new List<SelectableButton>();

	public bool AllowMultiSelect
	{
		get
		{
			return _allowMultiSelect;
		}
		set
		{
			_allowMultiSelect = value;
			UpdateButtonGroup();
		}
	}

	public bool AlwaysMustSelectOne
	{
		get
		{
			return _alwaysMustSelectOne;
		}
		set
		{
			_alwaysMustSelectOne = value;
			UpdateButtonGroup();
		}
	}

	public Action<bool[]>? OnUserSelectionChanged { get; set; }

	public Action<bool[]>? OnSelectionChanged { get; set; }

	public int LatestSelection { get; private set; }

	public void Select(int button)
	{
		Select(Buttons[button]);
	}

	public void Select(SelectableButton button)
	{
		if (!AllowMultiSelect)
		{
			Buttons.ForEach(delegate(SelectableButton x)
			{
				if (x != button && x.IsSelected)
				{
					x.SetSelected(isSelected: false);
				}
			});
		}
		UpdateButtonGroup();
		LatestSelection = Buttons.IndexOf(button);
		bool[] array = new bool[Buttons.Count];
		for (int num = 0; num < Buttons.Count; num++)
		{
			array[num] = Buttons[num].IsSelected;
		}
		OnUserSelectionChanged?.Invoke(array);
		OnSelectionChanged?.Invoke(array);
	}

	public void Add(SelectableButton button)
	{
		Buttons.Add(button);
		if (AlwaysMustSelectOne && Buttons.Where((SelectableButton x) => x.IsSelected).Count() == 0)
		{
			button.SetSelected(isSelected: true);
		}
		else if (AlwaysMustSelectOne && Buttons.Where((SelectableButton x) => x.IsSelected).Count() != 0 && !AllowMultiSelect)
		{
			button.SetSelected(isSelected: false);
		}
		button.OnUserSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(button.OnUserSelectionChanged, new Action<bool, SelectableButton>(OnButtonChanged));
		button.ButtonGroup = this;
	}

	public void Remove(SelectableButton button)
	{
		Buttons.Remove(button);
		UpdateButtonGroup();
		button.OnUserSelectionChanged = (Action<bool, SelectableButton>)Delegate.Remove(button.OnUserSelectionChanged, new Action<bool, SelectableButton>(OnButtonChanged));
		if (button.ButtonGroup == this)
		{
			button.ButtonGroup = null;
		}
	}

	private void OnButtonChanged(bool isSelected, SelectableButton button)
	{
		if (!AllowMultiSelect && isSelected)
		{
			Buttons.ForEach(delegate(SelectableButton x)
			{
				if (x != button && x.IsSelected)
				{
					x.SetSelected(isSelected: false);
				}
			});
		}
		UpdateButtonGroup();
		LatestSelection = Buttons.IndexOf(button);
		bool[] array = new bool[Buttons.Count];
		for (int num = 0; num < Buttons.Count; num++)
		{
			array[num] = Buttons[num].IsSelected;
		}
		OnUserSelectionChanged?.Invoke(array);
		OnSelectionChanged?.Invoke(array);
	}

	private void UpdateButtonGroup()
	{
		if (Buttons.Count == 0)
		{
			return;
		}
		bool[] array = new bool[Buttons.Count];
		for (int i = 0; i < Buttons.Count; i++)
		{
			array[i] = Buttons[i].IsSelected;
		}
		if (!AllowMultiSelect)
		{
			Buttons.ForEach(delegate(SelectableButton x)
			{
				x.CanUnselect = !AlwaysMustSelectOne;
			});
		}
		else
		{
			Buttons.ForEach(delegate(SelectableButton x)
			{
				x.CanUnselect = !AlwaysMustSelectOne || Buttons.Where((SelectableButton selectableButton) => selectableButton.IsSelected).Count() > 1;
			});
		}
		if (!AllowMultiSelect && Buttons.Where((SelectableButton x) => x.IsSelected).Count() > 1)
		{
			int c = 0;
			Buttons.ForEach(delegate(SelectableButton x)
			{
				if (x.IsSelected && c != 0)
				{
					x.SetSelected(isSelected: false);
				}
				c++;
			});
		}
		else if (AlwaysMustSelectOne && Buttons.Where((SelectableButton x) => x.IsSelected).Count() == 0)
		{
			Buttons[0].SetSelected(isSelected: true);
		}
		bool[] array2 = new bool[Buttons.Count];
		for (int num = 0; num < Buttons.Count; num++)
		{
			array2[num] = Buttons[num].IsSelected;
		}
		if (!array2.SequenceEqual(array))
		{
			OnSelectionChanged?.Invoke(array2);
		}
	}
}
