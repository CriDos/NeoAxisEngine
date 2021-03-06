﻿// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.Drawing;
using System.Windows.Forms;
using ComponentFactory.Krypton.Ribbon;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using System.IO.Compression;
#if !PROJECT_DEPLOY
using Microsoft.WindowsAPICodePack.Dialogs;
#endif
using ComponentFactory.Krypton.Toolkit;

namespace NeoAxis.Editor
{
	/// <summary>
	/// Represents the Backstage of the editor.
	/// </summary>
	public partial class BackstageMenu : BackstageAppMenu
	{
		static bool backstageVisible;

		//

		public BackstageMenu()
		{
			InitializeComponent();

			kryptonNavigator1.AllowPageReorder = false;
			kryptonNavigator1.AllowTabFocus = false;

			kryptonLinkLabel1.LinkClicked += KryptonLinkLabel1_LinkClicked;

			// downscale back image. optional. we can downscale image on krypton render level based on button size
			//kryptonButtonBack.Values.Image = DpiHelper.Default.ScaleBitmapToSize(
			//	(Bitmap)kryptonButtonBack.Values.Image, DpiHelper.Default.ScaleValue( 45 ), false,
			//	System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic );

			kryptonNavigator1.Bar.ItemMinimumSize = DpiHelper.Default.ScaleValue( new Size( 160, 50 ) );
			kryptonNavigator1.Bar.BarFirstItemInset = DpiHelper.Default.ScaleValue( 65 );

			kryptonLabel12.Text = EngineInfo.NameWithoutVersion;

			try
			{
				var assembly = Assembly.GetExecutingAssembly();
				var fvi = FileVersionInfo.GetVersionInfo( assembly.Location );
				string version = fvi.FileVersion;
				kryptonLabelEngineVersion.Text = version;
			}
			catch { }
		}

		public void SelectDefaultPage()
		{
			kryptonNavigator1.SelectedPage = kryptonPageInfo;
		}

		string Translate( string text )
		{
			return EditorLocalization.Translate( "Backstage", text );
		}

		private void BackstageMenu_Load( object sender, EventArgs e )
		{
			if( DesignMode )
				return;
			if( EditorUtility.IsDesignerHosted( this ) )
				return;

			if( EditorAPI.DarkTheme )
			{
				kryptonNavigator1.StateCommon.Panel.Color1 = Color.FromArgb( 10, 10, 10 );
				kryptonNavigator1.StateSelected.Tab.Back.Color1 = Color.FromArgb( 60, 60, 60 );
				kryptonNavigator1.StatePressed.Tab.Back.Color1 = Color.FromArgb( 60, 60, 60 );
				kryptonNavigator1.StateTracking.Tab.Back.Color1 = Color.FromArgb( 50, 50, 50 );

				//kryptonNavigator1.StateSelected.Tab.Back.Color1 = Color.FromArgb( 70, 70, 70 );
				//kryptonNavigator1.StatePressed.Tab.Back.Color1 = Color.FromArgb( 60, 60, 60 );
				//kryptonNavigator1.StateTracking.Tab.Back.Color1 = Color.FromArgb( 60, 60, 60 );
				kryptonButtonBack.StateCommon.Back.Color1 = Color.FromArgb( 10, 10, 10 );

				BackColor = Color.FromArgb( 10, 10, 10 );

				kryptonLinkLabel1.LabelStyle = LabelStyle.Custom1;
				kryptonLinkLabel1.StateCommon.ShortText.Color1 = Color.FromArgb( 230, 230, 230 );

				//kryptonNavigator1.StateCommon.Panel.Color1 = Color.FromArgb( 40, 40, 40 );
				//kryptonNavigator1.StateSelected.Tab.Back.Color1 = Color.FromArgb( 54, 54, 54 );

				//kryptonNavigator1.StateCommon.Panel.Color1 = Color.FromArgb( 54, 54, 54 );
				//kryptonNavigator1.StateSelected.Tab.Back.Color1 = Color.FromArgb( 70, 70, 70 );
				//kryptonNavigator1.StatePressed.Tab.Back.Color1 = Color.FromArgb( 60, 60, 60 );
				//kryptonNavigator1.StateTracking.Tab.Back.Color1 = Color.FromArgb( 60, 60, 60 );
				//kryptonButtonBack.StateCommon.Back.Color1 = Color.FromArgb( 54, 54, 54 );

				foreach( var page in kryptonNavigator1.Pages )
				{
					//page.StateCommon.Page.Color1 = Color.FromArgb( 80, 80, 80 );
					page.StateCommon.Page.Color1 = Color.FromArgb( 40, 40, 40 );

					//page.StateCommon.Page.Color1 = Color.FromArgb( 54, 54, 54 );

					DarkThemeUtility.ApplyToForm( page );
				}

				kryptonLabelLoginError.StateCommon.ShortText.Color1 = Color.Red;
			}

			//translate
			{
				foreach( var page in kryptonNavigator1.Pages )
					page.Text = Translate( page.Text );

				EditorLocalization.TranslateForm( "Backstage", kryptonPageInfo );
				EditorLocalization.TranslateForm( "Backstage", kryptonPageNew );
				EditorLocalization.TranslateForm( "Backstage", kryptonPageBuild );
				//EditorLocalization.TranslateForm( kryptonPageAbout, "Backstage" );
			}

			LoginLoad();

			timer1.Start();
		}

		private void BackstageMenu_VisibleChanged( object sender, EventArgs e )
		{
			if( EditorUtility.IsDesignerHosted( this ) )
				return;

			if( Visible )
			{
				InfoInit();
				NewInit();
				//OpenInit();
				packagingNeedInit = true;
			}

			backstageVisible = Visible;
			//EditorForm.Instance.UpdateVisibilityOfFloatingWindows( !Visible );
		}

		private void kryptonNavigator1_TabClicked( object sender, ComponentFactory.Krypton.Navigator.KryptonPageEventArgs e )
		{
			//if( e.Item == kryptonPageSettings )
			//{
			//	Hide();
			//	EditorAPI.ShowProjectSettings();
			//	return;
			//}

			if( e.Item == kryptonPageBuild )
			{
				if( packagingNeedInit )
					PackagingInit();
				return;
			}

			if( e.Item == kryptonPageExit )
			{
				EditorForm.Instance.Close();
				return;
			}
		}

		private void kryptonButtonBack_Click( object sender, EventArgs e )
		{
			Hide();
		}

		private void timer1_Tick( object sender, EventArgs e )
		{
			if( !IsHandleCreated || EditorUtility.IsDesignerHosted( this ) || EditorAPI.ClosingApplication )
				return;
			if( !EditorUtility.IsControlVisibleInHierarchy( this ) )
				return;

			InfoUpdate();
			NewUpdate();
			//OpenUpdate();
			PackagingUpdate();
			LoginUpdate();
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		void InfoInit()
		{
			//InfoUpdate();
		}

		void InfoUpdate()
		{
			kryptonTextBoxInfoName.Text = ProjectSettings.Get != null ? ProjectSettings.Get.ProjectName.Value : "";
			kryptonTextBoxInfoLocation.Text = VirtualFileSystem.Directories.Project;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		volatile bool creationInProgress;
		Task creationTask;
		volatile int creationProgressBarValue;
		string creationDirectory;

		void NewInit()
		{
			{
				string folder = "";
				for( int n = 1; ; n++ )
				{
					folder = Path.Combine( GetNeoAxisProjectsFolder(), "New Project " + n.ToString() );
					if( !Directory.Exists( folder ) )
						break;
				}
				kryptonTextBoxNewFolder.Text = folder;
			}

			var items = new List<ContentBrowser.Item>();

			ContentBrowserItem_Virtual item;

			item = new ContentBrowserItem_Virtual( objectsBrowserNew, null, Translate( "Copy of this project" ) );
			item.Tag = "Copy";
			var firstItem = item;
			items.Add( item );

			//!!!!

			item = new ContentBrowserItem_Virtual( objectsBrowserNew, null, Translate( "Default project (Use NeoAxis Launcher to create project from a basic template)" ) );
			item.ShowDisabled = true;
			item.Tag = "DefaultProjectDisabled";
			items.Add( item );

			//item = new ContentBrowserItem_Virtual( objectsBrowserNew, null, Translate( "Empty project (Use NeoAxis Launcher to create project from a basic template)" ) );
			//item.showDisabled = true;
			//item.Tag = "EmptyProject";
			//items.Add( item );

			//item = new ContentBrowserItem_Virtual( objectsBrowserNew, null, "Template 1" );
			//item.showDisabled = true;
			//items.Add( item );

			//item = new ContentBrowserItem_Virtual( objectsBrowserNew, null, "Template 2" );
			//item.showDisabled = true;
			//items.Add( item );

			//item = new ContentBrowserItem_Virtual( objectsBrowserNew, null, "Template 3" );
			//item.showDisabled = true;
			//items.Add( item );

			bool developerBuildConfig = File.Exists( Path.Combine( VirtualFileSystem.Directories.Project, "NeoAxis.DeveloperBuild.config" ) );
			if( developerBuildConfig )
			{
				item = new ContentBrowserItem_Virtual( objectsBrowserNew, null, "(Developer mode) Public build (with demos)" );
				item.Tag = "PublicBuild";
				items.Add( item );

				item = new ContentBrowserItem_Virtual( objectsBrowserNew, null, "(Developer mode) Public build (without demos)" );
				item.Tag = "PublicBuildEmpty";
				items.Add( item );

				//item = new ContentBrowserItem_Virtual( objectsBrowserNew, null, "(Developer mode) Public build (with sample content)" );
				//item.Tag = "PublicBuild";
				//items.Add( item );

				//item = new ContentBrowserItem_Virtual( objectsBrowserNew, null, "(Developer mode) Public build (Empty project)" );
				//item.Tag = "PublicBuildEmpty";
				//items.Add( item );
			}

			objectsBrowserNew.SetData( items, false );

			objectsBrowserNew.SelectItems( new ContentBrowser.Item[] { firstItem } );
		}

		private void kryptonButtonNewBrowse_Click( object sender, EventArgs e )
		{
			var path = kryptonTextBoxNewFolder.Text;

			while( true )
			{
				if( !string.IsNullOrEmpty( path ) && !Directory.Exists( path ) )
				{
					try
					{
						path = Path.GetDirectoryName( path );
						continue;
					}
					catch { }
				}
				break;
			}

#if !PROJECT_DEPLOY
			CommonOpenFileDialog dialog = new CommonOpenFileDialog();
			dialog.InitialDirectory = path;
			dialog.IsFolderPicker = true;
			if( dialog.ShowDialog() == CommonFileDialogResult.Ok )
				kryptonTextBoxNewFolder.Text = dialog.FileName;
#endif
		}

		void NewUpdate()
		{
			kryptonButtonNewCreate.Enabled = CanCreateProject();
			kryptonTextBoxNewFolder.Enabled = !creationInProgress;
			kryptonButtonNewBrowse.Enabled = !creationInProgress;
			objectsBrowserNew.Enabled = !creationInProgress;

			progressBarNew.Value = creationProgressBarValue;
			progressBarNew.Visible = creationInProgress;
			kryptonButtonNewCancel.Visible = creationInProgress;
		}

		bool CanCreateProject()
		{
			string folder = kryptonTextBoxNewFolder.Text.Trim();
			if( string.IsNullOrEmpty( folder ) )
				return false;
			if( !Path.IsPathRooted( folder ) )
				return false;
			if( objectsBrowserNew.SelectedItems.Length == 0 )
				return false;

			if( objectsBrowserNew.SelectedItems[ 0 ].ShowDisabled )
				return false;
			//var tag = objectsBrowserNew.SelectedItems[ 0 ].Tag as string;
			//if( tag != "Copy" && tag != "Clean" )
			//	return false;

			if( creationInProgress )
				return false;
			return true;
		}

		private void kryptonButtonNewCreate_Click( object sender, EventArgs e )
		{
			//!!!!

			string folder = kryptonTextBoxNewFolder.Text.Trim();
			if( string.IsNullOrEmpty( folder ) )
				return;

			try
			{
				while( Directory.Exists( folder ) && !IOUtility.IsDirectoryEmpty( folder ) )
				{
					var text = string.Format( Translate( "Destination folder \'{0}\' is not empty. Clear folder and continue?" ), folder );
					var result = EditorMessageBox.ShowQuestion( text, MessageBoxButtons.OKCancel );
					if( result == DialogResult.Cancel )
						return;

					IOUtility.ClearDirectory( folder );
				}

				if( !Directory.Exists( folder ) )
					Directory.CreateDirectory( folder );

				creationInProgress = true;
				progressBarNew.Visible = true;
				kryptonButtonNewCancel.Visible = true;
				creationDirectory = folder;

				var tag = objectsBrowserNew.SelectedItems[ 0 ].Tag as string;
				creationTask = new Task( CreationFunction, tag );
				creationTask.Start();
			}
			catch( Exception ex )
			{
				EditorMessageBox.ShowWarning( ex.Message );
				return;
			}
		}

		void CreationFunction( object parameter )
		{
			var tag = (string)parameter;

			creationProgressBarValue = 0;

			try
			{
				string sourcePath = VirtualFileSystem.Directories.Project;

				FileInfo[] allFiles = new DirectoryInfo( sourcePath ).GetFiles( "*.*", SearchOption.AllDirectories );

				long totalLength = 0;
				foreach( var fileInfo in allFiles )
					totalLength += fileInfo.Length;

				foreach( string dirPath in Directory.GetDirectories( sourcePath, "*", SearchOption.AllDirectories ) )
				{
					if( Directory.Exists( dirPath ) )
						Directory.CreateDirectory( dirPath.Replace( sourcePath, creationDirectory ) );
				}

				long processedLength = 0;
				foreach( var fileInfo in allFiles )
				{
					if( File.Exists( fileInfo.FullName ) )
						File.Copy( fileInfo.FullName, fileInfo.FullName.Replace( sourcePath, creationDirectory ), false );

					processedLength += fileInfo.Length;
					creationProgressBarValue = (int)( (double)processedLength / (double)totalLength * 100.0 );
					if( creationProgressBarValue > 100 )
						creationProgressBarValue = 100;

					if( !creationInProgress )
						return;
				}

				//if( tag == "Clean" )
				//{
				//	var directory = Path.Combine( creationDirectory, @"Project\Data\_Dev" );
				//	EditorMessageBox.ShowInfo( directory );
				//}

				//Public build
				if( tag == "PublicBuild" || tag == "PublicBuildEmpty" )
				{
					//delete files from root of Assets
					{
						var dataDirectory = Path.Combine( creationDirectory, @"Assets" );
						var info = new DirectoryInfo( dataDirectory );
						foreach( var file in info.GetFiles() )
							file.Delete();
					}

					//clear User settings
					var path = Path.Combine( creationDirectory, @"User settings" );
					if( Directory.Exists( path ) )
						IOUtility.ClearDirectory( path );

					//delete _Dev
					{
						path = Path.Combine( creationDirectory, @"Assets\_Dev" );
						if( Directory.Exists( path ) )
							Directory.Delete( path, true );
					}

					//delete Caches\Files\_Dev
					{
						path = Path.Combine( creationDirectory, @"Caches\Files\_Dev" );
						if( Directory.Exists( path ) )
							Directory.Delete( path, true );
					}

					//delete Binaries\NeoAxis.Internal\Localization
					path = Path.Combine( creationDirectory, @"Binaries\NeoAxis.Internal\Localization" );
					if( Directory.Exists( path ) )
						Directory.Delete( path, true );

					if( tag == "PublicBuildEmpty" )
					{
						//delete _Tests
						path = Path.Combine( creationDirectory, @"Assets\_Tests" );
						if( Directory.Exists( path ) )
							Directory.Delete( path, true );

						//delete Samples\Nature Demo
						path = Path.Combine( creationDirectory, @"Assets\Samples\Nature Demo" );
						if( Directory.Exists( path ) )
							Directory.Delete( path, true );

						//delete Samples\Sci-fi Demo
						path = Path.Combine( creationDirectory, @"Assets\Samples\Sci-fi Demo" );
						if( Directory.Exists( path ) )
							Directory.Delete( path, true );

						////delete Samples
						//path = Path.Combine( creationDirectory, @"Assets\Samples" );
						//if( Directory.Exists( path ) )
						//	Directory.Delete( path, true );

						//delete Caches\Files\_Tests
						path = Path.Combine( creationDirectory, @"Caches\Files\_Tests" );
						if( Directory.Exists( path ) )
							Directory.Delete( path, true );

						//delete Caches\Files\Samples\Sci-fi Demo
						path = Path.Combine( creationDirectory, @"Caches\Files\Samples\Sci-fi Demo" );
						if( Directory.Exists( path ) )
							Directory.Delete( path, true );

						////delete Caches\Files
						//path = Path.Combine( creationDirectory, @"Caches\Files" );
						//if( Directory.Exists( path ) )
						//	Directory.Delete( path, true );

						//fix EditorDockingDefault.config
						try
						{
							File.Copy(
								Path.Combine( sourcePath, @"Assets\Base\Tools\EditorDockingDefault_Empty.config" ),
								Path.Combine( creationDirectory, @"Assets\Base\Tools\EditorDockingDefault.config" ), true );
						}
						catch { }

						////fix Project.csproj
						//try
						//{
						//	var fileName = Path.Combine( creationDirectory, "Project.csproj" );

						//	var newLines = new List<string>();
						//	foreach( var line in File.ReadAllLines( fileName ) )
						//	{
						//		if( !line.Contains( @"Assets\Samples\" ) )
						//			newLines.Add( line );
						//	}

						//	File.WriteAllLines( fileName, newLines );
						//}
						//catch { }
					}

					//delete EditorDockingDefault_Empty.config
					try
					{
						File.Delete( Path.Combine( creationDirectory, @"Assets\Base\Tools\EditorDockingDefault_Empty.config" ) );
					}
					catch { }

					//delete NeoAxis.DeveloperBuild.config
					path = Path.Combine( creationDirectory, @"NeoAxis.DeveloperBuild.config" );
					if( File.Exists( path ) )
						File.Delete( path );

					//delete obj folders
					foreach( string dirPath in Directory.GetDirectories( creationDirectory, "obj", SearchOption.AllDirectories ) )
					{
						if( Directory.Exists( dirPath ) )
							Directory.Delete( dirPath, true );
					}

					//delete .vs folders
					foreach( string dirPath in Directory.GetDirectories( creationDirectory, ".vs", SearchOption.AllDirectories ) )
					{
						if( Directory.Exists( dirPath ) )
							Directory.Delete( dirPath, true );
					}

					//rewrite ProjectSettings.component
					{
						var content = @".component NeoAxis.Component_ProjectSettings
{
	Name = All

	.component NeoAxis.Component_ProjectSettings_PageBasic
	{
		Name = General
	}
	.component NeoAxis.Component_ProjectSettings_PageBasic
	{
		Name = Scene Editor
	}
	.component NeoAxis.Component_ProjectSettings_PageBasic
	{
		Name = UI Editor
	}
	.component NeoAxis.Component_ProjectSettings_PageBasic
	{
		Name = C# Editor
	}
	.component NeoAxis.Component_ProjectSettings_PageBasic
	{
		Name = Shader Editor
	}
	.component NeoAxis.Component_ProjectSettings_PageBasic
	{
		Name = Text Editor
	}
	.component NeoAxis.Component_ProjectSettings_PageBasic
	{
		Name = Ribbon and Toolbar
	}
	.component NeoAxis.Component_ProjectSettings_PageBasic
	{
		Name = Shortcuts
	}
	.component NeoAxis.Component_ProjectSettings_PageBasic
	{
		Name = Custom Splash Screen
	}
}";
						var fileName = Path.Combine( creationDirectory, @"Assets\Base\ProjectSettings.component" );
						File.WriteAllText( fileName, content );
					}

					//make zip archive
					{
						var fileName = tag == "PublicBuild" ? "Default project with Starter Content, Sci-fi Demo, Nature Demo, Test scenes.zip" : "Default project with Starter Content.zip";
						//var fileName = tag == "PublicBuild" ? "Default project with sample content.zip" : "Empty project.zip";
						var zipfileName = Path.Combine( creationDirectory, fileName );

						//can't write zip file to same folder. using temp file.
						var tempFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".zip";
						ZipFile.CreateFromDirectory( creationDirectory, tempFileName, CompressionLevel.Optimal, false );
						File.Copy( tempFileName, zipfileName );
						File.Delete( tempFileName );

						//ZipFile.CreateFromDirectory( creationDirectory, zipfileName, CompressionLevel.Optimal, false );
					}
				}

				creationProgressBarValue = 100;

				//open folder
				try
				{
					Win32Utility.ShellExecuteEx( null, creationDirectory );
					//Process.Start( "explorer.exe", creationDirectory );
				}
				catch { }

				////run process
				//string executableFileName = Path.Combine( creationDirectory, "NeoAxis.Studio.exe" );
				//var runMapProcess = Process.Start( executableFileName, "" );

				//end
				creationInProgress = false;

				ScreenNotifications.Show( Translate( "The project was created successfully." ) );
			}
			catch( Exception ex )
			{
				EditorMessageBox.ShowWarning( ex.Message );
			}
		}

		private void kryptonButtonNewCancel_Click( object sender, EventArgs e )
		{
			//!!!!удалить незаконченное?
			//!!!!спросить MessageBox'ом об отмене?

			creationInProgress = false;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		bool packagingNeedInit;
		ProductBuildInstance packageBuildInstance;

		void PackagingInit()
		{
			packagingNeedInit = false;

			//files
			string[] files;
			try
			{
				files = VirtualDirectory.GetFiles( "", "*.product", SearchOption.AllDirectories );
			}
			catch
			{
				files = new string[ 0 ];
			}

			var items = new List<ContentBrowser.Item>();

			foreach( var virtualPath in files )
			{
				string fileName = Path.GetFileName( virtualPath );

				var alreadyAddedImages = new ESet<string>();

				var packageComponent = ResourceManager.LoadResource<Component_Product>( virtualPath );
				if( packageComponent != null )
				{
					string imageKey = packageComponent.Platform.ToString();

					bool imageExists = false;
					if( !alreadyAddedImages.Contains( imageKey ) )
					{
						var image16 = Properties.Resources.ResourceManager.GetObject( imageKey + "_16", Properties.Resources.Culture ) as Image;
						var image32 = Properties.Resources.ResourceManager.GetObject( imageKey + "_32", Properties.Resources.Culture ) as Image;
						if( image16 != null )
						{
							contentBrowserPackage.ImageHelper.AddImage( imageKey, image16, image32 );

							alreadyAddedImages.Add( imageKey );
							imageExists = true;
						}
					}

					string packageName = packageComponent.ProductName.Value;
					if( string.IsNullOrEmpty( packageName ) )
						packageName = "\'No name\'";

					var text = string.Format( "{0} - {1} - {2}", packageName, packageComponent.Platform, virtualPath );
					var item = new ContentBrowserItem_Virtual( contentBrowserPackage, null, text );
					item.Tag = packageComponent;
					if( imageExists )
						item.imageKey = imageKey;
					items.Add( item );
				}
			}

			CollectionUtility.MergeSort( items, delegate ( ContentBrowser.Item item1, ContentBrowser.Item item2 )
			{
				var c1 = (Component_Product)item1.Tag;
				var c2 = (Component_Product)item2.Tag;

				var order1 = c1.SortOrderInEditor.Value;
				var order2 = c2.SortOrderInEditor.Value;

				if( order1 < order2 )
					return -1;
				if( order1 > order2 )
					return 1;

				return string.Compare( c1.Name, c2.Name );
			} );

			contentBrowserPackage.SetData( items, false );
			if( items.Count != 0 )
				contentBrowserPackage.SelectItems( new ContentBrowser.Item[] { items[ 0 ] } );
		}

		private void kryptonButtonPackageBrowse_Click( object sender, EventArgs e )
		{
#if !PROJECT_DEPLOY
			CommonOpenFileDialog dialog = new CommonOpenFileDialog();
			dialog.InitialDirectory = kryptonTextBoxPackageDestinationFolder.Text;
			dialog.IsFolderPicker = true;
			if( dialog.ShowDialog() == CommonFileDialogResult.Ok )
				kryptonTextBoxPackageDestinationFolder.Text = dialog.FileName;
#endif
		}

		void PackagingUpdate()
		{
			//check ended
			if( packageBuildInstance != null && packageBuildInstance.State != ProductBuildInstance.StateEnum.Building )
			{
				var instance2 = packageBuildInstance;
				packageBuildInstance = null;

				if( instance2.State == ProductBuildInstance.StateEnum.Error )
					EditorMessageBox.ShowWarning( instance2.Error );
			}

			//update controls

			var building = packageBuildInstance != null && packageBuildInstance.State == ProductBuildInstance.StateEnum.Building;

			kryptonButtonPackageCreate.Enabled = CanPackageProject();
			kryptonButtonPackageCreateAndRun.Enabled = kryptonButtonPackageCreate.Enabled && GetSelectedBuildConfiguration() != null && GetSelectedBuildConfiguration().SupportsBuildAndRun;
			kryptonTextBoxPackageDestinationFolder.Enabled = !building;
			kryptonButtonPackageBrowse.Enabled = !building;
			contentBrowserPackage.Enabled = !building;
			if( packageBuildInstance != null )
				progressBarBuild.Value = (int)( packageBuildInstance.Progress * 100 );
			progressBarBuild.Visible = building;
			kryptonButtonBuildCancel.Visible = building;
		}

		bool CanPackageProject()
		{
			if( GetSelectedBuildConfiguration() == null )
				return false;
			string folder = kryptonTextBoxPackageDestinationFolder.Text.Trim();
			if( string.IsNullOrEmpty( folder ) )
				return false;
			if( !Path.IsPathRooted( folder ) )
				return false;
			if( contentBrowserPackage.SelectedItems.Length == 0 )
				return false;
			if( packageBuildInstance != null )
				return false;

			return true;
		}

		Component_Product GetSelectedBuildConfiguration()
		{
			if( contentBrowserPackage.SelectedItems.Length != 0 )
				return (Component_Product)contentBrowserPackage.SelectedItems[ 0 ].Tag;
			return null;
		}

		void PackageCreate( bool run )
		{
			string folder = kryptonTextBoxPackageDestinationFolder.Text.Trim();
			if( string.IsNullOrEmpty( folder ) )
				return;

			//clear destination folder
			if( Directory.Exists( folder ) && !IOUtility.IsDirectoryEmpty( folder ) )
			{
				var text = string.Format( Translate( "Destination folder \'{0}\' is not empty. Clear folder and continue?" ), folder );
				if( EditorMessageBox.ShowQuestion( text, MessageBoxButtons.OKCancel ) == DialogResult.Cancel )
					return;

				//delete
				try
				{
					DirectoryInfo info = new DirectoryInfo( folder );
					foreach( FileInfo file in info.GetFiles() )
						file.Delete();
					foreach( DirectoryInfo directory in info.GetDirectories() )
						directory.Delete( true );
				}
				catch( Exception e )
				{
					EditorMessageBox.ShowWarning( e.Message );
					return;
				}
			}

			//start
			try
			{
				if( !Directory.Exists( folder ) )
					Directory.CreateDirectory( folder );

				packageBuildInstance = ProductBuildInstance.Start( GetSelectedBuildConfiguration(), folder, run );
			}
			catch( Exception e )
			{
				EditorMessageBox.ShowWarning( e.Message );
				return;
			}

			PackagingUpdate();
		}

		private void kryptonButtonPackageCreate_Click( object sender, EventArgs e )
		{
			PackageCreate( false );
		}

		private void kryptonButtonPackageCreateAndRun_Click( object sender, EventArgs e )
		{
			PackageCreate( true );
		}

		private void kryptonButtonPackageCancel_Click( object sender, EventArgs e )
		{
			if( packageBuildInstance != null )
				packageBuildInstance.RequestCancel = true;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		//void OpenInit()
		//{
		//	var items = new List<ContentBrowser.Item>();

		//	//ContentBrowserItem_Virtual item;

		//	//item = new ContentBrowserItem_Virtual( objectsBrowserOpen, null, "impl Recent Projects" );
		//	//var firstItem = item;
		//	//items.Add( item );
		//	objectsBrowserOpen.SetData( items, false );

		//	//objectsBrowserOpen.SelectItems( new ContentBrowser.Item[] { firstItem } );
		//}

		//void OpenUpdate()
		//{
		//	kryptonButtonOpen.Enabled = false;
		//}

		private void KryptonLinkLabel1_LinkClicked( object sender, EventArgs e )
		{
			Process.Start( "https://www.neoaxis.com/" );
		}

		string GetNeoAxisProjectsFolder()
		{
			return Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), "NeoAxis Projects" );
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams handleParam = base.CreateParams;
				handleParam.ExStyle |= 0x02000000;//WS_EX_COMPOSITED       
				return handleParam;
			}
		}

		public static bool BackstageVisible
		{
			get { return backstageVisible; }
		}

		/////////////////////////////////////////

		bool IsValidEmail( string email )
		{
			try
			{
				var addr = new System.Net.Mail.MailAddress( email );
				return addr.Address == email;
			}
			catch
			{
				return false;
			}
		}

		void LoginLoad()
		{
			kryptonLabelLoginError.Text = "";

			//if( LoginUtility.GetCurrentLicense( out var email, out _ ) )
			//	kryptonTextBoxLoginEmail.Text = email;

			//LoginUpdate();
		}

		bool loginFirstUpdate = true;

		void LoginUpdate()
		{
			//поятояннозапрашивается когда открыт бекстейдж

			if( loginFirstUpdate )
			{
				if( LoginUtility.GetCurrentLicense( out var email, out _ ) )
					kryptonTextBoxLoginEmail.Text = email;

				loginFirstUpdate = false;
			}

			{
				string text;
				string error = "";
				if( LoginUtility.GetCurrentLicense( out var email, out var hash ) )
				{
					text = email;
					if( LoginUtility.GetRequestedFullLicenseInfo( out var license, out var error2 ) )
					{
						text += " " + license;
						error = error2;
					}
				}
				else
					text = "Not registered.";

				kryptonLabelLicense.Text = text;
				kryptonLabelLoginError.Text = error;
			}

			{
				var email = kryptonTextBoxLoginEmail.Text.Trim().ToLower();
				var password = kryptonTextBoxLoginPassword.Text;
				kryptonButtonLogin.Enabled = !string.IsNullOrEmpty( email ) && !string.IsNullOrEmpty( password );
			}
		}

		private void kryptonButtonLogin_Click( object sender, EventArgs e )
		{
			var email = kryptonTextBoxLoginEmail.Text.Trim().ToLower();
			var password = kryptonTextBoxLoginPassword.Text;

			if( string.IsNullOrEmpty( email ) || string.IsNullOrEmpty( password ) )
				return;

			if( !IsValidEmail( email ) )
			{
				EditorMessageBox.ShowWarning( "Invalid email." );
				return;
			}

			LoginUtility.SetCurrentLicense( kryptonTextBoxLoginEmail.Text, kryptonTextBoxLoginPassword.Text );
		}

		private void kryptonButtonRegister_Click( object sender, EventArgs e )
		{
			Process.Start( "https://www.neoaxis.com/user" );
		}
	}
}