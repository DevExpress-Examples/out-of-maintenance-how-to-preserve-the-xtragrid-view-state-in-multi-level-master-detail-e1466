Imports Microsoft.VisualBasic
Imports System
Imports System.Collections
Imports DevExpress.XtraGrid
Imports DevExpress.Utils
Imports DevExpress.XtraGrid.Columns
Imports DevExpress.XtraGrid.Views.Grid

Namespace DevExpress.XtraGrid.Helpers

	Public Class GridControlState

		Public Structure RowInfo
			Public Id As Object
			Public level As Integer
		End Structure

		Public Structure ViewDescriptor
			Public relationName As String
			Public keyFieldName As String

			Public Sub New(ByVal relationName As String, ByVal keyFieldName As String)
				Me.relationName = relationName
				Me.keyFieldName = keyFieldName
			End Sub
		End Structure

		Public Class ViewState
			Private gridState As GridControlState
			Private parent As ViewState
			Private descriptor As ViewDescriptor
			Private saveExpList_Renamed As ArrayList
			Private saveSelList_Renamed As ArrayList
			Private saveMasterRowsList_Renamed As ArrayList
			Private visibleRowIndex As Integer = -1
			Private detailViews_Renamed As Hashtable
			Private horzScrollPos As Integer
			Private cellSelection_Renamed As Hashtable

			Protected Sub New(ByVal gridState As GridControlState, ByVal descriptor As ViewDescriptor)
				Me.gridState = gridState
				Me.descriptor = descriptor
				Me.parent = Nothing
			End Sub

			Protected Sub New(ByVal parent As ViewState, ByVal descriptor As ViewDescriptor)
				Me.parent = parent
				Me.gridState = parent.gridState
				Me.descriptor = descriptor
			End Sub

			Public Shared Function Create(ByVal gridState As GridControlState, ByVal view As GridView) As ViewState
				If (Not gridState.viewDescriptors.ContainsKey(view.LevelName)) Then
					Return Nothing
				End If
				Dim state As New ViewState(gridState, CType(gridState.viewDescriptors(view.LevelName), ViewDescriptor))
				Return state
			End Function

			Private Shared Function Create(ByVal parent As ViewState, ByVal view As GridView) As ViewState
				If (Not parent.gridState.viewDescriptors.ContainsKey(view.LevelName)) Then
					Return Nothing
				End If
				Dim state As New ViewState(parent, CType(parent.gridState.viewDescriptors(view.LevelName), ViewDescriptor))
				Return state
			End Function

			Public Function IsLevel(ByVal level As String) As Boolean
				Return level = Me.descriptor.relationName
			End Function

			Public ReadOnly Property SaveExpList() As ArrayList
				Get
					If saveExpList_Renamed Is Nothing Then
						saveExpList_Renamed = New ArrayList()
					End If
					Return saveExpList_Renamed
				End Get
			End Property

			Public ReadOnly Property SaveSelList() As ArrayList
				Get
					If saveSelList_Renamed Is Nothing Then
						saveSelList_Renamed = New ArrayList()
					End If
					Return saveSelList_Renamed
				End Get
			End Property

			Public ReadOnly Property SaveMasterRowsList() As ArrayList
				Get
					If saveMasterRowsList_Renamed Is Nothing Then
						saveMasterRowsList_Renamed = New ArrayList()
					End If
					Return saveMasterRowsList_Renamed
				End Get
			End Property

			Public ReadOnly Property DetailViews() As Hashtable
				Get
					If detailViews_Renamed Is Nothing Then
						detailViews_Renamed = New Hashtable()
					End If
					Return detailViews_Renamed
				End Get
			End Property

			Public ReadOnly Property CellSelection() As Hashtable
				Get
					If cellSelection_Renamed Is Nothing Then
						cellSelection_Renamed = New Hashtable()
					End If
					Return cellSelection_Renamed
				End Get
			End Property

			Protected Function FindParentRowHandle(ByVal view As GridView, ByVal rowInfo As RowInfo, ByVal rowHandle As Integer) As Integer
				Dim result As Integer = view.GetParentRowHandle(rowHandle)
				Do While view.GetRowLevel(result) <> rowInfo.level
					result = view.GetParentRowHandle(result)
				Loop
				Return result
			End Function

			Protected Sub ExpandRowByRowInfo(ByVal view As GridView, ByVal rowInfo As RowInfo)
				Dim dataRowHandle As Integer = LocateRowByKeyValue(view, rowInfo.Id)
				If dataRowHandle <> GridControl.InvalidRowHandle Then
					Dim parentRowHandle As Integer = FindParentRowHandle(view, rowInfo, dataRowHandle)
					view.SetRowExpanded(parentRowHandle, True, False)
				End If
			End Sub

			Protected Function GetRowHandleToSelect(ByVal view As GridView, ByVal rowInfo As RowInfo) As Integer
				Dim dataRowHandle As Integer = LocateRowByKeyValue(view, rowInfo.Id)
				If dataRowHandle <> GridControl.InvalidRowHandle Then
					If view.GetRowLevel(dataRowHandle) <> rowInfo.level Then
						Return FindParentRowHandle(view, rowInfo, dataRowHandle)
					End If
				End If
				Return dataRowHandle
			End Function

			Protected Sub SelectRowByRowInfo(ByVal view As GridView, ByVal rowInfo As RowInfo, ByVal isFocused As Boolean)
				Dim rowHandle As Integer = GetRowHandleToSelect(view, rowInfo)
				If isFocused Then
					view.FocusedRowHandle = rowHandle
				Else
					If view.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CellSelect Then
						Dim names() As String = TryCast(CellSelection(rowInfo.Id), String())
						If names IsNot Nothing Then
							For j As Integer = 0 To names.Length - 1
								view.SelectCell(rowHandle, view.Columns(names(j)))
							Next j
						End If
					Else
						view.SelectRow(rowHandle)
					End If
				End If
			End Sub

			Public Sub SaveSelectionViewInfo(ByVal view As GridView)
				SaveSelList.Clear()
				CellSelection.Clear()
				Dim selectionArray() As Integer = view.GetSelectedRows()
				If selectionArray IsNot Nothing Then ' otherwise we have a single focused but not selected row
					For i As Integer = 0 To selectionArray.Length - 1
						Dim dataRowHandle As Integer = AddRowToSelection(view, selectionArray(i))
						If view.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CellSelect Then
							Dim columns() As GridColumn = view.GetSelectedCells(dataRowHandle)
							Dim names(columns.Length - 1) As String
							For j As Integer = 0 To columns.Length - 1
								names(j) = columns(j).FieldName
							Next j
							CellSelection(view.GetRowCellValue(dataRowHandle, descriptor.keyFieldName)) = names
						End If
					Next i
				End If
				AddRowToSelection(view, view.FocusedRowHandle)
			End Sub
			Private Function AddRowToSelection(ByVal view As GridView, ByVal handle As Integer) As Integer
				Dim rowInfo As RowInfo
				rowInfo.level = view.GetRowLevel(handle)
				If handle < 0 Then ' group row
					handle = view.GetDataRowHandleByGroupRowHandle(handle)
				End If
				rowInfo.Id = view.GetRowCellValue(handle, descriptor.keyFieldName)
				SaveSelList.Add(rowInfo)
				Return handle
			End Function

			Public Sub SaveExpansionViewInfo(ByVal view As GridView)
				If view.GroupedColumns.Count = 0 Then
					Return
				End If
				SaveExpList.Clear()
				For i As Integer = -1 To Integer.MinValue + 1 Step -1
					If (Not view.IsValidRowHandle(i)) Then
						Exit For
					End If
					If view.GetRowExpanded(i) Then
						Dim rowInfo As RowInfo
						Dim dataRowHandle As Integer = view.GetDataRowHandleByGroupRowHandle(i)
						rowInfo.Id = view.GetRowCellValue(dataRowHandle, descriptor.keyFieldName)
						rowInfo.level = view.GetRowLevel(i)
						SaveExpList.Add(rowInfo)
					End If
				Next i
			End Sub

			Public Sub SaveExpandedMasterRows(ByVal view As GridView)
				If view.GridControl.Views.Count = 1 Then
					Return
				End If
				SaveMasterRowsList.Clear()
				For i As Integer = 0 To view.DataRowCount - 1
					If view.GetMasterRowExpanded(i) Then
						Dim key As Object = view.GetRowCellValue(i, descriptor.keyFieldName)
						SaveMasterRowsList.Add(key)
						Dim detail As GridView = TryCast(view.GetVisibleDetailView(i), GridView)
						If detail IsNot Nothing Then
							Dim state As ViewState = ViewState.Create(Me, detail)
							If state IsNot Nothing Then
								DetailViews.Add(key, state)
								state.SaveState(detail)
							End If
						End If
					End If
				Next i
			End Sub

			Public Sub SaveVisibleIndex(ByVal view As GridView)
				visibleRowIndex = view.GetVisibleIndex(view.FocusedRowHandle) - view.TopRowIndex
			End Sub

			Public Sub LoadVisibleIndex(ByVal view As GridView)
				view.MakeRowVisible(view.FocusedRowHandle, True)
				view.TopRowIndex = view.GetVisibleIndex(view.FocusedRowHandle) - visibleRowIndex
			End Sub

			Public Sub LoadSelectionViewInfo(ByVal view As GridView)
				view.BeginSelection()
				Try
					view.ClearSelection()
					For i As Integer = 0 To SaveSelList.Count - 1
						SelectRowByRowInfo(view, CType(SaveSelList(i), RowInfo), i = SaveSelList.Count - 1)
					Next i
				Finally
					view.EndSelection()
				End Try
			End Sub

			Public Sub LoadExpansionViewInfo(ByVal view As GridView)
				If view.GroupedColumns.Count = 0 Then
					Return
				End If
				view.BeginUpdate()
				Try
					view.CollapseAllGroups()
					For Each info As RowInfo In SaveExpList
						ExpandRowByRowInfo(view, info)
					Next info
				Finally
					view.EndUpdate()
				End Try
			End Sub

			Public Sub LoadExpandedMasterRows(ByVal view As GridView)
				'view.BeginUpdate();
				Try
					view.CollapseAllDetails()
					For i As Integer = 0 To SaveMasterRowsList.Count - 1
						Dim rowHandle As Integer = LocateRowByKeyValue(view, SaveMasterRowsList(i))
						Dim state As ViewState = CType(DetailViews(SaveMasterRowsList(i)), ViewState)
						If state Is Nothing Then
							view.SetMasterRowExpanded(rowHandle, True)
						Else
							view.SetMasterRowExpandedEx(rowHandle, view.GetRelationIndex(rowHandle, state.descriptor.relationName), True)
							Dim detail As GridView = TryCast(view.GetVisibleDetailView(rowHandle), GridView)
							If detail IsNot Nothing Then
								state.LoadState(detail)
							End If
						End If
					Next i
				Finally
					'view.EndUpdate();
				End Try
			End Sub

			Private Function LocateRowByKeyValue(ByVal view As GridView, ByVal value As Object) As Integer
				If view.IsServerMode Then
					Return view.DataController.FindRowByValue(descriptor.keyFieldName, value)
				End If
				For i As Integer = 0 To view.DataRowCount - 1
					If Equals(value, view.GetRowCellValue(i, descriptor.keyFieldName)) Then
						Return i
					End If
				Next i
				Return GridControl.InvalidRowHandle
			End Function

			Public Sub SaveState(ByVal view As GridView)
				If ReferenceEquals(view.GridControl.FocusedView, view) Then
					gridState.focused = Me
				End If
				SaveExpandedMasterRows(view)
				SaveExpansionViewInfo(view)
				SaveSelectionViewInfo(view)
				SaveVisibleIndex(view)
				horzScrollPos = view.LeftCoord
			End Sub

			Public Sub LoadState(ByVal view As GridView)
				LoadExpandedMasterRows(view)
				LoadExpansionViewInfo(view)
				LoadSelectionViewInfo(view)
				LoadVisibleIndex(view)
				view.LeftCoord = horzScrollPos
				If ReferenceEquals(gridState.focused, Me) Then
					view.GridControl.FocusedView = view
				End If
			End Sub
		End Class

		Private viewDescriptors As Hashtable
		Private root As ViewState
		Private focused As ViewState

		Public Sub New(ByVal views() As ViewDescriptor)
			viewDescriptors = New Hashtable()
			For Each desc As ViewDescriptor In views
				viewDescriptors.Add(desc.relationName, desc)
			Next desc
		End Sub

		Public Sub SaveViewInfo(ByVal view As GridView)
			root = ViewState.Create(Me, view)
			root.SaveState(view)
		End Sub

		Public Sub LoadViewInfo(ByVal view As GridView)
			If root Is Nothing OrElse (Not root.IsLevel(view.LevelName)) Then
				Return
			End If
			root.LoadState(view)
		End Sub

	End Class
End Namespace