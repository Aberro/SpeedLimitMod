using System;
using Unity.Entities;

namespace SpeedLimitEditor.Utils
{
	public static class HelperExtensions
	{
		public static void ForEach<T>(this DynamicBuffer<T> source, Action<T> action) where T : unmanaged
		{
			for (int i = 0; i < source.Length; i++)
				action(source[i]);
		}
	}
}
