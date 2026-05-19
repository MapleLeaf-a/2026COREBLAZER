using Arch.Tools;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Events
{
	/// <summary>
	/// 消息管道的发布者
	/// </summary>
	public partial class EventBus : Singleton<EventBus>
	{
		private const int MAX_DEPTH = 0;

		private static class Handlers<T> where T : struct
		{
			public static readonly List<Action<T>> actions = new List<Action<T>>();
			public static int publishDepth;
		}

		public static void Subscribe<T>(Action<T> handler) where T : struct
		{
			Handlers<T>.actions.Add(handler);
		}

		public static void Unsubscribe<T>(Action<T> handler) where T : struct
		{
			Handlers<T>.actions.Remove(handler);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Publish<T>(in T eventData) where T : struct
		{
			if (Handlers<T>.publishDepth++ > MAX_DEPTH)
			{
				throw new InvalidOperationException($"递归触发事件 {typeof(T).Name} 被禁止");
			}
			if (Handlers<T>.actions.Count == 0)
			{
				Debug.LogWarning($"没有关于 {typeof(T).Name} 的事件处理器");
				return;
			}
			try
			{
				var actions = Handlers<T>.actions;
				for (int i = 0; i < actions.Count; i++)
				{
					actions[i](eventData);
				}
			}
			finally
			{
				Handlers<T>.publishDepth--;
			}
		}
	}
}