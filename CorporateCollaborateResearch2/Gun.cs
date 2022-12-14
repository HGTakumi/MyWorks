using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Gun : MonoBehaviour
{
    public ThemeColor gunColor;
    public GameObject bulletPrehab;
    public Transform muzzle;
    public float coolTime = 0.2f;
    public float triggerThreshold = 0.5f;
    public float force = 10f;

    public bool IsGrabbed { get; private set; } = false;
    public Hand GrabbingHand { get; private set; } = Hand.None;
    public float CoolTime { get; private set; } = 0f;

    [SerializeField] private InputManager inputManagerInstance;

    public bool InCoolTime { get; private set; } = false;
    private bool isReleasedTrigger = true;


    // Start is called before the first frame update
    void Start()
    {
        CheckColor();

        SetScripts();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsGrabbed && !InCoolTime)
        {
            if (CheckTriggered() && isReleasedTrigger)
            {
                Fire();

                isReleasedTrigger = false;
                InCoolTime = true;
                CoolTime = coolTime;
            }
        }

        if (InCoolTime)
        {
            UpdateCoolTime();
        }
    }

    private void CheckColor()
    {
        if ((bulletPrehab == null | !bulletPrehab.TryGetComponent<Bullet>(out Bullet burretScript)) || burretScript.bulletColor != gunColor)
        {
            Debug.LogError(gameObject.name + " is not input the bullet prefab properly, so this GameObject will be destroyed.");

            Destroy(gameObject);
        }
    }

    private void SetScripts()
    {
        inputManagerInstance = GameObject.Find("/XR Player/Input Manager").GetComponent<InputManager>();
        GetComponent<XRGrabInteractable>().interactionManager = GameObject.Find("/XR Player/XR Interaction Manager").GetComponent<XRInteractionManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "RightController")
        {
            SwitchRight();

            Debug.Log(gameObject.name + " is grabbed by right controller");
        }
        else if (other.tag == "LeftController")
        {
            SwichLeft();

            Debug.Log(gameObject.name + " is grabbed by left controller");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "RightController" || other.tag == "LeftController")
        {
            SwichNone();
        }
    }

    public void Activate()
    {
        IsGrabbed = true;

        Debug.Log(gameObject.name + " is activated");
    }

    public void Inactivate()
    {
        IsGrabbed = false;

        Debug.Log(gameObject.name + " is inactivated");
    }

    private void SwitchRight()
    {
        GrabbingHand = Hand.Right;
    }

    private void SwichLeft()
    {
        GrabbingHand = Hand.Left;
    }

    private void SwichNone()
    {
        GrabbingHand = Hand.None;
    }

    private void UpdateCoolTime()
    {
        CoolTime -= Time.deltaTime;

        if (CoolTime < 0)
        {
            CoolTime = 0f;

            InCoolTime = false;
        }
    }

    private bool CheckTriggered()
    {
        if (GrabbingHand == Hand.Right)
        {
            if (inputManagerInstance.RTrigger > triggerThreshold)
            {
                return true;
            }
            else
            {
                isReleasedTrigger = true;

                return false;
            }
        }
        else if (GrabbingHand == Hand.Left)
        {
            if (inputManagerInstance.LTrigger > triggerThreshold)
            {
                return true;
            }
            else
            {
                isReleasedTrigger = true;

                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public void Fire()
    {
        GameObject bulletInstance = Instantiate(bulletPrehab, muzzle.position, muzzle.rotation);
        bulletInstance.GetComponent<Bullet>().Shoot(force);
    }
}
