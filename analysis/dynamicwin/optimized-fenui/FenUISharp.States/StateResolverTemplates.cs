using System;
using System.Collections.Generic;
using System.Linq;

namespace FenUISharp.States;

public static class StateResolverTemplates
{
	public static Func<List<StateEntry<float>>, StateEntry<float>> BiggestFloatResolver => (List<StateEntry<float>> entries) => entries.OrderBy((StateEntry<float> e) => e.Value()).Last();

	public static Func<List<StateEntry<int>>, StateEntry<int>> BiggestIntResolver => (List<StateEntry<int>> entries) => entries.OrderBy((StateEntry<int> e) => e.Value()).Last();

	public static Func<List<StateEntry<float>>, StateEntry<float>> SmallestFloatResolver => (List<StateEntry<float>> entries) => entries.OrderByDescending((StateEntry<float> e) => e.Value()).Last();

	public static Func<List<StateEntry<int>>, StateEntry<int>> SmallestIntResolver => (List<StateEntry<int>> entries) => entries.OrderByDescending((StateEntry<int> e) => e.Value()).Last();
}
