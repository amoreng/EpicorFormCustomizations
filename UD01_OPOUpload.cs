// **************************************************
// Custom code for UD01Form
// Created: 6/1/2018 12:10:31 PM
// **************************************************
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using Ice.BO;
using Ice.UI;
using Ice.Lib;
using Ice.Adapters;
using Ice.Lib.Customization;
using Ice.Lib.ExtendedProps;
using Ice.Lib.Framework;
using Ice.Lib.Searches;
using Ice.UI.FormFunctions;
using Infragistics.Win.UltraWinToolbars;
using Infragistics.Win.UltraWinGrid.ExcelExport;
using Ice.Proxy.Lib;
using Ice.Core;
using Ice.Tablesets;
using System.Collections;
using System.Text;
using System.ComponentModel;
using System.Linq;

public class Script
{
	// ** Wizard Insert Location - Do Not Remove 'Begin/End Wizard Added Module Level Variables' Comments! **
	// Begin Wizard Added Module Level Variables **

	// End Wizard Added Module Level Variables **

	// Add Custom Module Level Variables Here **
	DataTable dtInput = new DataTable();
	private EpiDataView edvInput;
	
	private StringBuilder builder;
	private BackgroundWorker worker;
	private System.Windows.Forms.ProgressBar pbProgress;
	private int totalRecCount = 0;

	public void InitializeCustomCode()
	{
		// ** Wizard Insert Location - Do not delete 'Begin/End Wizard Added Variable Initialization' lines **
		// Begin Wizard Added Variable Initialization

		// End Wizard Added Variable Initialization

		// Begin Wizard Added Custom Method Calls

		this.grdInput.InitializeLayout += new Infragistics.Win.UltraWinGrid.InitializeLayoutEventHandler(this.grdInput_InitializeLayout);
		this.btnProcesss.Click += new System.EventHandler(this.btnProcesss_Click);
		this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
		// End Wizard Added Custom Method Calls
		dtInput.Columns.Add("Donor",typeof(string));
		dtInput.Columns.Add("OPO Name", typeof(string));
		dtInput.Columns.Add("OPO Donor Number", typeof(string));	
		
		edvInput = new EpiDataView();
		edvInput.dataView = dtInput.DefaultView;
		oTrans.Add("edvInput", edvInput);
	
		grdInput.EpiAllowPaste = true;
		grdInput.EpiAllowPasteInsert = true;
		//grdInput.InsertNewRowAfterLastRow = true;
	
		builder = new StringBuilder();
		SetUpPB();
	
		this.worker = new BackgroundWorker();
		this.worker.DoWork += new DoWorkEventHandler(this.worker_DoWork);
		this.worker.ProgressChanged += new ProgressChangedEventHandler(this.worker_ProgressChanged);
		this.worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.worker_RunWorkerCompleted);
		this.worker.WorkerReportsProgress = true;
		this.worker.WorkerSupportsCancellation = false;
	}

	public void DestroyCustomCode()
	{
		// ** Wizard Insert Location - Do not delete 'Begin/End Wizard Added Object Disposal' lines **
		// Begin Wizard Added Object Disposal
		this.grdInput.InitializeLayout -= new Infragistics.Win.UltraWinGrid.InitializeLayoutEventHandler(this.grdInput_InitializeLayout);
		this.btnProcesss.Click -= new System.EventHandler(this.btnProcesss_Click);
		this.btnClear.Click -= new System.EventHandler(this.btnClear_Click);
		// End Wizard Added Object Disposal

		// Begin Custom Code Disposal

		// End Custom Code Disposal
		pbProgress.Dispose();
		pbProgress=null;
	
		this.worker.DoWork -= new DoWorkEventHandler(this.worker_DoWork);
		this.worker.ProgressChanged -= new ProgressChangedEventHandler(this.worker_ProgressChanged);
		this.worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(this.worker_RunWorkerCompleted);
		
	}

	private void SetUpPB()
	{
		pbProgress = new System.Windows.Forms.ProgressBar();
		//Draw a textbox where you'd like the PB to be and get its Location from there then remove textbox
		pbProgress.Location = new System.Drawing.Point(14, 335);
		pbProgress.Name = "pbProgress";
		//Draw a TextBox where you'd like the PB to be and get its Size from there then remove textbox
		pbProgress.Size = new System.Drawing.Size(688, 20);
		//Get a Hold of the Parent Container where you'd like the PB to be
		Ice.UI.App.UD01Entry.DetailPanel pnl =(Ice.UI.App.UD01Entry.DetailPanel)csm.GetNativeControlReference("d5488fbc-e47b-46b6-aa3e-9ab7d923315a");		
		//Add PB to the above container
		pnl.Controls.Add(pbProgress);
	}	

	private void UD01Form_Load(object sender, EventArgs args)
	{
		// Add Event Handler Code
		// Hide Native Toolbar Controls 
		baseToolbarsManager.Tools["NewTool"].SharedProps.Visible=false;
		baseToolbarsManager.Tools["RefreshTool"].SharedProps.Visible=false;
		baseToolbarsManager.Tools["DeleteTool"].SharedProps.Visible=false;
		baseToolbarsManager.Tools["SaveTool"].SharedProps.Visible=false;
		baseToolbarsManager.Tools["EditMenu"].SharedProps.Visible=false;
		baseToolbarsManager.Tools["HelpMenu"].SharedProps.Visible=false;	
		baseToolbarsManager.Tools["ToolsMenu"].SharedProps.Visible=false;
		baseToolbarsManager.Tools["ActionsMenu"].SharedProps.Visible=false;
		baseToolbarsManager.Tools["FileMenu"].SharedProps.Visible=false;
		baseToolbarsManager.Tools["AttachmentTool"].SharedProps.Visible=false;
		baseToolbarsManager.Tools["ClearTool"].SharedProps.Visible=false;
		baseToolbarsManager.Tools["CopyTool"].SharedProps.Visible=false;
		baseToolbarsManager.Tools["CutTool"].SharedProps.Visible=false;
		baseToolbarsManager.Tools["PasteTool"].SharedProps.Visible=false;
		baseToolbarsManager.Tools["UndoTool"].SharedProps.Visible=false;
		baseToolbarsManager.Tools["PrimarySearchTool"].SharedProps.Visible=false;

		dtInput.Clear();
		
		txtProgress.Text = builder.ToString();
	}

	private void grdInput_InitializeLayout(object sender, Infragistics.Win.UltraWinGrid.InitializeLayoutEventArgs args)
	{
		// ** Place Event Handling Code Here **
		grdInput.EpiAllowPaste = true;
		grdInput.EpiAllowPasteInsert = true;
		grdInput.InsertNewRowAfterLastRow = false;
	}



	private void worker_DoWork(object sender, DoWorkEventArgs e)
    {
		//sender is BackgoundWorker
		//this is where the background operation goes
		using(UD100Adapter adapterUD100 = new UD100Adapter(oTrans))
		{
			adapterUD100.BOConnect();	
			builder.Append("Start Process...").AppendLine();	
			
			//foreach(DataRow dr in dtInput.Rows)		
			for(int i = 0; i<dtInput.Rows.Count; i++)		
			{
				var dr = dtInput.Rows[i];
				string whereClause = string.Format("Key1 like '{0}-%'", dr["Donor"].ToString());
				//start log
				builder.Append(string.Format("Looking up Donor {0}...", dr["Donor"].ToString())).AppendLine();		

				//the Hashtable stores the runtime search criteria
				System.Collections.Hashtable wcs = new Hashtable(1);
				wcs.Clear();	
				wcs.Add("UD100", whereClause);
				Ice.Lib.Searches.SearchOptions opts = Ice.Lib.Searches.SearchOptions.CreateRuntimeSearch(wcs, Ice.Lib.Searches.DataSetMode.RowsDataSet);	   
				adapterUD100.InvokeSearch(opts);
				
				int rowCount = adapterUD100.UD100Data.UD100.Rows.Count;
				if(rowCount>0)	
				{	
					foreach(DataRow udRow in adapterUD100.UD100Data.UD100.Rows)
					{	
						//modify here
						udRow.BeginEdit();
						udRow["Character04"] = dr["OPO Name"];
						udRow["Character05"] = dr["OPO Donor Number"];
						udRow["RowMod"] = "U";
						udRow.EndEdit();
						//update log
						builder.Append(string.Format("{0}: Success!", udRow["Key1"].ToString())).AppendLine();
						totalRecCount++;
					}
					//update
					adapterUD100.Update();	
					worker.ReportProgress(i);
				}
				
				else
				{
					//update log
					builder.Append(string.Format("******Failed to find UD100 recs for {0}******", dr["Donor"].ToString())).AppendLine();
				}					
				txtProgress.Text = builder.ToString();
			}
			adapterUD100.Dispose();
			worker.ReportProgress(100);
			
		}	
	}
	
	private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
		//notifies progress bar when changed
		pbProgress.Value = e.ProgressPercentage;		
	}
	
	private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		//On completed, do the appropriate task		
		if(e.Error !=null)		
		{
			MessageBox.Show("Error while performing tasks!");
		}
		else
		{
			MessageBox.Show(string.Format("Complete. {0} records updated!", totalRecCount.ToString()));
		}
		//re-enable the UI interface to prevent weird threading issues
		btnProcesss.ReadOnly = false;
		btnClear.ReadOnly = false;
	}

	private void btnProcesss_Click(object sender, System.EventArgs args)
	{
		// ** Place Event Handling Code Here **
	
		if (dtInput.Rows.Count>0)
		{
			try
			{
				worker.RunWorkerAsync();

				builder.Clear();
				pbProgress.Value = 0;
				totalRecCount = 0;
				btnProcesss.ReadOnly = true;
				btnClear.ReadOnly = true;				
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}	
		else
		{
			MessageBox.Show("Add Inputs");
		}
	}

	private void btnClear_Click(object sender, System.EventArgs args)
	{
		// ** Place Event Handling Code Here **
		dtInput.Clear();
		builder.Clear();
		txtProgress.Text = string.Empty;
		pbProgress.Value = 0;
		totalRecCount = 0;
	}
}
