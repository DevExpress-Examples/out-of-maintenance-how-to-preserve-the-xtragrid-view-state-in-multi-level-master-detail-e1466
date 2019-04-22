using System;
using System.Collections;
using DevExpress.XtraGrid;
using DevExpress.Utils;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;

namespace DevExpress.XtraGrid.Helpers {

    public class GridControlState {

        public struct RowInfo {
            public object Id;
            public int level;
        }

        public struct ViewDescriptor {
            public string relationName;
            public string keyFieldName;

            public ViewDescriptor(string relationName, string keyFieldName) {
                this.relationName = relationName;
                this.keyFieldName = keyFieldName;
            }
        }

        public class ViewState {
            private GridControlState gridState;
            private ViewState parent;
            private ViewDescriptor descriptor;
            private ArrayList saveExpList;
            private ArrayList saveSelList;
            private ArrayList saveMasterRowsList;
            private int visibleRowIndex = -1;
            private Hashtable detailViews;
            private int horzScrollPos;
            private Hashtable cellSelection;

            protected ViewState(GridControlState gridState, ViewDescriptor descriptor) {
                this.gridState = gridState;
                this.descriptor = descriptor;
                this.parent = null;
            }

            protected ViewState(ViewState parent, ViewDescriptor descriptor) {
                this.parent = parent;
                this.gridState = parent.gridState;
                this.descriptor = descriptor;
            }

            public static ViewState Create(GridControlState gridState, GridView view) {
                if (!gridState.viewDescriptors.ContainsKey(view.LevelName)) return null;
                ViewState state = new ViewState(gridState, (ViewDescriptor)gridState.viewDescriptors[view.LevelName]);
                return state;
            }

            private static ViewState Create(ViewState parent, GridView view) {
                if (!parent.gridState.viewDescriptors.ContainsKey(view.LevelName)) return null;
                ViewState state = new ViewState(parent, (ViewDescriptor)parent.gridState.viewDescriptors[view.LevelName]);
                return state;
            }

            public bool IsLevel(string level) {
                return level == this.descriptor.relationName;
            }

            public ArrayList SaveExpList {
                get {
                    if (saveExpList == null)
                        saveExpList = new ArrayList();
                    return saveExpList;
                }
            }

            public ArrayList SaveSelList {
                get {
                    if (saveSelList == null)
                        saveSelList = new ArrayList();
                    return saveSelList;
                }
            }

            public ArrayList SaveMasterRowsList {
                get {
                    if (saveMasterRowsList == null)
                        saveMasterRowsList = new ArrayList();
                    return saveMasterRowsList;
                }
            }

            public Hashtable DetailViews {
                get {
                    if (detailViews == null)
                        detailViews = new Hashtable();
                    return detailViews;
                }
            }

            public Hashtable CellSelection {
                get {
                    if (cellSelection == null)
                        cellSelection = new Hashtable();
                    return cellSelection;
                }
            }

            protected int FindParentRowHandle(GridView view, RowInfo rowInfo, int rowHandle) {
                int result = view.GetParentRowHandle(rowHandle);
                while (view.GetRowLevel(result) != rowInfo.level)
                    result = view.GetParentRowHandle(result);
                return result;
            }

            protected void ExpandRowByRowInfo(GridView view, RowInfo rowInfo) {
                int dataRowHandle = LocateRowByKeyValue(view, rowInfo.Id);
                if (dataRowHandle != GridControl.InvalidRowHandle) {
                    int parentRowHandle = FindParentRowHandle(view, rowInfo, dataRowHandle);
                    view.SetRowExpanded(parentRowHandle, true, false);
                }
            }

            protected int GetRowHandleToSelect(GridView view, RowInfo rowInfo) {
                int dataRowHandle = LocateRowByKeyValue(view, rowInfo.Id);
                if (dataRowHandle != GridControl.InvalidRowHandle)
                    if (view.GetRowLevel(dataRowHandle) != rowInfo.level)
                        return FindParentRowHandle(view, rowInfo, dataRowHandle);
                return dataRowHandle;
            }

            protected void SelectRowByRowInfo(GridView view, RowInfo rowInfo, bool isFocused) {
                int rowHandle = GetRowHandleToSelect(view, rowInfo);
                if (isFocused)
                    view.FocusedRowHandle = rowHandle;
                else {
                    if (view.OptionsSelection.MultiSelectMode == GridMultiSelectMode.CellSelect) {
                        string[] names = CellSelection[rowInfo.Id] as string[];
                        if (names != null) {
                            for (int j = 0; j < names.Length; j++) {
                                view.SelectCell(rowHandle, view.Columns[names[j]]);
                            }
                        }
                    }
                    else {
                        view.SelectRow(rowHandle);
                    }
                }
            }

            public void SaveSelectionViewInfo(GridView view) {
                SaveSelList.Clear();
                CellSelection.Clear();
                int[] selectionArray = view.GetSelectedRows();
                if (selectionArray != null)  // otherwise we have a single focused but not selected row
                    for (int i = 0; i < selectionArray.Length; i++) {
                        int dataRowHandle = AddRowToSelection(view, selectionArray[i]);
                        if (view.OptionsSelection.MultiSelectMode == GridMultiSelectMode.CellSelect) {
                            GridColumn[] columns = view.GetSelectedCells(dataRowHandle);
                            string[] names = new string[columns.Length];
                            for (int j = 0; j < columns.Length; j++) {
                                names[j] = columns[j].FieldName;
                            }
                            CellSelection[view.GetRowCellValue(dataRowHandle, descriptor.keyFieldName)] = names;
                        }
                    }
                AddRowToSelection(view, view.FocusedRowHandle);
            }
            private int AddRowToSelection(GridView view, int handle) {
                RowInfo rowInfo;
                rowInfo.level = view.GetRowLevel(handle);
                if (handle < 0) // group row
                    handle = view.GetDataRowHandleByGroupRowHandle(handle);
                rowInfo.Id = view.GetRowCellValue(handle, descriptor.keyFieldName);
                SaveSelList.Add(rowInfo);
                return handle;
            }

            public void SaveExpansionViewInfo(GridView view) {
                if (view.GroupedColumns.Count == 0) return;
                SaveExpList.Clear();
                for (int i = -1; i > int.MinValue; i--) {
                    if (!view.IsValidRowHandle(i)) break;
                    if (view.GetRowExpanded(i)) {
                        RowInfo rowInfo;
                        int dataRowHandle = view.GetDataRowHandleByGroupRowHandle(i);
                        rowInfo.Id = view.GetRowCellValue(dataRowHandle, descriptor.keyFieldName);
                        rowInfo.level = view.GetRowLevel(i);
                        SaveExpList.Add(rowInfo);
                    }
                }
            }

            public void SaveExpandedMasterRows(GridView view) {
                if (view.GridControl.Views.Count == 1) return;
                SaveMasterRowsList.Clear();
                for (int i = 0; i < view.DataRowCount; i++)
                    if (view.GetMasterRowExpanded(i)) {
                        object key = view.GetRowCellValue(i, descriptor.keyFieldName);
                        SaveMasterRowsList.Add(key);
                        GridView detail = view.GetVisibleDetailView(i) as GridView;
                        if (detail != null) {
                            ViewState state = ViewState.Create(this, detail);
                            if (state != null) {
                                DetailViews.Add(key, state);
                                state.SaveState(detail);
                            }
                        }
                    }
            }

            public void SaveVisibleIndex(GridView view) {
                visibleRowIndex = view.GetVisibleIndex(view.FocusedRowHandle) - view.TopRowIndex;
            }

            public void LoadVisibleIndex(GridView view) {
                view.MakeRowVisible(view.FocusedRowHandle, true);
                view.TopRowIndex = view.GetVisibleIndex(view.FocusedRowHandle) - visibleRowIndex;
            }

            public void LoadSelectionViewInfo(GridView view) {
                view.DataController.EnsureFindRowByValueCache(view.DataController.Columns[descriptor.keyFieldName], view.RowCount);
                view.BeginSelection();
                try {
                    view.ClearSelection();
                    for (int i = 0; i < SaveSelList.Count; i++)
                        SelectRowByRowInfo(view, (RowInfo)SaveSelList[i], i == SaveSelList.Count - 1);
                }
                finally {
                    view.EndSelection();
                    view.DataController.DestroyFindRowByValueCache();
                }
            }

            public void LoadExpansionViewInfo(GridView view) {
                if (view.GroupedColumns.Count == 0) return;
                view.BeginUpdate();
                try {
                    view.CollapseAllGroups();
                    foreach (RowInfo info in SaveExpList)
                        ExpandRowByRowInfo(view, info);
                }
                finally {
                    view.EndUpdate();
                }
            }

            public void LoadExpandedMasterRows(GridView view) {
                //view.BeginUpdate();
                try {
                    view.CollapseAllDetails();
                    for (int i = 0; i < SaveMasterRowsList.Count; i++) {
                        int rowHandle = LocateRowByKeyValue(view, SaveMasterRowsList[i]);
                        ViewState state = (ViewState)DetailViews[SaveMasterRowsList[i]];
                        if (state == null) {
                            view.SetMasterRowExpanded(rowHandle, true);
                        }
                        else {
                            view.SetMasterRowExpandedEx(rowHandle, view.GetRelationIndex(rowHandle, state.descriptor.relationName), true);
                            GridView detail = view.GetVisibleDetailView(rowHandle) as GridView;
                            if (detail != null) {
                                state.LoadState(detail);
                            }
                        }
                    }
                }
                finally {
                    //view.EndUpdate();
                }
            }

            private int LocateRowByKeyValue(GridView view, object value) {
                if (view.IsServerMode) {
                    return view.DataController.FindRowByValue(descriptor.keyFieldName, value);
                }
                for (int i = 0; i < view.DataRowCount; i++) {
                    if (Equals(value, view.GetRowCellValue(i, descriptor.keyFieldName)))
                        return i;
                }
                return GridControl.InvalidRowHandle;
            }

            public void SaveState(GridView view) {
                if (ReferenceEquals(view.GridControl.FocusedView, view)) gridState.focused = this;
                SaveExpandedMasterRows(view);
                SaveExpansionViewInfo(view);
                SaveSelectionViewInfo(view);
                SaveVisibleIndex(view);
                horzScrollPos = view.LeftCoord;
            }

            public void LoadState(GridView view) {
                LoadExpandedMasterRows(view);
                LoadExpansionViewInfo(view);
                LoadSelectionViewInfo(view);
                LoadVisibleIndex(view);
                view.LeftCoord = horzScrollPos;
                if (ReferenceEquals(gridState.focused, this)) view.GridControl.FocusedView = view;
            }
        }

        private Hashtable viewDescriptors;
        private ViewState root;
        private ViewState focused;

        public GridControlState(ViewDescriptor[] views) {
            viewDescriptors = new Hashtable();
            foreach (ViewDescriptor desc in views) {
                viewDescriptors.Add(desc.relationName, desc);
            }
        }

        public void SaveViewInfo(GridView view) {
            root = ViewState.Create(this, view);
            root.SaveState(view);
        }

        public void LoadViewInfo(GridView view) {
            if (root == null || !root.IsLevel(view.LevelName)) return;
            root.LoadState(view);
        }

    }
}