using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using System.Linq;
using Unity.Android.Gradle;
using UnityEngine.Rendering.Universal;
using Unity.VisualScripting;

public class Game : MonoBehaviour
{
    public enum HeartColor
    {
        Yellow,
        Green,
        Black,
        Red,
        Blue,
    }

    public class GameData
    {
        public Dictionary<HeartColor, int> heartColorAmount;
        public int credit;
        public int experience;
        public int level;
        public int score;
        public float payoutMultiplier;
        public int earning;
        public float totalTimeLeft;

        public int yellowHeartCount;
        public int redHeartCount;
        public int blueHeartCount;
        public int greenHeartCount;
        public int blackHeartCount;

        const int MINIMAL_CREDIT = 3;
        const int STARTING_EXPERIENCE = 30;
        const int EXPERIENCE_TO_LEVEL_UP = 100;
        const int TIME_PER_ROUND = 60;

        public void WhenGameJustStart()
        {
            if (credit >= MINIMAL_CREDIT)
            {
                credit -= MINIMAL_CREDIT;
                experience += STARTING_EXPERIENCE;
                // NOTE: The extra points are not stored.
                if (experience >= EXPERIENCE_TO_LEVEL_UP)
                {
                    level += 1;
                    payoutMultiplier += 0.1f;
                    experience %= EXPERIENCE_TO_LEVEL_UP;
                }
            }
            else
            {
                Debug.Log("Cant Start. Credit is not enough. . Your current credit is " + credit);
            }
        }

        public void WhenTimeOut()
        {
            Debug.Log("This round is done");
            earning += score * (int)payoutMultiplier;
            totalTimeLeft = TIME_PER_ROUND;
            score = 0;
        }
    }
    public class HeartData
    {
        public int id;
        public bool active;
        public HeartColor color;
        public GameObject gameObject;
        public int columnNumber;
        public int rowNumber;
        public int basePoint;

        public void SetColor(HeartColor c, Dictionary<HeartColor, Sprite> hs)
        {
            color = c;
            gameObject.GetComponent<Image>().sprite = hs[c];
            if (c == HeartColor.Yellow)
            {
                basePoint = 1;
            }
            else if (c == HeartColor.Green)
            {
                basePoint = 2;
            }
            else if (c == HeartColor.Black)
            {
                basePoint = 3;
            }
            else if (c == HeartColor.Red)
            {
                basePoint = 4;
            }
            else if (c == HeartColor.Blue)
            {
                basePoint = 5;
            }
        }

        public void SetActive(bool b)
        {
            active = b;
            gameObject.GetComponent<Image>().enabled = b;
        }

        public bool GetActive()
        {
            return active;
        }
    }

    public Dictionary<HeartColor, Sprite> heartSprites = new Dictionary<HeartColor, Sprite>();
    private Dictionary<int, List<HeartData>> heartData = new Dictionary<int, List<HeartData>>();
    private List<HeartData> linkedHearts = new List<HeartData>();

    private GameObject[] heartRows;
    private GameData gameData = new GameData();

    private GameObject parentUI;
    private GameObject pointLabelUI;
    private GameObject pointMultiplierUI;
    private GameObject creditLabelUI;
    private GameObject levelLabelUI;
    private GameObject timeLabelUI;
    private GameObject scoreLabelUI;
    /**
        total hearts = 7 x 8 = 56
        max yellow = 22
        max green = 14
        max black = 11
        max red = 6
        max blue = 3
    **/

    // These are the weight a.k.a. the distribution for every heart color
    const int MAX_YELLOW = 22;
    const int MAX_GREEN = 14;
    const int MAX_BLACK = 11;
    const int MAX_RED = 6;
    const int MAX_BLUE = 3;
    void Start()
    {
        // Setup gameData
        gameData.heartColorAmount = new Dictionary<HeartColor, int>();
        gameData.level = 1;
        gameData.credit = 90;
        gameData.score = 0;
        gameData.payoutMultiplier = 1.0f;
        gameData.experience = 0;
        gameData.totalTimeLeft = 60.0f; // Time per round

        gameData.WhenGameJustStart();

        // Get and Set UI
        parentUI = GameObject.FindWithTag("UI");
        foreach (Transform uit in parentUI.transform)
        {
            if (uit.name == "Point")
            {
                foreach (Transform cuit in uit)
                {
                    if (cuit.name == "PointLabel")
                    {
                        pointLabelUI = cuit.gameObject;
                        pointLabelUI.GetComponent<Text>().text = gameData.earning.ToString();
                    }
                    if (cuit.name == "PointMultiplier")
                    {
                        pointMultiplierUI = cuit.gameObject;
                        pointMultiplierUI.GetComponent<Text>().text = $"Skor multiplier: {gameData.payoutMultiplier.ToString()}x";
                    }
                }
            }
            else if (uit.name == "Credit")
            {
                foreach (Transform cuit in uit)
                {
                    if (cuit.name == "CreditLabel")
                    {
                        creditLabelUI = cuit.gameObject;
                        creditLabelUI.GetComponent<Text>().text = $"{gameData.credit.ToString()}/100";
                    }
                    if (cuit.name == "LevelLabel")
                    {
                        levelLabelUI = cuit.gameObject;
                        levelLabelUI.GetComponent<Text>().text = $"Level: {gameData.level.ToString()}";
                    }
                }
            }
            else if (uit.name == "GamePlay")
            {
                foreach (Transform cuit in uit)
                {
                    if (cuit.name == "TimeLabel")
                    {
                        timeLabelUI = cuit.gameObject;
                        timeLabelUI.GetComponent<Text>().text = $"Time Left\n{gameData.totalTimeLeft.ToString()}";
                    }
                    if (cuit.name == "ScoreLabel")
                    {
                        scoreLabelUI = cuit.gameObject;
                        scoreLabelUI.GetComponent<Text>().text = $"Score : {gameData.score.ToString()}";
                    }
                }
            }
        }

        // Setup sprite
        foreach (HeartColor hc in System.Enum.GetValues(typeof(HeartColor)))
        {
            Sprite loadedHeartSprite = null;
            if (hc == HeartColor.Yellow && gameData.yellowHeartCount <= MAX_YELLOW)
            {
                loadedHeartSprite = Resources.Load<Sprite>("yellow");
            }
            else if (hc == HeartColor.Green && gameData.greenHeartCount <= MAX_GREEN)
            {
                loadedHeartSprite = Resources.Load<Sprite>("green");
            }
            else if (hc == HeartColor.Black && gameData.blackHeartCount <= MAX_BLACK)
            {
                loadedHeartSprite = Resources.Load<Sprite>("black");
            }
            else if (hc == HeartColor.Red && gameData.redHeartCount <= MAX_RED)
            {
                loadedHeartSprite = Resources.Load<Sprite>("red");
            }
            else if (hc == HeartColor.Blue && gameData.blueHeartCount <= MAX_BLUE)
            {
                loadedHeartSprite = Resources.Load<Sprite>("blue");
            }
            heartSprites.Add(hc, loadedHeartSprite);
        }

        // Setup heartData
        heartRows = GameObject.FindGameObjectsWithTag("Row");
        int id = 0;
        foreach (GameObject r in heartRows)
        {
            List<HeartData> heartButtons = new List<HeartData>();
            foreach (Transform heartsButtonTransform in r.transform)
            {
                GameObject heartButton = heartsButtonTransform.gameObject;
                HeartData data = new HeartData();
                data.id = id;
                data.active = true;
                heartButton.transform.GetChild(0).gameObject.GetComponent<Text>().text = id.ToString();
                id += 1;
                data.gameObject = heartButton;
                data.columnNumber = r.transform.GetSiblingIndex();
                data.rowNumber = heartsButtonTransform.GetSiblingIndex();
                data.SetColor(PickRandomHeartWithWeight(), heartSprites);
                heartButton.GetComponent<Button>().onClick.AddListener(() => ButtonPressed(data));
                heartButtons.Add(data);
            }
            heartData.Add(heartButtons[0].columnNumber, heartButtons);

        }
        // Debug.Log("Col Amount : " + heartData.Count);
        // Debug.Log("Row Amount : " + heartData[0].Count);
    }

    public HeartColor PickRandomHeartWithWeight()
    {
        HeartColor hc = (HeartColor)Random.Range(0, System.Enum.GetValues(typeof(HeartColor)).Length);
        while (!CheckHeartCount(hc))
        {
            hc = (HeartColor)Random.Range(0, System.Enum.GetValues(typeof(HeartColor)).Length);
        }
        return hc;
    }

    private HeartColor RandomHeartColorBasedOnWeight()
    {
        float rnd = ((float)Random.Range(2, 20)) * 0.5f;
        if (rnd >= 1.0f || rnd < 4.0f)
        {
            return HeartColor.Yellow;
        }
        else if (rnd >= 4.0f || rnd < 6.5f)
        {
            return HeartColor.Green;
        }
        else if (rnd >= 6.5f || rnd < 8.5f)
        {
            return HeartColor.Black;
        }
        else if (rnd >= 8.5f || rnd < 9.5f)
        {
            return HeartColor.Red;
        }
        else
        {
            return HeartColor.Blue;
        }
    }

    private bool CheckHeartCount(HeartColor hc)
    {
        if (hc == HeartColor.Yellow && gameData.yellowHeartCount < MAX_YELLOW)
        {
            gameData.yellowHeartCount += 1;
            return true;
        }
        else if (hc == HeartColor.Green && gameData.greenHeartCount < MAX_GREEN)
        {
            gameData.greenHeartCount += 1;
            return true;

        }
        else if (hc == HeartColor.Black && gameData.blackHeartCount < MAX_BLACK)
        {
            gameData.blackHeartCount += 1;
            return true;

        }
        else if (hc == HeartColor.Red && gameData.redHeartCount < MAX_RED)
        {
            gameData.redHeartCount += 1;
            return true;

        }
        else if (hc == HeartColor.Blue && gameData.blueHeartCount < MAX_BLUE)
        {
            gameData.blueHeartCount += 1;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ReduceHeartCountBasedOnColor(HeartColor hc)
    {
        if (hc == HeartColor.Yellow)
        {
            gameData.yellowHeartCount -= 1;
        }
        else if (hc == HeartColor.Green)
        {
            gameData.greenHeartCount -= 1;

        }
        else if (hc == HeartColor.Black)
        {
            gameData.blackHeartCount -= 1;

        }
        else if (hc == HeartColor.Red)
        {
            gameData.redHeartCount -= 1;

        }
        else if (hc == HeartColor.Blue)
        {
            gameData.blueHeartCount -= 1;
        }
    }

    public void ButtonPressed(HeartData data)
    {
        Debug.Log(data.GetActive());
        Debug.Log("Button Col/Row : " + data.columnNumber + "/" + data.rowNumber);
        if (data.GetActive() == true)
        {
            DetermineLinkedHearts(data);
        }
    }

    private void DetermineLinkedHearts(HeartData hd)
    {
        linkedHearts.Add(hd); // include the pressed heart
        List<HeartData> nh = FindNeighbourHearts(hd);

        // after the first neighbour found, search chaining heart across the grid
        while (nh.Count != 0)
        {
            List<HeartData> newNH = new List<HeartData>();
            foreach (HeartData d in nh)
            {
                if (!IsHeartDataFound(d)) { linkedHearts.Add(d); } // add to linkedHearts if not already exist
                List<HeartData> n = FindNeighbourHearts(d);
                newNH.AddRange(n);
            }
            nh = newNH;
        }

        // calculate point
        if (linkedHearts.Count > 3)
        {
            gameData.score += 1 + linkedHearts[0].basePoint + BasePointIncrementByOne(linkedHearts[0].basePoint, linkedHearts.Count - 2);
        }
        else if (linkedHearts.Count == 3)
        {
            gameData.score += 1 + linkedHearts[0].basePoint;
        }
        else if (linkedHearts.Count == 2)
        {
            gameData.score += 1;
        }
        Debug.Log("Current Score : " + gameData.score);


        foreach (HeartData d in linkedHearts)
        {
            // Debug.Log(d.id);
            d.SetActive(false);

            // ReduceHeartCountBasedOnColor(d.color); // reduce 
            HeartColor changedColor = RandomHeartColorBasedOnWeight();
            d.SetColor(changedColor, heartSprites);
            d.SetActive(true); // show new heart image 

            /// Change erased hearts color, sprite, and basePoint
            // do transition animation
        }

        linkedHearts.Clear();
        UpdateUI(); // updating UI
    }

    private List<HeartData> FindNeighbourHearts(HeartData hd)
    {
        int top = hd.columnNumber - 1; // key
        int bot = hd.columnNumber + 1; // key
        int left = hd.rowNumber - 1; // index
        int right = hd.rowNumber + 1; // index

        List<HeartData> neighbourHearts = new List<HeartData>();

        if (top >= 0)
        {
            HeartData topData = heartData[top][hd.rowNumber];
            if (!IsHeartDataFound(topData))
            {
                CompareAndAddHeartColor(neighbourHearts, hd, topData);
            }
        }
        if (bot <= heartData.Keys.Count - 1)
        {
            HeartData botData = heartData[bot][hd.rowNumber];
            if (!IsHeartDataFound(botData))
            {
                CompareAndAddHeartColor(neighbourHearts, hd, botData);
            }
        }
        if (left >= 0)
        {
            HeartData leftData = heartData[hd.columnNumber][left];
            if (!IsHeartDataFound(leftData))
            {
                CompareAndAddHeartColor(neighbourHearts, hd, leftData);
            }

        }
        if (right <= heartData.Last().Value.Count - 1)
        {
            HeartData rightData = heartData[hd.columnNumber][right];
            if (!IsHeartDataFound(rightData))
            {
                CompareAndAddHeartColor(neighbourHearts, hd, rightData);
            }
        }

        return neighbourHearts;
    }

    private int BasePointIncrementByOne(int basePoint, int numberOfElement)
    {
        int result = basePoint;
        for (int i = 1; i != numberOfElement; i += 1)
        {
            result += 1;
        }
        return result;
    }

    private bool IsHeartDataFound(HeartData hd)
    {
        return linkedHearts.Exists(d => d.id == hd.id);
    }
    private void CompareAndAddHeartColor(List<HeartData> ld, HeartData cd, HeartData d)
    {
        if (cd.color.Equals(d.color))
        {
            ld.Add(d);
        }
    }

    private void UpdateUI()
    {
        pointLabelUI.GetComponent<Text>().text = gameData.earning.ToString();
        pointMultiplierUI.GetComponent<Text>().text = $"Skor multiplier: {gameData.payoutMultiplier.ToString()}x";
        creditLabelUI.GetComponent<Text>().text = $"{gameData.credit.ToString()}/100";
        levelLabelUI.GetComponent<Text>().text = $"Level: {gameData.level.ToString()}";
        scoreLabelUI.GetComponent<Text>().text = $"Score : {gameData.score.ToString()}";
    }

    // Timer implementation
    void Update()
    {
        gameData.totalTimeLeft -= Time.deltaTime;

        timeLabelUI.GetComponent<Text>().text = $"Time Left\n{((int)gameData.totalTimeLeft).ToString()}";
        // when timeout
        if (gameData.totalTimeLeft <= 0.0f)
        {
            gameData.WhenTimeOut();
            UpdateUI();

            // reset heart count
            gameData.yellowHeartCount = 0;
            gameData.greenHeartCount = 0;
            gameData.blackHeartCount = 0;
            gameData.redHeartCount = 0;
            gameData.blueHeartCount = 0;

            // reset the grid
            foreach (int k in heartData.Keys)
            {
                for (int i = 0; i < heartData.Values.Count; i += 1)
                {
                    heartData[k][i].SetColor(PickRandomHeartWithWeight(), heartSprites);
                }
            }

        }
    }
}
