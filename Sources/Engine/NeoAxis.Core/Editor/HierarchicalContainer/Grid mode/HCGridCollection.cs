﻿// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;

namespace NeoAxis.Editor
{
	public partial class HCGridCollection : UserControl, IHCCollection
	{
		public HCGridCollection()
		{
			InitializeComponent();

			if( EditorAPI.DarkTheme )
				label2.StateCommon.Back.Color1 = Color.FromArgb( 54, 54, 54 );
		}

		public HCKryptonTextBox Label2
		{
			get { return label2; }
		}

		public KryptonButton ButtonEdit
		{
			get { return buttonEdit; }
		}
	}
}
