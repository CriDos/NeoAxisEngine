﻿// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
#if !PROJECT_DEPLOY
using Microsoft.WindowsAPICodePack.Dialogs;
#endif
using ComponentFactory.Krypton.Toolkit;

namespace NeoAxis.Editor
{
	/// <summary>
	/// Represents the Import Window.
	/// </summary>
	public partial class ImportWindow : DockWindow
	{
		[Browsable( false )]
		public string InitialDestinationFolder = "";

		/////////////////////////////////////////

		public ImportWindow()
		{
			InitializeComponent();

			//HACK: kryptonsplitcontainer childs layout broken. see comments in kryptonsplitcontainer.cs
			panelName.Width = panelName.Parent.Width - DpiHelper.Default.ScaleValue( 8 );

			buttonDestinationFolderBrowse.Values.Image = EditorResourcesCache.SelectFolder;

			CloseByEscape = true;

			WindowTitle = EditorLocalization.Translate( "ImportWindow", WindowTitle );
			EditorLocalization.TranslateForm( "ImportWindow", eUserControl1 );

			DarkThemeUtility.ApplyToForm( eUserControl1 );
			DarkThemeUtility.ApplyToForm( panelName );
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			if( EditorUtility.IsDesignerHosted( this ) )
				return;

			textBoxDestinationFolder.Location = new Point( labelName.Location.X + labelName.Width + DpiHelper.Default.ScaleValue( 2 ), DpiHelper.Default.ScaleValue( 2 ) );
			textBoxDestinationFolder.Width = panelName.Width - labelName.Width - buttonDestinationFolderBrowse.Width - DpiHelper.Default.ScaleValue( 7 );
			textBoxDestinationFolder.Text = InitialDestinationFolder;
			UpdateContentBrowser();

			timer1.Start();

			UpdateControls();
		}

		void UpdateContentBrowser()
		{
			contentBrowser1.Init( null, null, null );

			var roots = new List<ContentBrowser.Item>();
			foreach( var drive in DriveInfo.GetDrives() )
			{
				var dataItem = new ContentBrowserItem_File( contentBrowser1, null, drive.Name, true );
				dataItem.imageKey = "Folder";
				roots.Add( dataItem );
			}
			contentBrowser1.SetData( roots );
		}

		private void contentBrowser1_ItemAfterChoose( ContentBrowser sender, ContentBrowser.Item item, ref bool handled )
		{
			if( EditorUtility.IsDesignerHosted( this ) )
				return;

			handled = true;
			if( IsReadyToImport( out _ ) )
				ButtonImport_Click( null, null );
		}

		string Translate( string text )
		{
			return EditorLocalization.Translate( "ImportWindow", text );
		}

		bool IsReadyToImport( out string reason )
		{
			reason = "";

			if( contentBrowser1.SelectedItems.Length == 0 )
			{
				reason = Translate( "No selected items." );
				return false;
			}

			return true;
		}

		void UpdateControls()
		{
			buttonImport.Enabled = IsReadyToImport( out string reason );
			//buttonCreateAndClose.Enabled = buttonCreate.Enabled;
			labelError.Text = reason;
		}

		//public Control AddCell( Type cellClass )//, bool getIfAlreadyCreated )
		//{
		//	//if( getIfAlreadyCreated )
		//	//{
		//	//	foreach( Control c in layoutPanel.Controls )
		//	//	{
		//	//		if( cellClass.IsAssignableFrom( c.GetType() ) )
		//	//			return c;
		//	//	}
		//	//}

		//	Control cell = (Control)cellClass.GetConstructor( new Type[ 0 ] ).Invoke( new object[ 0 ] );
		//	cell.Anchor = AnchorStyles.Left | AnchorStyles.Right;
		//	tableLayoutPanel1.Controls.Add( cell );

		//	return cell;
		//}

		//void UpdateCells()
		//{
		//	tableLayoutPanel1.Controls.Clear();

		//	if( SelectedType != null )
		//	{
		//		//add cells by EditorNewObjectSettingsAttribute
		//		{
		//			var ar = SelectedType.GetNetType().GetCustomAttributes( typeof( EditorNewObjectSettingsAttribute ), true );
		//			if( ar.Length != 0 )
		//			{
		//				Type settingsClass = ( (EditorNewObjectSettingsAttribute)ar[ 0 ] ).SettingsClass;
		//				var cell = (NewObjectSettingsCell)AddCell( typeof( NewObjectSettingsCell ) );
		//				cell.Init( settingsClass, this );// IsFileCreation() );
		//			}
		//		}

		//		//add cells by EditorNewObjectCellAttribute
		//		{
		//			var ar = SelectedType.GetNetType().GetCustomAttributes( typeof( EditorNewObjectCellAttribute ), true );
		//			foreach( EditorNewObjectCellAttribute attr in ar )
		//				AddCell( attr.CellClass );
		//		}

		//		//!!!!
		//		//change docking
		//		if( tableLayoutPanel1.Controls.Count == 1 )
		//		{
		//			tableLayoutPanel1.Controls[ 0 ].Size = new Size( 30, 15 );
		//			tableLayoutPanel1.Controls[ 0 ].Dock = DockStyle.Fill;
		//		}
		//	}
		//}

		//private void TextBoxName_TextChanged( object sender, EventArgs e )
		//{
		//	UpdateCreationPath();
		//}

		private void ButtonImport_Click( object sender, EventArgs e )
		{
			if( !Import() )
				return;
			Close();
		}

		//private void ButtonCreateAndClose_Click( object sender, EventArgs e )
		//{
		//	//if( !CreateObject() )
		//	//	return;

		//	Close();
		//}

		private void ButtonClose_Click( object sender, EventArgs e )
		{
			Close();
		}

		private void timer1_Tick( object sender, EventArgs e )
		{
			if( !IsHandleCreated || EditorUtility.IsDesignerHosted( this ) || EditorAPI.ClosingApplication )
				return;
			if( !EditorUtility.IsControlVisibleInHierarchy( this ) )
				return;

			UpdateControls();
		}

		private void textBoxDestinationFolder_TextChanged( object sender, EventArgs e )
		{
		}

		private void buttonDestinationFolderBrowse_Click( object sender, EventArgs e )
		{
#if !PROJECT_DEPLOY
			again:;

			string destRealFolder = VirtualPathUtility.GetRealPathByVirtual( textBoxDestinationFolder.Text );

			var dialog = new CommonOpenFileDialog();
			dialog.InitialDirectory = destRealFolder;
			dialog.IsFolderPicker = true;
			if( dialog.ShowDialog() == CommonFileDialogResult.Ok )
			{
				destRealFolder = dialog.FileName;

				if( !VirtualPathUtility.GetVirtualPathByReal( destRealFolder, out var virtualPath ) )
				{
					EditorMessageBox.ShowWarning( Translate( "Destination folder must be inside Assets folder." ) );
					goto again;
				}

				textBoxDestinationFolder.Text = virtualPath;
			}
#endif
		}

		//public override ObjectsInFocus GetObjectsInFocus()
		//{
		//	return new ObjectsInFocus( contentBrowser1.DocumentWindow, contentBrowser1.GetSelectedContainedObjects() );
		//}

		bool Import()
		{
			string destRealFolder = VirtualPathUtility.GetRealPathByVirtual( textBoxDestinationFolder.Text );

			List<string> fileNames = new List<string>();
			foreach( var item in contentBrowser1.SelectedItems )
			{
				if( item is ContentBrowserItem_File fileItem )
					fileNames.Add( fileItem.FullPath );
			}

			if( fileNames.Count != 0 )
				EditorImportResource.Import( fileNames.ToArray(), destRealFolder );

			return true;
		}

		private void eUserControl1_Load( object sender, EventArgs e )
		{

		}
	}
}
