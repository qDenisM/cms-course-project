using UnityEngine;

/// <summary>
/// Кнопки Footer: таблица, запись, очистка.
/// </summary>
public class TableFooterActions : MonoBehaviour
{
    public ResultsTableVal resultsTable;
    public TableOpenClose tableView;
    public GameObject tableRoot;

    public void ShowTable()
    {
        if (tableView != null)
            tableView.OpenTable();
        else if (tableRoot != null)
            tableRoot.SetActive(true);
    }

    public void HideTable()
    {
        if (tableView != null)
            tableView.CloseTable();
        else if (tableRoot != null)
            tableRoot.SetActive(false);
    }

    public void ToggleTable()
    {
        if (tableRoot == null && tableView != null)
            tableRoot = tableView.tableRoot;

        if (tableRoot != null)
            tableRoot.SetActive(!tableRoot.activeSelf);
    }

    public void WriteToTable()
    {
        if (resultsTable != null)
            resultsTable.WriteValue();
    }

    public void ClearTable()
    {
        if (resultsTable != null)
            resultsTable.Clean();
    }
}
