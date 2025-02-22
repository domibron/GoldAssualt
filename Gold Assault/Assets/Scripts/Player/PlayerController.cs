using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interface.IInteractable;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    private List<INoiseAlert> noiseAlertSub;

    private CharacterController CC;

    private GameObject cam;

    [SerializeField] private float speed = 6f;

    [SerializeField] private float jumpHeight = 3f;

    //gravity
    private bool isGrounded = false;

    private Vector3 velocity;

    [SerializeField] private float gravity = -9.81f;

    // camera
    private float yRotation;
    private float xRotation;

    [SerializeField] private float sensitivity = 1f;

    [SerializeField] private GameObject groundCheck;

    // this is for rappelling.  
    public GameObject rappellingObject = null;
    public bool canRappel = false;
    public Vector2 lowerLimit;
    public Vector2 upperLimit;
    public GameObject targetLandingArea;
    public bool atWindow = false;
    private bool isRappelling = false;
    private bool waiting = false;


    // [SerializeField] private GameObject rappelInteraction;
    // [SerializeField] private Image rappelImage;

    private PlayerInteractionText playerInteractionText;

    private DisplayText rappelDisplayText;

    private float currentTime = 0f;


    //leaning stuff
    public GameObject LeanPoint;
    public float leanAmount = 30f;
    public float currentLean;
    private float leanTime = 0f;
    private float savedLeanStore = 0f;
    private float otherLeanStore = 0f;
    private float deviationCorrection = 0f;
    private float reduction = 1f;
    private bool leftLean = false;
    private bool rightLean = false;

    public Animator playerBodyAnimator;

    // * for the inventory so this knows what is equiped.
    public GameObject[] L_inventory = new GameObject[5];

    public TMP_Text enemiesLeftText;

    // Start is called before the first frame update
    void Start()
    {
        CC = GetComponent<CharacterController>();
        cam = transform.Find("Lean Point").Find("Camera Holder").gameObject;
        print(cam.name);
        LeanPoint = transform.Find("Lean Point").gameObject;

        Cursor.lockState = CursorLockMode.Locked;

        // rappelInteraction.SetActive(false);
        playerInteractionText = GetComponent<PlayerInteractionText>();

        rappelDisplayText = new DisplayText();

        rappelDisplayText.text = "Hold <color=blue>F</color> to <color=blue>Rappel</color>";
        rappelDisplayText.priority = 2;

        // noiseAlertSub = new List<INoiseAlert>(FindObjectsOfType<Object>().OfType<INoiseAlert>());
        PlayerRefernceItems.current.AINoiseAlertSubs = new List<GameObject>(GameObject.FindGameObjectsWithTag("AI"));

        sensitivity = SaveData.current.sensitivity;

        SaveManager.current.onSave += OnGameSave;
    }

    private void OnGameSave()
    {
        sensitivity = SaveData.current.sensitivity;
    }

    // Update is called once per frame
    void Update()
    {
        // if (currentHealth <= 0)
        // {
        //     SceneManager.LoadScene(SceneManager.GetSceneAt(0).buildIndex);
        // }

        // float ans = currentHealth / maxHealth;
        // ans = 1 - ans;
        // ans /= 2;
        // //ans = 127.5f * ans;
        // HurtImage.color = new Color(255, 0, 0, ans);

        // ^^^^^^ move into function;

        // print(ans);





        // ! TERRIBLE DO NOT USE THE NOISE ALERTS, CREATE ANOTHER ARRAY OF BOOLS OR SOMTHING
        if (PlayerRefernceItems.current.AINoiseAlertSubs.Count <= 0)
        {
            GetComponent<PauseMenu>().GameEnd();
        }
        else
        {
            enemiesLeftText.text = $"Enemies Left\n[{PlayerRefernceItems.current.AINoiseAlertSubs.Count}]";
        }










        // =============================================================== leaning =====================================================================
        // yes, we are in update.

        // lerping. input a, input b and value between 0 and 1.
        // if the value is closer to 0 then a is more so if a is 10 and b is 20 and value is 0.2 then it will give 12.


        float _increaser = 3f;

        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E))
        {
            // currentLean = 0f;
            leanTime = 0f;

            if ((currentLean > 0 && rightLean) || (currentLean < 0 && leftLean))
            {
                float _temp = Mathf.Abs(currentLean) / leanAmount; // gets the decimal percentage.
                _temp = 1 - _temp; // invert decimal percentage. tip, -= is the same as var - value not value - var.
                _temp += 0.5f; // add 0.5 for reduction.

                reduction = _temp;
            }



            // savedLeanStore = 0f;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            leanTime += Time.deltaTime * _increaser * reduction;
            currentLean = Mathf.Lerp(savedLeanStore, leanAmount, leanTime);

            savedLeanStore = currentLean;
            leftLean = true;
        }
        else
        {
            leftLean = false;
        }

        if (Input.GetKey(KeyCode.E))
        {
            leanTime += Time.deltaTime * _increaser * reduction;
            currentLean = Mathf.Lerp(savedLeanStore, -leanAmount, leanTime);

            savedLeanStore = currentLean;
            rightLean = true;
        }
        else
        {
            rightLean = false;
        }

        if (leanTime > 0 && !rightLean && !leftLean)
        {
            if (leanTime > 1) leanTime = 1f;
            if (reduction != 1) reduction = 1f;

            leanTime -= Time.deltaTime * _increaser * reduction * 2f;
            currentLean = Mathf.Lerp(0, savedLeanStore, leanTime);


            // otherLeanStore = currentLean;
        }
        else if (!rightLean && !leftLean)
        {
            currentLean = 0f;
            leanTime = 0f;
            reduction = 1f;

            savedLeanStore = 0f;
        }

        LeanPoint.transform.localRotation = Quaternion.Euler(0, 0, currentLean);



        // ========================================================== end of leaning ==============================================================

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // remove
        if (Input.GetKeyDown(KeyCode.L))
        {
            MadeSomeNoise();
        }

        //move /= speedNurf;

        if (canRappel && !isRappelling)
        {
            // rappelInteraction.SetActive(true);



            // rappelImage.fillAmount = currentTime;
            //do something to indicate to player.
            if (!playerInteractionText.IsInTheList(rappelDisplayText))
            {
                playerInteractionText.AddInteractionText(rappelDisplayText);
            }

            if (Input.GetKey(KeyCode.F))
            {
                //print(currentTime);
                currentTime += Time.deltaTime;
                if (currentTime >= 1f)
                {
                    isRappelling = true;
                    currentTime = 0f;

                    Transform parentT = rappellingObject.transform.parent.transform;
                    // float rot = 0;

                    // if (parentT.eulerAngles.y < 90)
                    // {
                    //     rot = 90 - parentT.eulerAngles.y;
                    //     rot = 360 - rot;
                    // }
                    // else
                    // {
                    //     rot = parentT.eulerAngles.y - 90;
                    // }

                    // TODO Please rotate the camera.

                    transform.rotation = Quaternion.Euler(-90, parentT.eulerAngles.y, 0);
                    // GetComponent<Animator>().SetBool("rappelling", true);
                    //cam.transform.localRotation = Quaternion.Euler(0, rot, 0);

                    transform.SetParent(rappellingObject.transform);

                    if (transform.localPosition.y > upperLimit.y)
                    {
                        ChangeLocalPositionController(new Vector3(transform.localPosition.x, upperLimit.y, rappellingObject.transform.localPosition.z - (CC.height / 4)));
                        transform.rotation = Quaternion.Euler(transform.eulerAngles.x + 180, transform.eulerAngles.y, transform.eulerAngles.z + 180);
                    }
                    else
                    {
                        ChangeLocalPositionController(new Vector3(transform.localPosition.x, transform.localPosition.y, rappellingObject.transform.localPosition.z - (CC.height / 4)));
                    }

                    //ChangePositionController(new Vector3(transform.position.x, transform.position.y, rappellingObject.transform.position.z));

                    canRappel = false;

                }
            }
            else
            {
                currentTime = 0f;
            }
        }
        else if (!canRappel && !isRappelling)
        {
            // rappelInteraction.SetActive(false);
            if (playerInteractionText.IsInTheList(rappelDisplayText))
            {
                playerInteractionText.RemoveInteractionText(rappelDisplayText);
            }

        }


        if (!isRappelling)
        {
            Vector3 move = transform.right * x + transform.forward * z;

            // this is to check if the player is holding shift bit not left control.
            if (Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.LeftControl))
                CC.Move(move.normalized * 2 * speed * Time.smoothDeltaTime);

            // this is to check if the player is not holding both keys.
            else if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.LeftControl))
                CC.Move(move.normalized * speed * Time.smoothDeltaTime);

            // this is to check if the player is just holding left control.
            else if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl))
                CC.Move(move.normalized / 2 * speed * Time.smoothDeltaTime);

            //this is to check if the player is holding both keys.
            else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl))
                CC.Move(move.normalized / 1.5f * speed * Time.smoothDeltaTime);

            // this is to catch and unpinplimented checks.
            else
                Debug.LogError("character vars not set");


            playerBodyAnimator.SetFloat("X", move.x);
            playerBodyAnimator.SetFloat("Z", move.z);

            if (Input.GetKey(KeyCode.LeftControl)) // REPLACE WITH TIME OTHERWISE ISSUES WILL OCCUR - the lerps
                CC.height = Mathf.Lerp(CC.height, 1f, CC.height * 0.2f);
            else
                CC.height = Mathf.Lerp(CC.height, 2f, CC.height * 0.2f);

            // player jump
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // gravity
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
            else
            {
                velocity.y += gravity * Time.deltaTime;
            }

        }
        else if (isRappelling)
        {
            Vector3 move = Vector3.zero;
            Transform RepelT = rappellingObject.transform;

            Vector3 move2 = Vector3.zero;

            // used to get the future posistion.
            move = transform.right * x + transform.forward * z;
            move2 = move.normalized * speed * Time.smoothDeltaTime;

            Vector3 predicted = transform.position;
            predicted = RepelT.InverseTransformPoint(predicted + move2);

            if (predicted.x <= upperLimit.x && predicted.y <= upperLimit.y && predicted.x >= lowerLimit.x && predicted.y >= lowerLimit.y)
            {
                CC.Move(move2);

            }


            if (Input.GetKeyDown(KeyCode.C))
            {
                transform.rotation = Quaternion.Euler(transform.eulerAngles.x + 180, transform.eulerAngles.y, transform.eulerAngles.z + 180);

            }

            if (atWindow)
            {
                print("SAPCE!!!");
            }

            if (atWindow && Input.GetKeyDown(KeyCode.Space))
            {
                transform.rotation = Quaternion.Euler(0, rappellingObject.transform.eulerAngles.y, 0);
                cam.transform.localRotation = Quaternion.Euler(0, 0, 0);

                ChangeLocalPositionController(targetLandingArea.transform.position);


                transform.SetParent(null);
                isRappelling = false;
            }

            velocity.y = 0f;

            if (!playerInteractionText.IsInTheList(rappelDisplayText))
            {
                rappelDisplayText.text = "Hold <color=blue>F</color> to <color=blue>Stop rappelling</color>";
                playerInteractionText.AddInteractionText(rappelDisplayText);
            }

            // rappelImage.fillAmount = currentTime;
            if (Input.GetKey(KeyCode.F))
            {
                //print(currentTime);
                currentTime += Time.deltaTime;
                if (currentTime >= 1f)
                {
                    if (transform.localPosition.y >= upperLimit.y - 2 || waiting)
                    {
                        waiting = true;
                        DismountAtRoof();

                        //ChangeLocalPositionController(new Vector3(transform.localPosition.x, upperLimit.y + (CC.height - 1), 2));
                    }
                    else if (isRappelling && !waiting)
                    {
                        // GetComponent<Animator>().SetBool("rappelling", false);

                        currentTime = 0f;
                        transform.rotation = Quaternion.Euler(0, rappellingObject.transform.eulerAngles.y, 0);
                        cam.transform.localRotation = Quaternion.Euler(0, 0, 0);


                        //print(transform.localPosition.y >= upperLimit.y - 2);
                        //print(transform.localPosition.y + " " + upperLimit.y);


                        transform.SetParent(null);
                        isRappelling = false;

                    }


                }
            }
            else
            {
                currentTime = 0f;
            }

        }



        CC.Move(velocity * Time.deltaTime);



        //Camera
        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity;

        if (Time.timeScale == 1)
        {
            if (!isRappelling) // put in the uper if statement
            {
                yRotation -= mouseY;
                yRotation = Mathf.Clamp(yRotation, -90f, 90f);


                transform.Rotate(Vector3.up * mouseX);
                cam.transform.localRotation = Quaternion.Euler(yRotation, 0, 0);
            }
            else if (isRappelling)
            {
                yRotation -= mouseY;
                yRotation = Mathf.Clamp(yRotation, -90f, 90f);


                xRotation += mouseX;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);

                cam.transform.localRotation = Quaternion.Euler(yRotation, xRotation, 0);
            }
        }

        // ground check
        isGrounded = CC.isGrounded;




        // Transform parentT = rappellingObject.transform.parent.transform;
        // float minRot = 0;
        // float maxRot = 0;

        // if (parentT.eulerAngles.y > 90) // <
        // {
        //     float temp;
        //     temp = 90 - parentT.eulerAngles.y;
        //     minRot = 360 - temp;
        //     maxRot = parentT.eulerAngles.y - 90;
        // }
        // else if (parentT.eulerAngles.y < 90) // >
        // {
        //     float temp;
        //     temp = 360 - parentT.eulerAngles.y;
        //     maxRot = 90 - temp;
        //     minRot = parentT.eulerAngles.y + 90;
        // }
        // else
        // {
        //     maxRot = parentT.eulerAngles.y + 90;
        //     minRot = parentT.eulerAngles.y - 90;
        // }




        // Interactions

        RaycastHit Ihit;
        bool wasHit = Physics.Raycast(cam.transform.position, cam.transform.forward, out Ihit, 2.2f);

        if (wasHit && Ihit.collider.gameObject.GetComponent<IInteractable>() != null)
        {
            Ihit.collider.gameObject.GetComponent<IInteractable>().lookingAt();
            if (Input.GetKeyDown(KeyCode.F))
            {
                Ihit.collider.gameObject.GetComponent<IInteractable>().RunInteract();
            }
        }
    }

    public void DismountAtRoof()
    {
        StartCoroutine(GetOffAtRoofRappel());
    }

    IEnumerator GetOffAtRoofRappel()
    {
        Vector3 targPos = new Vector3(transform.localPosition.x, upperLimit.y + (CC.height / 2), rappellingObject.transform.localPosition.z + 2);
        //targPos = transform.TransformPoint(targPos);
        //print(new Vector3(transform.localPosition.x, upperLimit.y + CC.height, 2));
        CC.enabled = false;
        yield return null;
        transform.localPosition = targPos;
        targPos = transform.position;
        yield return null;
        CC.enabled = true;

        //Debug.LogAssertion("wait");
        yield return null;

        currentTime = 0f;
        transform.rotation = Quaternion.Euler(0, rappellingObject.transform.eulerAngles.y, 0);
        cam.transform.localRotation = Quaternion.Euler(0, 0, 0);

        print(transform.localPosition.y >= upperLimit.y - 2);
        print(transform.localPosition.y + " " + upperLimit.y);

        yield return null;

        while (transform.localPosition != targPos)
        {
            CC.enabled = false;
            transform.localPosition = targPos;
            yield return null;
        }

        CC.enabled = true;

        transform.SetParent(null);

        waiting = false;

        isRappelling = false;

    }

    public void SetGrounded(bool _isGrounded)
    {
        isGrounded = _isGrounded;
    }

    private void MadeSomeNoise()
    {
        foreach (GameObject go in PlayerRefernceItems.current.AINoiseAlertSubs)
        {
            go.GetComponent<INoiseAlert>().NoiseMade(transform.position);
        }
    }






    // !========================================================================   no   =========================================================================================


    public void ChangePositionController(GameObject targetObj)
    {
        StartCoroutine(ChangePos(targetObj));
        // print("A");
    }

    public void ChangePositionController(Transform targetObj)
    {
        StartCoroutine(ChangePos(targetObj));
        // print("B");
    }

    public void ChangePositionController(Vector3 targetObj)
    {
        StartCoroutine(ChangePos(targetObj));
        // print("C");
    }

    IEnumerator ChangePos(GameObject targetObj)
    {
        CC.enabled = false;
        yield return null;
        transform.position = targetObj.transform.position;
        yield return null;
        CC.enabled = true;
        // print("a");
    }

    IEnumerator ChangePos(Transform targetObj)
    {
        CC.enabled = false;
        yield return null;
        transform.position = targetObj.position;
        yield return null;
        CC.enabled = true;
        // print("b");
    }

    IEnumerator ChangePos(Vector3 targetObj)
    {
        CC.enabled = false;
        yield return null;
        transform.position = targetObj;
        yield return null;
        CC.enabled = true;
        // print("c");
    }

    public void ChangeLocalPositionController(Vector3 targetObj)
    {
        StartCoroutine(ChangeLocalPos(targetObj));
        // print("D");
    }

    IEnumerator ChangeLocalPos(Vector3 targetObj)
    {
        CC.enabled = false;
        yield return null;
        transform.localPosition = targetObj;
        yield return null;
        CC.enabled = true;
        // print("d");
    }
}
