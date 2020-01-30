using System.Collections;

namespace FifaLibrary
{
	public class RosterComparer : IComparer
	{
		int IComparer.Compare(object x, object y)
		{
			TeamPlayer teamPlayer = (TeamPlayer)x;
			TeamPlayer teamPlayer2 = (TeamPlayer)y;
			if (teamPlayer.position != teamPlayer2.position)
			{
				return teamPlayer.position - teamPlayer2.position;
			}
			return teamPlayer.Player.preferredposition1 - teamPlayer2.Player.preferredposition1;
		}
	}
}
