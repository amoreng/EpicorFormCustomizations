// **************************************************
// Custom code for UD01Form
// Created By: Aaron Moreng (aaron.moreng@gmail.com)
// Created: 11/1/2018 12:10:31 PM
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
using Ice.Core;
using Ice.Lib.Customization;
using Ice.Lib.ExtendedProps;
using Ice.Lib.Framework;
using Ice.Lib.Searches;
using Ice.UI.FormFunctions;
using Infragistics.Win;
using Infragistics.Win.UltraWinToolbars;
using Infragistics.Win.UltraWinGrid;
using System.Net;
using System.Collections.Generic;
using System.Collections.Specialized;
using Newtonsoft.Json;
using System.IO;
using System.Text;

public class Script
{
	// ** Wizard Insert Location - Do Not Remove 'Begin/End Wizard Added Module Level Variables' Comments! **
	// Begin Wizard Added Module Level Variables **

	// End Wizard Added Module Level Variables **

	// Add Custom Module Level Variables Here **
	private const string SEND_REQUEST_WITH_TEMPLATE_URL = "https://api.hellosign.com/v3/signature_request/send_with_template";//this is the base url
	private const string FRESH_TEMPLATE_ID = "<templateID>";//this is fresh template id

	//TODO: Add the Meniscus tempate here 
	private const string API_KEY = "<apiKey>"; //this is provided by HelloSign
	private const string CLIENT_ID = "<clientID>";//this is the app ID for the embedded signing
	
	DataSet dsInput; //this is the dataset that the LFO will fill out and this will be used locally to store the quote data
	DataTable dtSigners = new DataTable(); //stores all the signers
	DataTable dtCCs = new DataTable(); //stores all the CCs	

	string userName;
	string userEmail;
		
	string relMsg;//Global string; holds template msg from query
	string stdMsg;//Global string; holds template msg from query
	string replyTo;
	bool isReleased;

	string templateType;//stores the template type to be passed in custom UD06 data

	public void InitializeCustomCode()
	{
		// ** Wizard Insert Location - Do not delete 'Begin/End Wizard Added Variable Initialization' lines **
		// Begin Wizard Added Variable Initialization

		// End Wizard Added Variable Initialization

		// Begin Wizard Added Custom Method Calls

		
		this.btnOpenFileDialog.Click += new System.EventHandler(this.btnOpenFileDialog_Click);
		this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
		this.btnAddSigner.Click += new System.EventHandler(this.btnAddSigner_Click);
		this.grdSigners.InitializeLayout += new Infragistics.Win.UltraWinGrid.InitializeLayoutEventHandler(this.grdSigners_InitializeLayout);
		this.btnRemoveSigner.Click += new System.EventHandler(this.btnRemoveSigner_Click);
		this.btnAddCC.Click += new System.EventHandler(this.btnAddCC_Click);
		this.btnRemoveCC.Click += new System.EventHandler(this.btnRemoveCC_Click);
		this.grdSigners.AfterSelectChange += new Infragistics.Win.UltraWinGrid.AfterSelectChangeEventHandler(this.grdSigners_AfterSelectChange);
		this.lblSignersToolTip.Click += new System.EventHandler(this.lblSignersToolTip_Click);
		this.lblCCsToolTip.Click += new System.EventHandler(this.lblCCsToolTip_Click);
		this.grdCCs.InitializeLayout += new Infragistics.Win.UltraWinGrid.InitializeLayoutEventHandler(this.grdCCs_InitializeLayout);
		this.grdCCs.AfterSelectChange += new Infragistics.Win.UltraWinGrid.AfterSelectChangeEventHandler(this.grdCCs_AfterSelectChange);	
		this.grdSigners.CellChange += new Infragistics.Win.UltraWinGrid.CellEventHandler(this.grdSigners_CellChange);
		this.btnMoveToCC.Click += new System.EventHandler(this.btnMoveToCC_Click);
		this.grdCCs.AfterSelectChange += new Infragistics.Win.UltraWinGrid.AfterSelectChangeEventHandler(this.grdCCs_AfterSelectChange);
		this.grdCCs.AfterCellUpdate += new Infragistics.Win.UltraWinGrid.CellEventHandler(this.grdCCs_AfterCellUpdate);
		this.btnMoveToSigner.Click += new System.EventHandler(this.btnMoveToSigner_Click);
		// End Wizard Added Custom Method Calls
		dsInput = new DataSet();
		//setup dsSigners and dsCCs
		dtSigners.Columns.Add("Name", typeof(string));
		dtSigners.Columns.Add("Email Address", typeof(string));		
		dtSigners.Columns.Add("Function", typeof(string));

		dtCCs.Columns.Add("Name", typeof(string));
		dtCCs.Columns.Add("Email Address", typeof(string));		
		dtCCs.Columns.Add("Function", typeof(string));

		Session session = (Session)oTrans.Session;
		userEmail = session.UserEmail.ToString();
		userName = session.UserName.ToString();	
		
		relMsg = string.Empty;
		stdMsg = string.Empty;
		isReleased = false;
		replyTo = string.Format("Please reply to {0} at {1} with any questions or concerns.", userName, userEmail);

		templateType = string.Empty;
	}

	public void DestroyCustomCode()
	{
		// ** Wizard Insert Location - Do not delete 'Begin/End Wizard Added Object Disposal' lines **
		// Begin Wizard Added Object Disposal

		
		this.btnOpenFileDialog.Click -= new System.EventHandler(this.btnOpenFileDialog_Click);
		this.btnPreview.Click -= new System.EventHandler(this.btnPreview_Click);
		this.btnAddSigner.Click -= new System.EventHandler(this.btnAddSigner_Click);
		this.grdSigners.InitializeLayout -= new Infragistics.Win.UltraWinGrid.InitializeLayoutEventHandler(this.grdSigners_InitializeLayout);
		this.btnRemoveSigner.Click -= new System.EventHandler(this.btnRemoveSigner_Click);
		this.btnAddCC.Click -= new System.EventHandler(this.btnAddCC_Click);
		this.btnRemoveCC.Click -= new System.EventHandler(this.btnRemoveCC_Click);
		this.grdSigners.AfterSelectChange -= new Infragistics.Win.UltraWinGrid.AfterSelectChangeEventHandler(this.grdSigners_AfterSelectChange);
		this.lblSignersToolTip.Click -= new System.EventHandler(this.lblSignersToolTip_Click);
		this.lblCCsToolTip.Click -= new System.EventHandler(this.lblCCsToolTip_Click);
		this.grdCCs.InitializeLayout -= new Infragistics.Win.UltraWinGrid.InitializeLayoutEventHandler(this.grdCCs_InitializeLayout);
		this.grdCCs.AfterSelectChange -= new Infragistics.Win.UltraWinGrid.AfterSelectChangeEventHandler(this.grdCCs_AfterSelectChange);	
		this.grdSigners.CellChange -= new Infragistics.Win.UltraWinGrid.CellEventHandler(this.grdSigners_CellChange);
		this.btnMoveToCC.Click -= new System.EventHandler(this.btnMoveToCC_Click);
		this.grdCCs.AfterSelectChange -= new Infragistics.Win.UltraWinGrid.AfterSelectChangeEventHandler(this.grdCCs_AfterSelectChange);
		this.grdCCs.AfterCellUpdate -= new Infragistics.Win.UltraWinGrid.CellEventHandler(this.grdCCs_AfterCellUpdate);
		this.btnMoveToSigner.Click -= new System.EventHandler(this.btnMoveToSigner_Click);
		// End Wizard Added Object Disposal

		// Begin Custom Code Disposal

		// End Custom Code Disposal
		dsInput = null;
	}

	private void UD01Form_Load(object sender, EventArgs args)
	{
		//Center the form
		this.UD01Form.StartPosition = FormStartPosition.CenterScreen;
		this.UD01Form.MaximizeBox = false;
  	  this.UD01Form.MinimizeBox = false;
  	  this.UD01Form.FormBorderStyle = FormBorderStyle.FixedDialog;	
		StartFormCleanup();		
		/*Prepare LFO*/
		if(UD01Form.LaunchFormOptions !=null)
		{
					
			//Add current user to the signers list
			dtSigners.Rows.Add(userName, userEmail, "ASC");

			dsInput = (DataSet)UD01Form.LaunchFormOptions.ContextValue;
			grdLFO.DataSource = dsInput;//testing only, to view data in dsInput 			
			SetInitialFields(dsInput);

			//Fill out grdSigners and grdCCs from query based on dsInput values (quoteNum)
			GetSignersAndCCs((int)dsInput.Tables[0].Rows[0]["QuoteHed_QuoteNum"]);
		
		}
		
		
	}

	#region HELLOSIGN
	private void SendFreshOfferEmbeddedTemplate()
	{
		try
		{
			//write rec out to UD06
			
		}
		catch(Exception ex)
		{
			
		}
		
	}
	#endregion

	



	#region ACTIONS

	private void btnOpenFileDialog_Click(object sender, System.EventArgs args)
	{
		OpenFileDialog fdlg = new OpenFileDialog();		
        fdlg.Title = "Browse for Dissection Sheets";
        fdlg.InitialDirectory = @"\\file01\JRF_Secured\";//moved from mapped K drive to FQDN for safety
        fdlg.RestoreDirectory = true;
        fdlg.Filter = "PDF Files (*.pdf)|*.pdf";
        if (fdlg.ShowDialog() == DialogResult.OK)
        {
            txtFilePath.Text = fdlg.FileName;
        }
	}

	private void btnPreview_Click(object sender, System.EventArgs args)
	{
		// ** Place Event Handling Code Here **
		//SendFreshOfferEmbeddedTemplate();
		//MessageBox.Show(GridHasValues(this.grdSigners).ToString());
		//MessageBox.Show(GridHasValues(this.grdCCs).ToString());


		/*TEST: Copy file to share drive*/
		CopyFile(txtFilePath.Text.ToString());
	}

	private void btnAddSigner_Click(object sender, System.EventArgs args)
	{
		/*Add Signer Row to grdSigners*/
		this.grdSigners.DisplayLayout.Bands[0].AddNew();
	}

	private void btnRemoveSigner_Click(object sender, System.EventArgs args)
	{
		/*Remove Selected rows from grdSigners*/
		this.grdSigners.DeleteSelectedRows(true);
	}

	private void btnAddCC_Click(object sender, System.EventArgs args)
	{
		/*Add Signer Row to grdCCs*/
		this.grdCCs.DisplayLayout.Bands[0].AddNew();
	}

	private void btnRemoveCC_Click(object sender, System.EventArgs args)
	{
		/*Remove Selected rows from grdCCs*/
		this.grdCCs.DeleteSelectedRows(true);
	}
		
	private void lblSignersToolTip_Click(object sender, System.EventArgs args)
	{
		// ** Place Event Handling Code Here **
		//maybe put a non-idiot message here 
		string msg = String.Format("Signers are people who can possibly sign or be reassigned to sign the document");
		MessageBox.Show(msg);
	}

	private void lblCCsToolTip_Click(object sender, System.EventArgs args)
	{
		// ** Place Event Handling Code Here **
		string msg = "List of CCs means people who can view the document";
		MessageBox.Show(msg);
	}

	
	private void btnMoveToCC_Click(object sender, System.EventArgs args)
	{
		/*Moves selected rows to CC grid from Signers grid*/
		if(this.grdSigners.Selected.Rows.Count >0)
		{
			List<UltraGridRow> deleteRows = new List<UltraGridRow>();
			
			foreach(var row in this.grdSigners.Selected.Rows)
			{	
				if((bool)row.Cells["Primary"].Value == false)
				{
					//don't allow primary signer to be moved to CC
					this.dtCCs.Rows.Add((string)row.Cells["Name"].Text, (string)row.Cells["Email Address"].Text, (string)row.Cells["Function"].Text);
					this.dtCCs.AcceptChanges();					
					deleteRows.Add(row);				
				}
				else
				{
					MessageBox.Show("Primary signer not allowed to be moved to CC");
					return;
				}				
			}
			//kill the rows that were added to the deleteRows list
			foreach(var drow in deleteRows)
			{
				drow.Delete(false);
			}
			
			//activate all rows in the new grid
			foreach(var ccrow in grdCCs.Rows)
			{
				ccrow.Activate();
			}			
			this.grdCCs.DisplayLayout.PerformAutoResizeColumns(false, Infragistics.Win.UltraWinGrid.PerformAutoSizeType.VisibleRows);
		}
		else
		MessageBox.Show("No rows selected to move");
	}

	private void btnMoveToSigner_Click(object sender, System.EventArgs args)
	{
		/*Moves selected rows to Signer grid from CC grid*/
		if(this.grdCCs.Selected.Rows.Count >0)
		{
			List<UltraGridRow> deleteRows = new List<UltraGridRow>();
			
			foreach(var row in this.grdCCs.Selected.Rows)
			{			
				//don't allow primary signer to be moved to CC
				this.dtSigners.Rows.Add((string)row.Cells["Name"].Text, (string)row.Cells["Email Address"].Text, (string)row.Cells["Function"].Text);
				this.dtSigners.AcceptChanges();					
				deleteRows.Add(row);								
			}
			//kill the rows that were added to the deleteRows list
			foreach(var drow in deleteRows)
			{
				drow.Delete(false);
			}
			
			//activate all rows in the new grid
			foreach(var ccrow in grdSigners.Rows)
			{
				ccrow.Activate();
			}			
			this.grdSigners.DisplayLayout.PerformAutoResizeColumns(false, Infragistics.Win.UltraWinGrid.PerformAutoSizeType.VisibleRows);
		}
		else
		MessageBox.Show("No rows selected to move");
	}
	private void GetSignersAndCCs(int quoteNum)
	{
		//Call query to return CCs and Signers, bind to grd data sources, refresh grids
		DataSet queryDS = SignerCC(quoteNum);				
		try
		{			
			/*BAQ = JRF-QuoteCnts_HelloSign
			//BAQ Schema = Customer_Name; Customer_EMailAddress; Calculated_Function; Calculated_Signer
			//dtSigners.Rows.Add(name, email, "ASC");
			*/
			foreach(DataRow row in queryDS.Tables["Results"].Rows)
			{
				if((int)row["Calculated_Signer"] == 1)
				{
					//add "signers" to dtSigners (Surgeon and office staff)
					if((string)row["Calculated_Function"] == "Surgeon")
					{
						//transform surgeon name into presentable format, instead of Lastname, Firstname
						string newName = GetNewName((string)row["Customer_Name"]);
						dtSigners.Rows.Add(newName, (string)row["Customer_EMailAddress"], (string)row["Calculated_Function"]);						
					}
					else
					dtSigners.Rows.Add((string)row["Customer_Name"], (string)row["Customer_EMailAddress"], (string)row["Calculated_Function"]);
				}
				else
				if((int)row["Calculated_Signer"] == 0)
				{
					//add "Ccs" to dtCCs (reps)
					dtCCs.Rows.Add((string)row["Customer_Name"], (string)row["Customer_EMailAddress"], (string)row["Calculated_Function"]);
				}
			}				
			/*Set up grid bindings*/	
			this.grdSigners.DataSource = dtSigners;
			this.grdCCs.DataSource = dtCCs;
			/*mark the "Surgeon" as primary*/
			foreach(var item in grdSigners.Rows)
			{			
				if((string)item.Cells["Function"].Value =="Surgeon")
				{				
					item.Cells["Primary"].Value = true;
					item.CancelUpdate();
				}
			}	
			/*re-sort grdSigners by primary*/
			UltraGridBand band = this.grdSigners.DisplayLayout.Bands[0];
			band.Columns["Primary"].SortIndicator = SortIndicator.Descending;			
		}	
		catch(Exception ex)
		{
			MessageBox.Show("Oops! Something went wrong: " + ex.Message);
		}
	
	}
	#endregion

	#region HELPERS
	
	private bool GridHasValues(Ice.Lib.Framework.EpiUltraGrid grd)
	{
		//This method loops through the grdSigners and makes sure there is at least 1 value	
		bool hasValues = false;
		int rowCount = 0;
		GridRowType rowTypes = GridRowType.DataRow;
		UltraGridBand band = grd.DisplayLayout.Bands[0];
		var enumerator = band.GetRowEnumerator(rowTypes);	
		foreach(UltraGridRow row in enumerator)
		{
			//MessageBox.Show(String.Format("Signers: {0}",row.Cells[0].Value.ToString()));
			rowCount++;
		}
		if(rowCount>0)
		{
			hasValues = true;
		}
		return hasValues;
	}

	private string GetNewName(string inputName)
	{
		string outputName = string.Empty;
		//this splits apart the cust name and then re-constructs for easy read.			
		string[] values = inputName.Split(',');
		Array.Reverse(values);
		foreach (string value in values)
		{
			outputName = outputName + " " + value.Trim().ToString();				
		}
		outputName = "Dr." + outputName;
		return outputName;
	}

	private void CopyFile(string inputFilePath)
	{
		/*Copys a file from a given file path to a new directory*/
		try
		{
			if(!string.IsNullOrEmpty(inputFilePath))
			{
				/*Copy the file to a new file with logic to the name, then place new file on //file01/HelloSign*/
				
				string inputFileName = Path.GetFileName(inputFilePath);
				string timeStamp = DateTime.Now.ToString();
				
				string outputFileName = "";

				//File.Copy(inputFilePath, outputFileName);
			}
		}
		catch(Exception ex)
		{
			MessageBox.Show(ex.Message.ToString());
		}
	}	

	private DataSet SignerCC(int quoteNum)
	{
		//returns a list of signers (doc + non-arthrex ppl) and cc's (reps)
		//parsed in GetSignersAndCCs method	
		DataSet dsResults = new DataSet();
		
		using(DynamicQueryAdapter querySigners = new DynamicQueryAdapter(oTrans))
		{
			//Create QuoteNum parameter
			QueryExecutionDataSet parameters = new QueryExecutionDataSet();	
			DataRow paramRow = parameters.ExecutionParameter.NewRow();
			paramRow["ParameterID"] = "QuoteNum";
			paramRow["ParameterValue"] = quoteNum;
			paramRow["ValueType"]= "int";
			paramRow["IsEmpty"] = "False";
			parameters.ExecutionParameter.Rows.Add(paramRow);
			//Call JRF-QuoteCnts_HelloSign
			querySigners.BOConnect();	
			querySigners.ExecuteByID("JRF-QuoteCnts_HelloSign",parameters);
			
			dsResults = querySigners.QueryResults;
			
			querySigners.Dispose();			
		}
		return dsResults;
	}


	private void StartFormCleanup()
	{
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
	}

	private void grdSigners_InitializeLayout(object sender, Infragistics.Win.UltraWinGrid.InitializeLayoutEventArgs args)
	{
		// ** Place Event Handling Code Here **
		
		//add mutually exclusive boolean to the grid to select primary signer 
		//args.Layout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.False;
	    args.Layout.Override.MinRowHeight = 20;
	    args.Layout.Override.DefaultRowHeight = 20;
	    
	    args.Layout.Bands[0].Columns.Add("Primary");
	    args.Layout.Bands[0].Columns["Primary"].DataType = typeof(bool);
	    args.Layout.Bands[0].Columns["Primary"].Header.VisiblePosition = 0;
	    //args.Layout.Bands[0].Columns["Primary"].EditorComponent = this.grdSigners;
	    
	    args.Layout.Bands[0].Columns["Primary"].Width = 16;
	    args.Layout.Bands[0].Columns["Primary"].MinWidth = 16;
	    args.Layout.Bands[0].Columns["Primary"].MaxWidth = 16;

		grdSigners.DisplayLayout.PerformAutoResizeColumns(false, Infragistics.Win.UltraWinGrid.PerformAutoSizeType.VisibleRows);		
	}	

	private void grdSigners_AfterSelectChange(object sender, Infragistics.Win.UltraWinGrid.AfterSelectChangeEventArgs args)
	{
		// ** Place Event Handling Code Here **
		grdSigners.DisplayLayout.PerformAutoResizeColumns(false, Infragistics.Win.UltraWinGrid.PerformAutoSizeType.VisibleRows);
	}

	private void grdCCs_InitializeLayout(object sender, Infragistics.Win.UltraWinGrid.InitializeLayoutEventArgs args)
	{		
		// ** Place Event Handling Code Here **
		grdCCs.DisplayLayout.PerformAutoResizeColumns(false, Infragistics.Win.UltraWinGrid.PerformAutoSizeType.VisibleRows);
	}

	private void grdCCs_AfterSelectChange(object sender, Infragistics.Win.UltraWinGrid.AfterSelectChangeEventArgs args)
	{
		// ** Place Event Handling Code Here **
		grdCCs.DisplayLayout.PerformAutoResizeColumns(false, Infragistics.Win.UltraWinGrid.PerformAutoSizeType.VisibleRows);
	}

	private void grdCCs_AfterCellUpdate(object sender, Infragistics.Win.UltraWinGrid.CellEventArgs args)
	{
		// ** Place Event Handling Code Here **	
		grdCCs.DisplayLayout.PerformAutoResizeColumns(false, Infragistics.Win.UltraWinGrid.PerformAutoSizeType.VisibleRows);
	}

	private void grdSigners_CellChange(object sender, Infragistics.Win.UltraWinGrid.CellEventArgs args)
	{
		//Allows for only 1 row to be selected as "Primary"
		foreach(var item in grdSigners.Rows)
		{
			if(args.Cell.Row.Index != item.Index)
			{
				item.Cells["Primary"].Value = false;
				item.CancelUpdate();
			}
		}
	}

	private void SetInitialFields(DataSet input)
	{	
		/*Sets the initial field values by the given input*/
		txtOfferComment.Text = (string)input.Tables[0].Rows[0]["QuoteDtl_Character03"];
		txtMessageBody.Text = HelloSignOfferBodyTemplate(input);
		/*Toggle Template radio for relMsg or stdMsg on isReleased*/
		if(isReleased)
		{
			
		}
	}
		
	private string HelloSignOfferBodyTemplate(DataSet input)
	{
		//Switches on isReleased from dsInput
		//Message is generated by BAQ
		string body = string.Empty;	
		//setting msgs outside of method makes these available to toggle later
		relMsg = (string)input.Tables[0].Rows[0]["Calculated_HSMsgRel"];
		stdMsg = (string)input.Tables[0].Rows[0]["Calculated_HSMsgStd"];
		
		string cRet = "\r\n";

		isReleased = (bool)input.Tables[0].Rows[0]["Calculated_IsReleased"];
		//MessageBox.Show(isReleased.ToString());
		switch(isReleased)
		{	
			case true:
				//Graft is Released template
				//adds on the current userName and userEmail to body
				body = string.Format("{0}{1}{2}",relMsg,cRet,replyTo);
				break;
			case false:
				//Graft is Pending Release template			
				//adds on the current userName and userEmail to body	
				body = string.Format("{0}{1}{2}",stdMsg,cRet,replyTo);
				break;
		}
		return body;
	}

	#endregion




}
