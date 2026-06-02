using UnityEngine;

/// <summary>Кнопка задания: OnClick -> OnClickTask.</summary>
public class PracticeTaskButton : MonoBehaviour
{
    public int taskIndex = 1;
    public PracticeTasksController tasks;

    public void OnClickTask()
    {
        if (tasks == null)
            return;
        switch (taskIndex)
        {
            case 1: tasks.OnTask1(); break;
            case 2: tasks.OnTask2(); break;
            case 3: tasks.OnTask3(); break;
            case 4: tasks.OnTask4(); break;
            case 5: tasks.OnTask5(); break;
            case 6: tasks.OnTask6(); break;
            case 7: tasks.OnTask7(); break;
        }
        tasks.RunTaskActions(taskIndex);
    }
}
