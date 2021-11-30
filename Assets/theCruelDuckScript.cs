using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using KModkit;
using System.Text.RegularExpressions;

public class theCruelDuckScript : MonoBehaviour {

    public KMBombModule Module; // this code is a shitshow and i'm sorry
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable curtainSelectable;
    public KMSelectable[] duckParts, btnSelectables, itemSelectables;
    public TextMesh[] btnText;
    public Material[] polkaCurtains, vertiCurtains, horiCurtains, ringCurtains, items, open;
    public MeshRenderer surface;
    public MeshRenderer[] btnRenderers, itemRenderers;

    private static int _moduleIdCounter = 1;
    private int _moduleId;
    private bool solved = false;

    private int curtainType = 0;
    private int mainCurtainColor = 0;
    private bool topToBottom = true;
    private int curtainNum = 0;
    private static readonly int[] topToBottomIndices = { 0, 10, 18, 24, 28, 30 };
    private static readonly int[] theresProbablyABetterWayOfDoingThisButWhatever = {
        2, 7, 11, 15, 18, 21, 24, 26, 28, 29, // orange
        3, 8, 12, 16, 19, 22, 25, 27, // purple
        4, 9, 13, 17, 20, 23, // yellow
        0, 5, 10, 14, // blue
        1, 6 }; // green
    private static readonly string[] curtainTypeNames = { "polka dots", "vertical stripes", "horizontal stripes", "rings" };
    private static readonly string[] colors = { "red", "green", "blue", "yellow", "purple", "orange" };
    private int behindCurtain = 0;
    private int nothingPress = 0;
    private int[] shownApproaches = { 0, 0, 0, 0 };
    private int[] shownItems = { 0, 0, 0, 0 };

    private int correctApproach = 0;
    private int[] fuckPelicans = {
        0, 1, 2,
        3, 4, 1,
        0, 3, 5,
        1, 5, 4,
        2, 4, 0,
        4, 0, 5,
        5, 2, 4,
        3, 2, 0, 
        4, 2, 5,
        1, 5, 2
    };
    private static readonly string[] waysToFuckPelicans = { "shoot sky with shotgun", "whack with\ntennis racket", "kick with boots", "blast with airhorn", "spray with water gun", "throw a tomato at it" };
    
    private static readonly string[] possibleApproaches = { "walk to the duck", "run to the duck", "dive to the duck", "fly to the duck", "sneak to the duck", "don't approach" };
    private int correctItemRule = 0;
    // item rules: "nothing", "book", "slice of cake, kite, or bell", "slice of cake", "book or pencil", "nothing, because you're not approaching"
    private static readonly string[] possibleItems = { "nothing", "bell", "bomb", "book", "slice of cake", "kite", "pencil" };
    
    private static readonly string[] possibleParts = { "belly", "afro", "beak", "left foot", "eye", "tail", "right foot" };

    private bool pelicanIsGonePoggers = false;
    private bool itemSelected = false;
    private int iDidntPlanThisThroughSoICantNameThisVariableItemSelectedSadface = 0;

    private int[] duckPartSequence;
    private int duckPartIndex = 0;

    private bool placeholder = false;

    // Use this for initialization
    void Start () {
        _moduleId = _moduleIdCounter++;
        Module.OnActivate += Activate;

        Init();
    }
	
    void Activate()
    {
        for (int i = 0; i < 7; i++)
        {
            int j = i;
            duckParts[i].OnInteract += delegate ()
            {
                if (!solved)
                    Press(j);
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, duckParts[j].transform);
                duckParts[j].AddInteractionPunch(2);
                return false;
            };
        }
        
        for (int i = 0; i < 4; i++)
        {
            int j = i;
            btnSelectables[i].OnInteract += delegate ()
            {
                if (!solved)
                    BtnPress(j);
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, btnSelectables[j].transform);
                btnSelectables[j].AddInteractionPunch();
                return false;
            };
        }

        for (int i = 0; i < 4; i++)
        {
            int j = i;
            itemSelectables[i].OnInteract += delegate ()
            {
                if (!solved)
                    ItemPress(j);
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, itemSelectables[j].transform);
                itemSelectables[j].AddInteractionPunch();
                return false;
            };
        }

        curtainSelectable.OnInteract += delegate ()
        {
            if (!solved && curtainSelectable.enabled)
            {
                OpenCurtain();
            }

            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
            curtainSelectable.AddInteractionPunch();

            return false;
        };
    }

    void Init()
    {
        placeholder = false;
        duckPartIndex = 0;
        curtainSelectable.enabled = true;
        for (int i = 0; i < 7; i++)
        {
            duckParts[i].gameObject.SetActive(false);
            duckParts[i].enabled = false;
        }
        for (int i = 0; i < 4; i++)
        {
            itemRenderers[i].gameObject.SetActive(false);
            itemSelectables[i].enabled = false;
        }
        for (int i = 0; i < 4; i++)
        {
            btnRenderers[i].gameObject.SetActive(false);
            btnSelectables[i].enabled = false;
            btnText[i].text = "";
        }

        curtainType = Random.Range(0, 4);
        topToBottom = Bomb.GetBatteryCount() < 2;
        if (topToBottom)
        {
            mainCurtainColor = Random.Range(0, 5);
            curtainNum = Random.Range(topToBottomIndices[mainCurtainColor], topToBottomIndices[mainCurtainColor + 1]);
        }
        else
        {
            mainCurtainColor = Random.Range(1, 6);
            curtainNum = theresProbablyABetterWayOfDoingThisButWhatever[Random.Range(topToBottomIndices[5 - mainCurtainColor], topToBottomIndices[5 - mainCurtainColor + 1])];
        }

        switch (curtainType)
        {
            case 0:
                surface.material = polkaCurtains[curtainNum];
                break;
            case 1:
                surface.material = vertiCurtains[curtainNum];
                break;
            case 2:
                surface.material = horiCurtains[curtainNum];
                break;
            case 3:
                surface.material = ringCurtains[curtainNum];
                break;
        }

        Debug.LogFormat("[The Cruel Duck #{0}] The curtain has {1}, and the prioritized color is {2}.", _moduleId, curtainTypeNames[curtainType], colors[mainCurtainColor]);
    }

    void Press(int duckPart)
    {
        switch (correctApproach)
        {
            case 0:
                switch (iDidntPlanThisThroughSoICantNameThisVariableItemSelectedSadface)
                {
                    case 0:
                        duckPartSequence = new int[3] { 5, 6, 3 };
                        break;
                    case 4:
                        duckPartSequence = new int[1] { 4 };
                        break;
                    default:
                        duckPartSequence = new int[3] { 3, 5, 2 };
                        break;
                }
                break;
            case 1:
                switch (iDidntPlanThisThroughSoICantNameThisVariableItemSelectedSadface)
                {
                    case 0:
                        duckPartSequence = new int[3] { 4, 3, 1 };
                        break;
                    default:
                        duckPartSequence = new int[3] { 0, 5, 6 };
                        break;
                }
                break;
            case 2:
                duckPartSequence = new int[6] { 1, 0, 2, 6, 5, 3 };
                break;
            case 3:
                duckPartSequence = new int[1] { 2 };
                break;
            case 4:
                switch (iDidntPlanThisThroughSoICantNameThisVariableItemSelectedSadface)
                {
                    case 0:
                        duckPartSequence = new int[4] { 3, 2, 1, 5 };
                        break;
                    default:
                        duckPartSequence = new int[1] { 0 };
                        break;
                }
                break;
            default:
                duckPartSequence = new int[1] { 1 };
                break;
        }
        
        Debug.LogFormat("[The Cruel Duck #{0}] You should now press the duck's {1}.", _moduleId, possibleParts[duckPartSequence[duckPartIndex]]);

        if (duckPart != duckPartSequence[duckPartIndex])
        {
            Module.HandleStrike();
            Debug.LogFormat("[The Cruel Duck #{0}] You pressed {1}. Strike.", _moduleId, possibleParts[duckPart]);
            Init();
        }

        else if (correctApproach == 0 && iDidntPlanThisThroughSoICantNameThisVariableItemSelectedSadface == 0 && duckPartIndex == 1 && !Bomb.GetFormattedTime().Contains('5'))
        {
            Module.HandleStrike();
            Debug.LogFormat("[The Cruel Duck #{0}] You were supposed to press the right foot when there was a 5 in the timer!", _moduleId, possibleParts[duckPart]);
            Init();
        }

        else if (correctApproach == 0 && iDidntPlanThisThroughSoICantNameThisVariableItemSelectedSadface == 4 && (int)Bomb.GetTime() % 10 + ((int)(Bomb.GetTime() % 60 / 10)) != 7)
        {
            Module.HandleStrike();
            Debug.LogFormat("[The Cruel Duck #{0}] You were supposed to press the eye when the seconds digits added up to 7!", _moduleId, possibleParts[duckPart]);
            Init();
        }

        else if (correctApproach == 1 && iDidntPlanThisThroughSoICantNameThisVariableItemSelectedSadface == 3 && duckPartIndex == 1 && (int)Bomb.GetTime() % 10 + ((int)(Bomb.GetTime() % 60 / 10)) != 6)
        {
            Module.HandleStrike();
            Debug.LogFormat("[The Cruel Duck #{0}] You were supposed to press the tail when the seconds digits added up to 6!", _moduleId, possibleParts[duckPart]);
            Init();
        }

        else if (correctApproach == 2 && duckPartIndex == 0 && Bomb.GetSolvedModuleNames().Count() % 2 == 1)
        {
            Module.HandleStrike();
            Debug.LogFormat("[The Cruel Duck #{0}] You were supposed to press the afro when there were an even number of solves!", _moduleId, possibleParts[duckPart]);
            Init();
        }

        else if (Random.Range(0, duckPartSequence.Length) != 0 && duckPartIndex != duckPartSequence.Length - 1)
        {
            Audio.PlaySoundAtTransform("quack", Module.transform);
            Debug.LogFormat("[The Cruel Duck #{0}] Quack.", _moduleId);
            duckPartIndex++;
        }
        else
        {
            Module.HandlePass();
            Audio.PlaySoundAtTransform("cruelSolveSound", Module.transform);
            solved = true;
            Debug.LogFormat("[The Cruel Duck #{0}] That was correct! Module solved.", _moduleId);
            surface.material = open[3];

            for (int i = 0; i < 7; i++)
            {
                duckParts[i].gameObject.SetActive(false);
                duckParts[i].enabled = false;
            }
        }
    }

    void BtnPress(int btnNumber)
    {
        if (shownApproaches[btnNumber] != correctApproach)
        {
            Module.HandleStrike();
            Debug.LogFormat("[The Cruel Duck #{0}] You pressed {1} when you should've pressed {2}. Strike.", _moduleId, possibleApproaches[shownApproaches[btnNumber]], possibleApproaches[correctApproach]);
            Init();
        }

        else if (!itemSelected && correctItemRule != 0 && correctItemRule != 5)
        {
            Module.HandleStrike();
            Debug.LogFormat("[The Cruel Duck #{0}] You had the right approach ({1}), but you were supposed to select an item! Strike.", _moduleId, possibleApproaches[correctApproach]);
            Init();
        }

        else
        {
            Debug.LogFormat("[The Cruel Duck #{0}] You pressed {1}. That was correct.", _moduleId, possibleApproaches[correctApproach]);
            if (behindCurtain == 1)
            {
                pelicanIsGonePoggers = true;
                OpenCurtain();
            }
            else
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, Module.transform);
                for (int i = 0; i < 7; i++)
                    duckParts[i].gameObject.SetActive(true);
            }

            if (shownApproaches[btnNumber] == 5 && Random.Range(0, 3) == 0 && behindCurtain > 1)
            {
                Module.HandlePass();
                Audio.PlaySoundAtTransform("cruelSolveSound", Module.transform);
                solved = true;
                Debug.LogFormat("[The Cruel Duck #{0}] That was correct! Module solved.", _moduleId);
                surface.material = open[3];
            }
        }
        
        for (int i = 0; i < 4; i++)
        {
            btnRenderers[i].gameObject.SetActive(false);
            btnSelectables[i].enabled = false;
            itemRenderers[i].gameObject.SetActive(false);
            itemSelectables[i].enabled = false;
            btnText[i].text = "";
        }
    }

    void ItemPress(int btnNumber)
    {
        if (correctItemRule == 0 || correctItemRule == 6)
        {
            Module.HandleStrike();
            Debug.LogFormat("[The Cruel Duck #{0}] You selected an item when you shouldn't have. Strike.", _moduleId);
            Init();
        }

        else if (correctItemRule == 1 && shownItems[btnNumber] != 3)
        {
            Module.HandleStrike();
            Debug.LogFormat("[The Cruel Duck #{0}] You were supposed to select a book, but you selected a {1}. Strike.", _moduleId, possibleItems[shownItems[btnNumber]]);
            Init();
        }

        else if (correctItemRule == 2 && shownItems[btnNumber] != 1 && shownItems[btnNumber] != 4 && shownItems[btnNumber] != 5)
        {
            Module.HandleStrike();
            Debug.LogFormat("[The Cruel Duck #{0}] You were supposed to select a slice of cake, kite, or bell, but you selected a {1}. Strike.", _moduleId, possibleItems[shownItems[btnNumber]]);
            Init();
        }

        else if (correctItemRule == 3 && shownItems[btnNumber] != 4)
        {
            Module.HandleStrike();
            Debug.LogFormat("[The Cruel Duck #{0}] You were supposed to select a slice of cake, but you selected a {1}. Strike.", _moduleId, possibleItems[shownItems[btnNumber]]);
            Init();
        }

        else if (correctItemRule == 4 && shownItems[btnNumber] != 3 && shownItems[btnNumber] != 6)
        {
            Module.HandleStrike();
            Debug.LogFormat("[The Cruel Duck #{0}] You were supposed to select a book or pencil, but you selected a {1}. Strike.", _moduleId, possibleItems[shownItems[btnNumber]]);
            Init();
        }

        else
        {
            Debug.LogFormat("[The Cruel Duck #{0}] You selected {1}. That was correct.", _moduleId, possibleItems[shownItems[btnNumber]]);
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, Module.transform);
            iDidntPlanThisThroughSoICantNameThisVariableItemSelectedSadface = shownItems[btnNumber];
            itemSelected = true;

            for (int i = 0; i < 4; i++)
                itemRenderers[i].gameObject.SetActive(false);
        }
    }

    void OpenCurtain()
    {
        behindCurtain = Random.Range(0, 6);
        if (pelicanIsGonePoggers)
            behindCurtain = 0;
        pelicanIsGonePoggers = false;
        correctApproach = correctItemRule = 0;

        if (placeholder)
        {
            if ((int)Bomb.GetTime() % 10 == nothingPress)
            {
                Module.HandlePass();
                Audio.PlaySoundAtTransform("cruelSolveSound", Module.transform);
                solved = true;
                Debug.LogFormat("[The Cruel Duck #{0}] That was correct! Module solved.", _moduleId);
            }

            else
            {
                Module.HandleStrike();
                Debug.LogFormat("[The Cruel Duck #{0}] You pressed at {1}. Strike.", _moduleId, ((int)Bomb.GetTime() % 10).ToString());
                Init();
            }
        }

        else if (behindCurtain == 0)
        {
            surface.material = open[0];
            curtainSelectable.enabled = placeholder = true;

            for (int i = 0; i < 4; i++)
            {
                btnRenderers[i].gameObject.SetActive(false);
                btnSelectables[i].enabled = false;
                btnText[i].text = "";
            }

            nothingPress = Bomb.GetSerialNumberNumbers().Sum();
            while (nothingPress > 9)
                nothingPress -= 5;

            Debug.LogFormat("[The Cruel Duck #{0}] There is nothing behind the curtain.", _moduleId);
            Debug.LogFormat("[The Cruel Duck #{0}] You should press the module when the last digit of the timer is {1}.", _moduleId, nothingPress);
        }
        else if (behindCurtain == 1)
        {
            surface.material = open[1];
            curtainSelectable.enabled = false;
            
            var literallyABunchOfNumbers = Enumerable.Range(0, 6).ToList().Shuffle().ToArray();
            shownApproaches = new int[4] { literallyABunchOfNumbers[0], literallyABunchOfNumbers[1], literallyABunchOfNumbers[2], literallyABunchOfNumbers[3] };

            correctApproach = -1;
            while (correctApproach == -1)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (shownApproaches.Contains(fuckPelicans[i + Bomb.GetSerialNumberNumbers().Last() * 3]))
                    {
                        correctApproach = fuckPelicans[i + Bomb.GetSerialNumberNumbers().Last() * 3];
                        break;
                    }
                }
                if (correctApproach == -1)
                {
                    literallyABunchOfNumbers = Enumerable.Range(0, 6).ToList().Shuffle().ToArray();
                    shownApproaches = new int[4] { literallyABunchOfNumbers[0], literallyABunchOfNumbers[1], literallyABunchOfNumbers[2], literallyABunchOfNumbers[3] };
                }
            }

            for (int i = 0; i < 4; i++)
            {
                btnRenderers[i].gameObject.SetActive(true);
                btnSelectables[i].enabled = true;
                btnText[i].text = waysToFuckPelicans[shownApproaches[i]];
            }

            Debug.LogFormat("[The Cruel Duck #{0}] There is a pelican behind the curtain.", _moduleId);
            Debug.LogFormat("[The Cruel Duck #{0}] The possible ways to shoo the pelican are to {1}, {2}, {3}, and {4}.", _moduleId, waysToFuckPelicans[shownApproaches[0]].Replace('\n', ' '), waysToFuckPelicans[shownApproaches[1]].Replace('\n', ' '), waysToFuckPelicans[shownApproaches[2]].Replace('\n', ' '), waysToFuckPelicans[shownApproaches[3]].Replace('\n', ' '));
            Debug.LogFormat("[The Cruel Duck #{0}] You should {1}.", _moduleId, waysToFuckPelicans[correctApproach].Replace('\n', ' '));
        }
        else
        {
            surface.material = open[2];
            curtainSelectable.enabled = false;

            var literallyABunchOfNumbers = Enumerable.Range(0, 5).ToList().Shuffle().ToArray();
            shownApproaches = new int[4] { literallyABunchOfNumbers[0], literallyABunchOfNumbers[1], literallyABunchOfNumbers[2], literallyABunchOfNumbers[3] };
            literallyABunchOfNumbers = Enumerable.Range(1, 6).ToList().Shuffle().ToArray();
            shownItems = new int[4] { literallyABunchOfNumbers[0], literallyABunchOfNumbers[1], literallyABunchOfNumbers[2], literallyABunchOfNumbers[3] };
            
            switch (mainCurtainColor)
            {
                case 0:
                    if (Bomb.GetIndicators().Count() < 2)
                    {
                        correctApproach = 0;
                        correctItemRule = 0;
                    }
                    else if (curtainType == 1 || curtainType == 3)
                    {
                        correctApproach = 1;
                        correctItemRule = 1;
                    }
                    else
                    {
                        correctApproach = 0;
                        correctItemRule = 1;
                    }
                    break;
                case 1:
                    if (curtainType != 1)
                    {
                        correctApproach = 5;
                        correctItemRule = 5;
                    }
                    else
                    {
                        correctApproach = 3;
                        correctItemRule = 0;
                    }
                    break;
                case 2:
                    if (Bomb.IsPortPresent(Port.RJ45))
                    {
                        correctApproach = 2;
                        correctItemRule = 0;
                    }
                    else
                    {
                        correctApproach = 0;
                        correctItemRule = 2;
                    }
                    break;
                case 3:
                    if (Bomb.IsIndicatorOn("SIG") || Bomb.IsIndicatorOff("BOB"))
                    {
                        correctApproach = 1;
                        correctItemRule = 0;
                    }
                    else if (curtainType != 3)
                    {
                        correctApproach = 3;
                        correctItemRule = 0;
                    }
                    else
                    {
                        correctApproach = 0;
                        correctItemRule = 3;
                    }
                    break;
                case 4:
                    if (Bomb.GetPortPlateCount() > 1)
                    {
                        correctApproach = 4;
                        correctItemRule = 4;
                    }
                    else
                    {
                        correctApproach = 4;
                        correctItemRule = 0;
                    }
                    break;
                case 5:
                    if (curtainType == 0 || curtainType == 3)
                    {
                        correctApproach = 4;
                        correctItemRule = 0;
                    }
                    else
                    {
                        correctApproach = 5;
                        correctItemRule = 5;
                    }
                    break;
            }

            if (!shownApproaches.Contains(correctApproach))
                shownApproaches[Random.Range(0, 4)] = correctApproach;

            if (correctItemRule == 1 && !shownItems.Contains(3))
                shownItems[Random.Range(0, 4)] = 3;
            else if (correctItemRule == 3 && !shownItems.Contains(4))
                shownItems[Random.Range(0, 4)] = 4;
            else if (correctItemRule == 4 && !shownItems.Contains(3) && !shownItems.Contains(6))
                shownItems[Random.Range(0, 4)] = 6;
            
            for (int i = 0; i < 4; i++)
            {
                btnRenderers[i].gameObject.SetActive(true);
                btnSelectables[i].enabled = false;
                itemRenderers[i].gameObject.SetActive(true);
                itemSelectables[i].enabled = false;
                btnText[i].text = possibleApproaches[shownApproaches[i]];
                itemRenderers[i].material = items[shownItems[i] - 1];
            }

            for (int i = 0; i < 7; i++)
            {
                duckParts[i].gameObject.SetActive(false);
                duckParts[i].enabled = false;
            }
        }
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} curtain (#) [Pulls back the curtain (optionally when the last digit of the bomb's timer is '#')] | !{0} TL/TR/BL/BR [Presses the approach button in the specified positon (top-left, top-right, etc.) | !{0} item <pos> [Takes the item in the specified position (the same positions as approach)] | !{0} belly/beak/afro/tail/eye/left/right (##:##) [Clicks the specified part of the duck (optionally when the bomb timer is '##:##')]";
    bool ZenModeActive;
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*tl\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*topleft\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*top-left\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (curtainSelectable.enabled)
            {
                yield return "sendtochaterror The approach buttons must be present before an approach button can be pressed!";
                yield break;
            }
            else if (duckParts[0].gameObject.activeSelf)
            {
                yield return "sendtochaterror The duck has already been approached!";
                yield break;
            }
            btnSelectables[0].OnInteract();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*bl\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*bottomleft\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*bottom-left\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (curtainSelectable.enabled)
            {
                yield return "sendtochaterror The curtain must be pulled back before an approach button can be pressed!";
                yield break;
            }
            else if (duckParts[0].gameObject.activeSelf)
            {
                yield return "sendtochaterror The duck has already been approached!";
                yield break;
            }
            btnSelectables[1].OnInteract();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*tr\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*topright\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*top-right\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (curtainSelectable.enabled)
            {
                yield return "sendtochaterror The curtain must be pulled back before an approach button can be pressed!";
                yield break;
            }
            else if (duckParts[0].gameObject.activeSelf)
            {
                yield return "sendtochaterror The duck has already been approached!";
                yield break;
            }
            btnSelectables[2].OnInteract();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*br\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*bottomright\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*bottom-right\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (curtainSelectable.enabled)
            {
                yield return "sendtochaterror The curtain must be pulled back before an approach button can be pressed!";
                yield break;
            }
            else if (duckParts[0].gameObject.activeSelf)
            {
                yield return "sendtochaterror The duck has already been approached!";
                yield break;
            }
            btnSelectables[3].OnInteract();
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*curtain\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 2)
            {
                if (!curtainSelectable.enabled)
                {
                    yield return "sendtochaterror The curtains are currently not present!";
                    yield break;
                }
                int temp = 0;
                if (!int.TryParse(parameters[1], out temp))
                {
                    yield return "sendtochaterror!f The specified digit '" + parameters[1] + "' is invalid!";
                    yield break;
                }
                if (temp < 0 || temp > 9)
                {
                    yield return "sendtochaterror The specified digit '" + parameters[1] + "' is out of range 0-9!";
                    yield break;
                }
                while ((int)Bomb.GetTime() % 10 != temp) { yield return "trycancel"; }
                curtainSelectable.OnInteract();
            }
            else if (parameters.Length == 1)
            {
                if (!curtainSelectable.enabled)
                {
                    yield return "sendtochaterror The curtains are currently not present!";
                    yield break;
                }
                curtainSelectable.OnInteract();
            }
            yield break;
        }
        if (Regex.IsMatch(parameters[0], @"^\s*item\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 2)
            {
                if (!itemSelectables[0].gameObject.activeSelf && !itemSelectables[1].gameObject.activeSelf && !itemSelectables[2].gameObject.activeSelf && !itemSelectables[3].gameObject.activeSelf)
                {
                    yield return "sendtochaterror The items must be present before an item button can be taken!";
                    yield break;
                }
                string[] pos1 = new string[] { "tl", "topleft", "top-left" };
                string[] pos2 = new string[] { "tr", "topright", "top-right" };
                string[] pos3 = new string[] { "bl", "bottomleft", "bottom-left" };
                string[] pos4 = new string[] { "br", "bottomright", "bottom-right" };
                if (pos1.Contains(parameters[1].ToLower()))
                    itemSelectables[0].OnInteract();
                else if (pos2.Contains(parameters[1].ToLower()))
                    itemSelectables[1].OnInteract();
                else if (pos3.Contains(parameters[1].ToLower()))
                    itemSelectables[2].OnInteract();
                else if (pos4.Contains(parameters[1].ToLower()))
                    itemSelectables[3].OnInteract();
                else
                    yield return "sendtochaterror!f The specified position '" + parameters[1] + "' is invalid!";
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the position of the item you wish to take!";
            }
            yield break;
        }
        if (Regex.IsMatch(parameters[0], @"^\s*belly\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[0], @"^\s*afro\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[0], @"^\s*beak\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[0], @"^\s*left\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[0], @"^\s*eye\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[0], @"^\s*tail\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[0], @"^\s*right\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            List<string> parts = new List<string>() { "belly", "afro", "beak", "left", "eye", "tail", "right" };
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 2)
            {
                if (!duckParts[0].gameObject.activeSelf)
                {
                    yield return "sendtochaterror The duck must be present for a duck part to be clicked!";
                    yield break;
                }
                int minutes = 0;
                int seconds = 0;
                string[] time = parameters[1].Split(':');
                if (time.Length != 2)
                {
                    yield return "sendtochaterror Invalid time format! Expected '<minutes>:<seconds>'!";
                    yield break;
                }
                if (!int.TryParse(time[0], out minutes))
                {
                    yield return "sendtochaterror!f The specified minutes '" + time[0] + "' is invalid!";
                    yield break;
                }
                if (minutes < 10 && !time[0].StartsWith("0"))
                {
                    yield return "sendtochaterror The specified minutes '" + time[0] + "' is invalid!";
                    yield break;
                }
                if (minutes < 0)
                {
                    yield return "sendtochaterror The specified minutes '" + time[0] + "' is less than 0!";
                    yield break;
                }
                if (!int.TryParse(time[1], out seconds))
                {
                    yield return "sendtochaterror!f The specified seconds '" + time[1] + "' is invalid!";
                    yield break;
                }
                if (seconds < 10 && !time[1].StartsWith("0"))
                {
                    yield return "sendtochaterror The specified seconds '" + time[1] + "' is invalid!";
                    yield break;
                }
                if (seconds < 0 || seconds > 59)
                {
                    yield return "sendtochaterror The specified seconds '" + time[1] + "' is out of range 00-59!";
                    yield break;
                }
                seconds += minutes * 60;
                if (ZenModeActive)
                {
                    if ((int)Bomb.GetTime() > seconds)
                    {
                        yield return "sendtochaterror The specified time has already passed!";
                        yield break;
                    }
                    if ((seconds - (int)Bomb.GetTime()) > 15)
                        yield return "waiting music";
                }
                else
                {
                    if ((int)Bomb.GetTime() < seconds)
                    {
                        yield return "sendtochaterror The specified time has already passed!";
                        yield break;
                    }
                    if (((int)Bomb.GetTime() - seconds) > 15)
                        yield return "waiting music";
                }
                while ((int)Bomb.GetTime() != seconds) { yield return "trycancel"; }
                yield return "end waiting music";
                duckParts[parts.IndexOf(parameters[0].ToLower())].OnInteract();
            }
            else if (parameters.Length == 1)
            {
                if (!duckParts[0].gameObject.activeSelf)
                {
                    yield return "sendtochaterror The duck must be present for a duck part to be clicked!";
                    yield break;
                }
                duckParts[parts.IndexOf(parameters[0].ToLower())].OnInteract();
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (curtainSelectable.enabled && !placeholder)
        {
            curtainSelectable.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        if (behindCurtain == 0)
        {
            while ((int)Bomb.GetTime() % 10 != nothingPress) { yield return true; }
            curtainSelectable.OnInteract();
        }
        else if (behindCurtain == 1)
        {
            for (int i = 0; i < 4; i++)
            {
                if (btnText[i].text.Equals(waysToFuckPelicans[correctApproach]))
                {
                    btnSelectables[i].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                    break;
                }
            }
            while ((int)Bomb.GetTime() % 10 != nothingPress) { yield return true; }
            curtainSelectable.OnInteract();
        }
        else if (behindCurtain > 1)
        {
            if (!itemSelected)
            {
                if (correctItemRule == 1)
                {
                    int index = -1;
                    for (int i = 0; i < 4; i++)
                    {
                        if (shownItems[i] == 3)
                        {
                            index = i;
                            break;
                        }
                    }
                    itemSelectables[index].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
                else if (correctItemRule == 2)
                {
                    List<int> indexes = new List<int>();
                    for (int i = 0; i < 4; i++)
                    {
                        if (shownItems[i] == 1 || shownItems[i] == 4 || shownItems[i] == 5)
                        {
                            indexes.Add(i);
                        }
                    }
                    itemSelectables[indexes[Random.Range(0, indexes.Count)]].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
                else if (correctItemRule == 3)
                {
                    int index = -1;
                    for (int i = 0; i < 4; i++)
                    {
                        if (shownItems[i] == 4)
                        {
                            index = i;
                            break;
                        }
                    }
                    itemSelectables[index].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
                else if (correctItemRule == 4)
                {
                    List<int> indexes = new List<int>();
                    for (int i = 0; i < 4; i++)
                    {
                        if (shownItems[i] == 3 || shownItems[i] == 6)
                        {
                            indexes.Add(i);
                        }
                    }
                    itemSelectables[indexes[Random.Range(0, indexes.Count)]].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
                else if (correctItemRule == 5)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (btnText[i].text.Equals(possibleApproaches[correctApproach]))
                        {
                            btnSelectables[i].OnInteract();
                            if (!solved)
                                yield return new WaitForSeconds(0.1f);
                            else
                                yield break;
                        }
                    }
                }
            }
            for (int i = 0; i < 4; i++)
            {
                if (btnText[i].text.Equals(possibleApproaches[correctApproach]))
                {
                    btnSelectables[i].OnInteract();
                    break;
                }
            }
            switch (correctApproach)
            {
                case 0:
                    switch (iDidntPlanThisThroughSoICantNameThisVariableItemSelectedSadface)
                    {
                        case 0:
                            duckPartSequence = new int[3] { 5, 6, 3 };
                            break;
                        case 4:
                            duckPartSequence = new int[1] { 4 };
                            break;
                        default:
                            duckPartSequence = new int[3] { 3, 5, 2 };
                            break;
                    }
                    break;
                case 1:
                    switch (iDidntPlanThisThroughSoICantNameThisVariableItemSelectedSadface)
                    {
                        case 0:
                            duckPartSequence = new int[3] { 4, 3, 1 };
                            break;
                        default:
                            duckPartSequence = new int[3] { 0, 5, 6 };
                            break;
                    }
                    break;
                case 2:
                    duckPartSequence = new int[6] { 1, 0, 2, 6, 5, 3 };
                    break;
                case 3:
                    duckPartSequence = new int[1] { 2 };
                    break;
                case 4:
                    switch (iDidntPlanThisThroughSoICantNameThisVariableItemSelectedSadface)
                    {
                        case 0:
                            duckPartSequence = new int[4] { 3, 2, 1, 5 };
                            break;
                        default:
                            duckPartSequence = new int[1] { 0 };
                            break;
                    }
                    break;
                default:
                    duckPartSequence = new int[1] { 1 };
                    break;
            }
            while (!solved)
            {
                if (correctApproach == 0 && iDidntPlanThisThroughSoICantNameThisVariableItemSelectedSadface == 0 && duckPartIndex == 1)
                {
                    while (!Bomb.GetFormattedTime().Contains('5')) { yield return true; }
                }
                else if (correctApproach == 0 && iDidntPlanThisThroughSoICantNameThisVariableItemSelectedSadface == 4)
                {
                    while ((int)Bomb.GetTime() % 10 + ((int)(Bomb.GetTime() % 60 / 10)) != 7) { yield return true; }
                }
                else if (correctApproach == 1 && iDidntPlanThisThroughSoICantNameThisVariableItemSelectedSadface == 3 && duckPartIndex == 1)
                {
                    while ((int)Bomb.GetTime() % 10 + ((int)(Bomb.GetTime() % 60 / 10)) != 6) { yield return true; }
                }
                else if (correctApproach == 2 && duckPartIndex == 0)
                {
                    while (Bomb.GetSolvedModuleNames().Count() % 2 == 1) { yield return true; }
                }
                duckParts[duckPartSequence[duckPartIndex]].OnInteract();
                if (!solved)
                    yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
