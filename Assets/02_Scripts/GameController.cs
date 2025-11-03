using UnityEngine;
using UnityEngine.Events;

public enum GameState { Gameplay, Paused, Loading, Cutscene }
public class GameController : MonoBehaviour
{
    [Header("Refs")]
    public TimeManager timeManager;
    public ElectricityMachine electricityMachine;
    public PanelDay panelDay;

    public UnityEvent OnGameLoaded;
    public UnityEvent OnGamePaused;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

    }

    private void Start()
    {

    }

}
