using System;
using System.Collections;
using System.Collections.Generic;

namespace UniFramework.Event
{
	//作用是集中管理一组事件监听器，方便批量移除。
	//这样设计的好处是，当我们需要管理多个事件监听器时（例如一个UI界面注册了多个事件）
	//可以在界面关闭时一次性移除所有监听器，避免忘记移除导致的问题。
	public class EventGroup
	{
		private readonly Dictionary<System.Type, List<Action<IEventMessage>>> _cachedListener = new Dictionary<System.Type, List<Action<IEventMessage>>>();

		/// <summary>
		/// 添加一个监听
		/// </summary>
		public void AddListener<TEvent>(System.Action<IEventMessage> listener) where TEvent : IEventMessage
		{
			System.Type eventType = typeof(TEvent);
			if (_cachedListener.ContainsKey(eventType) == false)
				_cachedListener.Add(eventType, new List<Action<IEventMessage>>());

			if (_cachedListener[eventType].Contains(listener) == false)
			{
				_cachedListener[eventType].Add(listener);
				UniEvent.AddListener(eventType, listener);
			}
			else
			{
				UniLogger.Warning($"Event listener is exist : {eventType}");
			}
		}

		/// <summary>
		/// 移除所有缓存的监听
		/// </summary>
		public void RemoveAllListener()
		{
			foreach (var pair in _cachedListener)
			{
				System.Type eventType = pair.Key;
				for (int i = 0; i < pair.Value.Count; i++)
				{
					UniEvent.RemoveListener(eventType, pair.Value[i]);
				}
				pair.Value.Clear();
			}
			_cachedListener.Clear();
		}
	}
}