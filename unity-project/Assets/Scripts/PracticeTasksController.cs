using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>Подсказки по заданиям ПРАКТИКА в нижнее текстовое поле.</summary>
public class PracticeTasksController : MonoBehaviour, IPointerEnterHandler
{
    public Text practiceHintText;
    public InstallationUIHub uiHub;
    public SteamIntensityController intensity;
    public SteamEngineSimulator engine;
    public PracticeModeUI practiceMode;

    [TextArea(2, 4)]
    public string task1 = "Задание 1. Ознакомьтесь с элементами установки в разделе УСТАНОВКА.";
    [TextArea(2, 4)]
    public string task2 = "Задание 2. Выберите интенсивность пара: 25%, 50% или 100%.";
    [TextArea(2, 4)]
    public string task3 = "Задание 3. Включите рычаг подачи пара и дождитесь устойчивого вращения маховика.";
    [TextArea(2, 4)]
    public string task4 = "Задание 4. Запишите в таблицу КПД, скорость маховика (°/с) и интенсивность пара.";
    [TextArea(2, 4)]
    public string task5 = "Задание 5. Повторите опыт при другой интенсивности и снова запишите строку.";
    [TextArea(2, 4)]
    public string task6 = "Задание 6. Сравните скорости маховика при 25%, 50% и 100% пара.";
    [TextArea(2, 4)]
    public string task7 = "Задание 7. Очистите таблицу при необходимости и завершите работу.";

    void Show(string msg)
    {
        if (practiceHintText != null)
            practiceHintText.text = msg;
        if (uiHub != null)
            uiHub.SetBottomMessage(msg);
    }

    public void OnTask1()
    {
        if (practiceMode != null)
            practiceMode.EnterPracticeMode();
        Show(task1);
    }
    public void OnTask2() { Show(task2); }
    public void OnTask3() { Show(task3); }
    public void OnTask4() { Show(task4); }
    public void OnTask5() { Show(task5); }
    public void OnTask6() { Show(task6); }
    public void OnTask7() { Show(task7); }

    public void OnPointerEnter(PointerEventData eventData) { }

    public void RunTaskActions(int index)
    {
        switch (index)
        {
            case 2:
                break;
            case 3:
                if (intensity != null && intensity.IntensityPercent == 0)
                    Show("Сначала выберите интенсивность пара.");
                break;
        }
    }
}
