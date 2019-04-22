Imports System
Imports System.Drawing
Imports System.Collections
Imports System.ComponentModel
Imports System.Windows.Forms
Imports System.Data
Imports DevExpress.XtraGrid.Views.Grid
Imports DevExpress.XtraGrid.Helpers


Namespace RefreshHelperApp
	''' <summary>
	''' Summary description for Form1.
	''' </summary>
	Public Class Form1
		Inherits System.Windows.Forms.Form

		Private gridControl1 As DevExpress.XtraGrid.GridControl
		Private gridView1 As DevExpress.XtraGrid.Views.Grid.GridView
		Private WithEvents simpleButton1 As DevExpress.XtraEditors.SimpleButton
		Private WithEvents simpleButton2 As DevExpress.XtraEditors.SimpleButton
		Private panel1 As System.Windows.Forms.Panel
		Private WithEvents checkEdit1 As DevExpress.XtraEditors.CheckEdit
		''' <summary>
		''' Required designer variable.
		''' </summary>
		Private components As System.ComponentModel.Container = Nothing

		Public Sub New()
			'
			' Required for Windows Form Designer support
			'
			InitializeComponent()

			'
			' TODO: Add any constructor code after InitializeComponent call
			'
		End Sub

		''' <summary>
		''' Clean up any resources being used.
		''' </summary>
		Protected Overrides Sub Dispose(ByVal disposing As Boolean)
			If disposing Then
				If components IsNot Nothing Then
					components.Dispose()
				End If
			End If
			MyBase.Dispose(disposing)
		End Sub

		#Region "Windows Form Designer generated code"
		''' <summary>
		''' Required method for Designer support - do not modify
		''' the contents of this method with the code editor.
		''' </summary>
		Private Sub InitializeComponent()
			Me.gridControl1 = New DevExpress.XtraGrid.GridControl()
			Me.gridView1 = New DevExpress.XtraGrid.Views.Grid.GridView()
			Me.simpleButton1 = New DevExpress.XtraEditors.SimpleButton()
			Me.simpleButton2 = New DevExpress.XtraEditors.SimpleButton()
			Me.panel1 = New System.Windows.Forms.Panel()
			Me.checkEdit1 = New DevExpress.XtraEditors.CheckEdit()
			DirectCast(Me.gridControl1, System.ComponentModel.ISupportInitialize).BeginInit()
			DirectCast(Me.gridView1, System.ComponentModel.ISupportInitialize).BeginInit()
			Me.panel1.SuspendLayout()
			DirectCast(Me.checkEdit1.Properties, System.ComponentModel.ISupportInitialize).BeginInit()
			Me.SuspendLayout()
			' 
			' gridControl1
			' 
			Me.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill
			Me.gridControl1.EmbeddedNavigator.Name = ""
			Me.gridControl1.Location = New System.Drawing.Point(0, 40)
			Me.gridControl1.MainView = Me.gridView1
			Me.gridControl1.Name = "gridControl1"
			Me.gridControl1.Size = New System.Drawing.Size(816, 510)
			Me.gridControl1.TabIndex = 0
			Me.gridControl1.ViewCollection.AddRange(New DevExpress.XtraGrid.Views.Base.BaseView() { Me.gridView1})
			' 
			' gridView1
			' 
			Me.gridView1.GridControl = Me.gridControl1
			Me.gridView1.Name = "gridView1"
			Me.gridView1.OptionsSelection.MultiSelect = True
			' 
			' simpleButton1
			' 
			Me.simpleButton1.Location = New System.Drawing.Point(8, 8)
			Me.simpleButton1.Name = "simpleButton1"
			Me.simpleButton1.Size = New System.Drawing.Size(75, 23)
			Me.simpleButton1.TabIndex = 1
			Me.simpleButton1.Text = "Save"
'INSTANT VB NOTE: The following InitializeComponent event wireup was converted to a 'Handles' clause:
'ORIGINAL LINE: this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
			' 
			' simpleButton2
			' 
			Me.simpleButton2.Location = New System.Drawing.Point(104, 8)
			Me.simpleButton2.Name = "simpleButton2"
			Me.simpleButton2.Size = New System.Drawing.Size(72, 24)
			Me.simpleButton2.TabIndex = 2
			Me.simpleButton2.Text = "Load"
'INSTANT VB NOTE: The following InitializeComponent event wireup was converted to a 'Handles' clause:
'ORIGINAL LINE: this.simpleButton2.Click += new System.EventHandler(this.simpleButton2_Click);
			' 
			' panel1
			' 
			Me.panel1.Controls.Add(Me.checkEdit1)
			Me.panel1.Controls.Add(Me.simpleButton1)
			Me.panel1.Controls.Add(Me.simpleButton2)
			Me.panel1.Dock = System.Windows.Forms.DockStyle.Top
			Me.panel1.Location = New System.Drawing.Point(0, 0)
			Me.panel1.Name = "panel1"
			Me.panel1.Size = New System.Drawing.Size(816, 40)
			Me.panel1.TabIndex = 3
			' 
			' checkEdit1
			' 
			Me.checkEdit1.Location = New System.Drawing.Point(208, 12)
			Me.checkEdit1.Name = "checkEdit1"
			Me.checkEdit1.Properties.Caption = "Cell selection"
			Me.checkEdit1.Size = New System.Drawing.Size(123, 19)
			Me.checkEdit1.TabIndex = 3
'INSTANT VB NOTE: The following InitializeComponent event wireup was converted to a 'Handles' clause:
'ORIGINAL LINE: this.checkEdit1.CheckedChanged += new System.EventHandler(this.checkEdit1_CheckedChanged);
			' 
			' Form1
			' 
			Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
			Me.ClientSize = New System.Drawing.Size(816, 550)
			Me.Controls.Add(Me.gridControl1)
			Me.Controls.Add(Me.panel1)
			Me.Name = "Form1"
			Me.Text = "Form1"
'INSTANT VB NOTE: The following InitializeComponent event wireup was converted to a 'Handles' clause:
'ORIGINAL LINE: this.Load += new System.EventHandler(this.Form1_Load);
			DirectCast(Me.gridControl1, System.ComponentModel.ISupportInitialize).EndInit()
			DirectCast(Me.gridView1, System.ComponentModel.ISupportInitialize).EndInit()
			Me.panel1.ResumeLayout(False)
			DirectCast(Me.checkEdit1.Properties, System.ComponentModel.ISupportInitialize).EndInit()
			Me.ResumeLayout(False)

		End Sub
		#End Region

		''' <summary>
		''' The main entry point for the application.
		''' </summary>
		<STAThread>
		Shared Sub Main()
			Application.Run(New Form1())
		End Sub

		Private helper As GridControlState

		Private ds As DataSet
		Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
			ds = New DataSet()
			ds.ReadXml("nwind.xml")
			gridView1.OptionsView.ColumnAutoWidth = False
			gridControl1.DataSource = ds.Tables("Categories")

			helper = New GridControlState(New GridControlState.ViewDescriptor(){
				New GridControlState.ViewDescriptor("", "CategoryID"),
				New GridControlState.ViewDescriptor("CategoriesProducts", "ProductID"),
				New GridControlState.ViewDescriptor("ProductsOrderDetails", "OrderID")
			})
		End Sub

		Private Sub simpleButton1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles simpleButton1.Click
			helper.SaveViewInfo(gridView1)
		End Sub

		Private Sub simpleButton2_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles simpleButton2.Click
			helper.LoadViewInfo(gridView1)
		End Sub

		Private Sub checkEdit1_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles checkEdit1.CheckedChanged
			gridView1.OptionsSelection.MultiSelectMode = If(DirectCast(sender, DevExpress.XtraEditors.CheckEdit).Checked, GridMultiSelectMode.CellSelect, GridMultiSelectMode.RowSelect)
		End Sub
	End Class
End Namespace
