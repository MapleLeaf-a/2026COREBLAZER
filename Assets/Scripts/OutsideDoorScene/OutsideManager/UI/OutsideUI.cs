using Assets.Scripts.Tools.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.OutsideDoorScene.OutsideManager.UI
{
	/// <summary>
	/// 时间紧迫，不考虑资源回收
	/// </summary>
	public class OutsideUI : MonoSingleton<OutsideUI>
	{
		public GameObject m_uiPanel;

		public void OnEnable()
		{
			DontDestroyOnLoad(this);
			this.Open();
		}

		public void OnDisable()
		{
			this.Close();
		}

		public void Close()
		{
			m_uiPanel.SetActive(false);
		}

		public void Open()
		{
			m_uiPanel.SetActive(true);
		}
	}
}