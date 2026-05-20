using StaticTemplates.MusicGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarJudger : MonoBehaviour
{
	//public PopupImage popupImage;

	//判定时间
	public const float perfect = 0.04f; //<40ms

	public const float good = 0.08f; //<80ms
	public const float soso = 0.12f; //<120ms
	public const float miss = 0.18f; //<180ms

	public PopUpText text;

	//当前轨道的索引
	private int trackIndex;

	//轨道总数
	private int trackCount;

	//当前轨道的音符管理
	private NoteManager noteManager;

	//轨道管理者
	private TracksManager tracksManager;

	private void Start()
	{
		text = GetComponentInChildren<PopUpText>();
	}

	private void Update()
	{
		bool keyDown = InputManager.instance.GetJudgeKeyDown_MusicGame(trackCount, trackIndex);
		bool keyUp = InputManager.instance.GetJudgeKeyUp_MusicGame(trackCount, trackIndex);

		// 按下: 优先判长音符头, 否则判点音符
		if (keyDown)
		{
			// 看队列里有没有等待判头的长音符
			LongNote pendingLong = null;
			foreach (var ln in noteManager.ActiveLongNotes)
			{
				if (ln.state == LongNote.State.WaitingForHead)
				{
					pendingLong = ln;
					break;
				}
			}

			if (pendingLong != null && pendingLong.JudgeHead())
			{
				noteManager.AddBarIndex(trackIndex);
				return;
			}

			// 否则判普通点音符
			if (noteManager.NoteListCount > 0)
			{
				Note note = noteManager.PeekFirstNote();
				if (note.JudgeTime())
				{
					ScoreManager.ScoreManagerInstance?.score.AddNoteCount();
					ScoreManager.ScoreManagerInstance?.score.UpdateCurrentRate();
					ScoreManager.ScoreManagerInstance?.UpdateCurrentScoreText();
					noteManager.AddBarIndex(trackIndex);

					if (trackCount == 4)
					{
						string id = TrackGenerator.index2id[Random.Range(0, TrackGenerator.index2id.Count)];
						FoodMaterial foodMaterial = FoodMaterials.LookUpFoodMaterial(id);
						TestBackpack.instance.UAVBackpackView.backpackViewModel.AddItem(new Statics.Classes.BagItem(foodMaterial, 1));
						//popupImage.Show(SpriteStatic.GetSprite(foodMaterial.spritePath));
					}
				}
			}
		}

		// 松开: 处理 Holding 中或 WaitingForTail 的长音符
		if (keyUp)
		{
			foreach (var ln in noteManager.ActiveLongNotes)
			{
				if (ln.state == LongNote.State.Holding || ln.state == LongNote.State.WaitingForTail)
				{
					ln.OnRelease();
					break;
				}
			}
		}
	}

	public void Initialize(NoteManager noteManager, int trackIndex, int trackCount)
	{
		this.noteManager = noteManager;
		this.trackIndex = trackIndex;
		this.trackCount = trackCount;
	}

	public void ShowText(string message, Color color, float duration = 0f)
	{
		text.ShowText(message, color, duration);
	}
}