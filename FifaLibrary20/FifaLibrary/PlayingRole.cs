using System.Drawing;

namespace FifaLibrary
{
	public class PlayingRole : IdObject
	{
		private int m_RoleId;

		private Role m_Role;

		public EPlayingDirection OffDirection0;

		public EPlayingDirection DefDirection0;

		public EPlayingIntensity OffIntensity;

		public EPlayingIntensity DefIntensity;

		private static int[] c_DefaultInstrucion = new int[30]
		{
			0,
			16,
			8,
			8,
			16,
			16,
			16,
			8,
			8,
			320,
			320,
			320,
			2394112,
			134220032,
			134220032,
			134220032,
			2394112,
			134252544,
			134252544,
			134252544,
			294920,
			294920,
			294920,
			2394112,
			294920,
			294920,
			294920,
			2394112,
			0,
			0
		};

		public static int[] c_InstrucionNumber = new int[30]
		{
			0,
			0,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			2,
			2,
			2,
			4,
			3,
			3,
			3,
			4,
			3,
			3,
			3,
			3,
			3,
			3,
			4,
			3,
			3,
			3,
			4,
			0,
			0
		};

		public static int[,] c_InstrucionSetSelection = new int[30, 5]
		{
			{
				-1,
				-1,
				-1,
				-1,
				-1
			},
			{
				2,
				-1,
				-1,
				-1,
				-1
			},
			{
				0,
				-1,
				-1,
				-1,
				-1
			},
			{
				1,
				-1,
				-1,
				-1,
				-1
			},
			{
				2,
				-1,
				-1,
				-1,
				-1
			},
			{
				2,
				-1,
				-1,
				-1,
				-1
			},
			{
				2,
				-1,
				-1,
				-1,
				-1
			},
			{
				1,
				-1,
				-1,
				-1,
				-1
			},
			{
				0,
				-1,
				-1,
				-1,
				-1
			},
			{
				4,
				3,
				-1,
				-1,
				-1
			},
			{
				4,
				3,
				-1,
				-1,
				-1
			},
			{
				4,
				3,
				-1,
				-1,
				-1
			},
			{
				6,
				5,
				7,
				8,
				9
			},
			{
				3,
				5,
				9,
				-1,
				-1
			},
			{
				3,
				5,
				9,
				-1,
				-1
			},
			{
				3,
				5,
				9,
				-1,
				-1
			},
			{
				6,
				5,
				7,
				8,
				9
			},
			{
				6,
				5,
				9,
				-1,
				-1
			},
			{
				6,
				5,
				9,
				-1,
				-1
			},
			{
				6,
				5,
				9,
				-1,
				-1
			},
			{
				12,
				10,
				11,
				-1,
				-1
			},
			{
				12,
				10,
				11,
				-1,
				-1
			},
			{
				12,
				10,
				11,
				-1,
				-1
			},
			{
				6,
				5,
				7,
				8,
				-1
			},
			{
				12,
				10,
				11,
				-1,
				-1
			},
			{
				12,
				10,
				11,
				-1,
				-1
			},
			{
				12,
				10,
				11,
				-1,
				-1
			},
			{
				6,
				5,
				7,
				8,
				-1
			},
			{
				-1,
				-1,
				-1,
				-1,
				-1
			},
			{
				-1,
				-1,
				-1,
				-1,
				-1
			}
		};

		public static int[][] c_InstrucionSet = new int[13][]
		{
			new int[2]
			{
				2,
				3
			},
			new int[3]
			{
				2,
				3,
				4
			},
			new int[3]
			{
				0,
				1,
				4
			},
			new int[3]
			{
				4,
				8,
				9
			},
			new int[3]
			{
				5,
				6,
				7
			},
			new int[3]
			{
				10,
				11,
				12
			},
			new int[3]
			{
				14,
				15,
				16
			},
			new int[3]
			{
				17,
				18,
				19
			},
			new int[3]
			{
				20,
				21,
				22
			},
			new int[2]
			{
				13,
				27
			},
			new int[3]
			{
				18,
				24,
				25
			},
			new int[4]
			{
				3,
				20,
				26,
				28
			},
			new int[3]
			{
				5,
				15,
				23
			}
		};

		public static string[] c_InstructionDescription = new string[32]
		{
			"Occasionally make forward runs when the opportunity arises",
			"Go up front in the last few minutes of a match if losing",
			"Make forward runs as much as possible",
			"Occasionally make forward runs when the opportunity arises",
			"Never make forward runs while on attack",
			"Split the opposition and cut out the passing lanes",
			"Keep your shape and stay in position to defend",
			"Mark up tight and stick with your opponent",
			"Occasionally make forward runs when the opportunity arises",
			"Join the attack and make runs beyond the striker(s)",
			"Make runs into the penalty area in crossing situations",
			"Run into the penalty area or stay on the edge in crossing situations",
			"Stay on the edge of the penalty area in crossing situations",
			"Take a free role and roam the attacking third",
			"Always try to track back and support the defence",
			"Come back to support the defence when needed",
			"Do not come back to support the defence",
			"Make cutting runs to the inside from out wide",
			"Stay wide or cut inside depending on the situation",
			"Always try to stay wide and close to the line",
			"Make forward runs in behind the defence",
			"Make forward runs or come short depending on the situation",
			"Come short and ask for the ball to feet",
			"Apply pressure on the back line",
			"Make runs to wide areas of the pitch",
			"Stay in central areas of the pitch",
			"Back into an opponent and ask for the ball to feet",
			"Stay in your formation position when attacking",
			"Occasionally make forward runs when the opportunity arises",
			"Description 29",
			"Play as Goalkeeper",
			"Description 31"
		};

		public static string[] c_InstructionCaption = new string[32]
		{
			"Join The Attack",
			"Play As Striker",
			"Always Overlap",
			"Balanced Attack-3",
			"Stay Back While Attacking",
			"Cut Passing Lanes",
			"Balanced Defense",
			"Man Mark",
			"Balanced Attack-8",
			"Get Forward",
			"Get Into The Box For Cross",
			"Balanced Crossing Runs",
			"Stay On Edge Of Box For Cross",
			"Free Roam",
			"Come Back On Defence",
			"Basic Defence Support",
			"Stay Forward",
			"Cut Inside",
			"Balanced Width",
			"Stay Wide",
			"Get In Behind",
			"Balanced Support",
			"Come Short",
			"Press Back Line",
			"Drift Wide",
			"Stay Central",
			"Target Man",
			"Stick To Position",
			"Balanced Attack-28",
			"Instruction 29",
			"Instruction 30",
			"Instruction 31"
		};

		public int m_OffsetX;

		public int m_OffsetY;

		private int m_PlayerInstruction_1;

		private int m_PlayerInstruction_2;

		public Role Role
		{
			get
			{
				return m_Role;
			}
			set
			{
				m_Role = value;
				base.Id = m_Role.Id;
			}
		}

		public int OffsetX
		{
			get
			{
				return m_OffsetX;
			}
			set
			{
				m_OffsetX = value;
			}
		}

		public int OffsetY
		{
			get
			{
				return m_OffsetY;
			}
			set
			{
				m_OffsetY = value;
			}
		}

		public int PlayerInstruction_1
		{
			get
			{
				return m_PlayerInstruction_1;
			}
			set
			{
				m_PlayerInstruction_1 = value;
			}
		}

		public int PlayerInstruction_2
		{
			get
			{
				return m_PlayerInstruction_2;
			}
			set
			{
				m_PlayerInstruction_2 = value;
			}
		}

		public static int GetDefaultInstruction(int roleid)
		{
			if (roleid >= 0 && roleid < c_DefaultInstrucion.Length)
			{
				return c_DefaultInstrucion[roleid];
			}
			return 0;
		}

		public PlayingRole(Record r, int roleOrder, int fieldIndex)
			: base(r.GetAndCheckIntField(fieldIndex))
		{
			Load(r, roleOrder);
		}

		public PlayingRole(Role role)
			: base(role.Id)
		{
			Point center = role.GetCenter();
			m_Role = role;
			m_OffsetX = center.X;
			m_OffsetY = center.Y;
			m_PlayerInstruction_1 = c_DefaultInstrucion[(int)role.RoleId];
			m_PlayerInstruction_2 = c_DefaultInstrucion[(int)role.RoleId];
		}

		public bool ReInitialize(PlayingRole playingRole)
		{
			if (playingRole == null)
			{
				return false;
			}
			m_Role = playingRole.Role;
			m_RoleId = playingRole.m_RoleId;
			m_OffsetX = playingRole.m_OffsetX;
			m_OffsetY = playingRole.m_OffsetY;
			m_PlayerInstruction_1 = playingRole.m_PlayerInstruction_1;
			m_PlayerInstruction_2 = playingRole.m_PlayerInstruction_2;
			return true;
		}

		public override string ToString()
		{
			return Role.ToString();
		}

		public void LinkRole(RoleList roleList)
		{
			m_Role = (Role)roleList.SearchId(m_RoleId);
		}

		public void Load(Record r, int i)
		{
			TableDescriptor tableDescriptor = r.TableDescriptor;
			base.Id = i;
			switch (i)
			{
			case 0:
				m_RoleId = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("position0"));
				m_OffsetX = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset0x")] * 100f);
				m_OffsetY = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset0y")] * 100f);
				if (tableDescriptor.GetFieldIndex("playerinstruction0_1") >= 0)
				{
					m_PlayerInstruction_1 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction0_1"));
				}
				if (tableDescriptor.GetFieldIndex("playerinstruction0_2") >= 0)
				{
					m_PlayerInstruction_2 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction0_2"));
				}
				break;
			case 1:
				m_RoleId = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("position1"));
				m_OffsetX = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset1x")] * 100f);
				m_OffsetY = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset1y")] * 100f);
				if (tableDescriptor.GetFieldIndex("playerinstruction1_1") >= 0)
				{
					m_PlayerInstruction_1 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction1_1"));
				}
				if (tableDescriptor.GetFieldIndex("playerinstruction1_2") >= 0)
				{
					m_PlayerInstruction_2 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction1_2"));
				}
				break;
			case 2:
				m_RoleId = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("position2"));
				m_OffsetX = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset2x")] * 100f);
				m_OffsetY = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset2y")] * 100f);
				if (tableDescriptor.GetFieldIndex("playerinstruction2_1") >= 0)
				{
					m_PlayerInstruction_1 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction2_1"));
				}
				if (tableDescriptor.GetFieldIndex("playerinstruction2_2") >= 0)
				{
					m_PlayerInstruction_2 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction2_2"));
				}
				break;
			case 3:
				m_RoleId = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("position3"));
				m_OffsetX = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset3x")] * 100f);
				m_OffsetY = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset3y")] * 100f);
				if (tableDescriptor.GetFieldIndex("playerinstruction3_1") >= 0)
				{
					m_PlayerInstruction_1 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction3_1"));
				}
				if (tableDescriptor.GetFieldIndex("playerinstruction3_2") >= 0)
				{
					m_PlayerInstruction_2 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction3_2"));
				}
				break;
			case 4:
				m_RoleId = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("position4"));
				m_OffsetX = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset4x")] * 100f);
				m_OffsetY = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset4y")] * 100f);
				if (tableDescriptor.GetFieldIndex("playerinstruction4_1") >= 0)
				{
					m_PlayerInstruction_1 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction4_1"));
				}
				if (tableDescriptor.GetFieldIndex("playerinstruction4_2") >= 0)
				{
					m_PlayerInstruction_2 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction4_2"));
				}
				break;
			case 5:
				m_RoleId = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("position5"));
				m_OffsetX = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset5x")] * 100f);
				m_OffsetY = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset5y")] * 100f);
				if (tableDescriptor.GetFieldIndex("playerinstruction5_1") >= 0)
				{
					m_PlayerInstruction_1 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction5_1"));
				}
				if (tableDescriptor.GetFieldIndex("playerinstruction5_2") >= 0)
				{
					m_PlayerInstruction_2 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction5_2"));
				}
				break;
			case 6:
				m_RoleId = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("position6"));
				m_OffsetX = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset6x")] * 100f);
				m_OffsetY = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset6y")] * 100f);
				if (tableDescriptor.GetFieldIndex("playerinstruction6_1") >= 0)
				{
					m_PlayerInstruction_1 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction6_1"));
				}
				if (tableDescriptor.GetFieldIndex("playerinstruction6_2") >= 0)
				{
					m_PlayerInstruction_2 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction6_2"));
				}
				break;
			case 7:
				m_RoleId = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("position7"));
				m_OffsetX = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset7x")] * 100f);
				m_OffsetY = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset7y")] * 100f);
				if (tableDescriptor.GetFieldIndex("playerinstruction7_1") >= 0)
				{
					m_PlayerInstruction_1 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction7_1"));
				}
				if (tableDescriptor.GetFieldIndex("playerinstruction7_2") >= 0)
				{
					m_PlayerInstruction_2 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction7_2"));
				}
				break;
			case 8:
				m_RoleId = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("position8"));
				m_OffsetX = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset8x")] * 100f);
				m_OffsetY = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset8y")] * 100f);
				if (tableDescriptor.GetFieldIndex("playerinstruction8_1") >= 0)
				{
					m_PlayerInstruction_1 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction8_1"));
				}
				if (tableDescriptor.GetFieldIndex("playerinstruction8_2") >= 0)
				{
					m_PlayerInstruction_2 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction8_2"));
				}
				break;
			case 9:
				m_RoleId = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("position9"));
				m_OffsetX = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset9x")] * 100f);
				m_OffsetY = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset9y")] * 100f);
				if (tableDescriptor.GetFieldIndex("playerinstruction9_1") >= 0)
				{
					m_PlayerInstruction_1 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction9_1"));
				}
				if (tableDescriptor.GetFieldIndex("playerinstruction9_2") >= 0)
				{
					m_PlayerInstruction_2 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction9_2"));
				}
				break;
			case 10:
				m_RoleId = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("position10"));
				m_OffsetX = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset10x")] * 100f);
				m_OffsetY = (int)(r.FloatField[tableDescriptor.GetFieldIndex("offset10y")] * 100f);
				if (tableDescriptor.GetFieldIndex("playerinstruction10_1") >= 0)
				{
					m_PlayerInstruction_1 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction10_1"));
				}
				if (tableDescriptor.GetFieldIndex("playerinstruction10_2") >= 0)
				{
					m_PlayerInstruction_2 = r.GetAndCheckIntField(tableDescriptor.GetFieldIndex("playerinstruction10_2"));
				}
				break;
			}
			LinkRole(FifaEnvironment.Roles);
		}

		public void Save(Record r, int i)
		{
			switch (i)
			{
			case 0:
				r.IntField[FI.formations_position0] = m_Role.Id;
				r.FloatField[FI.formations_offset0x] = (float)m_OffsetX / 100f;
				r.FloatField[FI.formations_offset0y] = (float)m_OffsetY / 100f;
				if (FI.formations_playerinstruction0_1 >= 0)
				{
					r.IntField[FI.formations_playerinstruction0_1] = m_PlayerInstruction_1;
				}
				if (FI.formations_playerinstruction0_2 >= 0)
				{
					r.IntField[FI.formations_playerinstruction0_2] = m_PlayerInstruction_2;
				}
				break;
			case 1:
				r.IntField[FI.formations_position1] = m_Role.Id;
				r.FloatField[FI.formations_offset1x] = (float)m_OffsetX / 100f;
				r.FloatField[FI.formations_offset1y] = (float)m_OffsetY / 100f;
				if (FI.formations_playerinstruction1_1 >= 0)
				{
					r.IntField[FI.formations_playerinstruction1_1] = m_PlayerInstruction_1;
				}
				if (FI.formations_playerinstruction1_2 >= 0)
				{
					r.IntField[FI.formations_playerinstruction1_2] = m_PlayerInstruction_2;
				}
				break;
			case 2:
				r.IntField[FI.formations_position2] = m_Role.Id;
				r.FloatField[FI.formations_offset2x] = (float)m_OffsetX / 100f;
				r.FloatField[FI.formations_offset2y] = (float)m_OffsetY / 100f;
				if (FI.formations_playerinstruction2_1 >= 0)
				{
					r.IntField[FI.formations_playerinstruction2_1] = m_PlayerInstruction_1;
				}
				if (FI.formations_playerinstruction2_2 >= 0)
				{
					r.IntField[FI.formations_playerinstruction2_2] = m_PlayerInstruction_2;
				}
				break;
			case 3:
				r.IntField[FI.formations_position3] = m_Role.Id;
				r.FloatField[FI.formations_offset3x] = (float)m_OffsetX / 100f;
				r.FloatField[FI.formations_offset3y] = (float)m_OffsetY / 100f;
				if (FI.formations_playerinstruction3_1 >= 0)
				{
					r.IntField[FI.formations_playerinstruction3_1] = m_PlayerInstruction_1;
				}
				if (FI.formations_playerinstruction3_2 >= 0)
				{
					r.IntField[FI.formations_playerinstruction3_2] = m_PlayerInstruction_2;
				}
				break;
			case 4:
				r.IntField[FI.formations_position4] = m_Role.Id;
				r.FloatField[FI.formations_offset4x] = (float)m_OffsetX / 100f;
				r.FloatField[FI.formations_offset4y] = (float)m_OffsetY / 100f;
				if (FI.formations_playerinstruction4_1 >= 0)
				{
					r.IntField[FI.formations_playerinstruction4_1] = m_PlayerInstruction_1;
				}
				if (FI.formations_playerinstruction4_2 >= 0)
				{
					r.IntField[FI.formations_playerinstruction4_2] = m_PlayerInstruction_2;
				}
				break;
			case 5:
				r.IntField[FI.formations_position5] = m_Role.Id;
				r.FloatField[FI.formations_offset5x] = (float)m_OffsetX / 100f;
				r.FloatField[FI.formations_offset5y] = (float)m_OffsetY / 100f;
				if (FI.formations_playerinstruction5_1 >= 0)
				{
					r.IntField[FI.formations_playerinstruction5_1] = m_PlayerInstruction_1;
				}
				if (FI.formations_playerinstruction5_2 >= 0)
				{
					r.IntField[FI.formations_playerinstruction5_2] = m_PlayerInstruction_2;
				}
				break;
			case 6:
				r.IntField[FI.formations_position6] = m_Role.Id;
				r.FloatField[FI.formations_offset6x] = (float)m_OffsetX / 100f;
				r.FloatField[FI.formations_offset6y] = (float)m_OffsetY / 100f;
				if (FI.formations_playerinstruction6_1 >= 0)
				{
					r.IntField[FI.formations_playerinstruction6_1] = m_PlayerInstruction_1;
				}
				if (FI.formations_playerinstruction6_2 >= 0)
				{
					r.IntField[FI.formations_playerinstruction6_2] = m_PlayerInstruction_2;
				}
				break;
			case 7:
				r.IntField[FI.formations_position7] = m_Role.Id;
				r.FloatField[FI.formations_offset7x] = (float)m_OffsetX / 100f;
				r.FloatField[FI.formations_offset7y] = (float)m_OffsetY / 100f;
				if (FI.formations_playerinstruction7_1 >= 0)
				{
					r.IntField[FI.formations_playerinstruction7_1] = m_PlayerInstruction_1;
				}
				if (FI.formations_playerinstruction7_2 >= 0)
				{
					r.IntField[FI.formations_playerinstruction7_2] = m_PlayerInstruction_2;
				}
				break;
			case 8:
				r.IntField[FI.formations_position8] = m_Role.Id;
				r.FloatField[FI.formations_offset8x] = (float)m_OffsetX / 100f;
				r.FloatField[FI.formations_offset8y] = (float)m_OffsetY / 100f;
				if (FI.formations_playerinstruction8_1 >= 0)
				{
					r.IntField[FI.formations_playerinstruction8_1] = m_PlayerInstruction_1;
				}
				if (FI.formations_playerinstruction8_2 >= 0)
				{
					r.IntField[FI.formations_playerinstruction8_2] = m_PlayerInstruction_2;
				}
				break;
			case 9:
				r.IntField[FI.formations_position9] = m_Role.Id;
				r.FloatField[FI.formations_offset9x] = (float)m_OffsetX / 100f;
				r.FloatField[FI.formations_offset9y] = (float)m_OffsetY / 100f;
				if (FI.formations_playerinstruction9_1 >= 0)
				{
					r.IntField[FI.formations_playerinstruction9_1] = m_PlayerInstruction_1;
				}
				if (FI.formations_playerinstruction9_2 >= 0)
				{
					r.IntField[FI.formations_playerinstruction9_2] = m_PlayerInstruction_2;
				}
				break;
			case 10:
				r.IntField[FI.formations_position10] = m_Role.Id;
				r.FloatField[FI.formations_offset10x] = (float)m_OffsetX / 100f;
				r.FloatField[FI.formations_offset10y] = (float)m_OffsetY / 100f;
				if (FI.formations_playerinstruction10_1 >= 0)
				{
					r.IntField[FI.formations_playerinstruction10_1] = m_PlayerInstruction_1;
				}
				if (FI.formations_playerinstruction10_2 >= 0)
				{
					r.IntField[FI.formations_playerinstruction10_2] = m_PlayerInstruction_2;
				}
				break;
			}
		}
	}
}
