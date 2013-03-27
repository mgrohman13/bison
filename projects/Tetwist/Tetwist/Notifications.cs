using System;
using System.Collections.Generic;
using System.Text;

namespace Tetwist
{
	static class Notifications
	{
		public enum Type
		{
			_Solidification,
			Iteration,
			BlockSet,
			LineCleared,
		}

		static Dictionary<Type, List<Block>> notifications = new Dictionary<Type, List<Block>>();

		public static void NewGame()
		{
			notifications.Clear();
		}

		public static void AddNotification(Type type, Block block)
		{
			if (type == Type._Solidification) throw new Exception();

			if (!notifications.ContainsKey(type))
				notifications.Add(type, new List<Block>());
			notifications[type].Add(block);
		}

		public static void RemoveNotification(Type type, Block block)
		{
			if (type == Type._Solidification) throw new Exception();

			if (notifications.ContainsKey(type))
				notifications[type].Remove(block);
		}

		public static void Notify(Type type,object information)
		{
			if (type == Type._Solidification) throw new Exception();

			if (notifications.ContainsKey(type))
			{
				List<Block> temp = new List<Block>(notifications[type].Count);

				int notiCount;
				while ((notiCount = notifications[type].Count) > 0)
				{
					int index = Game.Random.Next(notiCount);
					temp.Add(notifications[type][index]);
					notifications[type].RemoveAt(index);
				}
				notifications[type] = new List<Block>(temp.Count);
				notifications[type].AddRange(temp);

				List<Block> removeList = new List<Block>();
				foreach (Block b in temp)
					if (b.Dead)
						removeList.Add(b);
					else
						b.Notify(type, information);

				for (int i = removeList.Count; --i > -1; )
					notifications[type].Remove(removeList[i]);
			}
		}
	}
}
