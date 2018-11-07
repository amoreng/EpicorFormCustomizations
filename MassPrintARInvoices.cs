// **************************************************
/// Custom code for ARInvForm
// Created: 5/20/2016 3:08:40 PM
// **************************************************
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using Erp.UI;
using Ice.Lib.Customization;
using Ice.Lib.ExtendedProps;
using Ice.Lib.Framework;
using Ice.Lib.Searches;
using Ice.UI.FormFunctions;
using System.Text.RegularExpressions;
using System.Threading;

public class Script
{
	// ** Wizard Insert Location - Do Not Remove 'Begin/End Wizard Added Module Level Variables' Comments! **
	// Begin Wizard Added Module Level Variables **

	private EpiDataView edvReportParam;
	// End Wizard Added Module Level Variables **

	// Add Custom Module Level Variables Here **
	DataTable dtInput = new DataTable();
	string Invoice;
	bool printed = false;
	public void InitializeCustomCode()
	{
		// ** Wizard Insert Location - Do not delete 'Begin/End Wizard Added Variable Initialization' lines **
		// Begin Wizard Added Variable Initialization

		this.edvReportParam = ((EpiDataView)(this.oTrans.EpiDataViews["ReportParam"]));
		this.edvReportParam.EpiViewNotification += new EpiViewNotification(this.edvReportParam_EpiViewNotification);
		// End Wizard Added Variable Initialization

		// Begin Wizard Added Custom Method Calls

		this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
		this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
		this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
		this.txtAddInvoice.Validated += new System.EventHandler(this.txtAddInvoice_Validated);
		this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
		// End Wizard Added Custom Method Calls
		dtInput.Columns.Add("Invoice",typeof(int));
		dtInput.Columns.Add("Printed", typeof(bool));
	}

	public void DestroyCustomCode()
	{
		// ** Wizard Insert Location - Do not delete 'Begin/End Wizard Added Object Disposal' lines **
		// Begin Wizard Added Object Disposal

		this.edvReportParam.EpiViewNotification -= new EpiViewNotification(this.edvReportParam_EpiViewNotification);
		this.edvReportParam = null;
		this.btnAdd.Click -= new System.EventHandler(this.btnAdd_Click);
		this.btnRemove.Click -= new System.EventHandler(this.btnRemove_Click);
		this.btnPrint.Click -= new System.EventHandler(this.btnPrint_Click);
		this.txtAddInvoice.Validated -= new System.EventHandler(this.txtAddInvoice_Validated);
		this.btnClear.Click -= new System.EventHandler(this.btnClear_Click);
		// End Wizard Added Object Disposal

		// Begin Custom Code Disposal

		// End Custom Code Disposal
	}

	private void edvReportParam_EpiViewNotification(EpiDataView view, EpiNotifyArgs args)
	{
		// ** Argument Properties and Uses **
		// view.dataView[args.Row]["FieldName"]
		// args.Row, args.Column, args.Sender, args.NotifyType
		// NotifyType.Initialize, NotifyType.AddRow, NotifyType.DeleteRow, NotifyType.InitLastView, NotifyType.InitAndResetTreeNodes
		if ((args.NotifyType == EpiTransaction.NotifyType.Initialize))
		{
			if ((args.Row > -1))
			{
			//MessageBox.Show(edvReportParam.dataView[edvReportParam.Row]["InvoiceNum"].ToString());
			}
		}
	}
	

	private void ARInvForm_Load(object sender, EventArgs args)
	{
		// Add Event Handler Code
		EpiDataView dvRP = (EpiDataView)oTrans.EpiDataViews["ReportParam"];
		//MessageBox.Show(dvRP.dataView[dvRP.Row]["InvoiceNum"].ToString());
		dtInput.Clear();
		grdInput.DataSource = dtInput;
		//EpiDataView dvRP = (EpiDataView)oTrans.EpiDataViews["ReportParam"];
		Invoice = dvRP.dataView[dvRP.Row]["InvoiceNum"].ToString();	
		if (Invoice != "0")
		{
			dtInput.Rows.Add(Invoice, printed);
		}
	}

	private void ReadOnlyDT()
	{
	foreach (DataColumn dc in dtInput.Columns)
		{
		//dc.ReadOnly = true;
		}
	}
	
	private void btnAdd_Click(object sender, System.EventArgs args)
	{
		// ** Place Event Handling Code Here **
		//Invoice = txtAddInvoice.Value.ToString();
		if (String.IsNullOrEmpty(txtAddInvoice.Text)) return;
		Invoice = txtAddInvoice.Value.ToString();
		//regex
		Regex regex = new Regex(@"^[0-9]{6}$");
		Match match = regex.Match(Invoice);
		if (match.Success)	
		{
			dtInput.Rows.Add(Invoice, printed);
		}
		else
		{
			MessageBox.Show("Enter a 6 digit invoice number");
		}
		txtAddInvoice.Clear();
		txtAddInvoice.Focus();
	}

	private void btnRemove_Click(object sender, System.EventArgs args)
	{
		// ** Place Event Handling Code Here **	
		
		if (String.IsNullOrEmpty(txtAddInvoice.Text)) return;
		Invoice = txtAddInvoice.Value.ToString();					
		for(int i = dtInput.Rows.Count-1;i>=0;i--)//backwards loop 
		{
			DataRow dr = dtInput.Rows[i];
				if (dr["Invoice"].ToString() == Invoice)
				{
					dr.Delete();//will remove any duplicate entries, so be careful
				} 
		}	
		grdInput.Refresh();
		txtAddInvoice.Clear();
	}

	private void btnPrint_Click(object sender, System.EventArgs args)
	{
		// ** Place Event Handling Code Here **
		txtNumberInvoicesSent.Value = 0;
		if (dtInput.Rows.Count<1){ MessageBox.Show("No Invoices Selected for Print"); 
		return;}
		EpiDataView dvRP = (EpiDataView)oTrans.EpiDataViews["ReportParam"];
		string workID = oTrans.WorkStationID.ToString();
		try {
			foreach (DataRow dr in dtInput.Rows)
			{
				dvRP.dataView[dvRP.Row]["InvoiceNum"] = dr["Invoice"];
				dvRP.dataView[dvRP.Row]["AutoAction"] = "SSRSPREVIEW";
				dvRP.dataView[dvRP.Row]["WorkstationID"] = workID;
				dr["Printed"] = true;
				oTrans.Update();					
				oTrans.SubmitToAgent("SystemTaskAgent", 0, 0);
				Thread.Sleep(500);
			}
			
			txtNumberInvoicesSent.Value = dtInput.Rows.Count;
			oTrans.PushDisposableStatusText("Reports Submitted for Preview...", true);
		}
		catch (Exception ex)
		{
		MessageBox.Show(ex.Message);
		}
	}

	private void txtAddInvoice_Validated(object sender, System.EventArgs args)
	{
		// ** Place Event Handling Code Here **		
	}

	private void btnClear_Click(object sender, System.EventArgs args)
	{
		// ** Place Event Handling Code Here **
		dtInput.Clear();
		txtNumberInvoicesSent.Clear();
	}
}
