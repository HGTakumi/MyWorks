using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IFighter, IAttacker
{
    public ThemeColor enemyColor;
    public int hp = 100;
    public int atk = 100;
    public float speed = 1f;
    public Transform targetPosition;

    protected SceneMain sceneMainInstance;
    protected PlayerManager playerManagerInstance;
    protected DataManager dataManagerInstance;

    private ActionRunOnce DieOnce;
    private ActionRunOnce EndGame;

    // Start is called before the first frame update
    void Start()
    {
        HP = hp;
        ATK = atk;
        SPD = speed;

        DieOnce = new ActionRunOnce(Die);
        EndGame = new ActionRunOnce(SendEnemyInfo);

        sceneMainInstance = GameObject.Find("/Scene Controller").GetComponent<SceneMain>();
        playerManagerInstance = GameObject.Find("/XR Player").GetComponent<PlayerManager>();
        dataManagerInstance = GameObject.Find("/Scene Controller").GetComponent<DataManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (HP == 0)
        {
            DieOnce.RunOnce();
        }
        else if (playerManagerInstance.HP == 0)
        {
            EndGame.RunOnce();
        }
        else
        {
            GoStraight();
        }
    }

    public void SetManagerInstance(SceneMain sceneMain, PlayerManager playerManager, DataManager dataManager)
    {
        sceneMainInstance = sceneMain;
        playerManagerInstance = playerManager;
        dataManagerInstance = dataManager;

        SpawnedTime = sceneMainInstance.ElapsedTime;
    }

    public void Damage(int damage)
    {
        //Debug.Log($"Now {gameObject.name}'s HP is {HP}.");
        //Debug.Log($"The damage is {damage}");

        HP -= damage;

        Debug.Log($"{gameObject.name}'s HP is {HP}.");

        HP = Utilities.StopZero(HP);

        GiveScore(damage);
    }

    public void Heal(int heal)
    {
        HP += heal;
    }

    public void SetHP(int hp)
    {
        HP = hp;
        //Debug.Log($"{gameObject.name}'s HP is {HP}.");
    }

    private void GoStraight()
    {
        Vector3 dir = targetPosition.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (distanceThisFrame >= dir.magnitude)
        {
            transform.position = targetPosition.position;
            Attack();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        transform.LookAt(targetPosition);
    }

    private void GiveScore(int score)
    {
        playerManagerInstance.PlusScore(score);
    }

    private void Die()
    {
        AliveTime = sceneMainInstance.ElapsedTime - SpawnedTime;
        SendEnemyInfo();

        Destroy(gameObject);
    }

    public void Attack()
    {
        playerManagerInstance.Damage(ATK);
    }

    private void SendEnemyInfo()
    {
        dataManagerInstance.SetEnemyInfo(enemyColor, SpawnedTime, AliveTime);
    }

    public int HP { get; protected set; }
    public int ATK { get; protected set; }
    public float SPD { get; protected set; }
    public float SpawnedTime { get; protected set; }
    public float? AliveTime { get; protected set; } = null;
}
