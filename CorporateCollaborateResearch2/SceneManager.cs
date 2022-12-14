using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Runtime.InteropServices;

public class SceneManager : MonoBehaviour
{
    [Header("Script")]
    [SerializeField] private SceneMain sceneMainInstance;
    [SerializeField] private PlayerManager playerManagerInstance;
    [SerializeField] private InputManager inputManagerInstance;
    [SerializeField] private DataManager dataManagerInstance;
    [SerializeField] private Tutorial tutorialInstance;

    [Header("Canvas")]
    [SerializeField] private GameObject menuCanvas;

    [Header("GameObject")]
    [SerializeField] private List<GameObject> Guns = new List<GameObject>(3);
    [SerializeField] private List<GameObject> Stands = new List<GameObject>(3);

    private bool hasTutorialFinished = false;
    private bool isGameOver = false;

    private TMP_Text titleText, mainText, previousText, nextText;
    private TMP_Text centerText;

    private ActionRunOnce EndGame = new ActionRunOnce(() => Debug.Log("Game Over !!!"));
    private ActionRunOnce FinishTutorial;

    // Start is called before the first frame update
    void Start()
    {
        SetTexts();
        EndGame.OnActionOnce += GameOver;
        FinishTutorial = new ActionRunOnce(sceneMainInstance.FinishTutorial);

        InitTexts();

        StartCoroutine(StartGameCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        if (hasTutorialFinished)
        {
            FinishTutorial.RunOnce();
        }

        isGameOver = CheckGameOver();

        if (isGameOver)
        {
            EndGame.RunOnce();
        }
    }

    private bool CheckGameOver()
    {
        if (sceneMainInstance.TimeLimit == 0 || playerManagerInstance.HP == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void GameOver()
    {
        SetCenterTextSize(0.25f);
        SetMenuTexts(center: "Game Over !!");
        ActiveMenuCanvas();

        dataManagerInstance.RunWriting();
    }

    private void SetTexts()
    {
        titleText = GameObject.Find($"/{menuCanvas.name}/Text Image/Title Text").GetComponent<TMP_Text>();
        mainText = GameObject.Find($"/{menuCanvas.name}/Text Image/Main Text").GetComponent<TMP_Text>();
        previousText = GameObject.Find($"/{menuCanvas.name}/Text Image/Previous Text").GetComponent<TMP_Text>();
        nextText = GameObject.Find($"/{menuCanvas.name}/Text Image/Next Text").GetComponent<TMP_Text>();
        centerText = GameObject.Find($"/{menuCanvas.name}/Text Image/Center Text").GetComponent<TMP_Text>();
    }

    private GameObject GetChildFromName(GameObject parent, string childName)
    {
        Transform children = parent.GetComponentInChildren<Transform>();

        if (children.childCount == 0)
        {
            return null;
        }

        foreach (Transform child in children)
        {
            if (child.name == childName)
            {
                return child.gameObject;
            }
        }

        return null;
    }

    private void InitTexts()
    {
        SetMenuTexts("FPS GAME", "Welcome to FPS GAME !\nLet's play the game\nto press (A) button !", "<< Exit (X)", "Start (A) >>");
    }

    public void SetMenuTexts(string title = "", string main = "", string previous = "", string next = "", string center = "")
    {
        titleText.text = title;
        mainText.text = main;
        previousText.text = previous;
        nextText.text = next;
        centerText.text = center;
    }

    public void SetCenterTextSize(float size)
    {
        centerText.fontSize = size;
    }

    public IEnumerator SwitchMenuTexts(string title = "", string main = "", string previous = "", string next = "", string center = "")
    {
        SetMenuTexts();
        yield return new WaitForSeconds(2f);

        SetMenuTexts(title, main, previous, next, center);
        yield return new WaitForSeconds(2f);
    }

    public IEnumerator StartGameCoroutine()
    {
        yield return StartCoroutine(inputManagerInstance.WaitForBoolInput(ActivationOculusRiftSInputs.APress, true));

        if (dataManagerInstance.HasPlayed)
        {
            yield return StartCoroutine(SwitchMenuTexts("Hello, again !", "You have already played\nthis game, right ?\n\nSo, let's play the game !"));
            yield return new WaitForSeconds(3f);

            SetMenuTexts();
        }
        else
        {
            yield return StartCoroutine(SwitchMenuTexts($"Hello, {PlayerProfile.instance.PlayerName} !", "You play this game\nfor the first time, right?\nSo, let's do\na turorial first !"));

            yield return StartCoroutine(tutorialInstance.RunTutorial());
        }

        yield return new WaitForSeconds(3f);

        hasTutorialFinished = true;
    }

    public void SpawnGameSet()
    {
        Instantiate(Stands[0], new Vector3(-0.4f, 0.7f, 1.0f), Quaternion.identity);
        Instantiate(Stands[1], new Vector3(0f, 0.7f, 1.0f), Quaternion.identity);
        Instantiate(Stands[2], new Vector3(0.4f, 0.7f, 1.0f), Quaternion.identity);

        Instantiate(Guns[0], new Vector3(-0.4f, 1.0f, 0.92f), Quaternion.identity);
        Instantiate(Guns[1], new Vector3(0f, 1.0f, 0.92f), Quaternion.identity);
        Instantiate(Guns[2], new Vector3(0.4f, 1.0f, 0.92f), Quaternion.identity);
    }

    public void HideMenuCanvas()
    {
        menuCanvas.SetActive(false);
    }

    public void ActiveMenuCanvas()
    {
        menuCanvas.SetActive(true);
    }
}
