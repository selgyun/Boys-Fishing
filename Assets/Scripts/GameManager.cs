using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct PlayerSetting{
    public GameObject playerObject;
}

public enum Daytime{
    Day,
    Night
}

[System.Serializable]
public struct StageSetting{
    public int HPPerSecond;
    public float dayDuration;
    public float nightDuration;
    public Daytime startDaytime;
}

[System.Serializable]
public struct UISetting{
    public GameObject marketObject;
    public GameObject storeObject;
}

public class GameManager : MonoBehaviour
{
    public PlayerSetting playerSetting;
    public StageSetting stageSetting;
    public UISetting uiSetting;
    public List<FishInfo> fishInfo;

    //상수들
    private const int OBJECT_NULL = 0;
    private const int OBJECT_STORE = 1;
    private const int OBJECT_MARKET = 2;
    private const int OBJECT_PORTAL = 3;
    private const int OBJECT_OBSTACLE = 4;
    public const string TAG_TERRAIN = "Terrain";
    public const string TAG_MARKET = "Market";
    public const string TAG_PORTAL = "Portal";
    public const string TAG_STORE = "Store";

    // 배의 속도
    private const float playerSpeed = 30.0f;
    // 배위 회전 속도
    private const float rotateSpeed = 180.0f;
    // 카메라 줌 인/아웃 속도
    private const float cameraSpeed = 10.0f;
    // 카메라 최대 줌인
    private const float minFOV = 40.0f;
    // 카메라 최대 줌아웃
    private const float maxFOV = 100.0f;

    // 시간 관련
    private Daytime daytime;
    private float timeFlow;

    private bool isOpenUI = false;

    // Start is called before the first frame update
    void Start()
    {
        timeFlow = 0;
        daytime = stageSetting.startDaytime;
        Fish.FishList = fishInfo;
        //RandomGenerateTest();
    }

    // Update is called once per frame
    void Update()
    {
        PlayerAction();
        CameraAction();
        TimeAction();
    }

    void RandomGenerateTest()
    {
        Dictionary<string, int> num = new Dictionary<string, int>();
        Dictionary<string, float> size = new Dictionary<string, float>();
        Dictionary<string, float> price = new Dictionary<string, float>();

        foreach(FishInfo fish in Fish.FishList)
        {
            num.Add(fish.name, 0);
            size.Add(fish.name, 0);
            price.Add(fish.name, 0);
        }

        for(int i = 0; i < 10000; i++)
        {
            Fish fish = Fish.RandomGenerate(3);

            num[fish.Name]++;
            size[fish.Name] += fish.Size;
            price[fish.Name] += fish.Price;
        }

        foreach(FishInfo fish in Fish.FishList)
        {
            size[fish.name] /= num[fish.name];
            price[fish.name] /= num[fish.name];
            Debug.Log("Name: " + fish.name + ", Num: " + num[fish.name] + ", Size: " + size[fish.name] + " , Price: " + price[fish.name]);
        }

    }

    void PlayerAction()
    {
        GameObject playerObject = playerSetting.playerObject;
        if(playerObject != null)
        {
            MovePlayer(playerObject);
            PlayerBehavior pb = playerSetting.playerObject.GetComponent<PlayerBehavior>();
            if(pb)
            {
                if(!isOpenUI)
                {
                    isOpenUI = true;
                    if(pb.IsMarket)
                    {
                        OpenMarket();
                    }
                    else if(pb.IsStore)
                    {
                        
                        OpenStore();
                    }
                    else if(pb.IsPortal)
                    {
                        ChangeStage();
                    }
                    else
                    {
                        isOpenUI = false;
                    }
                }
                else
                {
                    if(!pb.IsMarket && !pb.IsStore && !pb.IsPortal)
                    {
                        isOpenUI = false;
                    }
                }
            }
            else
            {
                Debug.Log("Player Behavior 를 찾을 수 없습니다");
            }
        }
    }

    void CameraAction()
    {
        float scrollSpeed = Input.GetAxis("Mouse ScrollWheel") * cameraSpeed * (-1);

        Camera.main.fieldOfView += scrollSpeed;
        if(Camera.main.fieldOfView > maxFOV)
        {
            Camera.main.fieldOfView = maxFOV;
        }
        else if(Camera.main.fieldOfView < minFOV)
        {
            Camera.main.fieldOfView = minFOV;
        }
    }

    void TimeAction()
    {
        timeFlow += Time.deltaTime;
        if(daytime == Daytime.Day)
        {
            if(timeFlow >= stageSetting.dayDuration)
            {
                daytime = Daytime.Night;
                timeFlow -= stageSetting.dayDuration;
            }
        }
        // if(daytime == Daytime.Night)
        else
        {
            if(timeFlow >= stageSetting.nightDuration)
            {
                daytime = Daytime.Day;
                timeFlow -= stageSetting.nightDuration;
            }
        }
        ((Ship)Player.Instance.Equip[Etype.Ship]).WearOut(stageSetting.HPPerSecond * Time.deltaTime);
    }

    void MovePlayer(GameObject target)
    {
        Vector3 deltaPosition = new Vector3(0, 0, 0);
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        float speed = playerSpeed * Time.deltaTime;

        if(horizontal != 0 || vertical != 0)
        {
            Vector3 dir = new Vector3(horizontal, 0, vertical).normalized;
            deltaPosition = dir * speed;

            if(!Physics.Raycast(target.transform.position, dir, 1f)){
                target.transform.position += deltaPosition;
                Camera.main.transform.position += deltaPosition;
            }

            float prevAngle = target.transform.eulerAngles.y;
            float angle = Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);
            float deltaAngle = Mathf.Min(posMod(angle - prevAngle, 360), rotateSpeed * Time.deltaTime);

            if(posMod(angle - prevAngle, 360) < posMod(prevAngle - angle, 360))
            {
                angle = posMod(prevAngle + deltaAngle, 360);
            }
            else
            {
                angle = posMod(prevAngle - deltaAngle, 360);
            }

            target.transform.eulerAngles = new Vector3(0, angle, 0);
        }
    }

    private float posMod(float num, float mod)
    {
        num = num % mod;
        while(num < 0)
        {
            num += mod;
        }
        return num;
    }

    int CollisionTest(GameObject target)
    {
        // 충돌 체크해서 뭐랑 충돌했는지 반환
        return 1;
    }

    void OpenMarket()
    {
        uiSetting.marketObject.SetActive(true);
    }

    void OpenStore()
    {
        uiSetting.storeObject.SetActive(true);
    }

    void OpenInventory()
    {
        // 인벤토리 열기
    }

    void OpenMenu()
    {
        // 메뉴 열기
    }

    void StartFishing()
    {
        // 낚시 시작
    }

    void ChangeStage()
    {
        // 다음 맵으로 장면 전환
        Debug.Log("다음맵으로 전환");
    }
}
