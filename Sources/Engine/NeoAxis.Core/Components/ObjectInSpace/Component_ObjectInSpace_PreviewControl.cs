﻿// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;

namespace NeoAxis.Editor
{
	public partial class Component_ObjectInSpace_PreviewControl : PreviewControlWithViewport
	{
		public Component_ObjectInSpace_PreviewControl()
		{
			InitializeComponent();
		}

		[Browsable( false )]
		protected virtual bool EnableViewportControl
		{
			get
			{
				var objectInSpace = ObjectForPreview as Component_ObjectInSpace;
				if( objectInSpace != null && objectInSpace.ParentScene == null )//show only if no scene
					return true;
				return false;
			}
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			if( EnableViewportControl )
			{
				var objectInSpace = ObjectForPreview as Component_ObjectInSpace;
				if( objectInSpace != null && objectInSpace.ParentScene == null )//show only if no scene
				{
					var scene = CreateScene( false );

					var type = objectInSpace.GetProvidedType();
					if( type != null )
					{
						var obj = (Component_ObjectInSpace)scene.CreateComponent( type );
						obj.Transform = Transform.Identity;
					}

					scene.Enabled = true;
					SetCameraByBounds( scene.CalculateTotalBoundsOfObjectsInSpace() );
				}
			}
			else
			{
				ViewportControl.AllowCreateRenderWindow = false;
				ViewportControl.Visible = false;
			}
		}
	}
}
