using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Tools.Common
{
	/// <summary>
	/// MonoSingleton 是基于 Unity MonoBehaviour 的单例基类。
	///
	/// 单例：
	/// 全局只允许存在一个实例的对象。
	///
	/// MonoBehaviour：
	/// Unity 中可以挂载到 GameObject 上的脚本基类。
	///
	/// 使用方式：
	/// public class AudioManager : MonoSingleton<AudioManager>
	/// {
	///     protected override bool DontDestroyOnLoad => true;
	/// }
	/// </summary>
	/// <typeparam name="T">
	/// 单例组件类型。
	/// 必须继承 MonoBehaviour，因为它需要挂载在 GameObject 上。
	/// </typeparam>
	public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		/// <summary>
		/// 当前单例实例。
		/// static 表示这个字段属于类型本身，而不是某个对象。
		/// </summary>
		private static T m_instance;

		/// <summary>
		/// 防止多线程同时创建实例的锁对象。
		/// Unity 主逻辑通常在主线程运行，但保留锁可以避免一些异步场景下的重复创建。
		/// </summary>
		private static readonly object m_lock = new object();

		/// <summary>
		/// 标记应用是否正在退出。
		/// 如果应用正在退出，就不再自动创建新的 GameObject，避免退出时报错或生成无意义对象。
		/// </summary>
		private static bool m_isApplicationQuitting;

		/// <summary>
		/// 是否在切换场景时保留该单例。
		/// 子类可以重写这个属性。
		///
		/// true：
		/// 切换场景不销毁。
		///
		/// false：
		/// 跟随当前场景销毁。
		/// </summary>
		protected virtual bool DontDestroyOnLoad => false;

		/// <summary>
		/// 获取单例实例。
		/// 如果场景中已有对应组件，就复用已有组件。
		/// 如果场景中没有对应组件，就自动创建一个新的 GameObject 并挂载该组件。
		/// </summary>
		public static T Instance
		{
			get
			{
				if (m_isApplicationQuitting)
				{
					Debug.LogWarning(
						$"[MonoSingleton] {typeof(T).Name} is already destroyed because application is quitting.");

					return null;
				}

				if (m_instance == null)
				{
					m_instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);

					if (m_instance == null)
					{
						// MonoBehaviour 不能通过 new T() 创建。
						// 所以这里先创建一个 GameObject，再把 T 作为组件挂上去。
						var singletonObject = new GameObject($"[MonoSingleton] {typeof(T).Name}");

						m_instance = singletonObject.AddComponent<T>();
					}
				}

				return m_instance;
			}
		}

		/// <summary>
		/// Unity 生命周期方法。
		/// 当脚本实例被加载时调用，通常早于 Start。
		///
		/// 这里用于：
		/// 1. 初始化单例引用。
		/// 2. 删除重复实例。
		/// 3. 根据 DontDestroyOnLoad 决定是否跨场景保留。
		/// </summary>
		protected virtual void Awake()
		{
			if (m_instance == null)
			{
				m_instance = this as T;

				if (DontDestroyOnLoad)
				{
					// 让当前 GameObject 在切换场景时不被 Unity 自动销毁。
					UnityEngine.Object.DontDestroyOnLoad(gameObject);
				}
				return;
			}

			if (m_instance != this)
			{
				// 如果场景里出现了第二个同类型单例，就销毁重复对象。
				// 这样可以保证全局只存在一个 T 实例。
				Destroy(gameObject);
			}
		}

		/// <summary>
		/// Unity 生命周期方法。
		/// 当对象被销毁时调用。
		///
		/// 如果被销毁的是当前单例实例，就清空静态引用，
		/// 避免下次访问 Instance 时拿到已经被 Unity 销毁的对象。
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (m_instance == this)
			{
				m_instance = null;
			}
		}

		/// <summary>
		/// Unity 生命周期方法。
		/// 当应用退出时调用。
		///
		/// 设置退出标记后，Instance 不会再自动创建新对象。
		/// </summary>
		protected virtual void OnApplicationQuit()
		{
			m_isApplicationQuitting = true;
		}
	}
}