using Arch.Tools;
using Assets.Scripts.Tools.Unity;
using Events;
using System;
using UnityEngine;
using Assets.Scripts.OutsideDoorScene.OutsideManager.Manager;

namespace Assets.Scripts.OutsideDoorScene.OutsideManager.Manager
{
	public class OutsideDoorCharacterPowerManager : Singleton<OutsideDoorCharacterPowerManager>
	{
		public bool IsOutsideDoor = false;

		public float CharacterPower = 100f;

		private const float PerThreeSecondeLimitPower = 1;
		private const float LimitTime = 3;
		private float timer = 0;

		public override void OnInit()
		{
			EventBus.Subscribe<GoToOutsideEvent>(a => OnReadyGoOut(a));
			EventBus.Subscribe<ResetCharacterPowerEvent>(a => ResetPower(a));
		}

		public void OnReadyGoOut(in GoToOutsideEvent goToOutsideEvent)
		{
			if (CharacterPower <= 0)
			{
				Debug.Log("能量已消耗殆尽");
				EventBus.Publish(new OpenPowerUseUpTipsEvent());
				return;
			}
			if (string.IsNullOrEmpty(goToOutsideEvent.targetSceneName))
			{
				Debug.LogError("Empty Scene Name");
				return;
			}

			// 必须在 LoadScene 前记录。
			// 因为 LoadScene 之后，当前激活场景会变成新场景，旧场景名就取不到了。
			SceneTransitionContext.RecordTransition(goToOutsideEvent.targetSceneName);

			var sceneChangeRequest = new SceneLoadRequestEvent(requestId: "default", loadMode: GameSceneLoadMode.Single, targetSceneName: goToOutsideEvent.targetSceneName);

			try
			{
				EventBus.Publish(sceneChangeRequest);
				Debug.LogError("开始场景转化");
				IsOutsideDoor = true;
			}
			catch (Exception e)
			{
				IsOutsideDoor = false;
				Debug.LogError(e);
			}
		}

		public void ResetPower(in ResetCharacterPowerEvent resetCharacterPowerEvent)
		{
			CharacterPower = 100;
			EventBus.Publish(new SyncPowerSlider() { currentCharacterPower = CharacterPower });
			timer = 0f;
			IsOutsideDoor = false;
		}

		public void Update()
		{
			if (IsOutsideDoor)
			{
				timer += Time.deltaTime;
				if (timer >= LimitTime)
				{
					timer = 0;
					CharacterPower -= PerThreeSecondeLimitPower;
					CharacterPower = Mathf.Clamp(CharacterPower, 0, 100);
					//通知Slider变化能量
					EventBus.Publish(new SyncPowerSlider() { currentCharacterPower = CharacterPower });
				}
				if (CharacterPower <= 0)
				{
					try
					{
						//强制返回大厅
						EventBus.Publish(new ForceReturnMainScene());
						//关闭户外UI
						EventBus.Publish(new CloseOutsideUIEvent());
					}
					catch (Exception e)
					{
						Debug.LogError(e);
					}
					finally
					{
						IsOutsideDoor = false;
						timer = 0f;
					}
				}
			}
		}
	}
}