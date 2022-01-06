using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using Microsoft.Win32;

namespace AOMRecordedRenamer
{
	/// <summary>
	/// Summary description for AOMRecordedRenamer.
	/// </summary>
	public class AOMRecordedRenamer : System.Windows.Forms.Form
	{
    // // // // // // // // // // // // // // // // // // // // // // // // //
    private System.Windows.Forms.Label m_labelDirectory;
    private System.Windows.Forms.TextBox m_editDirectory;
    private System.Windows.Forms.Button m_buttonBrowse;
    private System.Windows.Forms.CheckBox m_checkWatchAuto;
    private System.Windows.Forms.Button m_buttonRename;
    private System.Windows.Forms.Button m_buttonClose;
    private System.Windows.Forms.GroupBox m_groupRename;
    private System.Windows.Forms.Button m_buttonAbout;
    private System.Windows.Forms.Label m_labelFilename;
    private System.Windows.Forms.TextBox m_editFilename;
    private System.Windows.Forms.Label m_labelEachPlayer;
    private System.Windows.Forms.Label m_labelPlayerSeparator;
    private System.Windows.Forms.Label m_labelTeamSeparator;
    private System.Windows.Forms.TextBox m_editPlayerSeparator;
    private System.Windows.Forms.TextBox m_editTeamSeparator;
    private System.Windows.Forms.TextBox m_editEachPlayer;
    private System.Windows.Forms.LinkLabel m_linkSoftware;
    private System.Windows.Forms.Label m_labelCopyright;
    private System.Windows.Forms.Label m_labelReplacements;
    private System.Windows.Forms.TextBox m_editStatus;
    private System.Windows.Forms.FolderBrowserDialog m_dlgOpenFolder;
    private System.Windows.Forms.Button m_buttonPreview;
    private System.Windows.Forms.CheckBox m_checkMove;
    private System.Windows.Forms.TextBox m_editMove;
    private System.Windows.Forms.Button m_buttonBrowseMove;
    private System.Windows.Forms.CheckBox m_checkTrimPlayer;
    private System.Windows.Forms.TextBox m_editTrimPlayer;
    private System.Windows.Forms.Label m_labelTrimCharacters;
    private System.IO.FileSystemWatcher m_fswDirWatch;

    // // // // // // // // // // // // // // // // // // // // // // // // //
    /// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

    // // // // // // // // // // // // // // // // // // // // // // // // //
    
    public AOMRecordedRenamer()
		{
			InitializeComponent();  // Required for Windows Form Designer support

      this.m_fswDirWatch.EnableRaisingEvents = false;  // start disabled
      //this.m_fswDirWatch.Changed += new FileSystemEventHandler(DirWatch_Changed);  // unneeded
      //this.m_fswDirWatch.Created += new FileSystemEventHandler(DirWatch_Changed);  // unneeded
      this.m_fswDirWatch.Renamed += new RenamedEventHandler(DirWatch_Renamed);

      this.m_labelReplacements.Text =
        "{YYYY} / {YY}, {MM} / {M}, {DD} / {D} = Year, Month, Day\n" +
        "{hh} / {h} / {ii} / {i}, {mm} / {m}, {ss} / {s}, {am} = Hour, Minute, Second, am/pm\n" +
        "{map} = Map name or scenario name\n" +
        "{shortmap} = Shortened map name or scenario name\n" +
        "{mode} = Game mode (ie: Supremacy, Deathmatch, Lightning, ...)\n" +
        "{shortmode} = Shortened game mode\n" +
        "{comp} = \"human\" if game is against all humans or \"computer\" if game has computer players\n" +
        "{numplayers} = Number of players in the game\n" +
        "{numteams} = Number of teams in the game\n" +
        "{vs} = Players per team matchups (ie: \"1v1\" or \"2v2v2v2\")\n" +
        "{myname} = Name of player who recorded the game\n" +
        "{myciv} = Chosen civilization of player who recorded the game\n" +
        "{shortmyciv} = Shortened chosen civilization of player who recorded the game\n" +
        "{myrate} / {myraterounded} = Rating of player who recorded the game at time of game start\n" +
        "{myteam} = Team number of player who recorded the game" +
        "{name} = (Per player only) Name of player\n" +
        "{civ} = (Per player only) Chosen civilization of player\n" +
        "{shortciv} = (Per player only) Shortened chosen civilization of player\n" +
        "{rate} / {raterounded} = (Per player only) Rating of player at time of game start\n" +
        "{team} = (Per player only) Team number of player";

      this.m_editDirectory.Text = Environment.GetFolderPath(Environment.SpecialFolder.Personal) +
                                  "\\My Games\\Age of Mythology\\Savegame\\Recorded Game *.rcx";
      this.m_checkMove.Checked = false;
      this.m_editMove.Text = Environment.GetFolderPath(Environment.SpecialFolder.Personal) +
                             "\\My Games\\Age of Mythology\\Savegame\\{YYYY}";

      try
      {
        Registry.CurrentUser.CreateSubKey("Software\\AOMRecordedRenamer");
        RegistryKey OurKey = Registry.CurrentUser.OpenSubKey("Software\\AOMRecordedRenamer");
        this.m_editDirectory.Text = OurKey.GetValue("Files", this.m_editDirectory.Text).ToString();
        this.m_editFilename.Text = OurKey.GetValue("RenameFileName", this.m_editFilename.Text).ToString();
        this.m_editEachPlayer.Text = OurKey.GetValue("RenameEachPlayer", this.m_editEachPlayer.Text).ToString();
        this.m_editTeamSeparator.Text = OurKey.GetValue("RenameTeamSeparator", this.m_editTeamSeparator.Text).ToString();
        this.m_editPlayerSeparator.Text = OurKey.GetValue("RenamePlayerSeparator", this.m_editPlayerSeparator.Text).ToString();
        this.m_checkMove.Checked = Convert.ToBoolean(OurKey.GetValue("Move", this.m_checkMove.Checked));
        this.m_editMove.Text = OurKey.GetValue("MoveDir", this.m_editMove.Text).ToString();
        this.m_checkTrimPlayer.Checked = Convert.ToBoolean(OurKey.GetValue("TrimPlayer", this.m_checkTrimPlayer.Checked));
        this.m_editTrimPlayer.Text = OurKey.GetValue("TrimPlayerChar", this.m_editTrimPlayer.Text).ToString();

        this.m_editStatus.Text += "Ready.\r\n";
        // Last because it can start watching/renaming right away.
        this.m_checkWatchAuto.Checked = Convert.ToBoolean(OurKey.GetValue("WatchDir", this.m_checkWatchAuto.Checked));
      }
      catch(Exception e)
      {
        this.m_editStatus.Text += "Error: Could not read settings (" + e + ").\r\n";
      }

      if(this.m_checkMove.Checked)
      {
        this.m_editMove.Enabled = true;
        this.m_buttonBrowseMove.Enabled = true;
      }
      else
      {
        this.m_editMove.Enabled = false;
        this.m_buttonBrowseMove.Enabled = false;
      }

      if(this.m_checkTrimPlayer.Checked)
      {
        this.m_editTrimPlayer.Enabled = true;
      }
      else
      {
        this.m_editTrimPlayer.Enabled = false;
      }
    }

    // // // // // // // // // // // // // // // // // // // // // // // // //

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

    // // // // // // // // // // // // // // // // // // // // // // // // //

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AOMRecordedRenamer));
      this.m_labelDirectory = new System.Windows.Forms.Label();
      this.m_editDirectory = new System.Windows.Forms.TextBox();
      this.m_buttonBrowse = new System.Windows.Forms.Button();
      this.m_checkWatchAuto = new System.Windows.Forms.CheckBox();
      this.m_buttonRename = new System.Windows.Forms.Button();
      this.m_buttonClose = new System.Windows.Forms.Button();
      this.m_groupRename = new System.Windows.Forms.GroupBox();
      this.m_labelTrimCharacters = new System.Windows.Forms.Label();
      this.m_editTrimPlayer = new System.Windows.Forms.TextBox();
      this.m_checkTrimPlayer = new System.Windows.Forms.CheckBox();
      this.m_labelReplacements = new System.Windows.Forms.Label();
      this.m_editEachPlayer = new System.Windows.Forms.TextBox();
      this.m_editTeamSeparator = new System.Windows.Forms.TextBox();
      this.m_editPlayerSeparator = new System.Windows.Forms.TextBox();
      this.m_labelTeamSeparator = new System.Windows.Forms.Label();
      this.m_labelPlayerSeparator = new System.Windows.Forms.Label();
      this.m_labelEachPlayer = new System.Windows.Forms.Label();
      this.m_editFilename = new System.Windows.Forms.TextBox();
      this.m_labelFilename = new System.Windows.Forms.Label();
      this.m_buttonAbout = new System.Windows.Forms.Button();
      this.m_labelCopyright = new System.Windows.Forms.Label();
      this.m_linkSoftware = new System.Windows.Forms.LinkLabel();
      this.m_editStatus = new System.Windows.Forms.TextBox();
      this.m_dlgOpenFolder = new System.Windows.Forms.FolderBrowserDialog();
      this.m_buttonPreview = new System.Windows.Forms.Button();
      this.m_checkMove = new System.Windows.Forms.CheckBox();
      this.m_editMove = new System.Windows.Forms.TextBox();
      this.m_buttonBrowseMove = new System.Windows.Forms.Button();
      this.m_fswDirWatch = new System.IO.FileSystemWatcher();
      this.m_groupRename.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.m_fswDirWatch)).BeginInit();
      this.SuspendLayout();
      // 
      // m_labelDirectory
      // 
      this.m_labelDirectory.Location = new System.Drawing.Point(8, 16);
      this.m_labelDirectory.Name = "m_labelDirectory";
      this.m_labelDirectory.Size = new System.Drawing.Size(104, 16);
      this.m_labelDirectory.TabIndex = 0;
      this.m_labelDirectory.Text = "Re&name files:";
      this.m_labelDirectory.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // m_editDirectory
      // 
      this.m_editDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.m_editDirectory.Location = new System.Drawing.Point(112, 14);
      this.m_editDirectory.Name = "m_editDirectory";
      this.m_editDirectory.Size = new System.Drawing.Size(442, 20);
      this.m_editDirectory.TabIndex = 1;
      this.m_editDirectory.Text = "";
      // 
      // m_buttonBrowse
      // 
      this.m_buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.m_buttonBrowse.Location = new System.Drawing.Point(562, 12);
      this.m_buttonBrowse.Name = "m_buttonBrowse";
      this.m_buttonBrowse.Size = new System.Drawing.Size(72, 24);
      this.m_buttonBrowse.TabIndex = 2;
      this.m_buttonBrowse.Text = "&Browse...";
      this.m_buttonBrowse.Click += new System.EventHandler(this.m_buttonBrowse_Click);
      // 
      // m_checkWatchAuto
      // 
      this.m_checkWatchAuto.Location = new System.Drawing.Point(112, 40);
      this.m_checkWatchAuto.Name = "m_checkWatchAuto";
      this.m_checkWatchAuto.Size = new System.Drawing.Size(424, 16);
      this.m_checkWatchAuto.TabIndex = 3;
      this.m_checkWatchAuto.Text = "&Watch this directory and rename/move files automatically";
      this.m_checkWatchAuto.CheckedChanged += new System.EventHandler(this.m_checkWatchAuto_CheckedChanged);
      // 
      // m_buttonRename
      // 
      this.m_buttonRename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.m_buttonRename.Location = new System.Drawing.Point(450, 616);
      this.m_buttonRename.Name = "m_buttonRename";
      this.m_buttonRename.Size = new System.Drawing.Size(104, 24);
      this.m_buttonRename.TabIndex = 13;
      this.m_buttonRename.Text = "&Rename Now";
      this.m_buttonRename.Click += new System.EventHandler(this.m_buttonRename_Click);
      // 
      // m_buttonClose
      // 
      this.m_buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.m_buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.m_buttonClose.Location = new System.Drawing.Point(562, 616);
      this.m_buttonClose.Name = "m_buttonClose";
      this.m_buttonClose.Size = new System.Drawing.Size(72, 24);
      this.m_buttonClose.TabIndex = 14;
      this.m_buttonClose.Text = "&Close";
      this.m_buttonClose.Click += new System.EventHandler(this.m_buttonClose_Click);
      // 
      // m_groupRename
      // 
      this.m_groupRename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.m_groupRename.Controls.Add(this.m_labelTrimCharacters);
      this.m_groupRename.Controls.Add(this.m_editTrimPlayer);
      this.m_groupRename.Controls.Add(this.m_checkTrimPlayer);
      this.m_groupRename.Controls.Add(this.m_labelReplacements);
      this.m_groupRename.Controls.Add(this.m_editEachPlayer);
      this.m_groupRename.Controls.Add(this.m_editTeamSeparator);
      this.m_groupRename.Controls.Add(this.m_editPlayerSeparator);
      this.m_groupRename.Controls.Add(this.m_labelTeamSeparator);
      this.m_groupRename.Controls.Add(this.m_labelPlayerSeparator);
      this.m_groupRename.Controls.Add(this.m_labelEachPlayer);
      this.m_groupRename.Controls.Add(this.m_editFilename);
      this.m_groupRename.Controls.Add(this.m_labelFilename);
      this.m_groupRename.Location = new System.Drawing.Point(8, 88);
      this.m_groupRename.Name = "m_groupRename";
      this.m_groupRename.Size = new System.Drawing.Size(626, 400);
      this.m_groupRename.TabIndex = 7;
      this.m_groupRename.TabStop = false;
      this.m_groupRename.Text = "Rename game(s) as:";
      // 
      // m_labelTrimCharacters
      // 
      this.m_labelTrimCharacters.Location = new System.Drawing.Point(288, 114);
      this.m_labelTrimCharacters.Name = "m_labelTrimCharacters";
      this.m_labelTrimCharacters.Size = new System.Drawing.Size(72, 16);
      this.m_labelTrimCharacters.TabIndex = 9;
      this.m_labelTrimCharacters.Text = "c&haracters";
      // 
      // m_editTrimPlayer
      // 
      this.m_editTrimPlayer.Location = new System.Drawing.Point(240, 112);
      this.m_editTrimPlayer.Name = "m_editTrimPlayer";
      this.m_editTrimPlayer.Size = new System.Drawing.Size(40, 20);
      this.m_editTrimPlayer.TabIndex = 10;
      this.m_editTrimPlayer.Text = "8";
      // 
      // m_checkTrimPlayer
      // 
      this.m_checkTrimPlayer.Location = new System.Drawing.Point(104, 114);
      this.m_checkTrimPlayer.Name = "m_checkTrimPlayer";
      this.m_checkTrimPlayer.Size = new System.Drawing.Size(136, 16);
      this.m_checkTrimPlayer.TabIndex = 8;
      this.m_checkTrimPlayer.Text = "Tri&m player names to";
      this.m_checkTrimPlayer.CheckedChanged += new System.EventHandler(this.m_checkTrimPlayer_CheckedChanged);
      // 
      // m_labelReplacements
      // 
      this.m_labelReplacements.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.m_labelReplacements.Location = new System.Drawing.Point(104, 144);
      this.m_labelReplacements.Name = "m_labelReplacements";
      this.m_labelReplacements.Size = new System.Drawing.Size(514, 248);
      this.m_labelReplacements.TabIndex = 11;
      this.m_labelReplacements.Text = "<<insert description here>>";
      // 
      // m_editEachPlayer
      // 
      this.m_editEachPlayer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.m_editEachPlayer.Location = new System.Drawing.Point(104, 40);
      this.m_editEachPlayer.Name = "m_editEachPlayer";
      this.m_editEachPlayer.Size = new System.Drawing.Size(514, 20);
      this.m_editEachPlayer.TabIndex = 3;
      this.m_editEachPlayer.Text = "{name} ({civ},{raterounded})";
      // 
      // m_editTeamSeparator
      // 
      this.m_editTeamSeparator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.m_editTeamSeparator.Location = new System.Drawing.Point(104, 64);
      this.m_editTeamSeparator.Name = "m_editTeamSeparator";
      this.m_editTeamSeparator.Size = new System.Drawing.Size(514, 20);
      this.m_editTeamSeparator.TabIndex = 5;
      this.m_editTeamSeparator.Text = " vs ";
      // 
      // m_editPlayerSeparator
      // 
      this.m_editPlayerSeparator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.m_editPlayerSeparator.Location = new System.Drawing.Point(104, 88);
      this.m_editPlayerSeparator.Name = "m_editPlayerSeparator";
      this.m_editPlayerSeparator.Size = new System.Drawing.Size(514, 20);
      this.m_editPlayerSeparator.TabIndex = 7;
      this.m_editPlayerSeparator.Text = ", ";
      // 
      // m_labelTeamSeparator
      // 
      this.m_labelTeamSeparator.Location = new System.Drawing.Point(8, 64);
      this.m_labelTeamSeparator.Name = "m_labelTeamSeparator";
      this.m_labelTeamSeparator.Size = new System.Drawing.Size(96, 16);
      this.m_labelTeamSeparator.TabIndex = 4;
      this.m_labelTeamSeparator.Text = "&Team separator:";
      this.m_labelTeamSeparator.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // m_labelPlayerSeparator
      // 
      this.m_labelPlayerSeparator.Location = new System.Drawing.Point(8, 88);
      this.m_labelPlayerSeparator.Name = "m_labelPlayerSeparator";
      this.m_labelPlayerSeparator.Size = new System.Drawing.Size(96, 16);
      this.m_labelPlayerSeparator.TabIndex = 6;
      this.m_labelPlayerSeparator.Text = "Player &separator:";
      this.m_labelPlayerSeparator.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // m_labelEachPlayer
      // 
      this.m_labelEachPlayer.Location = new System.Drawing.Point(8, 40);
      this.m_labelEachPlayer.Name = "m_labelEachPlayer";
      this.m_labelEachPlayer.Size = new System.Drawing.Size(96, 16);
      this.m_labelEachPlayer.TabIndex = 2;
      this.m_labelEachPlayer.Text = "&Each player:";
      this.m_labelEachPlayer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // m_editFilename
      // 
      this.m_editFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.m_editFilename.Location = new System.Drawing.Point(104, 16);
      this.m_editFilename.Name = "m_editFilename";
      this.m_editFilename.Size = new System.Drawing.Size(514, 20);
      this.m_editFilename.TabIndex = 1;
      this.m_editFilename.Text = "{YYYY}.{MM}.{DD} {hh}.{mm}{am} - {vs} {map} - {teams}.rcx";
      // 
      // m_labelFilename
      // 
      this.m_labelFilename.Location = new System.Drawing.Point(8, 16);
      this.m_labelFilename.Name = "m_labelFilename";
      this.m_labelFilename.Size = new System.Drawing.Size(96, 16);
      this.m_labelFilename.TabIndex = 0;
      this.m_labelFilename.Text = "&File:";
      this.m_labelFilename.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // m_buttonAbout
      // 
      this.m_buttonAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.m_buttonAbout.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.m_buttonAbout.Location = new System.Drawing.Point(8, 616);
      this.m_buttonAbout.Name = "m_buttonAbout";
      this.m_buttonAbout.Size = new System.Drawing.Size(72, 24);
      this.m_buttonAbout.TabIndex = 9;
      this.m_buttonAbout.Text = "&About";
      this.m_buttonAbout.Click += new System.EventHandler(this.m_buttonAbout_Click);
      // 
      // m_labelCopyright
      // 
      this.m_labelCopyright.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.m_labelCopyright.Location = new System.Drawing.Point(88, 608);
      this.m_labelCopyright.Name = "m_labelCopyright";
      this.m_labelCopyright.Size = new System.Drawing.Size(242, 16);
      this.m_labelCopyright.TabIndex = 10;
      this.m_labelCopyright.Text = "Copyright 2004 by Peter Vasiliauskas";
      this.m_labelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // m_linkSoftware
      // 
      this.m_linkSoftware.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.m_linkSoftware.Location = new System.Drawing.Point(88, 624);
      this.m_linkSoftware.Name = "m_linkSoftware";
      this.m_linkSoftware.Size = new System.Drawing.Size(242, 16);
      this.m_linkSoftware.TabIndex = 11;
      this.m_linkSoftware.TabStop = true;
      this.m_linkSoftware.Text = "http://software.magneticpole.com/rcxren/";
      this.m_linkSoftware.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.m_linkSoftware.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_linkSoftware_LinkClicked);
      // 
      // m_editStatus
      // 
      this.m_editStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.m_editStatus.Location = new System.Drawing.Point(8, 496);
      this.m_editStatus.Multiline = true;
      this.m_editStatus.Name = "m_editStatus";
      this.m_editStatus.ReadOnly = true;
      this.m_editStatus.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.m_editStatus.Size = new System.Drawing.Size(626, 104);
      this.m_editStatus.TabIndex = 8;
      this.m_editStatus.Text = "";
      // 
      // m_dlgOpenFolder
      // 
      this.m_dlgOpenFolder.ShowNewFolderButton = false;
      // 
      // m_buttonPreview
      // 
      this.m_buttonPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.m_buttonPreview.Location = new System.Drawing.Point(338, 616);
      this.m_buttonPreview.Name = "m_buttonPreview";
      this.m_buttonPreview.Size = new System.Drawing.Size(104, 24);
      this.m_buttonPreview.TabIndex = 12;
      this.m_buttonPreview.Text = "&Preview Now";
      this.m_buttonPreview.Click += new System.EventHandler(this.m_buttonPreview_Click);
      // 
      // m_checkMove
      // 
      this.m_checkMove.Location = new System.Drawing.Point(8, 66);
      this.m_checkMove.Name = "m_checkMove";
      this.m_checkMove.Size = new System.Drawing.Size(104, 16);
      this.m_checkMove.TabIndex = 4;
      this.m_checkMove.Text = "&Move files to:";
      this.m_checkMove.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.m_checkMove.CheckedChanged += new System.EventHandler(this.m_checkMove_CheckedChanged);
      // 
      // m_editMove
      // 
      this.m_editMove.Location = new System.Drawing.Point(112, 64);
      this.m_editMove.Name = "m_editMove";
      this.m_editMove.Size = new System.Drawing.Size(424, 20);
      this.m_editMove.TabIndex = 5;
      this.m_editMove.Text = "";
      // 
      // m_buttonBrowseMove
      // 
      this.m_buttonBrowseMove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.m_buttonBrowseMove.Location = new System.Drawing.Point(562, 62);
      this.m_buttonBrowseMove.Name = "m_buttonBrowseMove";
      this.m_buttonBrowseMove.Size = new System.Drawing.Size(72, 24);
      this.m_buttonBrowseMove.TabIndex = 6;
      this.m_buttonBrowseMove.Text = "Br&owse...";
      this.m_buttonBrowseMove.Click += new System.EventHandler(this.m_buttonBrowseMove_Click);
      // 
      // m_fswDirWatch
      // 
      this.m_fswDirWatch.EnableRaisingEvents = true;
      this.m_fswDirWatch.SynchronizingObject = this;
      // 
      // AOMRecordedRenamer
      // 
      this.AcceptButton = this.m_buttonRename;
      this.AllowDrop = true;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.CancelButton = this.m_buttonClose;
      this.ClientSize = new System.Drawing.Size(642, 645);
      this.Controls.Add(this.m_buttonBrowseMove);
      this.Controls.Add(this.m_editMove);
      this.Controls.Add(this.m_editStatus);
      this.Controls.Add(this.m_editDirectory);
      this.Controls.Add(this.m_checkMove);
      this.Controls.Add(this.m_buttonPreview);
      this.Controls.Add(this.m_linkSoftware);
      this.Controls.Add(this.m_labelCopyright);
      this.Controls.Add(this.m_buttonAbout);
      this.Controls.Add(this.m_groupRename);
      this.Controls.Add(this.m_buttonClose);
      this.Controls.Add(this.m_buttonRename);
      this.Controls.Add(this.m_checkWatchAuto);
      this.Controls.Add(this.m_buttonBrowse);
      this.Controls.Add(this.m_labelDirectory);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "AOMRecordedRenamer";
      this.Text = "AOM Recorded Renamer v1.3";
      this.m_groupRename.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.m_fswDirWatch)).EndInit();
      this.ResumeLayout(false);

    }
		#endregion

    // // // // // // // // // // // // // // // // // // // // // // // // //

		[STAThread]
    static void Main() 
    {
	    Application.Run(new AOMRecordedRenamer());
    }

    // // // // // // // // // // // // // // // // // // // // // // // // //

    private void m_buttonBrowse_Click(object sender, System.EventArgs e)
    {
      if(this.m_editDirectory.Text.Length > 0  &&  this.m_editDirectory.Text.LastIndexOf('\\') > 0)
        this.m_dlgOpenFolder.SelectedPath = this.m_editDirectory.Text.Substring(0, this.m_editDirectory.Text.LastIndexOf('\\'));

      if(this.m_dlgOpenFolder.ShowDialog(this) == DialogResult.OK)
        this.m_editDirectory.Text = this.m_dlgOpenFolder.SelectedPath + "\\Recorded Game *.rcx";
    }

    // // // // // // // // // // // // // // // // // // // // // // // // //

    private void m_buttonBrowseMove_Click(object sender, System.EventArgs e)
    {
      if(this.m_editMove.Text.Length > 0  &&  this.m_editMove.Text.LastIndexOf('\\') > 0)
        this.m_dlgOpenFolder.SelectedPath = this.m_editMove.Text.Substring(0, this.m_editMove.Text.LastIndexOf('\\'));

      if(this.m_dlgOpenFolder.ShowDialog(this) == DialogResult.OK)
        this.m_editMove.Text = this.m_dlgOpenFolder.SelectedPath;
    }

    // // // // // // // // // // // // // // // // // // // // // // // // //

    private void m_buttonAbout_Click(object sender, System.EventArgs e)
    {
      MessageBox.Show(this,
                      "AOM Recorded Renamer v1.3 - An Age of Mythology : Titans RCX Recorded game renamer.\n" +
                      "Copyright (c) 2004 by Peter Vasiliauskas.  All rights reserved.\n\n" +
                      "Utilizing SharpZipLib under the LGPL license for gz decompression.\n\n" +
                      "Visit http://software.magneticpole.com/rcxren/ for updated versions and documentation.\n" +
                      "Contact me at \"PeteVasi\" on ESO, or visit http://www.magneticpole.com/formmail.php",
                      "About...");
    }

    // // // // // // // // // // // // // // // // // // // // // // // // //

    private void m_checkWatchAuto_CheckedChanged(object sender, System.EventArgs e)
    {
      if(this.m_checkWatchAuto.Checked)
      {
        this.m_editDirectory.Enabled = false;
        this.m_buttonBrowse.Enabled = false;
        this.m_checkMove.Enabled = false;
        this.m_editMove.Enabled = false;
        this.m_buttonBrowseMove.Enabled = false;
        this.m_buttonPreview.Enabled = false;
        this.m_buttonRename.Enabled = false;
        this.m_fswDirWatch.Path = this.m_editDirectory.Text.Substring(0, this.m_editDirectory.Text.LastIndexOf('\\'));
        this.m_fswDirWatch.Filter = this.m_editDirectory.Text.Substring(this.m_editDirectory.Text.LastIndexOf('\\')+1);
        this.m_fswDirWatch.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
        this.m_fswDirWatch.EnableRaisingEvents = true;  // start the watching
        this.m_editStatus.Text += "Started watching directory \"" + this.m_fswDirWatch.Path + "\".\r\n";
        DoRename(false); // Do a rename in case there's files hanging out there
      }
      else
      {
        this.m_fswDirWatch.EnableRaisingEvents = false;  // stop watching
        this.m_editDirectory.Enabled = true;
        this.m_buttonBrowse.Enabled = true;
        this.m_checkMove.Enabled = true;
        if(this.m_checkMove.Checked)
        {
          this.m_editMove.Enabled = true;
          this.m_buttonBrowseMove.Enabled = true;
        }
        this.m_buttonPreview.Enabled = true;
        this.m_buttonRename.Enabled = true;
        this.m_editStatus.Text += "Stopped watching directory \"" + this.m_fswDirWatch.Path + "\".\r\n";
      }

      // Scroll to end of messages
      this.m_editStatus.SelectionLength = 0;
      this.m_editStatus.SelectionStart = this.m_editStatus.Text.Length;  
      this.m_editStatus.ScrollToCaret();
    }

    // // // // // // // // // // // // // // // // // // // // // // // // //

    private void DirWatch_Changed(object sender, FileSystemEventArgs e)
    {
      //WatchTestNewFilesInDir();  // unneeded...
    }

    // // // // // // // // // // // // // // // // // // // // // // // // //

    private void DirWatch_Renamed(object sender, RenamedEventArgs e)
    {
      WatchTestNewFilesInDir();
    }

    // // // // // // // // // // // // // // // // // // // // // // // // //

    private void WatchTestNewFilesInDir()
    {
      System.Threading.Thread.Sleep(1500); // Wait a sec for files to close
      bool bCanRenameSomething = false;
      String sPath = this.m_editDirectory.Text.Substring(0, this.m_editDirectory.Text.LastIndexOf('\\'));
      String sFiles = this.m_editDirectory.Text.Substring(this.m_editDirectory.Text.LastIndexOf('\\')+1);
      FileInfo[] listFiles = (new DirectoryInfo(sPath)).GetFiles(sFiles);
      foreach(FileInfo oFile in listFiles)
      {
        try
        {
          FileStream fsFile = File.Open(oFile.FullName, FileMode.Open, FileAccess.Read, FileShare.None);
          if(fsFile != null)
          {
            bCanRenameSomething = true;
            fsFile.Close();
          }
        }
        catch
        {
        }
      }

      if(bCanRenameSomething)
      {
        DoRename(false);
      }
    }

    // // // // // // // // // // // // // // // // // // // // // // // // //

    private void m_buttonPreview_Click(object sender, System.EventArgs e)
    {
      DoRename(true);
    }

    // // // // // // // // // // // // // // // // // // // // // // // // //

    private void m_buttonRename_Click(object sender, System.EventArgs evargs)
    {
      DoRename(false);
    }

    // // // // // // // // // // // // // // // // // // // // // // // // //

    private void DoRename(bool bIsPreview)
    {
      this.m_fswDirWatch.EnableRaisingEvents = false;  // disable while we run through stuff

      AOMRCXParser oRCX = new AOMRCXParser();
      String sPath = this.m_editDirectory.Text.Substring(0, this.m_editDirectory.Text.LastIndexOf('\\'));
      String sNewPath = this.m_editMove.Text;
      String sFiles = this.m_editDirectory.Text.Substring(this.m_editDirectory.Text.LastIndexOf('\\')+1);
      if(!this.m_checkMove.Checked)
        sNewPath = sPath;

      if(sNewPath.EndsWith("\\"))
        sNewPath = sNewPath.Substring(0, sNewPath.Length-1);

      try
      {
        if(Convert.ToInt32(this.m_editTrimPlayer.Text) < 1  ||  Convert.ToInt32(this.m_editTrimPlayer.Text) > 32767)
          this.m_editTrimPlayer.Text = "8";
      }
      catch
      {
        this.m_editTrimPlayer.Text = "8";
      }
      
      FileInfo[] listFiles = (new DirectoryInfo(sPath)).GetFiles(sFiles);
      if(listFiles.Length == 0  &&  !this.m_checkWatchAuto.Checked)
        this.m_editStatus.Text += "No files found to rename!\r\n";
      foreach(FileInfo oFile in listFiles)
      {
        bool bParseSuccess = false;
        try
        {
          bParseSuccess = oRCX.Parse(oFile.FullName, oFile.Length, oFile.CreationTime);
        }
        catch(Exception e)
        {
          this.m_editStatus.Text += "Error: Could not parse file (" + oFile.Name + ") (" + e + ").\r\n";
          bParseSuccess = false;
        }

        if(bParseSuccess)
        {
//this.m_editStatus.Text += oRCX.m_sXML + "\r\n";
//this.m_editStatus.Text += oRCX.m_timeGameStartTime.ToShortDateString() + " " + oRCX.m_timeGameStartTime.ToShortTimeString() + "\r\n";
          int i, j;
          bool bFirstTeam, bFirstPlayer, bFoundTeam;
          String sNewName = this.m_editFilename.Text;
          if(m_checkTrimPlayer.Checked  &&  Convert.ToInt32(this.m_editTrimPlayer.Text) < oRCX.m_aPlayerNames[oRCX.m_nCurrentPlayer].Length)
            sNewName = sNewName.Replace("{myname}", oRCX.m_aPlayerNames[oRCX.m_nCurrentPlayer].Substring(0, Convert.ToInt32(this.m_editTrimPlayer.Text)));
          else
            sNewName = sNewName.Replace("{myname}", oRCX.m_aPlayerNames[oRCX.m_nCurrentPlayer]);
          sNewName = sNewName.Replace("{myciv}", oRCX.m_aPlayerCivs[oRCX.m_nCurrentPlayer]);
          sNewName = sNewName.Replace("{shortmyciv}", oRCX.m_aPlayerShortCivs[oRCX.m_nCurrentPlayer]);
          sNewName = sNewName.Replace("{myrate}", Convert.ToString(oRCX.m_aPlayerRatings[oRCX.m_nCurrentPlayer]));
          sNewName = sNewName.Replace("{myraterounded}", Convert.ToString(Math.Round(oRCX.m_aPlayerRatings[oRCX.m_nCurrentPlayer])));
          sNewName = sNewName.Replace("{myteam}", Convert.ToString(oRCX.m_aPlayerTeams[oRCX.m_nCurrentPlayer]));
          sNewName = sNewName.Replace("{YYYY}", String.Format("{0:0000}", oRCX.m_timeGameStartTime.Year));
          sNewName = sNewName.Replace("{YY}", String.Format("{0:00}", (oRCX.m_timeGameStartTime.Year % 100)));
          sNewName = sNewName.Replace("{MM}", String.Format("{0:00}", oRCX.m_timeGameStartTime.Month));
          sNewName = sNewName.Replace("{M}", String.Format("{0}", oRCX.m_timeGameStartTime.Month));
          sNewName = sNewName.Replace("{DD}", String.Format("{0:00}", oRCX.m_timeGameStartTime.Day));
          sNewName = sNewName.Replace("{D}", String.Format("{0}", oRCX.m_timeGameStartTime.Day));
          sNewName = sNewName.Replace("{hh}", String.Format("{0:00}", (oRCX.m_timeGameStartTime.Hour == 0 ? 12 : (oRCX.m_timeGameStartTime.Hour % 12))));
          sNewName = sNewName.Replace("{h}", String.Format("{0}", (oRCX.m_timeGameStartTime.Hour == 0 ? 12 : (oRCX.m_timeGameStartTime.Hour % 12))));
          sNewName = sNewName.Replace("{ii}", String.Format("{0:00}", oRCX.m_timeGameStartTime.Hour));
          sNewName = sNewName.Replace("{i}", String.Format("{0}", oRCX.m_timeGameStartTime.Hour));
          sNewName = sNewName.Replace("{mm}", String.Format("{0:00}", oRCX.m_timeGameStartTime.Minute));
          sNewName = sNewName.Replace("{m}", String.Format("{0}", oRCX.m_timeGameStartTime.Minute));
          sNewName = sNewName.Replace("{ss}", String.Format("{0:00}", oRCX.m_timeGameStartTime.Second));
          sNewName = sNewName.Replace("{s}", String.Format("{0}", oRCX.m_timeGameStartTime.Second));
          sNewName = sNewName.Replace("{am}", oRCX.m_timeGameStartTime.Hour < 12 ? "am" : "pm");
          sNewName = sNewName.Replace("{map}", oRCX.m_sMap);
          sNewName = sNewName.Replace("{shortmap}", oRCX.m_sMap.Substring(0, 3));
          sNewName = sNewName.Replace("{mode}", oRCX.m_sMode);
          sNewName = sNewName.Replace("{shortmode}", oRCX.m_sShortMode);
          sNewName = sNewName.Replace("{comp}", oRCX.m_bAgainstComputer ? "computer" : "human");
          sNewName = sNewName.Replace("{numplayers}", Convert.ToString(oRCX.m_nPlayers));
          sNewName = sNewName.Replace("{numteams}", Convert.ToString(oRCX.m_nTeams));
          sNewName = sNewName.Replace("{vs}", oRCX.m_sTeamVsText);
          if(this.m_checkMove.Checked)
          {
            if(m_checkTrimPlayer.Checked  &&  Convert.ToInt32(this.m_editTrimPlayer.Text) < oRCX.m_aPlayerNames[oRCX.m_nCurrentPlayer].Length)
              sNewPath = sNewPath.Replace("{myname}", oRCX.m_aPlayerNames[oRCX.m_nCurrentPlayer].Substring(0, Convert.ToInt32(this.m_editTrimPlayer.Text)));
            else
              sNewPath = sNewPath.Replace("{myname}", oRCX.m_aPlayerNames[oRCX.m_nCurrentPlayer]);                
            sNewPath = sNewPath.Replace("{myciv}", oRCX.m_aPlayerCivs[oRCX.m_nCurrentPlayer]);
            sNewPath = sNewPath.Replace("{shortmyciv}", oRCX.m_aPlayerShortCivs[oRCX.m_nCurrentPlayer]);
            sNewPath = sNewPath.Replace("{myrate}", Convert.ToString(oRCX.m_aPlayerRatings[oRCX.m_nCurrentPlayer]));
            sNewPath = sNewPath.Replace("{myraterounded}", Convert.ToString(Math.Round(oRCX.m_aPlayerRatings[oRCX.m_nCurrentPlayer])));
            sNewPath = sNewPath.Replace("{myteam}", Convert.ToString(oRCX.m_aPlayerTeams[oRCX.m_nCurrentPlayer]));
            sNewPath = sNewPath.Replace("{YYYY}", String.Format("{0:0000}", oRCX.m_timeGameStartTime.Year));
            sNewPath = sNewPath.Replace("{YY}", String.Format("{0:00}", (oRCX.m_timeGameStartTime.Year % 100)));
            sNewPath = sNewPath.Replace("{MM}", String.Format("{0:00}", oRCX.m_timeGameStartTime.Month));
            sNewPath = sNewPath.Replace("{M}", String.Format("{0}", oRCX.m_timeGameStartTime.Month));
            sNewPath = sNewPath.Replace("{DD}", String.Format("{0:00}", oRCX.m_timeGameStartTime.Day));
            sNewPath = sNewPath.Replace("{D}", String.Format("{0}", oRCX.m_timeGameStartTime.Day));
            sNewPath = sNewPath.Replace("{hh}", String.Format("{0:00}", (oRCX.m_timeGameStartTime.Hour == 0 ? 12 : (oRCX.m_timeGameStartTime.Hour % 12))));
            sNewPath = sNewPath.Replace("{h}", String.Format("{0}", (oRCX.m_timeGameStartTime.Hour == 0 ? 12 : (oRCX.m_timeGameStartTime.Hour % 12))));
            sNewPath = sNewPath.Replace("{ii}", String.Format("{0:00}", oRCX.m_timeGameStartTime.Hour));
            sNewPath = sNewPath.Replace("{i}", String.Format("{0}", oRCX.m_timeGameStartTime.Hour));
            sNewPath = sNewPath.Replace("{mm}", String.Format("{0:00}", oRCX.m_timeGameStartTime.Minute));
            sNewPath = sNewPath.Replace("{m}", String.Format("{0}", oRCX.m_timeGameStartTime.Minute));
            sNewPath = sNewPath.Replace("{ss}", String.Format("{0:00}", oRCX.m_timeGameStartTime.Second));
            sNewPath = sNewPath.Replace("{s}", String.Format("{0}", oRCX.m_timeGameStartTime.Second));
            sNewPath = sNewPath.Replace("{am}", oRCX.m_timeGameStartTime.Hour < 12 ? "am" : "pm");
            sNewPath = sNewPath.Replace("{map}", oRCX.m_sMap);
            sNewPath = sNewPath.Replace("{shortmap}", oRCX.m_sMap.Substring(0, 3));
            sNewPath = sNewPath.Replace("{mode}", oRCX.m_sMode);
            sNewPath = sNewPath.Replace("{shortmode}", oRCX.m_sShortMode);
            sNewPath = sNewPath.Replace("{comp}", oRCX.m_bAgainstComputer ? "computer" : "human");
            sNewPath = sNewPath.Replace("{numplayers}", Convert.ToString(oRCX.m_nPlayers));
            sNewPath = sNewPath.Replace("{numteams}", Convert.ToString(oRCX.m_nTeams));
            sNewPath = sNewPath.Replace("{vs}", oRCX.m_sTeamVsText);
          }

          String sTeams = "";
          bFirstTeam = true;
          bFoundTeam = false;
          for(i=0; i<12; i++)
          {
            bFirstPlayer = true;
            for(j=0; j<12; j++)
            {
              if(oRCX.m_aPlayerTeams[j] == i+1)
              {
                String s1Player = this.m_editEachPlayer.Text;
                if(m_checkTrimPlayer.Checked  &&  Convert.ToInt32(this.m_editTrimPlayer.Text) < oRCX.m_aPlayerNames[j].Length)
                  s1Player = s1Player.Replace("{name}", oRCX.m_aPlayerNames[j].Substring(0, Convert.ToInt32(this.m_editTrimPlayer.Text)));
                else
                  s1Player = s1Player.Replace("{name}", oRCX.m_aPlayerNames[j]);
                s1Player = s1Player.Replace("{civ}", oRCX.m_aPlayerCivs[j]);
                s1Player = s1Player.Replace("{shortciv}", oRCX.m_aPlayerShortCivs[j]);
                s1Player = s1Player.Replace("{rate}", Convert.ToString(oRCX.m_aPlayerRatings[j]));
                s1Player = s1Player.Replace("{raterounded}", Convert.ToString(Math.Round(oRCX.m_aPlayerRatings[j])));
                s1Player = s1Player.Replace("{team}", Convert.ToString(oRCX.m_aPlayerTeams[j]));
                if(m_checkTrimPlayer.Checked  &&  Convert.ToInt32(this.m_editTrimPlayer.Text) < oRCX.m_aPlayerNames[oRCX.m_nCurrentPlayer].Length)
                  s1Player = s1Player.Replace("{myname}", oRCX.m_aPlayerNames[oRCX.m_nCurrentPlayer].Substring(0, Convert.ToInt32(this.m_editTrimPlayer.Text)));
                else
                  s1Player = s1Player.Replace("{myname}", oRCX.m_aPlayerNames[oRCX.m_nCurrentPlayer]);                
                s1Player = s1Player.Replace("{myciv}", oRCX.m_aPlayerCivs[oRCX.m_nCurrentPlayer]);
                s1Player = s1Player.Replace("{shortmyciv}", oRCX.m_aPlayerShortCivs[oRCX.m_nCurrentPlayer]);
                s1Player = s1Player.Replace("{myrate}", Convert.ToString(oRCX.m_aPlayerRatings[oRCX.m_nCurrentPlayer]));
                s1Player = s1Player.Replace("{myraterounded}", Convert.ToString(Math.Round(oRCX.m_aPlayerRatings[oRCX.m_nCurrentPlayer])));
                s1Player = s1Player.Replace("{myteam}", Convert.ToString(oRCX.m_aPlayerTeams[oRCX.m_nCurrentPlayer]));
                s1Player = s1Player.Replace("{YYYY}", String.Format("{0:0000}", oRCX.m_timeGameStartTime.Year));
                s1Player = s1Player.Replace("{YY}", String.Format("{0:00}", (oRCX.m_timeGameStartTime.Year % 100)));
                s1Player = s1Player.Replace("{MM}", String.Format("{0:00}", oRCX.m_timeGameStartTime.Month));
                s1Player = s1Player.Replace("{M}", String.Format("{0}", oRCX.m_timeGameStartTime.Month));
                s1Player = s1Player.Replace("{DD}", String.Format("{0:00}", oRCX.m_timeGameStartTime.Day));
                s1Player = s1Player.Replace("{D}", String.Format("{0}", oRCX.m_timeGameStartTime.Day));
                s1Player = s1Player.Replace("{hh}", String.Format("{0:00}", (oRCX.m_timeGameStartTime.Hour == 0 ? 12 : (oRCX.m_timeGameStartTime.Hour % 12))));
                s1Player = s1Player.Replace("{h}", String.Format("{0}", (oRCX.m_timeGameStartTime.Hour == 0 ? 12 : (oRCX.m_timeGameStartTime.Hour % 12))));
                s1Player = s1Player.Replace("{ii}", String.Format("{0:00}", oRCX.m_timeGameStartTime.Hour));
                s1Player = s1Player.Replace("{i}", String.Format("{0}", oRCX.m_timeGameStartTime.Hour));
                s1Player = s1Player.Replace("{mm}", String.Format("{0:00}", oRCX.m_timeGameStartTime.Minute));
                s1Player = s1Player.Replace("{m}", String.Format("{0}", oRCX.m_timeGameStartTime.Minute));
                s1Player = s1Player.Replace("{ss}", String.Format("{0:00}", oRCX.m_timeGameStartTime.Second));
                s1Player = s1Player.Replace("{s}", String.Format("{0}", oRCX.m_timeGameStartTime.Second));
                s1Player = s1Player.Replace("{am}", oRCX.m_timeGameStartTime.Hour < 12 ? "am" : "pm");
                s1Player = s1Player.Replace("{map}", oRCX.m_sMap);
                s1Player = s1Player.Replace("{shortmap}", oRCX.m_sMap.Substring(0, 3));
                s1Player = s1Player.Replace("{mode}", oRCX.m_sMode);
                s1Player = s1Player.Replace("{shortmode}", oRCX.m_sShortMode);
                s1Player = s1Player.Replace("{comp}", oRCX.m_bAgainstComputer ? "computer" : "human");
                s1Player = s1Player.Replace("{numplayers}", Convert.ToString(oRCX.m_nPlayers));
                s1Player = s1Player.Replace("{numteams}", Convert.ToString(oRCX.m_nTeams));
                s1Player = s1Player.Replace("{vs}", oRCX.m_sTeamVsText);

                if(bFoundTeam  &&  !bFirstTeam  &&  bFirstPlayer)
                  sTeams += this.m_editTeamSeparator.Text;
                else if(!bFirstPlayer)
                  sTeams += this.m_editPlayerSeparator.Text;
                sTeams += s1Player;
                bFoundTeam = true;
                bFirstPlayer = false;
                bFirstTeam = false;
              }
            }
          }

          sNewName = sNewName.Replace("{teams}", sTeams);

          if(bIsPreview)
          {
            if(sNewPath.Length + 1 + sNewName.Length >= 260)
            {
              this.m_editStatus.Text += "File \"" + oFile.Name + "\" CANNOT be renamed to \"" + sNewPath + "\\" + sNewName +
                "\" because the resulting path and file name is too long.\r\n";
            }
            else
            {
              if(oFile.Name != sNewName  ||  this.m_checkMove.Checked)
              {
                this.m_editStatus.Text += "File \"" + oFile.Name + "\" would be renamed to \"" + sNewName + "\"";
                if(this.m_checkMove.Checked)
                  this.m_editStatus.Text += " and moved to \"" + sNewPath + "\".\r\n";
                else
                  this.m_editStatus.Text += ".\r\n";
              }
            }
          }
          else
          {
            String sOrigName = oFile.Name;
            try
            {
              if(sOrigName != sNewName  ||  this.m_checkMove.Checked)
              {
                if(!Directory.Exists(sNewPath))
                  Directory.CreateDirectory(sNewPath);
                oFile.MoveTo(sNewPath+"\\"+sNewName);
                this.m_editStatus.Text += "Renamed file \"" + sOrigName + "\" to \"" + sNewName + "\"";
                if(this.m_checkMove.Checked)
                  this.m_editStatus.Text += " and moved to \"" + sNewPath + "\".\r\n";
                else
                  this.m_editStatus.Text += ".\r\n";
              }
            }
            catch(Exception e)
            {
              this.m_editStatus.Text += "Error: Could not rename file (" + sOrigName + ") (" + e + ").\r\n";
            }
          }
        }
        else
        {
          this.m_editStatus.Text += "Could not rename file \"" + oFile.Name + "\"!\r\n";
        }
      }

      this.m_editStatus.Text += "\r\n";

      // Scroll to end of messages
      this.m_editStatus.SelectionLength = 0;  
      this.m_editStatus.SelectionStart = this.m_editStatus.Text.Length;  
      this.m_editStatus.ScrollToCaret();

      // Restart the file watch if we need to
      if(this.m_checkWatchAuto.Checked)
      {
        this.m_fswDirWatch.Path = this.m_editDirectory.Text.Substring(0, this.m_editDirectory.Text.LastIndexOf('\\'));
        this.m_fswDirWatch.Filter = this.m_editDirectory.Text.Substring(this.m_editDirectory.Text.LastIndexOf('\\')+1);
        this.m_fswDirWatch.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
        this.m_fswDirWatch.EnableRaisingEvents = true;  // restart the watching
      }
    }

    // // // // // // // // // // // // // // // // // // // // // // // // //

    private void m_buttonClose_Click(object sender, System.EventArgs e)
    {
      this.m_fswDirWatch.EnableRaisingEvents = false;  // stop watching if we are
      this.Close();
    }

    // // // // // // // // // // // // // // // // // // // // // // // // //

    protected override void OnClosed(System.EventArgs e)
    {
      this.m_fswDirWatch.EnableRaisingEvents = false;  // stop watching if we are
      try
      {
        Registry.CurrentUser.CreateSubKey("Software\\AOMRecordedRenamer");
        RegistryKey OurKey = Registry.CurrentUser.OpenSubKey("Software\\AOMRecordedRenamer", true);
        OurKey.SetValue("Files", this.m_editDirectory.Text);
        OurKey.SetValue("WatchDir", this.m_checkWatchAuto.Checked);
        OurKey.SetValue("RenameFileName", this.m_editFilename.Text);
        OurKey.SetValue("RenameEachPlayer", this.m_editEachPlayer.Text);
        OurKey.SetValue("RenameTeamSeparator", this.m_editTeamSeparator.Text);
        OurKey.SetValue("RenamePlayerSeparator", this.m_editPlayerSeparator.Text);
        OurKey.SetValue("Move", this.m_checkMove.Checked);
        OurKey.SetValue("MoveDir", this.m_editMove.Text);
        OurKey.SetValue("TrimPlayer", this.m_checkTrimPlayer.Checked);
        OurKey.SetValue("TrimPlayerChar", this.m_editTrimPlayer.Text);
      }
      catch
      {
        // Too late to show status when the window closes
      }
    }

    // // // // // // // // // // // // // // // // // // // // // // // // //

    private void m_linkSoftware_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
    {
      System.Diagnostics.Process.Start(m_linkSoftware.Text);
    }

    // // // // // // // // // // // // // // // // // // // // // // // // //

    private void m_checkMove_CheckedChanged(object sender, System.EventArgs e)
    {
      if(this.m_checkMove.Checked)
      {
        this.m_editMove.Enabled = true;
        this.m_buttonBrowseMove.Enabled = true;
      }
      else
      {
        this.m_editMove.Enabled = false;
        this.m_buttonBrowseMove.Enabled = false;
      }
    }

    // // // // // // // // // // // // // // // // // // // // // // // // //

    private void m_checkTrimPlayer_CheckedChanged(object sender, System.EventArgs e)
    {
      if(this.m_checkTrimPlayer.Checked)
      {
        this.m_editTrimPlayer.Enabled = true;
      }
      else
      {
        this.m_editTrimPlayer.Enabled = false;
      }
    }

    // // // // // // // // // // // // // // // // // // // // // // // // //
  }
}
