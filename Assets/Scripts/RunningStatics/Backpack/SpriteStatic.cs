using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpriteStatic
{
	//string到Sprite的映射,用于读取每个item的图片
	private static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

	/// <summary>
	/// 给sprites增加键值对
	/// </summary>
	/// <param name="path"></param>
	public static Sprite AddPairToSprites(string path)
	{
		if (!sprites.ContainsKey(path))
		{
			Sprite res = Resources.Load<Sprite>(path);
			if (res != null)
			{
				sprites[path] = res;
				return res;
			}
			else
			{
				throw new UnityException($"图片路径不存在！path:{path}");
			}
		}
		else
		{
			return sprites[path];
		}
	}

	public static Sprite GetSprite(string path)
	{
		if (sprites.ContainsKey(path))
		{
			return sprites[path];
		}
		else
		{
			return AddPairToSprites(path);
		}
	}
}