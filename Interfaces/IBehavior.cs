using AscII_Game.Core;
using AscII_Game.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscII_Game.Interfaces
{
	public interface IBehavior
	{
		bool Act(Monster monster, CommandSystem commandSystem);
	}
}
